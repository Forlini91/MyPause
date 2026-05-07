using MyPause.Models;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace MyPause.Services
{
	/// <summary>
	/// Handles persistence of app configuration and runtime state to JSON files.
	/// </summary>
	public class StorageManager
	{
		private readonly string _configurationPath;
		private readonly string _runtimeStatePath;

		/// <summary>
		/// Initializes storage paths under AppData\MyPause.
		/// </summary>
		public StorageManager()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var appDir = Path.Combine(appData, "MyPause");
			Directory.CreateDirectory(appDir);
			_configurationPath = Path.Combine(appDir, "config.json");
			_runtimeStatePath = Path.Combine(appDir, "runtime-state.json");
		}



		#region Configuration

		/// <summary>
		/// Saves the full app configuration to disk.
		/// </summary>
		/// <param name="configuration">Configuration to persist.</param>
		public void SaveConfiguration(Configuration configuration)
		{
			try
			{
				var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
				File.WriteAllText(_configurationPath, json);
				Debug.WriteLine($"Configuration saved to {_configurationPath}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error saving configuration: {ex.Message}");
			}
		}

		/// <summary>
		/// Loads app configuration from disk.
		/// </summary>
		/// <returns>Loaded configuration, or null if not available.</returns>
		public Configuration? LoadConfiguration()
		{
			if (!File.Exists(_configurationPath))
			{
				Debug.WriteLine("Configuration file not found, loading defaults");
				return null;
			}

			try
			{
				var json = File.ReadAllText(_configurationPath);
				var configuration = JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
				Debug.WriteLine($"Configuration loaded from {_configurationPath}. Alerts count: {configuration.Alerts.Count}");
				return configuration;

			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error loading configuration: {ex.Message}");
				return null;
			}
		}

		#endregion



		#region RuntimeState

		/// <summary>
		/// Saves runtime state (currently running alert IDs) to disk.
		/// </summary>
		/// <param name="runtimeState">Runtime state to persist.</param>
		public void SaveRuntimeState(RuntimeState runtimeState)
		{
			try
			{
				var json = JsonConvert.SerializeObject(runtimeState, Formatting.Indented);
				File.WriteAllText(_runtimeStatePath, json);
				Debug.WriteLine($"Runtime state saved to {_runtimeStatePath}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error saving runtime state: {ex.Message}");
			}
		}

		/// <summary>
		/// Loads runtime state from disk.
		/// </summary>
		/// <returns>Loaded runtime state, or a new default instance on failure.</returns>
		public RuntimeState LoadRuntimeState()
		{
			try
			{
				if (!File.Exists(_runtimeStatePath))
				{
					Debug.WriteLine("Runtime state file not found, loading defaults");
					return new RuntimeState();
				}

				var json = File.ReadAllText(_runtimeStatePath);
				var state = JsonConvert.DeserializeObject<RuntimeState>(json) ?? new RuntimeState();
				Debug.WriteLine($"Runtime state loaded from {_runtimeStatePath}");
				return state;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error loading runtime state: {ex.Message}");
				return new RuntimeState();
			}
		}

		#endregion
	}

	/// <summary>
	/// Serializable application configuration container.
	/// </summary>
	public sealed class Configuration
	{
		/// <summary>Work schedule settings.</summary>
		public WorkScheduleConfiguration WorkSchedule { get; set; } = new();
		/// <summary>Configured alerts list.</summary>
		public List<AlertConfig> Alerts { get; set; } = new();
		/// <summary>Whether close action minimizes the app to tray.</summary>
		public bool MinimizeToTrayOnClose { get; set; } = true;
	}

	/// <summary>
	/// Serializable work schedule settings.
	/// </summary>
	public sealed class WorkScheduleConfiguration
	{
		/// <summary>Workday start hour.</summary>
		public int WorkScheduleStartHour { get; set; } = 8;
		/// <summary>Workday start minute.</summary>
		public int WorkScheduleStartMinute { get; set; } = 0;
		/// <summary>Workday end hour.</summary>
		public int WorkScheduleEndHour { get; set; } = 17;
		/// <summary>Workday end minute.</summary>
		public int WorkScheduleEndMinute { get; set; } = 0;
		/// <summary>Minutes to block new pauses after one is triggered (0 = disabled).</summary>
		public int PauseCooldownSeconds { get; set; } = 0;
	}

	/// <summary>
	/// Serializable runtime state persisted between sessions.
	/// </summary>
	public sealed class RuntimeState
	{
		/// <summary>IDs of alerts that should resume active state at startup.</summary>
		public HashSet<string> RunningAlertIds { get; set; } = new();
	}
}
