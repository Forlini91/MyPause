using MyPause.Helpers;
using MyPause.Models;
using MyPause.Resources;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MyPause.Views
{
	/// <summary>
	/// Modal break window shown when an alert enters paused state.
	/// </summary>
	public partial class PauseNotificationWindow : Window
	{
		#region Fields
		private readonly Alert _alert;
		private readonly Action _onSkip;
		private readonly Action _onSnooze;
		private bool _allowClose;
		#endregion

		#region Lifecycle
		/// <summary>
		/// Initializes a new pause notification window.
		/// </summary>
		/// <param name="alert">Alert in paused state.</param>
		/// <param name="onSkip">Callback invoked when skip/complete is requested.</param>
		/// <param name="onSnooze">Callback invoked when snooze is requested.</param>
		public PauseNotificationWindow(Alert alert, Action onSkip, Action onSnooze)
		{
			InitializeComponent();
			_alert = alert;
			_onSkip = onSkip;
			_onSnooze = onSnooze;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"[PauseNotificationWindow] Loaded for alert: {_alert.Name}");
			// Create and configure the progress bar without timer label.
			var pauseProgress = new AlertProgressBar(_alert, showTimerLabel: false);
			ProgressBarContainer.Children.Add(pauseProgress);
			AlertNameText.Text = _alert.Name;
			DurationText.Text = Strings.PauseNotification_DurationFormat(TimeFormatter.FormatSecondsLong(_alert.PauseDurationSeconds));
			SnoozeButton.Content = Strings.PauseNotification_SnoozeButtonFormat(TimeFormatter.FormatSecondsLong(_alert.SnoozeConfig.SnoozeSeconds));
			CompleteButton.Visibility = _alert.SnoozeConfig.MandatoryPause ? Visibility.Collapsed : Visibility.Visible;

			UpdateUI(_alert, _alert.GetSnapshot());
			_alert.OnUpdate += UpdateUI;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"[PauseNotificationWindow] Unloaded for alert: {_alert.Name}");
			_alert.OnUpdate -= UpdateUI;
		}
		#endregion

		#region Event Handlers
		private void UpdateUI(object? sender, AlertSnapshot snapshot)
		{
			Dispatcher.Invoke(() =>
			{
				if (snapshot.State != AlertState.Paused)
				{
					ForceClose();
					return;
				}

				UpdateCountdown(snapshot);
				UpdateSnooze();
			});
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.System && e.SystemKey == Key.F4)
			{
				e.Handled = true;
				Activate();
				Focus();
			}
		}

		private void OnClosing(object? sender, CancelEventArgs e)
		{
			if (_allowClose)
			{
				return;
			}

			// Block manual close attempts (including Alt+F4) while pause is active.
			e.Cancel = true;
			Activate();
			Focus();
		}
		#endregion

		#region UI Updates
		private void UpdateCountdown(AlertSnapshot snapshot)
		{
			long remainingMillis = Math.Max(snapshot.PauseMaxMs - snapshot.PauseElapsedMs, 0);
			int remainingSeconds = (int)Math.Ceiling(remainingMillis / 1000d);
			int minutes = remainingSeconds / 60;
			int seconds = remainingSeconds % 60;
			var remainingText = $"{minutes:D2}:{seconds:D2}";
			CountdownText.Text = remainingText;
			// Progress bar updates itself through alert events.
		}

		private void UpdateSnooze()
		{
			int remaining = _alert.SnoozeConfig.MaxSnoozeCount - _alert.SnoozeCount;
			SnoozeCountText.Text = Strings.PauseNotification_SnoozeAvailableFormat(remaining, _alert.SnoozeConfig.MaxSnoozeCount);
			SnoozeButton.Visibility = _alert.SnoozeCount < _alert.SnoozeConfig.MaxSnoozeCount ? Visibility.Visible : Visibility.Collapsed;
		}
		#endregion

		#region Actions
		private void SkipButton_Click(object sender, RoutedEventArgs e)
		{
			_onSkip?.Invoke();
			ForceClose();
		}

		private void SnoozeButton_Click(object sender, RoutedEventArgs e)
		{
			if (_alert.SnoozeCount >= _alert.SnoozeConfig.MaxSnoozeCount)
			{
				return;
			}

			_onSnooze?.Invoke();
			ForceClose();

			System.Media.SystemSounds.Beep.Play();
		}

		/// <summary>
		/// Forces the window to close even while pause is active.
		/// </summary>
		public void ForceClose()
		{
			_allowClose = true;
			Close();
		}
		#endregion
	}
}
