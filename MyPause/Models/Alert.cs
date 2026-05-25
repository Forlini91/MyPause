using System.Windows.Threading;
using System.Diagnostics;

namespace MyPause.Models
{
	/// <summary>
	/// Represents a single break rule and its runtime state.
	/// </summary>
	public class Alert : IAlert
	{
		/// <summary>Time window (in seconds) for fixed time triggers.</summary>
		private const int FixedTimeTriggerWindowSeconds = 60;

		#region Configuration

		private AlertConfig _config;

		/// <summary>
		/// Gets or sets the alert configuration (deep clone).
		/// </summary>
		public AlertConfig Config
		{
			get => _config.Clone();
			set
			{
				_config = value.Clone();
				ApplyConfig();
				EmitConfigUpdate();
			}
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
		/// <summary>Whether to reset the timer for every pause.</summary>
		public bool ResetTimerForEveryPause => _config.ResetTimerForEveryPause;
		/// <summary>Pause duration in seconds.</summary>
		public int PauseDurationSeconds => _config.PauseDurationSeconds;
		/// <summary>Snooze configuration.</summary>
		public SnoozeConfig SnoozeConfig => _config.SnoozeConfig;
		/// <summary>Active days set (0=Sunday..6=Saturday).</summary>
		public HashSet<int> ActiveDays => _config.ActiveDays;
		/// <summary>Optional path to custom notification sound.</summary>
		public string? NotificationSoundPath => _config.NotificationSoundPath;


		private WorkSchedule _workSchedule;

		#endregion



		#region Runtime State

		private DispatcherTimer? _tickTimer;
		private EventHandler? _tickHandler;
		private AlertState _state = AlertState.Stopped;
		private DateTime _stateTime = DateTime.Now;
		private bool _hasError = false;
		private HashSet<string> _freezeCauses = [];
		/// <summary>Target trigger time for fixed-time alerts.</summary>
		private TimeSpan? _targetTime;
		/// <summary>Number of snoozes already used in current cycle.</summary>
		public int SnoozeCount { get; private set; }

		/// <summary>
		/// Optional gate checked before triggering a pause. When set, the alert skips the pause
		/// (FixedTime → Fired, Timer → timer reset) if the function returns false.
		/// Typically injected by AlertsManager to enforce cooldown and stacking prevention.
		/// </summary>
		public Func<bool>? CanTriggerPause { get; set; }

		public Func<Alert?>? GetExistingPausedAlert { get; set; }

		/// <summary>Current runtime state.</summary>
		public AlertState State
		{
			get => IsActive ? _state : AlertState.Disabled;
			private set => SetState(value);
		}

		public bool IsRunning => _state is AlertState.Running or AlertState.Paused or AlertState.Snoozed or AlertState.PauseCompleted or AlertState.Waiting;

		/// <summary>Whether alert updates are temporarily frozen.</summary>
		public bool IsFrozen => _freezeCauses.Count > 0;

		/// <summary>Whether alert is in error state.</summary>
		public bool HasError
		{
			get => _hasError;
			private set => SetErrorState(value);
		}

		/// <summary>Raised whenever alert snapshot changes.</summary>
		public event EventHandler<AlertSnapshot>? OnUpdate;
		public event EventHandler<AlertSnapshot>? OnConfigUpdate;

		#endregion



		#region Lifecycle

		/// <summary>
		/// Create a new alert with default config and work schedule.
		/// </summary>
		public Alert() : this(new AlertConfig(), new WorkSchedule())
		{
		}

		/// <summary>
		/// Create a new alert with the given config and work schedule.
		/// </summary>
		/// <param name="config">Alert configuration</param>
		/// <param name="workSchedule">Work schedule for this alert</param>
		public Alert(AlertConfig config, WorkSchedule workSchedule)
		{
			_config = config.Clone();
			_workSchedule = workSchedule;
			ApplyConfig();
		}

		#endregion



		#region State Updates

		private void ApplyConfig()
		{
			_targetTime = Type == AlertType.FixedTime
					? new TimeSpan(FixedTimeHour, FixedTimeMinute, 0)
					: null;
			SnoozeCount = 0;
			_stateTime = DateTime.Now;
			Restart();
		}

		private bool SetState(AlertState state, bool forceState = false)
		{
			if (_state == AlertState.Destroyed)
				return false;

			if (state == _state && !forceState)
				return false;

			Debug.WriteLine($"[{Name}] State: {_state} → {state}");
			_state = state;
			_stateTime = DateTime.Now;

			OnBeforeStateEntered(State);
			EmitUpdate(true);
			OnAfterStateEntered(State);
			Refresh(false);
			return true;
		}

		private void OnBeforeStateEntered(AlertState state)
		{
			switch (state)
			{
				case AlertState.Disabled:
				case AlertState.Stopped:
					StopTicking();
					break;
				case AlertState.Running:
					SnoozeCount = 0;
					StartTicking();
					break;
				case AlertState.Snoozed:
					SnoozeCount++;
					StartTicking();
					break;
				case AlertState.Paused:
				case AlertState.PauseCompleted:
				case AlertState.Waiting:
					StartTicking();
					break;
				case AlertState.Destroyed:
					StopTicking();
					break;
			}
		}

		private void OnAfterStateEntered(AlertState state)
		{
			switch (state)
			{
				case AlertState.Disabled:
				case AlertState.Stopped:
				case AlertState.Running:
				case AlertState.Paused:
				case AlertState.Snoozed:
				case AlertState.PauseCompleted:
				case AlertState.Waiting:
					break;
				case AlertState.Destroyed:
					OnUpdate = null;
					OnConfigUpdate = null;
					break;
			}
		}

		private bool SetErrorState(bool value)
		{
			if (value == _hasError)
				return false;

			Debug.WriteLine($"[{Name}] ErrorState: {_hasError} → {value}");
			_hasError = value;
			EmitUpdate();
			return true;
		}

		private void StartTicking()
		{
			if (_tickTimer is not null)
				return;

			_tickTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(0.125)
			};
			_tickHandler = (_, _) => Refresh();
			_tickTimer.Tick += _tickHandler;
			_tickTimer.Start();
		}

