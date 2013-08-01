using System.Collections.Generic;

namespace AmpelLib
{
	public interface IProjectInfoProvider
	{
		List<ProjectInformationDto> GetProjectInfos(string groupMask);
	}
}