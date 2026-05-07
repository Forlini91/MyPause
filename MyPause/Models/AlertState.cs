namespace MyPause.Models
{
	/// <summary>
	/// Main state of an alert (pause rule).
	/// </summary>
	public enum AlertState
	{
		Disabled,
		Stopped,
		Running,
		Paused,
		Snoozed,
		PauseCompleted,
		Waiting,
		Destroyed
	}
}
