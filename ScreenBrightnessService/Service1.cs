using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;

namespace ScreenBrightnessService
{
    public partial class BrightControl : ServiceBase
    {
        public static string LogFileLocation = AppDomain.CurrentDomain.BaseDirectory + "\\BrightService.log";        
        private int DCBrightnessValue = 50, ACBrightnessValue = 100;
        private LogFile LF;

        public BrightControl()
        {
            InitializeComponent();
            CanHandlePowerEvent = true;
        }

        private void OpenConfig()
        {
            try
            {
                StreamReader cf = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\sbcs.cfg");
                string dc = cf.ReadLine();
                DCBrightnessValue = Convert.ToInt32(dc);
                string ac = cf.ReadLine();
                ACBrightnessValue = Convert.ToInt32(ac);
            }
            catch
            {
                LF.WriteLog("Unable to read Configuration file.");
            }
        }

        private bool RunSetBrightnessProc(int b)
        {
            string tm = DateTime.Now.ToString();
            Process p = new Process();
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\SetBrightness.exe"; //Own application. Faster than powershell.
            p.StartInfo.Arguments = Convert.ToString(b);
            //p.StartInfo.FileName = "powershell.exe"; //Slower especially weaker machines.
            //p.StartInfo.Arguments = "(Get-WmiObject -Namespace root/WMI -Class WmiMonitorBrightnessMethods).WmiSetBrightness(1," + b + ")";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            try { p.Start(); }
            catch
            {
                LF.WriteLog($"{tm} Error: SetBrightness.exe could not be started.");
                return false;
            }
            return true;
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            string tm = DateTime.Now.ToString();
            PowerState PS = new PowerState();

            if (PS.GetPowerState().ACLineStatus == ACLineStatus.Online)
            {
                if (RunSetBrightnessProc(ACBrightnessValue)) //Running external program that uses WMI
                    LF.WriteLog($"{tm} Power source changed to AC. Brightness set to {ACBrightnessValue}%");
            }
            if (PS.GetPowerState().ACLineStatus == ACLineStatus.Offline)
            {
                if (RunSetBrightnessProc(DCBrightnessValue))
                    LF.WriteLog($"{tm} Power source changed to DC. Brightness set to {DCBrightnessValue}%");
            }
            return true;
        }

        protected override void OnStart(string[] args)
        {
            string tm = DateTime.Now.ToString();
            LF = new LogFile(LogFileLocation);
            if (!File.Exists(LogFileLocation))
            {
                LF.CreateLog();
                LF.OpenLog();
                LF.WriteLog("Screen Brightness Control Service");
                LF.WriteLog($"Log created at: {tm}");
                LF.WriteLog($"{tm} Session started.");
            }
            else
            {
                LF.OpenLog();
                LF.WriteLog($"{tm} Session has started.");
            }
            OpenConfig();
            OnPowerEvent(PowerBroadcastStatus.PowerStatusChange); // Call PowerEvent on startup.
        }

        protected override void OnStop()
        {
            LF.WriteLog(DateTime.Now.ToString() + " Session has stopped.");
            LF.CloseLog();
        }
    }
}
