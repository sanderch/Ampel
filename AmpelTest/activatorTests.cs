using System;

namespace AmpelTest
{
	public class ActivatorTests
	{
		public void can_create_provider_instance_by_name()
		{
			var provider = Activator.CreateInstance(Type.GetType(""));
		}
	}
}