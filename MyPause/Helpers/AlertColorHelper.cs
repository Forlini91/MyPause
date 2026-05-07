using System.Windows.Media;
using MyPause.Models;

namespace MyPause.Helpers
{
	/// <summary>
	/// Helper for mapping alert states to UI colors.
	/// </summary>
	public static class AlertColorHelper
	{
		/// <summary>Color for disabled alerts.</summary>
		public static readonly Color DISABLED = Colors.DarkGray;
		/// <summary>Color for frozen (editing) alerts.</summary>
		public static readonly Color FROZEN = Colors.LightBlue;
		/// <summary>Color for alerts in error state.</summary>
		public static readonly Color ERROR = Colors.Red;
		/// <summary>Color for destroyed alerts.</summary>
		public static readonly Color DESTROYED = Colors.Black;
		/// <summary>Color for stopped alerts.</summary>
		public static readonly Color STOPPED = Colors.Orange;
		/// <summary>Color for running alerts.</summary>
		public static readonly Color RUNNING = Colors.Green;
		/// <summary>Color for snoozed alerts.</summary>
		public static readonly Color SNOOZED = Colors.GreenYellow;
		/// <summary>Color for paused alerts.</summary>
		public static readonly Color PAUSED = Colors.Yellow;
		/// <summary>Color for pause completed alerts.</summary>
		public static readonly Color PAUSECOMPLETED = Colors.Blue;
		/// <summary>Color for waiting alerts.</summary>
		public static readonly Color WAITING = Colors.Purple;

		/// <summary>
		/// Gets the color for the given alert (using its state, frozen, and error flags).
		/// </summary>
		/// <param name="alert">The alert</param>
		/// <returns>Color for the alert state</returns>
		public static Color GetStateColor(IAlert alert)
		{
			return GetStateColor(alert.State, alert.IsFrozen, alert.HasError);
		}

		/// <summary>
		/// Gets the color for the given state, frozen, and error flags.
		/// </summary>
		/// <param name="state">Alert state</param>
		/// <param name="isFrozen">Is frozen</param>
		/// <param name="hasError">Has error</param>
		/// <returns>Color for the alert state</returns>
		public static Color GetStateColor(AlertState state, bool isFrozen, bool hasError)
		{
			if (isFrozen)
				return FROZEN;
			else if (hasError)
				return ERROR;

			return state switch
			{
				AlertState.Disabled => DISABLED,
				AlertState.Stopped => STOPPED,
				AlertState.Running => RUNNING,
				AlertState.Snoozed => SNOOZED,
				AlertState.Paused => PAUSED,
				AlertState.PauseCompleted => PAUSECOMPLETED,
				AlertState.Waiting => WAITING,
				AlertState.Destroyed => DESTROYED,
				_ => DISABLED
			};
		}
	}
}