using MyPause.Helpers;
using MyPause.Models;
using MyPause.Resources;
using MyPause.Services;
using MyPause.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;


namespace MyPause
{
	/// <summary>
	/// Main application window. Hosts the alert cards, work schedule panel, and manages global app state.
	/// </summary>
	public partial class MainWindow : Window
	{


		private const string FreezeCauseIdle = "idle";
		private const string FreezeCauseCreate = "alertCreate";


		/// <summary>Handles configuration persistence (JSON).</summary>
		private StorageManager _storageManager;
		/// <summary>Stores and manages work hours configuration.</summary>
		private WorkSchedule _workSchedule;
		/// <summary>Manages all alert logic and state.</summary>
		private AlertsManager _alertsManager;

		/// <summary>Handles showing and tracking pause notifications.</summary>
		private NotificationService _notificationService;
		/// <summary>Service for detecting user inactivity.</summary>
		private UserIdleService _userIdleService;
		/// <summary>Manages the system tray icon and menu.</summary>
		private readonly TrayIconService _trayIconService;
		/// <summary>Service for managing the tray context menu.</summary>
		private TrayMenuService _trayMenuService;

		/// <summary>Timer for clearing status messages.</summary>
		private DispatcherTimer? _statusClearTimer;
		/// <summary>True if the app is exiting (prevents double shutdown).</summary>
		private bool _isExit;
		/// <summary>If true, minimize to tray on close instead of exiting.</summary>
		private bool _minimizeToTrayOnClose = true;

		#region Lifecycle



		/// <summary>
		/// Initializes the main window and all core services.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			_storageManager = new StorageManager();
			_workSchedule = new WorkSchedule();
			_alertsManager = new AlertsManager(_workSchedule);

			_notificationService = new NotificationService();
			_userIdleService = new UserIdleService();
			_trayIconService = new TrayIconService();
			_trayMenuService = new TrayMenuService();
		}

