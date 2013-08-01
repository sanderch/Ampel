using System.Collections.Generic;

namespace AmpelLib
{
	public class TeamCityProjectInfoProvider : IProjectInfoProvider
	{
		public List<ProjectInformationDto> GetProjectInfos(string groupMask)
		{
			return new List<ProjectInformationDto>(){new ProjectInformationDto(){}};
		}
	}
}