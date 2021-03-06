﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FBService;

namespace AmpelLib
{
    public interface IBuildServer
    {
        List<ProjectInformation> GetAllProjects();
    }

    public class FBGateway : IBuildServer
    {
        private string _authToken = "";
        public List<ProjectInformation> GetAllProjects()
        {
            var finalBuilderClient = new FinalBuilderServer(ConfigurationManager.AppSettings.Get("FBUrl"));
            if (!finalBuilderClient.IsAuthenticated(_authToken))
            {
                _authToken = finalBuilderClient.Authenticate(ConfigurationManager.AppSettings.Get("FBUser"),
                                                             ConfigurationManager.AppSettings.Get("FBPassword"));
            }

            var projects = finalBuilderClient.GetProjects(_authToken);
            return projects.ToList();
        }
    }
}
