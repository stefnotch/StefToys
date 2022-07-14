using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorBrightness
{
    internal class Monitors : IDisposable
    {
        public class MonitorBrightnessInfo
        {
            public uint MinValue { get; init; }
            public uint MaxValue { get; init; }
            public uint CurrentValue { get; set; }
        }

        private NativeMethods.PHYSICAL_MONITOR[]? _physicalMonitors;

        private Monitors(NativeMethods.PHYSICAL_MONITOR[]? physicalMonitors)
        {
            _physicalMonitors = physicalMonitors;
        }

        public class HMonitor
        {
            public IntPtr MonitorPtr { get; init; }
        }

        public static HMonitor? GetHMonitorAtCursor()
        {
            if (!NativeMethods.GetCursorPos(out var cursor))
            {
                return null;
            }
            IntPtr monitorPtr = NativeMethods.MonitorFromPoint(cursor, NativeMethods.MonitorFromPointFlags.MONITOR_DEFAULTTONEAREST);
            if (monitorPtr == IntPtr.Zero)
            {
                return null;
            }
            return new HMonitor()
            {
                MonitorPtr = monitorPtr
            };
        }

        public static Monitors? GetMonitorsFrom(HMonitor hMonitor)
        {
            uint physicalMonitorsCount = 0;
            if (!NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor.MonitorPtr, ref physicalMonitorsCount))
            {
                return null;
            }

            var physicalMonitors = new NativeMethods.PHYSICAL_MONITOR[physicalMonitorsCount];
            if (!NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor.MonitorPtr, physicalMonitorsCount, physicalMonitors))
            {
                return null;
            }

            return new Monitors(physicalMonitors);
        }

        public List<MonitorBrightnessInfo> GetMonitorBrightnesses()
        {
            var result = new List<MonitorBrightnessInfo>();
            if (_physicalMonitors == null)
            {
                return result;
            }

            foreach(var monitor in _physicalMonitors)
            {
                uint minValue = 0, currentValue = 0, maxValue = 0;
                if (!NativeMethods.GetMonitorBrightness(monitor.physicalMonitorPtr, ref minValue, ref currentValue, ref maxValue))
                {
                    continue;
                }

                result.Add(new MonitorBrightnessInfo()
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    CurrentValue = currentValue,
                });
            }
            return result;
        }

        public bool SetMonitorBrightnesses(Action<MonitorBrightnessInfo> callback)
        {
            if (_physicalMonitors == null) return false;

            var failedBrightnessSetting = new List<NativeMethods.PHYSICAL_MONITOR>();

            foreach (var monitor in _physicalMonitors)
            {
                uint minValue = 0, currentValue = 0, maxValue = 0;
                if (!NativeMethods.GetMonitorBrightness(monitor.physicalMonitorPtr, ref minValue, ref currentValue, ref maxValue))
                {
                    continue;
                }

                var monitorBrightness = new MonitorBrightnessInfo()
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    CurrentValue = currentValue,
                };
                callback(monitorBrightness);

                if(monitorBrightness.CurrentValue != currentValue)
                {
                    uint newBrightness = Math.Clamp(monitorBrightness.CurrentValue, minValue, maxValue);

                    if(!NativeMethods.SetMonitorBrightness(monitor.physicalMonitorPtr, newBrightness))
                    {
                        failedBrightnessSetting.Add(monitor);
                    }
                }
            }

            return failedBrightnessSetting.Count == 0;
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                if (_physicalMonitors != null)
                {
                    /*foreach (var monitor in _physicalMonitors)
                    {
                        NativeMethods.DestroyPhysicalMonitor(monitor.physicalMonitorPtr);
                    }*/
                    NativeMethods.DestroyPhysicalMonitors((uint)_physicalMonitors.Length, ref _physicalMonitors);
                }
                _physicalMonitors = null;
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Monitors()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
