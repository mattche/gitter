﻿namespace gitter.Git
{
	using System;
	using System.Collections.Generic;

	using gitter.Framework;
	using gitter.Git.AccessLayer;

	/// <summary>Represents a git configuration ini-style file.</summary>
	public class ConfigurationFile : IEnumerable<ConfigParameter>
	{
		#region Static

		/// <summary>Open config file with system-wide settings.</summary>
		/// <param name="configAccessor">Configuration file accessor.</param>
		public static ConfigurationFile OpenSystemFile(IConfigAccessor configAccessor)
		{
			return new ConfigurationFile(configAccessor, ConfigFile.System, true);
		}

		/// <summary>Open config file with user-specific settings.</summary>
		/// <param name="configAccessor">Configuration file accessor.</param>
		public static ConfigurationFile OpenCurrentUserFile(IConfigAccessor configAccessor)
		{
			return new ConfigurationFile(configAccessor, ConfigFile.User, true);
		}

		#endregion

		#region Events

		/// <summary>New parameter created/detected.</summary>
		public event EventHandler<ConfigParameterEventArgs> ParameterCreated;

		/// <summary>Parameter removed/lost.</summary>
		public event EventHandler<ConfigParameterEventArgs> ParameterDeleted;

		/// <summary>Invokes <see cref="ParameterCreated"/> event.</summary>
		/// <param name="parameter">New branch.</param>
		private void InvokeCreated(ConfigParameter parameter)
		{
			if(parameter == null) throw new ArgumentNullException("parameter");
			var handler = ParameterCreated;
			if(handler != null) handler(this, new ConfigParameterEventArgs(parameter));
		}

		/// <summary>Invokes <see cref="Deleted"/> & other related events.</summary>
		/// <param name="branch">Deleted branch.</param>
		private void InvokeDeleted(ConfigParameter parameter)
		{
			if(parameter == null) throw new ArgumentNullException("parameter");
			parameter.MarkAsDeleted();
			var handler = ParameterDeleted;
			if(handler != null) handler(this, new ConfigParameterEventArgs(parameter));
		}

		#endregion

		#region Data

		private readonly IConfigAccessor _configAccessor;
		private readonly Repository _repository;
		private readonly string _fileName;
		private readonly ConfigFile _configFile;
		private readonly Dictionary<string, ConfigParameter> _parameters;

		#endregion

		#region .ctor

		private ConfigurationFile(Repository repository, bool load)
		{
			_configAccessor = repository.Accessor;
			_parameters = new Dictionary<string, ConfigParameter>();
			_repository = repository;
			_configFile = ConfigFile.Repository;
			if(load) Refresh();
		}

		public ConfigurationFile(Repository repository, string fileName, bool load)
		{
			if(repository == null) throw new ArgumentNullException("repository");

			_configAccessor = repository.Accessor;
			_parameters = new Dictionary<string, ConfigParameter>();
			_repository = repository;
			_configFile = ConfigFile.Other;
			_fileName = fileName;
			if(load) Refresh();
		}

		private ConfigurationFile(IConfigAccessor configAccessor, ConfigFile configFile, bool load)
		{
			if(configAccessor == null) throw new ArgumentNullException("configAccessor");

			_configAccessor = configAccessor;
			_parameters = new Dictionary<string, ConfigParameter>();
			_configFile = configFile;
			if(load) Refresh();
		}

		/// <summary>Create <see cref="ConfigurationFile"/>.</summary>
		/// <param name="fileName">Name of config file.</param>
		public ConfigurationFile(IConfigAccessor configAccessor, string fileName)
			: this(configAccessor, fileName, true)
		{
		}

		/// <summary>Create <see cref="ConfigurationFile"/>.</summary>
		/// <param name="fileName">Name of config file.</param>
		/// <param name="load">Immediately load file contents.</param>
		public ConfigurationFile(IConfigAccessor configAccessor, string fileName, bool load)
		{
			if(configAccessor == null) throw new ArgumentNullException("configAccessor");

			_configAccessor = configAccessor;
			_fileName = fileName;
			_parameters = new Dictionary<string, ConfigParameter>();
			_configFile = ConfigFile.Other;
			if(load) Refresh();
		}

		#endregion

		#region Properties

		public IEnumerable<string> Names
		{
			get { return _parameters.Keys; }
		}

		public ConfigParameter this[string name]
		{
			get
			{
				ConfigParameter res;
				lock(_parameters)
				{
					if(!_parameters.TryGetValue(name, out res))
						throw new ArgumentException("Parameter not found.", "name");
				}
				return res;
			}
		}

		public bool Exists(string name)
		{
			lock(_parameters)
			{
				return _parameters.ContainsKey(name);
			}
		}

		public int Count
		{
			get
			{
				lock(_parameters)
				{
					return _parameters.Count;
				}
			}
		}

		/// <summary>Object used for cross-thread synchronization.</summary>
		public object SyncRoot
		{
			get { return _parameters; }
		}

		#endregion

		public ConfigParameter CreateParameter(string name, string value)
		{
			if(name == null) throw new ArgumentNullException("name");
			if(value == null) throw new ArgumentNullException("value");

			ConfigParameter configParameter;
			lock(_parameters)
			{
				if(!_parameters.ContainsKey(name))
				{
					switch(_configFile)
					{
						case ConfigFile.Other:
							_configAccessor.SetConfigValue(new SetConfigValueParameters(name, value)
							{
								ConfigFile = _configFile,
								FileName = _fileName,
							});
							break;
						case ConfigFile.System:
						case ConfigFile.User:
							_configAccessor.SetConfigValue(new SetConfigValueParameters(name, value)
							{
								ConfigFile = _configFile,
							});
							break;
						case ConfigFile.Repository:
							_configAccessor.SetConfigValue(new SetConfigValueParameters(name, value));
							break;
					}
					configParameter = _repository != null ?
						new ConfigParameter(_repository, _configFile, name, value) :
						new ConfigParameter(_configAccessor, _fileName, name, value);
					_parameters.Add(name, configParameter);
					InvokeCreated(configParameter);
				}
				else
				{
					throw new ArgumentException("Parameter already exists.", "name");
				}
			}
			return configParameter;
		}

		internal void Unset(ConfigParameter parameter)
		{
			if(parameter == null) throw new ArgumentNullException("parameter");

			switch(_configFile)
			{
				case ConfigFile.Other:
					_configAccessor.UnsetConfigValue(
						new UnsetConfigValueParameters(parameter.Name)
						{
							FileName = _fileName,
							ConfigFile = ConfigFile.Other,
						});
					break;
				case ConfigFile.System:
				case ConfigFile.User:
					_configAccessor.UnsetConfigValue(
						new UnsetConfigValueParameters(parameter.Name)
						{
							ConfigFile = _configFile,
						});
					break;
				case ConfigFile.Repository:
					_configAccessor.UnsetConfigValue(
						new UnsetConfigValueParameters(parameter.Name));
					break;
			}
			lock(_parameters)
			{
				if(_parameters.Remove(parameter.Name))
				{
					InvokeDeleted(parameter);
				}
			}
		}

		public bool TryGetParameter(string name, out ConfigParameter parameter)
		{
			lock(_parameters)
			{
				return _parameters.TryGetValue(name, out parameter);
			}
		}

		public ConfigParameter TryGetParameter(string name)
		{
			ConfigParameter parameter;
			lock(_parameters)
			{
				if(_parameters.TryGetValue(name, out parameter))
				{
					return parameter;
				}
			}
			return null;
		}

		public ConfigParameter SetValue(string name, string value)
		{
			ConfigParameter p;
			lock(_parameters)
			{
				if(_parameters.TryGetValue(name, out p))
				{
					p.Value = value;
				}
				else
				{
					p = CreateParameter(name, value);
				}
			}
			return p;
		}

		#region Refresh()

		/// <summary>Synchronize cached information with actual data.</summary>
		public void Refresh()
		{
			IList<ConfigParameterData> config;
			switch(_configFile)
			{
				case ConfigFile.Other:
					config = _configAccessor.QueryConfig(
						new QueryConfigParameters(_fileName));
					break;
				case ConfigFile.Repository:
					config = _configAccessor.QueryConfig(
						new QueryConfigParameters());
					break;
				case ConfigFile.System:
				case ConfigFile.User:
					config = _configAccessor.QueryConfig(
						new QueryConfigParameters(_configFile));
					break;
				default:
					throw new InvalidOperationException();
			}

			lock(_parameters)
			{
				if(_repository != null)
				{
					CacheUpdater.UpdateObjectDictionary<ConfigParameter, ConfigParameterData>(
						_parameters,
						null,
						null,
						config,
						configParameterData => ObjectFactories.CreateConfigParameter(_repository, configParameterData),
						ObjectFactories.UpdateConfigParameter,
						InvokeCreated,
						InvokeDeleted,
						true);
				}
				else
				{
					CacheUpdater.UpdateObjectDictionary<ConfigParameter, ConfigParameterData>(
						_parameters,
						null,
						null,
						config,
						configParameterData => ObjectFactories.CreateConfigParameter(_configAccessor, configParameterData),
						ObjectFactories.UpdateConfigParameter,
						InvokeCreated,
						InvokeDeleted,
						true);
				}
			}
		}

		#endregion

		#region IEnumerable<ConfigParameter>

		public IEnumerator<ConfigParameter> GetEnumerator()
		{
			return _parameters.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _parameters.Values.GetEnumerator();
		}

		#endregion
	}
}