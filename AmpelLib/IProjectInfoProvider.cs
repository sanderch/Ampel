using System.Collections.Generic;
using AmpelLib.ConfigSections;

namespace AmpelLib
{
	public interface IProjectInfoProvider
	{
		List<ProjectInformationDto> GetProjectInfos(ProviderConfigElement providerInfo);
	}
}