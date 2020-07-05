using System.Management;

namespace SetBrightness
{
    public class ScreenBrightness
    {
        private ManagementObject brightnessInstance;
        private ManagementBaseObject brightnessClass;

        public ScreenBrightness()
        {
            // Querying the Windows service to get the Brightness API.
            var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");
            
            var results = searcher.Get();
            var resultEnum = results.GetEnumerator();
            resultEnum.MoveNext();
            brightnessClass = resultEnum.Current;
            
            // We need to create an instance to use the Set method!
            var instanceName = (string)brightnessClass["InstanceName"];
            brightnessInstance = new ManagementObject("root\\WMI", "WmiMonitorBrightnessMethods.InstanceName='" + instanceName + "'", null);
        }

        public int GetDeviceCurrentBrightness()
        {
            // Getting the current value.
            var value = brightnessClass.GetPropertyValue("CurrentBrightness");
            var valueString = value.ToString();
            return int.Parse(valueString);
        }

        public void SetDeviceBrightness(int newValue)
        {
            if (newValue < 0) { newValue = 0; }
            if (newValue > 100) { newValue = 100; }

            var inParams = brightnessInstance.GetMethodParameters("WmiSetBrightness");
            inParams["Brightness"] = newValue;
            inParams["Timeout"] = 0;
            brightnessInstance.InvokeMethod("WmiSetBrightness", inParams, null);
        }
    }
}