		private void StopTicking()
		{
			if (_tickTimer is null)
				return;

			if (_tickHandler is not null)
			{
				_tickTimer.Tick -= _tickHandler;
				_tickHandler = null;
			}

			_tickTimer.Stop();
			_tickTimer = null;
		}
		#endregion



		#region State Machine

		/// <summary>Starts the alert if it is stopped, fired, or waiting.</summary>
		public bool Start()
		{
			if (State is AlertState.Stopped or AlertState.PauseCompleted or AlertState.Waiting)
				return SetState(AlertState.Running);
			return false;
		}

		/// <summary>Stops the alert unless destroyed.</summary>
		public bool Stop()
		{
			if (State is not AlertState.Destroyed)
				return SetState(AlertState.Stopped);
			return false;
		}

		/// <summary>Restarts alert runtime state and clears error.</summary>
		public bool Restart()
		{
			if (IsRunning)
				return SetState(AlertState.Running, true);
			return false;
		}

		/// <summary>Transitions the alert into paused state when allowed.</summary>
		public bool StartPause()
		{
			if (State is AlertState.Running or AlertState.Snoozed or AlertState.PauseCompleted)
				return SetState(AlertState.Paused);
			return false;
		}

		/// <summary>Applies one snooze if available.</summary>
		public bool Snooze()
		{
			if (State is AlertState.Paused && SnoozeCount < SnoozeConfig.MaxSnoozeCount)
				return SetState(AlertState.Snoozed);
			return false;
		}

		/// <summary>Completes the current pause cycle.</summary>
		public bool CompletePause()
		{
			if (State is AlertState.Running or AlertState.Paused or AlertState.Snoozed)
				return SetState(AlertState.PauseCompleted);
			return false;
		}

