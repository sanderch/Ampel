using System.Collections.Generic;
using AmpelLib.ConfigSections;

namespace AmpelLib
{
	public class TeamCityProjectInfoProvider : IProjectInfoProvider
	{
		public List<ProjectInformationDto> GetProjectInfos(ProviderConfigElement providerInfo)
		{
			return new List<ProjectInformationDto>(){new ProjectInformationDto(){}};
		}
	}
}