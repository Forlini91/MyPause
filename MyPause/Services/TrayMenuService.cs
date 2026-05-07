using System.Diagnostics;
using System.Windows.Controls;
using MyPause.Resources;

namespace MyPause.Services
{
	/// <summary>
	/// Manages the context menu for the system tray icon.
	/// Handles menu creation, display, and provides events for menu actions.
	/// </summary>
	public class TrayMenuService
	{
		#region Properties & Events

		/// <summary>Raised when the "Apri" (Open) menu item is clicked.</summary>
		public event Action? OnOpenClicked;

		/// <summary>Raised when the "Esci" (Exit) menu item is clicked.</summary>
		public event Action? OnExitClicked;

		private ContextMenu _trayContextMenu;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new TrayMenuService and creates the context menu.
		/// </summary>
		public TrayMenuService()
		{
			Debug.WriteLine("[TrayMenuService] Initializing tray menu...");
			_trayContextMenu = new ContextMenu();

			var openMenuItem = new MenuItem { Header = Strings.TrayMenu_Open };
			openMenuItem.Click += (s, e) =>
			{
				Debug.WriteLine("[TrayMenuService] Open menu item clicked");
				OnOpenClicked?.Invoke();
			};

			var exitMenuItem = new MenuItem { Header = Strings.TrayMenu_Exit };
			exitMenuItem.Click += (s, e) =>
			{
				Debug.WriteLine("[TrayMenuService] Exit menu item clicked");
				OnExitClicked?.Invoke();
			};

			_trayContextMenu.Items.Add(openMenuItem);
			_trayContextMenu.Items.Add(exitMenuItem);
		}

		#endregion



		#region Public Methods

		/// <summary>
		/// Shows the tray context menu at the current mouse position.
		/// </summary>
		public void ShowMenu()
		{
			if (_trayContextMenu is null)
			{
				Debug.WriteLine("[TrayMenuService] Context menu not initialized");
				return;
			}

			Debug.WriteLine("[TrayMenuService] Showing tray menu...");
			_trayContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
			_trayContextMenu.IsOpen = true;
		}

		#endregion
	}
}
