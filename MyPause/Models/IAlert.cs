namespace MyPause.Models
{
	/// <summary>
	/// Common interface for alert state, used by Alert and AlertRuntimeSnapshot for UI binding.
	/// </summary>
	public interface IAlert
	{
		/// <summary>Current state of the alert.</summary>
		AlertState State { get; }
		/// <summary>True if the alert is in an error state.</summary>
		bool HasError { get; }
		/// <summary>True if the alert is frozen (temporarily disabled for editing).</summary>
		bool IsFrozen { get; }

		/// <summary>Unique alert identifier.</summary>
		string Id { get; }
		/// <summary>User-facing alert name.</summary>
		string Name { get; }
		/// <summary>Scheduling type for this alert.</summary>
		AlertType Type { get; }
		/// <summary>Fixed trigger hour.</summary>
		int FixedTimeHour { get; }
		/// <summary>Fixed trigger minute.</summary>
		int FixedTimeMinute { get; }
		/// <summary>Timer interval in seconds.</summary>
		int TimerSeconds { get; }
		/// <summary>Pause duration in seconds.</summary>
		int PauseDurationSeconds { get; }
		/// <summary>Snooze configuration.</summary>
		SnoozeConfig SnoozeConfig { get; }
		/// <summary>Active days set (0=Sunday..6=Saturday).</summary>
		HashSet<int> ActiveDays { get; }
		/// <summary>Optional path to custom notification sound.</summary>
		string? NotificationSoundPath { get; }

		bool IsRunning { get; }
	}
}
