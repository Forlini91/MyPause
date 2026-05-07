using System.Diagnostics;
using MyPause.Models;
using MyPause.Resources;

namespace MyPause.Services
{
	/// <summary>
	/// Manages the in-memory lifecycle of all alerts and coordinates cross-alert behavior.
	/// </summary>
	public class AlertsManager
	{
		#region Fields & Properties

		private Dictionary<string, Alert> _alertById;
		/// <summary>Work schedule, used to read PauseCooldownMinutes at runtime.</summary>
		private WorkSchedule _workSchedule;
		/// <summary>Current alerts collection.</summary>
		public IEnumerable<Alert> Alerts => _alertById.Values.AsEnumerable();
		/// <summary>Timestamp of the last triggered pause, used to enforce cooldown.</summary>
		private DateTime? _lastTrigger;

		#endregion



		#region Constructor

		/// <summary>
		/// Initializes an empty alerts manager.
		/// </summary>
		/// <param name="workSchedule">Shared work schedule.</param>
		public AlertsManager(WorkSchedule workSchedule)
		{
			_workSchedule = workSchedule;
			_alertById = [];
		}

		#endregion



		#region Public Methods

		public int Count => _alertById.Count;

		/// <summary>
		/// Adds an alert and subscribes to its update events.
		/// </summary>
		/// <param name="alert">Alert instance to add.</param>
		public void AddAlert(Alert alert)
		{
			Debug.WriteLine($"[AlertsManager] Adding new alert: {alert.Name} (ID: {alert.Id})");
			_alertById[alert.Id] = alert;
			alert.OnUpdate += OnAlertUpdate;
			alert.CanTriggerPause = CanTriggerPause;
		}

		/// <summary>
		/// Deletes an alert, unsubscribes handlers, and destroys its runtime state.
		/// </summary>
		/// <param name="alert">Alert to remove.</param>
		public void DeleteAlert(Alert alert)
		{
			Debug.WriteLine($"[AlertsManager] Deleting alert: {alert.Name} (ID: {alert.Id})");
			_alertById.Remove(alert.Id);
			alert.OnUpdate -= OnAlertUpdate;
			alert.Destroy();
		}

		/// <summary>
		/// Sets frozen state for all alerts.
		/// </summary>
		/// <param name="frozen">True to freeze, false to unfreeze.</param>
		public void SetAllFrozen(string cause, bool frozen)
		{
			Debug.WriteLine($"[AlertsManager] Setting all alerts frozen: {frozen}");
			foreach (var alert in _alertById.Values)
			{
				alert.SetFreeze(cause, frozen);
			}
		}

		/// <summary>
		/// Forces refresh on every alert.
		/// </summary>
		public void RefreshAll()
		{
			foreach (var alert in _alertById.Values)
			{
				alert.Refresh();
			}
		}

		/// <summary>
		/// Starts all alerts from a clean state.
		/// </summary>
		public void StartAll()
		{
			foreach (var alert in _alertById.Values)
			{
				alert.Stop();
				alert.Start();
			}
		}

		/// <summary>
		/// Stops all alerts.
		/// </summary>
		public void StopAll()
		{
			foreach (var alert in _alertById.Values)
			{
				alert.Stop();
			}
		}

		public void ResetAllTimers(bool refresh)
		{
			foreach (var alert in _alertById.Values)
			{
				alert.ResetTimerCounter(refresh);
			}
		}

		/// <summary>
		/// Returns true if a new pause can be triggered right now.
		/// Returns false when any alert is already paused/snoozed, or when the cooldown
		/// window since the last triggered pause has not yet elapsed.
		/// </summary>
		public bool CanTriggerPause()
		{
			if (_alertById.Values.Any(a => a.State is AlertState.Paused or AlertState.Snoozed))
				return false;

			var cooldownSeconds = _workSchedule.PauseCooldownSeconds;
			if (cooldownSeconds > 0 && _lastTrigger.HasValue)
			{
				if ((DateTime.Now - _lastTrigger.Value).TotalSeconds < cooldownSeconds)
					return false;
			}

			return true;
		}

		public bool ValidateName(string name)
		{
			return !_alertById.Values.Any(alert => alert.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		#endregion



		#region Configuration Methods

		/// <summary>
		/// Initializes manager alerts from persisted config, or defaults if null.
		/// </summary>
		/// <param name="alertsConfigs">Persisted alert configurations.</param>
		public void Initialize(List<AlertConfig>? alertsConfigs)
		{
			List<AlertConfig> configs = alertsConfigs ?? DefaultAlertsConfigs();
			var alerts = configs.Select(config => new Alert(config, _workSchedule)).ToList();
			_alertById = alerts
				.DistinctBy(alert => alert.Id)
				.ToDictionary(
					alert => alert.Id,
					alert => alert
				);
			foreach (var alert in _alertById.Values)
				alert.CanTriggerPause = CanTriggerPause;
		}

		/// <summary>
		/// Returns current configuration for all alerts.
		/// </summary>
		/// <returns>List of alert configurations.</returns>
		public List<AlertConfig> GetConfiguration()
		{
			return _alertById.Values.Select(alert => alert.Config).ToList();
		}

		/// <summary>
		/// Returns IDs for alerts currently running or waiting due to pause/snooze.
		/// </summary>
		/// <returns>Set of alert IDs representing active runtime state.</returns>
		public HashSet<string> GetRuntimeState()
		{
			return _alertById.Values
			.Where(alert => alert.IsRunning)
			.Select(alert => alert.Id)
			.ToHashSet();
		}


		/// <summary>
		/// Builds default sample alerts used on first startup.
		/// </summary>
		private static List<AlertConfig> DefaultAlertsConfigs()
		{
			return new List<AlertConfig>
			{
				new() {
					Name = Strings.Alerts_Default_Morning,
					Type = AlertType.FixedTime,
					FixedTimeHour = 10,
					FixedTimeMinute = 30,
					PauseDurationSeconds = 300,
					IsActive = true
				},
				new() {
					Name = Strings.Alerts_Default_Afternoon,
					Type = AlertType.FixedTime,
					FixedTimeHour = 16,
					FixedTimeMinute = 0,
					PauseDurationSeconds = 300,
					IsActive = true
				},
				new() {
					Name = Strings.Alerts_Default_HourlyTimer,
					Type = AlertType.Timer,
					TimerSeconds = 3600,
					PauseDurationSeconds = 180,
					IsActive = true
				}
			};
		}

		#endregion



		#region Event handlers

		private void OnAlertUpdate(object? sender, AlertSnapshot snapshot)
		{
			switch (snapshot.State)
			{
				case AlertState.Paused:
					_lastTrigger = DateTime.Now;
					ResetAllTimers(false);
					break;
				case AlertState.Snoozed:
				case AlertState.PauseCompleted:
					if (snapshot.StateChanged)
					{
						_lastTrigger = DateTime.Now;
						ResetAllTimers(false);
					}
					break;
			}
		}

		#endregion
	}
}
