using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