		private bool ContinuePauseFrom(Alert other)
		{
			if (PauseDurationSeconds < other.PauseDurationSeconds)
				return CompletePause();

			bool changed;
			if (other.State is AlertState.Paused)
				changed = SetState(AlertState.Paused);
			else if (other.State is AlertState.Snoozed)
				changed = SetState(AlertState.Snoozed);
			else
				return false;

			if (!changed)
				return false;

			_stateTime = other._stateTime;
			SnoozeCount = other.SnoozeCount;
			other.CompletePause();
			return true;
		}

		public bool Wait()
		{
			if (State is AlertState.Running or AlertState.Paused or AlertState.Snoozed or AlertState.PauseCompleted)
				return SetState(AlertState.Waiting);
			return false;
		}


		/// <summary>Destroys the alert and releases its event handlers.</summary>
		public bool Destroy() => SetState(AlertState.Destroyed);


		/// <summary>Freezes or unfreezes the alert for the given cause. Multiple freeze causes are supported, alert is unfrozen only when all causes are removed.</summary>
		/// <param name="cause">Unique identifier for the freeze cause (e.g. alert ID for global freezes, or "UI" for freezes caused by user interaction).</param>
		/// <param name="value">True to freeze, false to unfreeze.</param>
		/// <param name="refresh">True to refresh the alert after changing frozen state.</param>
		public bool SetFreeze(string cause, bool value, bool refresh)
		{
			var isFrozen = IsFrozen;

			if (value)
			{
				_freezeCauses.Add(cause);
			}
			else
			{
				_freezeCauses.Remove(cause);
			}

			if (isFrozen == IsFrozen)
				return false;     //Unchanged. Was already frozen or unfrozen

			Debug.WriteLine($"[{Name}] Frozen: {isFrozen} → {value}");

			if (refresh)
				Refresh();
			return true;
		}




		/// <summary>
		/// Executes state machine transition logic and emits a new snapshot.
		/// </summary>
		public bool Refresh(bool alwaysEmitUpdate = true)
		{
			if (IsFrozen || State is AlertState.Disabled or AlertState.Stopped or AlertState.Destroyed)
			{
				if (alwaysEmitUpdate)
					EmitUpdate();
				return false;
			}

			if (IsInvalidTargetTime())
				return SetErrorState(true);

			var errorStateUpdated = SetErrorState(false);
			bool stateUpdated = State switch
			{
				AlertState.Running => WhenStateRunning(DateTime.Now),
				AlertState.Paused => WhenStatePaused(DateTime.Now),
				AlertState.Snoozed => WhenStateSnoozed(DateTime.Now),
				AlertState.PauseCompleted => WhenStatePauseCompleted(DateTime.Now),
				AlertState.Waiting => WhenStateWaiting(DateTime.Now),
				_ => false
			};

			if (alwaysEmitUpdate && !errorStateUpdated && !stateUpdated)
				// If no update was emitted by state or error changes, but alwaysEmitUpdate is true, force emit an update
				EmitUpdate();

			return true;
		}

		private bool WhenStateRunning(DateTime now)
		{
			if (!_workSchedule.IsWithinWorkHours(now.TimeOfDay))
				return Wait();
			else if (HasAlreadyFired(now))
				return CompletePause();
			else if (IsFiring(now))
			{
				if (CanTriggerPause?.Invoke() == false)
					return CompletePause();

				var prevPausedAlert = GetExistingPausedAlert?.Invoke();
				if (prevPausedAlert != null)
					return ContinuePauseFrom(prevPausedAlert);

				return StartPause();
			}
			return false;
		}

		private bool WhenStatePaused(DateTime now)
		{
			if (IsPauseCompleted(now))
				return CompletePause();
			return false;
		}

		private bool WhenStateSnoozed(DateTime now)
		{
			if (IsSnoozeElapsed(now))
				return StartPause();
			return false;
		}

		private bool WhenStatePauseCompleted(DateTime now)
		{
			if (!_workSchedule.IsWithinWorkHours(now.TimeOfDay))
				return Wait();
			else if (Type == AlertType.Timer)
				return Start();
			else if (!IsFiring(now) && !HasAlreadyFired(now))
				return Start();
			return false;
		}