		/// <summary>
		/// Called when the window source is initialized. Sets up tray icon and message hooks.
		/// </summary>
		/// <param name="e">Event args</param>
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			var windowHandle = new WindowInteropHelper(this).Handle;
			_trayIconService.Initialize(windowHandle);
			_trayIconService.ShowIcon();
			if (HwndSource.FromHwnd(windowHandle) is HwndSource source)
			{
				source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) => HandleWindowMessage(msg, lParam, ref handled));
			}
		}

		/// <summary>
		/// Handles window loaded event. Loads configuration and runtime state.
		/// </summary>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("[MainWindow] Loaded");

			var configuration = _storageManager.LoadConfiguration();
			var runtimeState = _storageManager.LoadRuntimeState();

			_alertsManager.Initialize(configuration?.Alerts);
			_workSchedule.Initialize(configuration?.WorkSchedule);
			_minimizeToTrayOnClose = configuration?.MinimizeToTrayOnClose ?? true;

			MinimizeToTrayCheckBox.IsChecked = _minimizeToTrayOnClose;
			StartupCheckBox.IsChecked = RegistryHelper.AutoStartup;

			var workSchedulePanel = new WorkSchedulePanel(_workSchedule);
			workSchedulePanel.ConfigurationChanged += (_, _) => OnWorkScheduleChanged();
			WorkSchedulePanelSlot.Content = workSchedulePanel;

			AlertsPanel.Children.Clear();

			_alertsManager.ForEach(alert =>
			{
				var card = CreateAlertCard(alert);
				AlertsPanel.Children.Add(card);

				if (runtimeState.RunningAlertIds.Contains(alert.Id))
				{
					alert.Start();
				}
			});

			ShowStatusDefault();

			// Initialize idle detection
			_userIdleService.OnUserBecameIdle += () =>
			{
				_alertsManager.SetAllFrozen(FreezeCauseIdle, true, true);
				ShowStatusMessage(Strings.MainWindow_UserIdlePaused);
			};
			_userIdleService.OnUserBecameActive += () =>
			{
				_alertsManager.ResetAllTimers(false);
				_alertsManager.SetAllFrozen(FreezeCauseIdle, false, true);
				ShowStatusMessage(Strings.MainWindow_UserActiveRestarted);
			};
			_userIdleService.Start();
			_trayIconService.StartRefreshTimer();

			// Tray menu is initialized in TrayMenuService constructor, just subscribe to events
			_trayMenuService.OnOpenClicked += ShowMainWindow;
			_trayMenuService.OnExitClicked += ExitApplication;
		}

		private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!_isExit && _minimizeToTrayOnClose)
			{
				e.Cancel = true;
				WindowState = WindowState.Minimized;
				return;
			}

			Debug.WriteLine("[MainWindow] Closing");
			_userIdleService?.Stop();
			_trayIconService.StopRefreshTimer();
			_trayIconService.HideIcon();
			SaveRuntimeStateSnapshot();
			_alertsManager.StopAll();
			_notificationService.CloseAll();
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (WindowState == WindowState.Minimized && !_isExit)
			{
				_trayIconService.ShowIcon();
				Hide();
			}
		}
		#endregion



		#region UI
		private void ShowStatusDefault()
		{
			_statusClearTimer?.Stop();
			StatusText.Text = Strings.MainWindow_StatusConfiguredBreaksFormat(_alertsManager.Count);
		}

		private void ShowStatusMessage(string message, long milliseconds = 5000)
		{
			if (_statusClearTimer is null)
			{
				_statusClearTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(milliseconds)
				};
				_statusClearTimer.Tick += (sender, e) =>
				{
					if (_statusClearTimer is null)
						return;

					_statusClearTimer.Stop();
					ShowStatusDefault();
				};
			}

			_statusClearTimer.Interval = TimeSpan.FromMilliseconds(milliseconds);
			StatusText.Text = message;

			_statusClearTimer.Stop();
			_statusClearTimer.Start();
		}
		#endregion



		#region Card Composition

		private void AddAlertCard(Alert alert, int index = -1)
		{
			var card = CreateAlertCard(alert);
			if (index >= 0 && index < AlertsPanel.Children.Count)
			{
				AlertsPanel.Children.Insert(index, card);
			}
			else
			{
				AlertsPanel.Children.Add(card);
			}
		}

		private int RemoveAlertCard(AlertCard card)
		{
			int index = -1;
			card.OnAlertEdit -= OnAlertEdit;
			card.OnAlertDelete -= OnAlertDeleted;
			index = AlertsPanel.Children.IndexOf(card);
			if (index >= 0)
			{
				AlertsPanel.Children.RemoveAt(index);
			}
			return index;
		}

		private AlertCard CreateAlertCard(Alert alert)
		{
			var card = new AlertCard(alert, _alertsManager, _notificationService);
			card.OnAlertEdit += OnAlertEdit;
			card.OnAlertDelete += OnAlertDeleted;
			return card;
		}
		#endregion



		#region Card Events
		private void OnAlertEdit(object? sender, AlertCardUpdateEvent updateEvent)
		{
			switch (updateEvent.State)
			{
				case AlertCardUpdateState.Completed:
					Debug.WriteLine($"[MainWindow] Alert update completed: {updateEvent.Alert.Name}");
					SaveConfiguration();
					ShowStatusMessage(Strings.MainWindow_AlertUpdatedFormat(updateEvent.Alert.Name));
					break;
			}
		}

		private void OnAlertDeleted(object? sender, AlertCardUpdateEvent updateEvent)
		{
			if (sender is not AlertCard card)
			{
				Debug.WriteLine("[MainWindow] Invalid sender for OnAlertDeleted");
				return;
			}

			switch (updateEvent.State)
			{
				case AlertCardUpdateState.Completed:
					Debug.WriteLine($"[MainWindow] Alert delete completed: {updateEvent.Alert.Name}");
					RemoveAlertCard(card);
					SaveConfiguration();
					ShowStatusMessage(Strings.MainWindow_AlertDeletedFormat(updateEvent.Alert.Name));
					break;
			}
		}
		#endregion



		#region App Actions
		private void AddPauseButton_Click(object sender, RoutedEventArgs e)
		{
			_alertsManager.SetAllFrozen(FreezeCauseCreate, true, true);
			try
			{
				var editWindow = new EditPauseWindow(new AlertConfig(), false, _alertsManager.ValidateName);
				if (editWindow.ShowDialog() == true)
				{
					var alert = new Alert(editWindow.Config, _workSchedule);
					_alertsManager.AddAlert(alert);
					alert.Refresh();
					AddAlertCard(alert);
					SaveConfiguration();
					ShowStatusMessage(Strings.MainWindow_AlertAddedFormat(alert.Name));
				}
			}
			finally
			{
				_alertsManager.SetAllFrozen(FreezeCauseCreate, false, true);
			}
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			_alertsManager.StartAll();
			ShowStatusMessage(Strings.MainWindow_AppActiveMonitoring);
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			_alertsManager.StopAll();
			ShowStatusMessage(Strings.MainWindow_AppStopped);
		}

		private void StartupCheckBox_Click(object sender, RoutedEventArgs e)
		{
			var enabled = StartupCheckBox.IsChecked == true;

			try
			{
				RegistryHelper.AutoStartup = enabled;
				var message = enabled ? Strings.MainWindow_StartupEnabled : Strings.MainWindow_StartupDisabled;
				MessageBox.Show(message, Strings.Common_InfoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(Strings.MainWindow_StartupUpdateErrorFormat(ex.Message), Strings.Common_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
				StartupCheckBox.IsChecked = RegistryHelper.AutoStartup;
			}
		}

		private void MinimizeToTrayCheckBox_Click(object sender, RoutedEventArgs e)
		{
			_minimizeToTrayOnClose = MinimizeToTrayCheckBox.IsChecked == true;
			Debug.WriteLine($"[MainWindow] Minimize to tray on close: {_minimizeToTrayOnClose}");
			SaveConfiguration();
		}

		private void OnWorkScheduleChanged()
		{
			_alertsManager.RefreshAll();
			SaveConfiguration();
		}
		#endregion



		private void SaveRuntimeStateSnapshot()
		{
			var runtimeState = new RuntimeState
			{
				RunningAlertIds = _alertsManager.GetRuntimeState()
			};

			_storageManager.SaveRuntimeState(runtimeState);
		}

		private void SaveConfiguration()
		{
			var configuration = new Configuration()
			{
				Alerts = _alertsManager.GetConfiguration(),
				WorkSchedule = _workSchedule.GetConfiguration(),
				MinimizeToTrayOnClose = _minimizeToTrayOnClose
			};
			_storageManager.SaveConfiguration(configuration);
		}









		private IntPtr HandleWindowMessage(int msg, IntPtr lParam, ref bool handled)
		{
			if (_trayIconService.TryHandleWindowMessage(msg, lParam, out var action))
			{
				switch (action)
				{
					case TrayIconAction.LeftClick:
						ShowMainWindow();
						handled = true;
						break;
					case TrayIconAction.RightClick:
						_trayMenuService.ShowMenu();
						handled = true;
						break;
				}
			}

			return IntPtr.Zero;
		}

		private void ShowMainWindow()
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void ExitApplication()
		{
			_isExit = true;
			Close();
		}
	}
}