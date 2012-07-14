using System.Collections.Generic;
using System.Linq;
using FBService;

namespace AmpelLib
{
    public class FBGateway
    {
        private string _authToken = "";
        public List<ProjectInformation> GetAllProjects()
        {
            var finalBuilderClient = new FinalBuilderServer();
            if (!finalBuilderClient.IsAuthenticated(_authToken))
            {
                _authToken = finalBuilderClient.Authenticate("admin", "adin1");
            }

            var projects = finalBuilderClient.GetProjects(_authToken);
            return projects.ToList();
        }
    }
}
