using System.Diagnostics;
using System.IO;

namespace AmpelLib
{
    public class AmpelGateway
    {
        public void Light(LightColor color)
        {
            var process = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Directory.GetCurrentDirectory() + @"\Externals\USBswitchCmd.exe",
                    Arguments = color.ToString()
                }
            };
            process.Start();
        }

        public void Light(LightColor color1, LightColor color2)
        {
            var p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Directory.GetCurrentDirectory() + @"\Externals\USBswitchCmd.exe",
                    Arguments = color1 + " " + color2
                }
            };
            p.Start();
        }

        public void Off()
        {
            var p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Directory.GetCurrentDirectory() + @"\Externals\USBswitchCmd.exe",
                    Arguments = "off"
                }
            };
            p.Start();
        }
    }

    public enum LightColor
    {
        Red,
        Yellow,
        Green,
        None
    }
}
