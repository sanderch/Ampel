using System;
using System.Collections.Generic;
using System.Linq;
using AmpelLib.ConfigSections;
using FBService;

namespace AmpelLib
{
	public class FinalBuilderProjectInfoProvider : IProjectInfoProvider
	{
		private readonly IBuildServer _buildServer;

		public FinalBuilderProjectInfoProvider()
		{
			_buildServer = new FBGateway();
		}

		public List<ProjectInformationDto> GetProjectInfos(ProviderConfigElement providerInfo)
		{
			return _buildServer.GetAllProjects().Select(x => new ProjectInformationDto
				{
					Name = x.Name,
					Status = ParseFBStatus(x.Status)
				}).ToList();
		}

		private static ProjectStatusEnum ParseFBStatus(ProjectStatus status)
		{
			ProjectStatusEnum enumRes;
			return Enum.TryParse(status.ToString(), out enumRes) ? enumRes : ProjectStatusEnum.Failure;
		}
	}
}