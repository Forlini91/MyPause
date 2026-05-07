namespace MyPause.Models
{
	/// <summary>
	/// Scheduling mode for an alert.
	/// </summary>
	public enum AlertType
	{
		/// <summary>Trigger at a specific time (for example, 10:30).</summary>
		FixedTime,

		/// <summary>Trigger after a time interval (timer).</summary>
		Timer
	}
}
