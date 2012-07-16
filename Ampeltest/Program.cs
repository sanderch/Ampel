using System;
using System.Threading;
using AmpelLib;
using FBService;

namespace Ampeltest
{
    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomainProcessExit);

            var ag = new Ampel();
            var fb = new FBGateway();
            var lastError = false;

            while (true)
            {
                try
                {
                    var projects = fb.GetAllProjects().FindAll(x => x.Group.Contains("ECA"));
                    var isYellow = projects.FindAll(x => x.Status == ProjectStatus.Running).Count > 0;
                    var isRed = projects.FindAll(x => x.Status == ProjectStatus.Failure || x.Status == ProjectStatus.ConfigurationError).Count > 0;

                    if (!isYellow)
                    {
                        lastError = isRed;
                    }

                    var color1 = isRed || lastError ? LightColor.Red : LightColor.Green;
                    var color2 = isYellow ? LightColor.Yellow : LightColor.None;

                    if (color2 != LightColor.None)
                    {
                        ag.Light(color1, color2);
                    }
                    else
                    {
                        lastError = isRed;
                        ag.Light(color1);
                    }

                }
                catch
                {
                    Console.WriteLine("FB not accessible");
                    ag.Off();
                }
                
                Thread.Sleep(5000);
                GC.Collect();
            }

        }

        static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            var ag = new Ampel();
            ag.Off();
        }
    }
}
