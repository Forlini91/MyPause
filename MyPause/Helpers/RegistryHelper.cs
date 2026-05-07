using Microsoft.Win32;

namespace MyPause.Helpers
{
	/// <summary>
	/// Handles registration of the app in Windows Run key for auto startup.
	/// </summary>
	public sealed class RegistryHelper
	{
		private const string RunRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

		/// <summary>
		/// Returns whether auto startup is enabled for the given app name.
		/// </summary>
		/// <param name="appName">Application name used as registry value key.</param>
		/// <returns>True if startup entry exists and has a value.</returns>
		public static bool AutoStartup
		{
			get
			{
				using var key = GetRegistryKey(false);
				if (key is null)
					return false;

				var keyValue = key.GetValue(AppData.ApplicationName);
				if (keyValue is not string strValue)
					return false;

				var processPath = Environment.ProcessPath;
				if (processPath is null)
					return false;

				return strValue == StartupPath(processPath);
			}
			set
			{
				using var key = GetRegistryKey(true);
				if (value)
				{
					if (key is null)
						throw new InvalidOperationException("Unable to access Windows Run registry key.");
					var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Unable to determine executable path.");
					key.SetValue(AppData.ApplicationName, StartupPath(processPath));
					return;
				}
				else if (key is not null && key.GetValue(AppData.ApplicationName) is not null)
					key.DeleteValue(AppData.ApplicationName, throwOnMissingValue: false);
			}
		}

		private static RegistryKey? GetRegistryKey(bool writable) => Registry.CurrentUser.OpenSubKey(RunRegistryPath, writable);

		private static string StartupPath(string path) => $"\"{path}\"";
	}
}
