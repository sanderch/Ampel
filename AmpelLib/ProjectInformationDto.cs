using FBService;

namespace AmpelLib
{
	public class ProjectInformationDto
	{
		public ProjectStatusEnum Status { get; set; }

		public string Name { get; set; }
	}

	public enum ProjectStatusEnum
	{
		NeverRun,
		Running,
		Failure,
		Success,
		ConfigurationError,
		Stopping,
		Terminating,
		Terminated,
		Suspended,
	}
}