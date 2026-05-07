using MyPause.Models;
using MyPause.Views;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace MyPause.Services
{
	/// <summary>
	/// Service responsible for displaying and managing break (pause) notifications.
	/// Handles showing modal windows, overlays, and notification sound.
	/// </summary>
	public class NotificationService
	{
		/// <summary>Delegate for monitor enumeration (Win32 interop).</summary>
		private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		private static extern bool EnumDisplayMonitors(
			IntPtr hdc,
			IntPtr lprcClip,
			MonitorEnumProc lpfnEnum,
			IntPtr dwData);

		/// <summary>Active notification windows, keyed by alert ID.</summary>
		private readonly Dictionary<string, PauseNotificationWindow> _activeNotifications;
		/// <summary>Active overlay windows, keyed by alert ID.</summary>
		private readonly Dictionary<string, List<Window>> _activeOverlays;

		/// <summary>
		/// Initializes a new NotificationService.
		/// </summary>
		public NotificationService()
		{
			_activeNotifications = new();
			_activeOverlays = new();
		}

		/// <summary>
		/// Shows a break notification window for the given alert. If already visible, brings it to front.
		/// </summary>
		/// <param name="alert">The alert to notify for</param>
		/// <param name="notificationSoundPath">Optional path to a custom notification sound</param>
		/// <param name="onSkip">Callback for skip action</param>
		/// <param name="onSnooze">Callback for snooze action</param>
		public void ShowPauseNotification(Alert alert, string? notificationSoundPath, Action onSkip, Action onSnooze)
		{
			// If the notification is already visible, bring it to front.
			if (_activeNotifications.TryGetValue(alert.Id, out var existingWindow))
			{
				if (_activeOverlays.TryGetValue(alert.Id, out var existingOverlays))
				{
					foreach (var overlay in existingOverlays)
					{
						overlay.Activate();
					}
				}

				existingWindow.Activate();
				existingWindow.Focus();
				return;
			}

			// var overlayWindows = CreateInputBlockerOverlays();
			// foreach (var overlayWindow in overlayWindows)
			// {
			// 	overlayWindow.Show();
			// }

			var notificationWindow = new PauseNotificationWindow(alert, onSkip, onSnooze)
			{
				// Owner = overlayWindows.Count > 0 ? overlayWindows[0] : Application.Current.MainWindow
			};

			notificationWindow.Closed += (s, e) =>
			{
				_activeNotifications.Remove(alert.Id);

				if (_activeOverlays.TryGetValue(alert.Id, out var overlays))
				{
					_activeOverlays.Remove(alert.Id);
					foreach (var overlay in overlays)
					{
						overlay.Close();
					}
				}
			};

			_activeNotifications[alert.Id] = notificationWindow;
			// _activeOverlays[alert.Id] = overlayWindows;

			// foreach (var overlayWindow in overlayWindows)
			// {
			// 	overlayWindow.MouseDown += (s, e) =>
			// 	{
			// 		notificationWindow.Activate();
			// 		notificationWindow.Focus();
			// 	};
			// }

			notificationWindow.Show();
			notificationWindow.Activate();
			notificationWindow.Focus();

			// Play notification sound (use alert-specific sound).
			PlayNotificationSound(notificationSoundPath);
		}

		private static List<Window> CreateInputBlockerOverlays()
		{
			var overlays = new List<Window>();
			var monitorBounds = GetMonitorBounds();

			foreach (var bounds in monitorBounds)
			{
				overlays.Add(new Window
				{
					WindowStyle = WindowStyle.None,
					ResizeMode = ResizeMode.NoResize,
					AllowsTransparency = true,
					ShowInTaskbar = false,
					Topmost = true,
					Background = new SolidColorBrush(Color.FromArgb(192, 0, 0, 0)),
					Left = bounds.Left,
					Top = bounds.Top,
					Width = bounds.Right - bounds.Left,
					Height = bounds.Bottom - bounds.Top,
					WindowStartupLocation = WindowStartupLocation.Manual
				});
			}

			return overlays;
		}

		private static List<RECT> GetMonitorBounds()
		{
			var bounds = new List<RECT>();

			bool Callback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT monitorRect, IntPtr dwData)
			{
				bounds.Add(monitorRect);
				return true;
			}

			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, IntPtr.Zero);

			if (bounds.Count == 0)
			{
				bounds.Add(new RECT
				{
					Left = (int)SystemParameters.VirtualScreenLeft,
					Top = (int)SystemParameters.VirtualScreenTop,
					Right = (int)(SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth),
					Bottom = (int)(SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
				});
			}

			return bounds;
		}

		/// <summary>
		/// Plays the alert notification sound (custom file when available, otherwise system fallback).
		/// </summary>
		private void PlayNotificationSound(string? soundPath)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(soundPath) && File.Exists(soundPath))
				{
					// Play custom sound.
					using (var soundPlayer = new SoundPlayer(soundPath))
					{
						soundPlayer.PlaySync();
					}
				}
				else
				{
					// Fallback to system sound (Windows Alert).
					SystemSounds.Exclamation.Play();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error playing notification sound: {ex.Message}");
				// If custom file fails, still use Windows Alert.
				SystemSounds.Exclamation.Play();
			}
		}

		/// <summary>
		/// Closes all active notification windows and overlays.
		/// </summary>
		public void CloseAll()
		{
			var windows = _activeNotifications.Values.ToList();
			foreach (var window in windows)
			{
				window.ForceClose();
			}
			_activeNotifications.Clear();

			var overlays = _activeOverlays.Values.ToList();
			foreach (var overlayList in overlays)
			{
				foreach (var overlay in overlayList)
				{
					overlay.Close();
				}
			}
			_activeOverlays.Clear();
		}
	}
}