		private bool WhenStateWaiting(DateTime now)
		{
			if (_workSchedule.IsWithinWorkHours(now.TimeOfDay))
				return Start();
			return false;
		}

		private void EmitUpdate(bool stateChanged = false)
		{
			var snapshot = GetSnapshot(stateChanged);
			OnUpdate?.Invoke(this, snapshot);
		}

		private void EmitConfigUpdate()
		{
			var snapshot = GetSnapshot();
			OnConfigUpdate?.Invoke(this, snapshot);
		}

		private bool IsInvalidTargetTime() => _targetTime.HasValue && !_workSchedule.IsWithinWorkHours(_targetTime.Value);
		private bool IsPauseCompleted(DateTime now) => (now - _stateTime).TotalSeconds >= PauseDurationSeconds;
		private bool IsSnoozeElapsed(DateTime now) => (now - _stateTime).TotalSeconds >= SnoozeConfig.SnoozeSeconds;
		private bool HasAlreadyFired(DateTime now)
		{
			if (Type == AlertType.Timer)
				return false;
			if (!_targetTime.HasValue)
				return false;
			if (!_workSchedule.IsWithinWorkHours(now.TimeOfDay))
				return false;

			var today = _workSchedule.GetEffectiveDayOfWeek(now);
			if (!ActiveDays.Contains((int)today))
				return false;

			var secs = (now.TimeOfDay - _targetTime.Value).TotalSeconds;
			return secs >= FixedTimeTriggerWindowSeconds;
		}
		private bool IsFiring(DateTime now)
		{
			return Type switch
			{
				AlertType.FixedTime => IsTargetTimeFiring(now),
				AlertType.Timer => IsTimerFiring(now),
				_ => false
			};
		}

		private bool IsTargetTimeFiring(DateTime now)
		{
			if (_targetTime.HasValue)
			{
				var secs = (now.TimeOfDay - _targetTime.Value).TotalSeconds;
				return secs > 0 && secs < FixedTimeTriggerWindowSeconds;
			}
			return false;
		}

		private bool IsTimerFiring(DateTime now) => (now - _stateTime).TotalSeconds >= TimerSeconds;

		/// <summary>
		/// Resets timer baseline to current time.
		/// </summary>
		public void ResetTimerCounter(bool refresh)
		{
			_stateTime = DateTime.Now;
			if (refresh)
				Refresh();
		}
		#endregion



		#region Snapshot Calculation
		/// <summary>
		/// Builds an immutable runtime snapshot used by UI components.
		/// </summary>
		/// <returns>Current alert snapshot.</returns>
		public AlertSnapshot GetSnapshot(bool stateChanged = false)
		{
			if (State == AlertState.Disabled)
			{
				return new AlertSnapshot(
					_config,
					_state,
					stateChanged,
					HasError,
					IsFrozen,
					0, 0, // timer
					0, 0, // pause
					0, 0, // snooze
					null
				);
			}

			var now = DateTime.Now;

			(long timerElapsed, long timerMax) = CalculateTimerElapsedMax(now);
			(long pauseElapsed, long pauseMax) = CalculatePauseElapsedMax(now);
			(long snoozeElapsed, long snoozeMax) = CalculateSnoozeElapsedMax(now);

			TimeSpan? timeUntilNextTrigger = Type switch
			{
				AlertType.Timer => GetTimeUntilNextTimerTrigger(now),
				AlertType.FixedTime => GetTimeUntilNextFixedTrigger(now),
				_ => null
			};

			return new AlertSnapshot(
				_config,
				_state,
				stateChanged,
				HasError,
				IsFrozen,
				timerElapsed,
				timerMax,
				pauseElapsed,
				pauseMax,
				snoozeElapsed,
				snoozeMax,
				timeUntilNextTrigger
			);
		}

