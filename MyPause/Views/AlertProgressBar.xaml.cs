using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MyPause.Models;
using MyPause.Helpers;
using System.Diagnostics;

namespace MyPause.Views
{
	/// <summary>
	/// Visual progress component for alert timer, pause, and snooze states.
	/// </summary>
	public partial class AlertProgressBar : UserControl
	{
		#region Fields & Properties

		private Alert? _alert;
		private bool _showTimerLabel;

		#endregion



		#region Constructors

		/// <summary>
		/// Initializes an empty progress bar without a bound alert.
		/// </summary>
		public AlertProgressBar()
		{
			InitializeComponent();
			_showTimerLabel = true;
		}

		/// <summary>
		/// Initializes a progress bar bound to an alert.
		/// </summary>
		/// <param name="alert">Alert model used as data source.</param>
		/// <param name="showTimerLabel">Whether to show remaining time label.</param>
		public AlertProgressBar(Alert alert, bool showTimerLabel = true)
		{
			InitializeComponent();
			_alert = alert;
			_showTimerLabel = showTimerLabel;
		}

		#endregion



		#region Lifecycle

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (_alert is null)
				return;

			Debug.WriteLine($"[AlertProgressBar] Loaded for alert: {_alert.Name}");
			ApplyLabelLayout();

			UpdateUI(_alert, _alert.GetSnapshot());
			_alert.OnUpdate += UpdateUI;
			_alert.OnConfigUpdate += UpdateUI;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (_alert is null)
				return;

			Debug.WriteLine($"[AlertProgressBar] Unloaded for alert: {_alert.Name}");
			_alert.OnUpdate -= UpdateUI;
		}

		#endregion



		#region Layout

		private void ApplyLabelLayout()
		{
			if (_showTimerLabel)
			{
				RemainingLabel.Visibility = Visibility.Visible;
				ProgressBarControl.Margin = new Thickness(0, 0, 8, 0);
				Grid.SetColumnSpan(ProgressBarControl, 1);
				return;
			}

			RemainingLabel.Visibility = Visibility.Collapsed;
			ProgressBarControl.Margin = new Thickness(0);
			Grid.SetColumnSpan(ProgressBarControl, 2);
		}

		#endregion



		#region Event Handlers

		private void UpdateUI(object? sender, AlertSnapshot snapshot)
		{
			Dispatcher.Invoke(() =>
			{
				UpdateProgress(snapshot);
				UpdateBarColor(snapshot);
				UpdatePulseState();
				UpdateRemainingText(snapshot);
			});
		}

		#endregion



		#region Progress Updates

		private void UpdateProgress(AlertSnapshot snapshot)
		{
			if (snapshot.HasError)
			{
				SetProgress(100);
				return;
			}

			double targetValue = snapshot.State switch
			{
				AlertState.Stopped => snapshot.TimerProgress,
				AlertState.Running => snapshot.TimerProgress,
				AlertState.Snoozed => snapshot.SnoozeProgress,
				AlertState.Paused => snapshot.PauseProgress,
				AlertState.PauseCompleted or AlertState.Waiting => 100,
				_ => 0
			};

			SetProgress(targetValue);
		}

		private void UpdateBarColor(AlertSnapshot snapshot)
		{
			Color color = AlertColorHelper.GetStateColor(snapshot);
			ProgressBarControl.Foreground = new SolidColorBrush(color);
		}

		private void UpdatePulseState()
		{
			if (_alert is null)
				return;

			// Pulsing solo quando lo stato è Snoozed e non è frozen
			if (_alert.State == AlertState.Snoozed && !_alert.IsFrozen)
				StartPulse();
			else
				StopPulse();
		}

		private void UpdateRemainingText(AlertSnapshot snapshot)
		{
			string text;
			if (snapshot.HasError)
			{
				text = string.Empty;
			}
			else
			{
				text = snapshot.State switch
				{
					AlertState.Paused =>
						TimeFormatter.FormatCountdown(TimeSpan.FromMilliseconds(Math.Max(snapshot.PauseMaxMs - snapshot.PauseElapsedMs, 0))),
					AlertState.Snoozed or AlertState.Running =>
						snapshot.TimeUntilNextTrigger.HasValue
							? TimeFormatter.FormatCountdown(snapshot.TimeUntilNextTrigger.Value)
							: string.Empty,
					AlertState.PauseCompleted or AlertState.Waiting => string.Empty,
					_ => string.Empty
				};
			}

			SetRemainingText(text);
		}

		#endregion



		#region Private Methods

		private void SetProgress(double targetValue)
		{
			var animation = new DoubleAnimation
			{
				To = targetValue,
				Duration = TimeSpan.FromMilliseconds(350),
				EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
			};

			ProgressBarControl.BeginAnimation(RangeBase.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);
		}

		private void SetRemainingText(string text)
		{
			RemainingLabel.Text = text;
		}

		private void StartPulse()
		{
			var pulseAnimation = new DoubleAnimation
			{
				From = 1.0,
				To = 0.55,
				Duration = TimeSpan.FromMilliseconds(650),
				AutoReverse = true,
				RepeatBehavior = RepeatBehavior.Forever,
				EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
			};

			BeginAnimation(OpacityProperty, pulseAnimation, HandoffBehavior.SnapshotAndReplace);
		}

		private void StopPulse()
		{
			BeginAnimation(OpacityProperty, null);
			Opacity = 1.0;
		}

		#endregion
	}
}
