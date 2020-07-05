using System;
using System.Runtime.InteropServices;

namespace ScreenBrightnessService
{
    [StructLayout(LayoutKind.Sequential)]
    public class PowerState
    {
        public ACLineStatus ACLineStatus;
        public BatteryFlag BatteryFlag;
        public byte BatteryLifePercent;
        public byte Reserved1;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;

        public PowerState() { }

        public PowerState GetPowerState()
        {
            PowerState state = new PowerState();
            if (GetSystemPowerStatusRef(state))
                return state;
            LogFile l = new LogFile(BrightControl.LogFileLocation);
            l.WriteLog("Unable to get power state");
            l.CloseLog();
            throw new ApplicationException("Unable to get power state");
        }

        [DllImport("Kernel32", EntryPoint = "GetSystemPowerStatus")]
        private static extern bool GetSystemPowerStatusRef(PowerState sps);
    }

    public enum ACLineStatus : byte
    {
        Offline = 0, Online = 1, Unknown = 255
    }

    public enum BatteryFlag : byte
    {
        High = 1, Low = 2, Critical = 4, Charging = 8,
        NoSystemBattery = 128, Unknown = 255
    }
}