		private (long elapsed, long max) CalculateTimerElapsedMax(DateTime now)
		{
			if (Type == AlertType.Timer)
			{
				long maxMs = TimerSeconds * 1000L;
				if (State != AlertState.Running)
					return (0, maxMs);

				long elapsedMs = (long)(now - _stateTime).TotalMilliseconds;
				return (Math.Min(elapsedMs, maxMs), maxMs);
			}

			var start = _workSchedule.WorkStart;
			var end = _workSchedule.WorkEnd;
			var target = _targetTime ?? new TimeSpan(FixedTimeHour, FixedTimeMinute, 0);

			if (State is AlertState.PauseCompleted or AlertState.Waiting)
			{
				return (0, 1);
			}

			var max = GetForwardDuration(start, target);
			if (max <= TimeSpan.Zero)
			{
				max = TimeSpan.FromMilliseconds(1);
			}

			var nowTime = now.TimeOfDay;
			if (start < end && nowTime < start)
			{
				return (0, Math.Max((long)max.TotalMilliseconds, 1));
			}

			var elapsed = GetForwardDuration(start, nowTime);
			if (elapsed > max)
			{
				elapsed = max;
			}
			return ((long)elapsed.TotalMilliseconds, Math.Max((long)max.TotalMilliseconds, 1));
		}

		private (long elapsed, long max) CalculatePauseElapsedMax(DateTime now)
		{
			if (State != AlertState.Paused)
				return (0, PauseDurationSeconds * 1000L);

			long elapsedMs = (long)(now - _stateTime).TotalMilliseconds;
			long maxMs = PauseDurationSeconds * 1000L;
			return (Math.Min(elapsedMs, maxMs), maxMs);
		}

		private (long elapsed, long max) CalculateSnoozeElapsedMax(DateTime now)
		{
			if (State != AlertState.Snoozed)
				return (0, SnoozeConfig.SnoozeSeconds * 1000L);

			long elapsedMs = (long)(now - _stateTime).TotalMilliseconds;
			long maxMs = SnoozeConfig.SnoozeSeconds * 1000L;
			return (Math.Min(elapsedMs, maxMs), maxMs);
		}

		private TimeSpan? GetTimeUntilNextTimerTrigger(DateTime now)
		{
			switch (State)
			{
				case AlertState.Stopped:
					return null;
				case AlertState.Snoozed:
					var delayEnd = _stateTime.AddSeconds(SnoozeConfig.SnoozeSeconds);
					return delayEnd > now ? delayEnd - now : TimeSpan.Zero;
				case AlertState.Paused:
					var pauseEnd = _stateTime.AddSeconds(PauseDurationSeconds);
					return pauseEnd > now ? pauseEnd - now : TimeSpan.Zero;
				case AlertState.Running:
					var nextTrigger = _stateTime.AddSeconds(TimerSeconds);
					if (nextTrigger <= now)
						return TimeSpan.Zero;

					return nextTrigger - now;
			}

			return null;
		}

		private TimeSpan? GetTimeUntilNextFixedTrigger(DateTime now)
		{
			var nextTrigger = GetNextFixedTriggerDateTime(now);
			if (!nextTrigger.HasValue)
				return null;

			return nextTrigger.Value > now ? nextTrigger.Value - now : TimeSpan.Zero;
		}

		private DateTime? GetNextFixedTriggerDateTime(DateTime now)
		{
			if (!_targetTime.HasValue || ActiveDays.Count == 0)
				return null;

			for (int offset = 0; offset <= 7; offset++)
			{
				var candidateDate = now.Date.AddDays(offset);
				if (!ActiveDays.Contains((int)candidateDate.DayOfWeek))
					continue;

				var candidate = candidateDate + _targetTime.Value;
				if (candidate > now)
					return candidate;
			}

			return null;
		}

		private static TimeSpan GetForwardDuration(TimeSpan from, TimeSpan to)
		{
			if (to >= from)
			{
				return to - from;
			}

			return TimeSpan.FromHours(24) - from + to;
		}

		#endregion
	}
}
