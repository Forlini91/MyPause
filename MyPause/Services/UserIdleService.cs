using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace MyPause.Services
{
	/// <summary>
	/// Monitors user inactivity (keyboard/mouse idle time) and provides events when user becomes idle or active.
	/// Uses Win32 GetLastInputInfo API to detect system-wide idle time.
	/// </summary>
	public class UserIdleService
	{
		#region Win32 P/Invoke

		[StructLayout(LayoutKind.Sequential)]
		private struct LASTINPUTINFO
		{
			public uint cbSize;
			public uint dwTime;
		}

		[DllImport("user32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		#endregion

		#region Properties & Events

		/// <summary>Idle threshold in seconds. Default: 300 (5 minutes).</summary>
		public int IdleThresholdSeconds { get; set; } = 300;

		/// <summary>Check interval in seconds. Default: 1 (checks every second).</summary>
		public int CheckIntervalSeconds { get; set; } = 1;

		/// <summary>True if the user is currently idle.</summary>
		public bool IsUserIdle { get; private set; } = false;

		/// <summary>Raised when user transitions from active to idle.</summary>
		public event Action? OnUserBecameIdle;

		/// <summary>Raised when user transitions from idle to active.</summary>
		public event Action? OnUserBecameActive;

		#endregion

		#region Fields

		private DispatcherTimer? _checkTimer;
		private bool _isRunning = false;

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts monitoring user idle state. Must be called from the UI thread (Dispatcher available).
		/// </summary>
		public void Start()
		{
			if (_isRunning)
			{
				Debug.WriteLine("[UserIdleService] Service already running");
				return;
			}

			Debug.WriteLine("[UserIdleService] Starting idle detection...");
			_isRunning = true;
			_checkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(CheckIntervalSeconds) };
			_checkTimer.Tick += (s, e) => CheckIdleState();
			_checkTimer.Start();
		}

		/// <summary>
		/// Stops monitoring user idle state.
		/// </summary>
		public void Stop()
		{
			if (!_isRunning)
			{
				Debug.WriteLine("[UserIdleService] Service not running");
				return;
			}

			Debug.WriteLine("[UserIdleService] Stopping idle detection...");
			_isRunning = false;
			_checkTimer?.Stop();
			_checkTimer = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the system idle time in seconds (time since last keyboard/mouse input).
		/// </summary>
		private int GetIdleTimeSeconds()
		{
			LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
			lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

			if (!GetLastInputInfo(ref lastInputInfo))
			{
				Debug.WriteLine("[UserIdleService] GetLastInputInfo failed");
				return 0;
			}

			uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
			return (int)(idleTime / 1000);
		}

		/// <summary>
		/// Checks the current idle state and fires events if transitioning between idle and active.
		/// </summary>
		private void CheckIdleState()
		{
			int idleSeconds = GetIdleTimeSeconds();

			if (!IsUserIdle && idleSeconds >= IdleThresholdSeconds)
			{
				IsUserIdle = true;
				Debug.WriteLine($"[UserIdleService] User became idle ({idleSeconds}s)");
				OnUserBecameIdle?.Invoke();
			}
			else if (IsUserIdle && idleSeconds < IdleThresholdSeconds)
			{
				IsUserIdle = false;
				Debug.WriteLine($"[UserIdleService] User became active ({idleSeconds}s)");
				OnUserBecameActive?.Invoke();
			}
		}

		#endregion
	}
}
