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
			var projectGroups = teamCityInfoService.GetFilteredProjectGroup(providerInfo);
			
			var result = new List<ProjectInformationDto>();
			
			foreach (var teamCityProjectGroupDto in projectGroups)
			{
				var buildTypes = teamCityInfoService.GetFilteredBuildTypes(providerInfo, teamCityProjectGroupDto);
				var builds = buildTypes.Select(teamCityBuildTypeDto => teamCityInfoService.GetBuildInfo(providerInfo, teamCityBuildTypeDto)).ToList();

				result.AddRange((from teamCityBuildDto in builds
				          select new ProjectInformationDto
						          {
							          Name = "Yo",
							          Status = teamCityBuildDto.GetBuildStatus()
						          }).ToList());
			}

			return result;
		}
	}

	public class TeamCityInfoService : ITeamCityInfoService
	{
		private const string queryProjectsUrlFormat = "{0}/httpAuth/app/rest/projects";
		private const string queryBuildTypesUrlFormat = "{0}{1}";
		private const string queryBuildInfoUrlFormat = "{0}{1}/builds/running:any";
		
		public IEnumerable<TeamCityProjectGroupDto> GetFilteredProjectGroup(ProviderConfigElement providerInfo)
		{
			string queryUrl = string.Format(queryProjectsUrlFormat, providerInfo.Url);
			var doc = ProcessWebRequest(providerInfo, queryUrl);

			var projectGroups = (from project in doc.Root.Elements("project")
						where ((string)project.Attribute("name")).Contains(providerInfo.GroupMask)
						select new TeamCityProjectGroupDto
						{
							Name = (string)project.Attribute("name"),
							Url = (string)project.Attribute("href")
						});

			return projectGroups;
		}

		public IEnumerable<TeamCityBuildTypeDto> GetFilteredBuildTypes(ProviderConfigElement providerInfo, TeamCityProjectGroupDto projectGroupDto)
		{
			string queryUrl = string.Format(queryBuildTypesUrlFormat, providerInfo.Url, projectGroupDto.Url);
			var doc = ProcessWebRequest(providerInfo, queryUrl);

			IEnumerable<TeamCityBuildTypeDto> buildTypes;
			if (providerInfo.Projects.Count == 0)
			{
				buildTypes = (from buildType in doc.Root.Elements("buildType")
				                select new TeamCityBuildTypeDto
								{
									Name = (string) buildType.Attribute("name"),
									Url = (string) buildType.Attribute("href")
								});
			}
			else
			{
				List<string> projectNames = providerInfo.Projects.Cast<ProjectConfigElement>().Select(x => x.Name).ToList();

				buildTypes = (from buildType in doc.Root.Elements("buildTypes").First().Elements("buildType")
							  where projectNames.Contains((string)buildType.Attribute("name"))
							  select new TeamCityBuildTypeDto
									{
										Name = (string)buildType.Attribute("name"),
										Url = (string)buildType.Attribute("href")
									});
			}

			return buildTypes;
		}

		public TeamCityBuildDto GetBuildInfo(ProviderConfigElement providerInfo, TeamCityBuildTypeDto buildTypeDto)
		{
			string queryUrl = string.Format(queryBuildInfoUrlFormat, providerInfo.Url, buildTypeDto.Url);
			
			var doc = ProcessWebRequest(providerInfo, queryUrl);

			var status = (string)doc.Elements("build").First().Attribute("status");
			var finishedDate = doc.Root.Elements("finishDate").First().Value;

			return new TeamCityBuildDto { FinishDate = finishedDate, Status = status };
		}

		private static XDocument ProcessWebRequest(ProviderConfigElement providerInfo, string queryUrl)
		{
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
			return doc;
		}
	}

	public class TeamCityProjectGroupDto
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}

	public class TeamCityBuildTypeDto
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}

	public class TeamCityBuildDto
	{
		public string FinishDate { get; set; }
		public string Status { get; set; }

		public ProjectStatusEnum GetBuildStatus()
		{
			if (string.IsNullOrEmpty(FinishDate))
				return ProjectStatusEnum.Running;
			
			if (System.String.CompareOrdinal(Status, "SUCCESS") == 0)
				return ProjectStatusEnum.Success;

			return ProjectStatusEnum.Failure;
		}
	}

	//public enum TeamCityBuildStatus
	//{
	//    InProgress,
	//    Success,
	//    Failed
	//}

	public interface ITeamCityInfoService
	{
		IEnumerable<TeamCityProjectGroupDto> GetFilteredProjectGroup(ProviderConfigElement providerInfo);
		IEnumerable<TeamCityBuildTypeDto> GetFilteredBuildTypes(ProviderConfigElement providerInfo, TeamCityProjectGroupDto projectGroupDto);
		TeamCityBuildDto GetBuildInfo(ProviderConfigElement providerInfo, TeamCityBuildTypeDto buildTypeDto);
	}
}