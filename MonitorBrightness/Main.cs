using System.Windows.Input;
using Wox.Plugin;


namespace MonitorBrightness
{
    public class Main : IPlugin, IDelayedExecutionPlugin, IContextMenu
    {
        private static readonly int NumberOfIncrements = 11;
        private PluginInitContext? Context { get; set; }

        public string Name => "Monitor Brightness";
        public string Description => "Adjusts the brightness of the current monitor";

        public void Init(PluginInitContext context)
        {
            Context = context;
        }

        public List<Result> Query(Query query)
        {
            return new List<Result>();
        }

        public List<Result> Query(Query query, bool delayedExecution)
        {
            if (query == null)
            {
                return new List<Result>();
            }
            var brightnesses = GetBrightnesses();
            return new List<Result>() {
                new Result()
                {
                    Title = "Change brightness",
                    SubTitle = GetSubtitle(brightnesses),
                    Action = e =>
                    {
                        return true;
                    },
                    ContextData = new ResultContext()
                    {
                        BrightnessInfos = brightnesses,
                    },
                }
            };
        }

        private static string GetSubtitle(IEnumerable<Monitors.MonitorBrightnessInfo> brightnesses)
        {
            return "Brightness: " + string.Join(
                                    ", ",
                                    brightnesses.Select(v => ToPercent(v.CurrentValue, v.MinValue, v.MaxValue))
                                    );
        }

        private static List<Monitors.MonitorBrightnessInfo> GetBrightnesses()
        {
            using var monitor = Monitors.GetMonitorsAtCursor();
            if (monitor == null)
            {
                return new();
            }

            return monitor.GetMonitorBrightnesses().ToList();
        }

        private static string ToPercent(uint value, uint min, uint max)
        {
            double zeroOnePercentage = ((double)value - min) / max;

            return (zeroOnePercentage * 100).ToString("0.##") + "%";
        }

        private static List<uint> GetBrightnessIncrements(uint min, uint max)
        {
            var result = new List<uint>();
            double delta = ((double)max - min) / (NumberOfIncrements - 1);
            for (int i = 0; i < NumberOfIncrements - 1; i++)
            {
                result.Add(min + (uint)(delta * i));
            }
            result.Add(max);
            return result;
        }

        private static uint GetNextBrightness(uint value, uint min, uint max)
        {
            var increments = GetBrightnessIncrements(min, max);

            for (int i = 0; i < increments.Count - 1; i++)
            {
                if(value >= increments[i])
                {
                    return increments[i + 1];
                }
            }
            return max;
        }

        private static uint GetPrevBrightness(uint value, uint min, uint max)
        {
            var increments = GetBrightnessIncrements(min, max);

            for (int i = increments.Count - 1; i >= 1; i--)
            {
                if (value <= increments[i])
                {
                    return increments[i - 1];
                }
            }
            return min;
        }

        internal class ResultContext
        {
            internal List<Monitors.MonitorBrightnessInfo> BrightnessInfos { get; init; }
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {

            if(!(selectedResult?.ContextData is ResultContext contextData))
            {
                return new();
            }

            return new()
            {
                new ()
                {
                    Title = "Brighter",
                    // TODO: The accelerator key doesn't work
                    AcceleratorKey = Key.Up,
                    AcceleratorModifiers = ModifierKeys.Control,
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE74A",
                    Action = _ => {
                        using(var monitors = Monitors.GetMonitorsAtCursor())
                        {
                            if(monitors == null) return false;
                            monitors.SetMonitorBrightnesses(v =>
                            {
                                // TODO: Fix the brightness increasing/decreasing
                                v.CurrentValue = v.CurrentValue + 10;//GetNextBrightness(v.CurrentValue, v.MinValue, v.MaxValue);
                            });
                            selectedResult.SubTitle = GetSubtitle(monitors.GetMonitorBrightnesses());
                        }
                        return false;
                    },
                },
                new ()
                {
                    Title = "Darker",
                    // TODO: The accelerator key doesn't work
                    AcceleratorKey = Key.Down,
                    AcceleratorModifiers = ModifierKeys.Control,
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE74B",
                    Action = _ => {
                        using(var monitors = Monitors.GetMonitorsAtCursor())
                        {
                            if(monitors == null) return false;
                            monitors.SetMonitorBrightnesses(v =>
                            {
                                // TODO: Fix the brightness increasing/decreasing
                                v.CurrentValue = v.CurrentValue - 5; //GetPrevBrightness(v.CurrentValue, v.MinValue, v.MaxValue);
                            });
                            selectedResult.SubTitle = GetSubtitle(monitors.GetMonitorBrightnesses());
                        }
                        return false;
                    },
                }
            };
        }
    }
}