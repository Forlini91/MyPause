using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace MyPause.Services
{
	/// <summary>
	/// High-level tray icon actions produced from Windows messages.
	/// </summary>
	public enum TrayIconAction
	{
		None,
		LeftClick,
		RightClick
	}

	/// <summary>
	/// Lightweight Win32 tray icon wrapper for showing and handling MyPause tray interactions.
	/// </summary>
	public sealed class TrayIconService
	{
		/// <summary>Custom Windows message for tray icon events.</summary>
		private const uint WM_TRAYICON = 0x8001;

		private const uint NIM_ADD = 0x00000000;
		private const uint NIM_MODIFY = 0x00000001;
		private const uint NIM_DELETE = 0x00000002;

		private const uint NIF_MESSAGE = 0x00000001;
		private const uint NIF_ICON = 0x00000002;
		private const uint NIF_TIP = 0x00000004;

		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_RBUTTONUP = 0x0205;

		private const uint IDI_APPLICATION = 0x7F00;
		private const uint TRAY_ICON_ID = 1;

		private IntPtr _windowHandle;
		private uint _callbackMessage;
		private string _tooltip = "MyPause";
		private bool _isVisible;
		private IntPtr _iconHandle;
		private DispatcherTimer? _refreshTimer;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct NotifyIconData
		{
			public uint cbSize;
			public IntPtr hWnd;
			public uint uID;
			public uint uFlags;
			public uint uCallbackMessage;
			public IntPtr hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string szTip;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern bool Shell_NotifyIcon(uint dwMessage, ref NotifyIconData lpData);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern uint ExtractIconEx(
			string szFileName,
			int nIconIndex,
			IntPtr[]? phiconLarge,
			IntPtr[]? phiconSmall,
			uint nIcons);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadImage(
			IntPtr hInst,
			string name,
			uint type,
			int cx,
			int cy,
			uint fuLoad);


		/// <summary>
		/// Initializes tray icon service with owning window and callback message.
		/// </summary>
		/// <param name="windowHandle">Main window handle.</param>
		public void Initialize(IntPtr windowHandle)
		{
			_windowHandle = windowHandle;
			_callbackMessage = WM_TRAYICON;
			_tooltip = AppData.ApplicationName;
			InitializeIconHandle();
		}

		private void InitializeIconHandle()
		{
			var executablePath = Environment.ProcessPath;
			if (!string.IsNullOrWhiteSpace(executablePath))
			{
				var smallIcons = new IntPtr[1];
				var extractedCount = ExtractIconEx(executablePath, 0, null, smallIcons, 1);
				if (extractedCount > 0 && smallIcons[0] != IntPtr.Zero)
				{
					_iconHandle = smallIcons[0];
				}
			}

			if (_iconHandle == IntPtr.Zero)
			{
				_iconHandle = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
			}
		}

		/// <summary>
		/// Shows or updates the tray icon.
		/// </summary>
		public void ShowIcon()
		{
			if (_windowHandle == IntPtr.Zero)
				return;

			var data = CreateTrayData();
			var action = _isVisible ? NIM_MODIFY : NIM_ADD;
			if (Shell_NotifyIcon(action, ref data))
			{
				_isVisible = true;
			}
		}

		/// <summary>
		/// Removes the tray icon if currently visible.
		/// </summary>
		public void HideIcon()
		{
			if (!_isVisible || _windowHandle == IntPtr.Zero)
				return;

			var data = CreateTrayData();
			Shell_NotifyIcon(NIM_DELETE, ref data);
			_isVisible = false;
		}

		/// <summary>
		/// Starts a periodic timer to refresh the tray icon, preventing Windows from removing it during explorer crashes or tray rebuilds.
		/// </summary>
		public void StartRefreshTimer()
		{
			if (_refreshTimer != null)
				return;

			_refreshTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(30)
			};
			_refreshTimer.Tick += (_, _) => ShowIcon();
			_refreshTimer.Start();
		}

		/// <summary>
		/// Stops the periodic tray icon refresh timer.
		/// </summary>
		public void StopRefreshTimer()
		{
			if (_refreshTimer != null)
			{
				_refreshTimer.Stop();
				_refreshTimer = null;
			}
		}

		/// <summary>
		/// Parses a window message and returns a tray action when relevant.
		/// </summary>
		/// <param name="msg">Window message ID.</param>
		/// <param name="lParam">Message parameter.</param>
		/// <param name="action">Resolved tray action.</param>
		/// <returns>True if message belongs to tray icon callbacks.</returns>
		public bool TryHandleWindowMessage(int msg, IntPtr lParam, out TrayIconAction action)
		{
			action = TrayIconAction.None;
			if ((uint)msg != _callbackMessage)
				return false;

			switch (lParam.ToInt32())
			{
				case WM_LBUTTONUP:
					action = TrayIconAction.LeftClick;
					return true;
				case WM_RBUTTONUP:
					action = TrayIconAction.RightClick;
					return true;
				default:
					return false;
			}
		}

		private NotifyIconData CreateTrayData()
		{
			return new NotifyIconData
			{
				cbSize = (uint)Marshal.SizeOf<NotifyIconData>(),
				hWnd = _windowHandle,
				uID = TRAY_ICON_ID,
				uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
				uCallbackMessage = _callbackMessage,
				hIcon = _iconHandle,
				szTip = _tooltip
			};
		}
	}
}
