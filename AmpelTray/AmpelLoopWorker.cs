using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using AmpelLib;
using AmpelLib.ConfigSections;
using AmpelTray.Properties;

namespace AmpelTray
{
    public class AmpelLoopWorker
    {
        private readonly ToggleIconDelegate _toggleIconDelegate;
        
        public AmpelLoopWorker(ToggleIconDelegate toggleIconDelegate)
        {
            _toggleIconDelegate = toggleIconDelegate;
        }
        
        public void DoAmpelLoop()
        {
            var ag = new Ampel();
            //var fb = new FBGateway();
            var ampelService = new AmpelService();

            while (!_shouldStop)
            {
                try
                {
	                var providerSection = ConfigurationManager.GetSection("ProjectInfoProvider") as ProjectInfoProviderSection;

					var pid = new List<ProjectInformationDto>();
					foreach (ProviderConfigElement providerInfo in providerSection.Providers)
					{
						var provider = (IProjectInfoProvider)Activator.CreateInstance(Type.GetType(providerInfo.Type));
						pid.AddRange(provider.GetProjectInfos(providerInfo));
					}

	                ampelService.ToggleAmpel(ag, pid);
                    _toggleIconDelegate(IconStatus.Enabled);
                }
                catch
                {
                    _toggleIconDelegate(IconStatus.Disabled);
                    ag.Off();
                }

                Thread.Sleep(5000);
                GC.Collect();
            }
            ag.Off();
            Console.WriteLine(Resources.Worker_DoWork_worker_thread__terminating_gracefully_);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;
    }

	
}