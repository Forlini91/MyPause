using MyPause.Helpers;
using MyPause.Models;
using MyPause.Resources;
using MyPause.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyPause.Views
{
	/// <summary>
	/// UI card representing a single alert (pause rule) in the main window.
	/// Handles its own state, UI updates, and notification logic.
	/// </summary>
	public partial class AlertCard : UserControl
	{
		private const string FreezeCauseUpdate = "alertUpdate";
		private const string FreezeCauseDelete = "alertDelete";


		#region Properties & Fields

		/// <summary>The alert model associated with this card.</summary>
		public Alert _alert;
		/// <summary>Reference to the global alerts manager.</summary>
		private AlertsManager _alertsManager;
		/// <summary>Reference to the notification service for showing pause modals.</summary>
		private NotificationService _notificationService;

		#endregion

		#region Events

		/// <summary>
		/// Raised when the alert is edited (before, completed, cancelled, or error).
		/// </summary>
		public event EventHandler<AlertCardUpdateEvent>? OnAlertEdit;
		/// <summary>
		/// Raised when the alert is deleted (before or completed).
		/// </summary>
		public event EventHandler<AlertCardUpdateEvent>? OnAlertDelete;

		#endregion

		#region Lifecycle

		/// <summary>
		/// Initializes a new AlertCard for the given alert.
		/// </summary>
		/// <param name="alert">The alert model to display and manage.</param>
		/// <param name="alertsManager">Reference to the global alerts manager.</param>
		/// <param name="notificationService">Reference to the notification service.</param>
		public AlertCard(Alert alert, AlertsManager alertsManager, NotificationService notificationService)
		{
			InitializeComponent();
			_alert = alert;
			_alertsManager = alertsManager;
			_notificationService = notificationService;
			UpdateCardInfo(alert);
			UpdateCardStatus(alert);
		}

		/// <summary>
		/// Handles the Loaded event. Sets up the progress bar and subscribes to alert updates.
		/// </summary>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var progressBar = new AlertProgressBar(_alert);
			ProgressRow.Children.Add(progressBar);

			AlertSnapshot snapshot = _alert.GetSnapshot();
			UpdateUI(_alert, snapshot, true);
			_alert.OnUpdate += UpdateUI;
			_alert.OnConfigUpdate += UpdateUI;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			_alert.OnUpdate -= UpdateUI;
		}

		#endregion



		#region Events Handlers

		private void UpdateUI(object? sender, AlertSnapshot snapshot)
		{
			UpdateUI(sender, snapshot, false);
		}

		private void UpdateUI(object? sender, AlertSnapshot snapshot, bool firstLoad)
		{
			Dispatcher.Invoke(() =>
			{
				UpdateCardInfo(snapshot);
				UpdateCardStatus(snapshot);
				UpdateStartStopButton(snapshot);

				switch (snapshot.State)
				{
					case AlertState.Paused:
						if (snapshot.StateChanged || firstLoad)
						{
							_notificationService.ShowPauseNotification(
								_alert,
								snapshot.NotificationSoundPath,
								() => _alert.CompletePause(),
								() => _alert.Snooze()
							);
						}
						break;
				}
			});
		}

		#endregion



		#region Rendering
		private void UpdateCardInfo(IAlert alert)
		{
			TitleText.Text = alert.Name;
			DescriptionText.Text = alert.Type == AlertType.FixedTime
				? Strings.AlertCard_DescriptionFixedTimeFormat(alert.FixedTimeHour, alert.FixedTimeMinute, TimeFormatter.FormatSecondsShort(alert.PauseDurationSeconds))
				: Strings.AlertCard_DescriptionTimerFormat(TimeFormatter.FormatSecondsShort(alert.TimerSeconds), TimeFormatter.FormatSecondsShort(alert.PauseDurationSeconds));
		}
		private void UpdateCardStatus(IAlert alert)
		{
			Color color = AlertColorHelper.GetStateColor(alert);
			StatusIndicator.Fill = new SolidColorBrush(color);
			ProgressRow.Visibility = alert.State is AlertState.Disabled ? Visibility.Collapsed : Visibility.Visible;
		}

		private void UpdateStartStopButton(IAlert info)
		{
			if (info.State is AlertState.Disabled or AlertState.Destroyed || info.HasError)
			{
				StartStopButton.Visibility = Visibility.Collapsed;
				return;
			}

			StartStopButton.Visibility = Visibility.Visible;
			if (info.IsRunning)
			{
				StartStopButton.Content = "⏹";
				StartStopButton.Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));
			}
			else
			{
				StartStopButton.Content = "▶";
				StartStopButton.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA0, 0x85));
			}
		}

		#endregion



		#region Button Handlers

		private void StartStopButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			if (_alert.IsRunning)
			{
				_alert.Stop();
			}
			else
			{
				_alert.Stop();
				_alert.Start();
			}
		}

		private void EditButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;
				_alertsManager.SetAllFrozen(FreezeCauseUpdate, true, true);
				OnAlertEdit?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Before));
				var editWindow = new EditPauseWindow(_alert.Config, true, _alertsManager.ValidateName);
				if (editWindow.ShowDialog() == true)
				{
					Debug.WriteLine($"[AlertCard] Updating alert: {_alert.Name} (ID: {_alert.Id})");
					OnAlertEdit?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Completed));
					_alert.Config = editWindow.Config;
				}
				else
				{
					Debug.WriteLine($"[AlertCard] Update cancelled for alert: {_alert.Name} (ID: {_alert.Id})");
					OnAlertEdit?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Cancelled));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[AlertCard] Error updating alert: {_alert.Name} - {ex}");
				OnAlertEdit?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Error));
			}
			finally
			{
				_alertsManager.SetAllFrozen(FreezeCauseUpdate, false, true);
			}
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;
				_alertsManager.SetAllFrozen(FreezeCauseDelete, true, true);
				OnAlertDelete?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Before));
				if (MessageBox.Show(Strings.AlertCard_DeleteConfirmFormat(_alert.Name), Strings.AlertCard_ConfirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					Debug.WriteLine($"[AlertCard] Deleting alert: {_alert.Name} (ID: {_alert.Id})");
					OnAlertDelete?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Completed));
					_alertsManager.DeleteAlert(_alert);
				}
				else
				{
					Debug.WriteLine($"[AlertCard] Delete cancelled for alert: {_alert.Name} (ID: {_alert.Id})");
					OnAlertDelete?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Cancelled));
				}
			}
			catch (Exception ex)
			{
				OnAlertDelete?.Invoke(this, new AlertCardUpdateEvent(_alert, AlertCardUpdateState.Error));
				Debug.WriteLine($"[AlertCard] Error deleting alert: {_alert.Name} - {ex}");
			}
			finally
			{
				_alertsManager.SetAllFrozen(FreezeCauseDelete, false, true);
			}
		}

		#endregion
	}



	public enum AlertCardUpdateState
	{
		Before,
		Completed,
		Cancelled,
		Error
	}
	public readonly record struct AlertCardUpdateEvent(
		Alert Alert,
		AlertCardUpdateState State
	)
	{ }
}
