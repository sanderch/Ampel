using System.Diagnostics;
using System.IO;

namespace AmpelLib
{
    public class Ampel : IAmpel
    {
        public void Light(LightColor color)
        {
            StartCommand(color.ToString());
        }

        public void Light(LightColor color1, LightColor color2)
        {
            StartCommand(string.Format("{0} {1}", color1, color2));
        }

        public void Off()
        {
            StartCommand("off");        
        }

        private static void StartCommand(string args)
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
    }
}
