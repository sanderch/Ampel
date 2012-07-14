using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FBService;

namespace AmpelLib
{
    public class FBGateway
    {
        private string _authToken = "";
        public List<ProjectInformation> GetAllProjects()
        {
            var finalBuilderClient = new FinalBuilderServer(ConfigurationManager.AppSettings.Get("FBUrl"));
            if (!finalBuilderClient.IsAuthenticated(_authToken))
            {
                _authToken = finalBuilderClient.Authenticate(ConfigurationManager.AppSettings.Get(""),
                                                             ConfigurationManager.AppSettings.Get(""));
            }

            var projects = finalBuilderClient.GetProjects(_authToken);
            return projects.ToList();
        }
    }
}
