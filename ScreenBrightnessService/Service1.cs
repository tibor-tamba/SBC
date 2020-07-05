using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Management;

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
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\SetBrightness.exe";                                              //Own application. Faster than powershell.
            p.StartInfo.Arguments = Convert.ToString(b);
            //p.StartInfo.FileName = "powershell.exe";
            //p.StartInfo.Arguments = "(Get-WmiObject -Namespace root/WMI -Class WmiMonitorBrightnessMethods).WmiSetBrightness(1," + b + ")";  //Slower especially weaker machines.
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
            //ScreenBrightness SB = new ScreenBrightness();                                                     //Using class. WMI method
            //IntPtr m = DisplayConfiguration.GetCurrentMonitor();                                              //Using windows dlls-s
            //DisplayConfiguration.PHYSICAL_MONITOR[] Monitors = DisplayConfiguration.GetPhysicalMonitors2();   //Using windows dlls-s

            if (PS.GetPowerState().ACLineStatus == ACLineStatus.Online)
            {
                //SB.SetDeviceBrightness(ACBrightnessValue);                                                    //Using class. WMI method
                //SetBR(ACBrightnessValue);                                                                     //Using WMI method without class.
                //DisplayConfiguration.SetMonitorBrightness(Monitors[0], ACBrightnessValue);                    //Using windows dlls-s
                if (RunSetBrightnessProc(ACBrightnessValue))                                                        //Run external program that uses WMI
                    LF.WriteLog($"{tm} Power source changed to AC. Brightness set to {ACBrightnessValue}%");// {SB.GetDeviceCurrentBrightness()}%");
            }
            if (PS.GetPowerState().ACLineStatus == ACLineStatus.Offline)
            {
                //SB.SetDeviceBrightness(DCBrightnessValue);                                                    //Using class. WMI method
                //SetBR(DCBrightnessValue);                                                                     //Using WMI method without class.
                //DisplayConfiguration.SetMonitorBrightness(Monitors[0], DCBrightnessValue);                    //Using windows dlls-s
                if (RunSetBrightnessProc(DCBrightnessValue))                                                        //Run external program that uses WMI
                    LF.WriteLog($"{tm} Power source changed to DC. Brightness set to {DCBrightnessValue}%");//{SB.GetDeviceCurrentBrightness()}%");
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

        // Built in solution without classes. Using WMI.
        //private void SetBR(int v)
        //{
        //    // Get the class by executing the query
        //    var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");
        //    var results = searcher.Get();
        //    var resultEnum = results.GetEnumerator();
        //    resultEnum.MoveNext();
        //    var result = resultEnum.Current;
        //    var instanceName = (string)result["InstanceName"];

        //    // Create the instance
        //    var classInstance = new ManagementObject("root\\WMI", "WmiMonitorBrightnessMethods.InstanceName='" + instanceName + "'", null);

        //    // Get the parameters for the method
        //    var inParams = classInstance.GetMethodParameters("WmiSetBrightness");
        //    inParams["Brightness"] = v;
        //    inParams["Timeout"] = 0;

        //    classInstance.InvokeMethod("WmiSetBrightness", inParams, null);
        //}
    }
}
