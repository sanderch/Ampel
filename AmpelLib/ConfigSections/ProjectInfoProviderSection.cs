using System.Configuration;

namespace AmpelLib.ConfigSections
{
	public class ProjectInfoProviderSection : ConfigurationSection
	{
		[ConfigurationProperty("Providers")]
		public ProvidersCollection Providers
		{
			get
			{
				return this["Providers"] as ProvidersCollection;
			}
		}
	}

	public class ProvidersCollection : ConfigurationElementCollection
	{
		const string ELEMENT_NAME = "Provider";

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName
		{
			get { return ELEMENT_NAME; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ProviderConfigElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ProviderConfigElement)element).Name;
		}

		public ProviderConfigElement this[int index]
		{
			get { return (ProviderConfigElement)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}
	}

	public class ProviderConfigElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }

		}

		[ConfigurationProperty("groupMask", IsRequired = true)]
		public string GroupMask
		{
			get
			{
				return this["groupMask"] as string;
			}
		}

		[ConfigurationProperty("user", IsRequired = true)]
		public string User
		{
			get
			{
				return this["user"] as string;
			}
		}

		[ConfigurationProperty("password", IsRequired = true)]
		public string Password
		{
			get
			{
				return this["password"] as string;
			}
		}

		[ConfigurationProperty("url", IsRequired = true)]
		public string Url
		{
			get
			{
				return this["url"] as string;
			}
		}

		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get
			{
				return this["type"] as string;
			}
		}

		[ConfigurationProperty("Projects")]
		public ProjectCollection Projects
		{
			get
			{
				return this["Projects"] as ProjectCollection;
			}
		}
	}

	public class ProjectCollection : ConfigurationElementCollection
	{
		const string ELEMENT_NAME = "Project";

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName
		{
			get { return ELEMENT_NAME; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ProjectConfigElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ProjectConfigElement)element).Name;
		}

		public ProjectConfigElement this[int index]
		{
			get { return (ProjectConfigElement)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}
	}

	public class ProjectConfigElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
			
		}
	}

}