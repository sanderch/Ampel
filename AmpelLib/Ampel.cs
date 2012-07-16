using System.Diagnostics;
using System.IO;

namespace AmpelLib
{
    public class Ampel : IAmpel
    {
        public void Light(LightColor color)
        {
            StartExeCommand(color.ToString());
        }

        public void Light(LightColor color1, LightColor color2)
        {
            StartExeCommand(string.Format("{0} {1}", color1, color2));
        }

        public void Off()
        {
            StartExeCommand("off");        
        }

        private static void StartExeCommand(string args)
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
