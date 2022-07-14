using System.Runtime.InteropServices;
using System.Windows.Input;
using Wox.Plugin;


namespace MonitorBrightness
{
    public class Main : IPlugin, IContextMenu
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
            // Powertoys has a bug where it doesn't re-query when you open the window again.
            // So we can't safely cache stuff.
            if(query.ActionKeyword != Context!.CurrentPluginMetadata.ActionKeyword)
            {
                return new List<Result>();
            }

            var monitor = Monitors.GetHMonitorAtCursor();
            if (monitor == null)
            {
                return new List<Result>() {
                    new Result()
                    {
                        Title = "Monitor fetching error",
                        SubTitle = Marshal.GetLastPInvokeError() + ""
                    }
                };
            }

            if (query.Search.Length < 2)
            {
                Context?.API?.ChangeQuery(query.RawQuery + "..", false);
                return new List<Result>();
            }

            var brightnesses = GetBrightnesses(monitor);
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
                        HMonitor = monitor,
                        BrightnessInfos = brightnesses,
                        Query = query.RawQuery
                    },
                },
            };
        }

        private static string GetSubtitle(IEnumerable<Monitors.MonitorBrightnessInfo> brightnesses)
        {
            string percentages = string.Join(
                                    ", ",
                                    brightnesses.Select(v => ToPercent(v.CurrentValue, v.MinValue, v.MaxValue))
                                    );
            return "Brightness: " + (percentages.Length > 0 ? percentages : "-");
        }

        private static List<Monitors.MonitorBrightnessInfo> GetBrightnesses(Monitors.HMonitor hMonitor)
        {
            using var monitor = Monitors.GetMonitorsFrom(hMonitor);
            if (monitor == null)
            {
                return new();
            }

            return monitor.GetMonitorBrightnesses();
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
                if(value <= increments[i])
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
                if (value >= increments[i])
                {
                    return increments[i - 1];
                }
            }
            return min;
        }

        internal class ResultContext
        {
            internal Monitors.HMonitor HMonitor { get; init; }
            internal List<Monitors.MonitorBrightnessInfo> BrightnessInfos { get; init; }
            internal string Query { get; init; }
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
                    // Figure out a nice accelerator key
                    //AcceleratorKey = Key.D,
                    //AcceleratorModifiers = ModifierKeys.None,
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE74A",
                    Action = _ => {
                        using(var monitors = Monitors.GetMonitorsFrom(contextData.HMonitor))
                        {
                            monitors?.SetMonitorBrightnesses(v =>
                            {
                                v.CurrentValue = GetNextBrightness(v.CurrentValue, v.MinValue, v.MaxValue);
                            });
                        }
                        Task.Run(() =>
                        {
                            //Context?.API?.ChangeQuery(contextData.Query, true);
                        });
                        return false;
                    },
                },
                new ()
                {
                    Title = "Darker",
                    // Figure out a nice accelerator key
                    //AcceleratorKey = Key.D,
                    //AcceleratorModifiers = ModifierKeys.None,
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE74B",
                    Action = _ => {
                        using(var monitors = Monitors.GetMonitorsFrom(contextData.HMonitor))
                        {
                            monitors?.SetMonitorBrightnesses(v =>
                            {
                                v.CurrentValue = GetPrevBrightness(v.CurrentValue, v.MinValue, v.MaxValue);
                            });
                        }
                        Task.Run(() =>
                        {
                            //Context?.API?.ChangeQuery(contextData.Query, true);
                        });
                        return false;
                    },
                }
            };
        }
    }
}