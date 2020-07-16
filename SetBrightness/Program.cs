using System;

namespace SetBrightness
{
    class Program
    {
        static void Main(string[] args)
        {
            ScreenBrightness SB = new ScreenBrightness();
            SB.SetDeviceBrightness(Convert.ToInt32(args[0]));
        }
    }
}
