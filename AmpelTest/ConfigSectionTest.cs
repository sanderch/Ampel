﻿using System.Collections.Generic;
using System.Configuration;
using AmpelLib.ConfigSections;
using NUnit.Framework;

namespace AmpelTest
{
	[TestFixture]
	public class ConfigSectionTest
	{
		[Test]
		public void can_read_config_sections()
		{
			var ps = ConfigurationManager.GetSection("ProjectInfoProviders") as List<ProjectInfoProviderSection>;
		}
	}
}