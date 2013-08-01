using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AmpelLib.ConfigSections;

namespace AmpelLib
{
	public class TeamCityProjectInfoProvider : IProjectInfoProvider
	{
		public List<ProjectInformationDto> GetProjectInfos(ProviderConfigElement providerInfo)
		{
			ITeamCityInfoService teamCityInfoService = new TeamCityInfoService();
			var projectGroupDoc = teamCityInfoService.GetFilteredProjectGroup(providerInfo);

			return new List<ProjectInformationDto>(){new ProjectInformationDto(){}};
		}
	}

	public class TeamCityInfoService : ITeamCityInfoService
	{
		private const string queryUrlFormat = "{0}httpAuth/app/rest/projects";
		
		public IEnumerable<TeamCityProjectGroupDto> GetFilteredProjectGroup(ProviderConfigElement providerInfo)
		{
			string queryUrl = string.Format(queryUrlFormat, providerInfo.Url);
			var queryUri = new Uri(queryUrl);
			var webRequest = (HttpWebRequest)WebRequest.Create(queryUrl);

			var cred = new NetworkCredential(providerInfo.User, providerInfo.Password);

			var cache = new CredentialCache { { queryUri, "Basic", cred } };
			webRequest.Credentials = cache;

			var webResponse = (HttpWebResponse)webRequest.GetResponse();

			var stream = webResponse.GetResponseStream();
			var xmlDoc = new XmlDocument();
			if (stream != null)
			{
				xmlDoc.Load(stream);
			}

			var doc = XDocument.Parse(xmlDoc.OuterXml);
			
			var projectGroups = (from project in doc.Root.Elements("project")
						where ((string)project.Attribute("name")).Contains(providerInfo.GroupMask)
						select new TeamCityProjectGroupDto
						{
							Name = (string)project.Attribute("name"),
							Url = (string)project.Attribute("href")
						});

			return projectGroups;
		}
	}

	public class TeamCityProjectGroupDto
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}

	public interface ITeamCityInfoService
	{
		IEnumerable<TeamCityProjectGroupDto> GetFilteredProjectGroup(ProviderConfigElement providerInfo);
	}
}