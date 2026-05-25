namespace MyPause.Models
{
	/// <summary>
	/// Persistent configuration for a single alert rule.
	/// </summary>
	public class AlertConfig
	{
		/// <summary>Unique identifier of the alert.</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString();
		/// <summary>Whether the alert is enabled.</summary>
		public bool IsActive { get; set; } = true;
		/// <summary>Display name of the alert.</summary>
		public string Name { get; set; } = "Nuova Pausa";
		/// <summary>Scheduling mode (fixed time or timer).</summary>
		public AlertType Type { get; set; } = AlertType.FixedTime;
		/// <summary>Fixed trigger hour (0-23).</summary>
		public int FixedTimeHour { get; set; } = 10;
		/// <summary>Fixed trigger minute (0-59).</summary>
		public int FixedTimeMinute { get; set; } = 0;
		/// <summary>Timer interval in seconds.</summary>
		public int TimerSeconds { get; set; } = 3600;
		/// <summary>Whether the alert should reset the timer when another alert triggers.</summary>
		public bool ResetTimerForEveryPause { get; set; } = true;
		/// <summary>Pause duration in seconds.</summary>
		public int PauseDurationSeconds { get; set; } = 300;
		/// <summary>Snooze behavior configuration.</summary>
		public SnoozeConfig SnoozeConfig { get; set; } = new();
		/// <summary>Active days of week as integer values (0=Sunday..6=Saturday).</summary>
		public HashSet<int> ActiveDays { get; set; } = new() { 1, 2, 3, 4, 5 };
		/// <summary>Optional path to custom notification sound.</summary>
		public string? NotificationSoundPath { get; set; }

		/// <summary>
		/// Creates a deep copy of this configuration.
		/// </summary>
		/// <returns>Cloned configuration instance.</returns>
		public AlertConfig Clone()
		{
			return new AlertConfig
			{
				Id = Id,
				IsActive = IsActive,
				Name = Name,
				Type = Type,
				FixedTimeHour = FixedTimeHour,
				FixedTimeMinute = FixedTimeMinute,
				TimerSeconds = TimerSeconds,
				PauseDurationSeconds = PauseDurationSeconds,
				SnoozeConfig = SnoozeConfig.Clone(),
				ResetTimerForEveryPause = ResetTimerForEveryPause,
				ActiveDays = [.. ActiveDays],
				NotificationSoundPath = NotificationSoundPath
			};
		}
	}
}