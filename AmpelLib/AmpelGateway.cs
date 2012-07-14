using System.Diagnostics;
using System.IO;

namespace AmpelLib
{
    public class AmpelGateway
    {
        public void Light(LightColor color)
        {
            StartAmpelCommand(color.ToString());
        }

        public void Light(LightColor color1, LightColor color2)
        {
            StartAmpelCommand(color1 + "" + color2);
        }

        private static void StartAmpelCommand(string args)
        {
            var process = new Process
                              {
                                  StartInfo =
                                      {
                                          WindowStyle = ProcessWindowStyle.Hidden,
                                          FileName = Directory.GetCurrentDirectory() + @"\Externals\USBswitchCmd.exe",
                                          Arguments = args
                                      }
                              };
            process.Start();
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
