using System;
using System.Configuration;

namespace AmpelLib.ConfigSections
{
	public class ProjectInfoProviderSection : ConfigurationSection
	{
		public static Configuration GetConfiguration()
		{
			var configuration =
				ConfigurationManager
				.GetSection("ProjectInfoProvider")
				as Configuration;

			if (configuration != null)
				return configuration;
			
			return null;
		}

		[ConfigurationProperty("GroupMask", IsRequired = true)]
		public string GroupMask
		{
			get
			{
				return this["GroupMask"] as string;
			}
		}

		[ConfigurationProperty("User", IsRequired = true)]
		public string User
		{
			get
			{
				return this["User"] as string;
			}
		}

		[ConfigurationProperty("Password", IsRequired = true)]
		public string Password
		{
			get
			{
				return this["Password"] as string;
			}
		}

		[ConfigurationProperty("Url", IsRequired = true)]
		public string Url
		{
			get
			{
				return this["Url"] as string;
			}
		}

		[ConfigurationProperty("Type", IsRequired = true)]
		public string Type
		{
			get
			{
				return this["Url"] as string;
			}
		}

		[ConfigurationProperty("", IsDefaultCollection = true)]
		public ProjectCollection Projects
		{
			get
			{
				var hostCollection = (ProjectCollection)base[""];

				return hostCollection;
			}
		}
	}

	public class ProjectCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ProjectConfigElement();
		}

		protected override Object GetElementKey(ConfigurationElement element)
		{
			return ((ProjectConfigElement)element).Name;
		}

		public ProjectConfigElement this[int index]
		{
			get
			{
				return (ProjectConfigElement)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		new public ProjectConfigElement this[string name]
		{
			get
			{
				return (ProjectConfigElement)BaseGet(name);
			}
		}

		protected override string ElementName
		{
			get { return "Project"; }
		}
	}

	public class ProjectConfigElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get { return (string)this["username"]; }
			set { this["username"] = value; }
		}
		

	}

}