namespace MyPause.Models
{
	/// <summary>
	/// Immutable runtime snapshot used by UI to render alert state and progress.
	/// </summary>
	/// <param name="config">Alert configuration captured in the snapshot.</param>
	/// <param name="state">Current alert state.</param>
	/// <param name="stateChanged">Whether state changed in this update.</param>
	/// <param name="hasError">Whether the alert is in error state.</param>
	/// <param name="isFrozen">Whether updates are frozen.</param>
	/// <param name="timerElapsedMs">Elapsed timer milliseconds.</param>
	/// <param name="timerMaxMs">Total timer milliseconds.</param>
	/// <param name="pauseElapsedMs">Elapsed pause milliseconds.</param>
	/// <param name="pauseMaxMs">Total pause milliseconds.</param>
	/// <param name="snoozeElapsedMs">Elapsed snooze milliseconds.</param>
	/// <param name="snoozeMaxMs">Total snooze milliseconds.</param>
	/// <param name="timeUntilNextTrigger">Time remaining until next trigger, if applicable.</param>
	public sealed class AlertSnapshot : IAlert
	{
		private readonly AlertConfig _config;
		private readonly AlertState _state;

		public AlertSnapshot(
			AlertConfig config,
			AlertState state,
			bool stateChanged,
			bool hasError,
			bool isFrozen,
			long timerElapsedMs,
			long timerMaxMs,
			long pauseElapsedMs,
			long pauseMaxMs,
			long snoozeElapsedMs,
			long snoozeMaxMs,
			TimeSpan? timeUntilNextTrigger
		)
		{
			_config = config;
			_state = state;
			StateChanged = stateChanged;
			HasError = hasError;
			IsFrozen = isFrozen;
			TimerElapsedMs = timerElapsedMs;
			TimerMaxMs = timerMaxMs;
			PauseElapsedMs = pauseElapsedMs;
			PauseMaxMs = pauseMaxMs;
			SnoozeElapsedMs = snoozeElapsedMs;
			SnoozeMaxMs = snoozeMaxMs;
			TimeUntilNextTrigger = timeUntilNextTrigger;
		}

		/// <summary>Unique alert identifier.</summary>
		public string Id => _config.Id;
		/// <summary>Whether the alert is enabled.</summary>
		public bool IsActive => _config.IsActive;
		/// <summary>User-facing alert name.</summary>
		public string Name => _config.Name;
		/// <summary>Scheduling type for this alert.</summary>
		public AlertType Type => _config.Type;
		/// <summary>Fixed trigger hour.</summary>
		public int FixedTimeHour => _config.FixedTimeHour;
		/// <summary>Fixed trigger minute.</summary>
		public int FixedTimeMinute => _config.FixedTimeMinute;
		/// <summary>Timer interval in seconds.</summary>
		public int TimerSeconds => _config.TimerSeconds;
		/// <summary>Pause duration in seconds.</summary>
		public int PauseDurationSeconds => _config.PauseDurationSeconds;
		/// <summary>Snooze configuration.</summary>
		public SnoozeConfig SnoozeConfig => _config.SnoozeConfig;
		/// <summary>Active days set (0=Sunday..6=Saturday).</summary>
		public HashSet<int> ActiveDays => _config.ActiveDays;
		/// <summary>Optional path to custom notification sound.</summary>
		public string? NotificationSoundPath => _config.NotificationSoundPath;

		public AlertState State => IsActive ? _state : AlertState.Disabled;
		public bool StateChanged { get; }
		public bool HasError { get; }
		public bool IsFrozen { get; }
		public long TimerElapsedMs { get; }
		public long TimerMaxMs { get; }
		public long PauseElapsedMs { get; }
		public long PauseMaxMs { get; }
		public long SnoozeElapsedMs { get; }
		public long SnoozeMaxMs { get; }
		public TimeSpan? TimeUntilNextTrigger { get; }

		public bool IsRunning => _state is AlertState.Running or AlertState.Paused or AlertState.Snoozed or AlertState.PauseCompleted or AlertState.Waiting;


		/// <summary>Timer progress percentage in range [0, 100].</summary>
		public double TimerProgress => TimerMaxMs > 0
			? Math.Min((double)TimerElapsedMs / TimerMaxMs * 100, 100)
			: 0;

		/// <summary>Pause progress percentage in range [0, 100].</summary>
		public double PauseProgress => PauseMaxMs > 0
			? Math.Max((PauseMaxMs - PauseElapsedMs) / (double)PauseMaxMs * 100, 0)
			: 0;

		/// <summary>Snooze progress percentage in range [0, 100].</summary>
		public double SnoozeProgress => SnoozeMaxMs > 0
			? Math.Min((double)SnoozeElapsedMs / SnoozeMaxMs * 100, 100)
			: 0;
	}
}