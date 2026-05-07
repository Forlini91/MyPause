namespace MyPause.Models
{
	/// <summary>
	/// Snooze behavior configuration for a break alert.
	/// </summary>
	public class SnoozeConfig
	{
		/// <summary>Snooze duration in seconds for each snooze action.</summary>
		public int SnoozeSeconds { get; set; } = 600;

		/// <summary>Maximum number of allowed snoozes.</summary>
		public int MaxSnoozeCount { get; set; } = 3;

		/// <summary>If true, the break is mandatory and cannot be skipped.</summary>
		public bool MandatoryPause { get; set; } = false;

		/// <summary>
		/// Creates a deep copy of this snooze configuration.
		/// </summary>
		/// <returns>Cloned snooze configuration.</returns>
		public SnoozeConfig Clone()
		{
			return new SnoozeConfig
			{
				SnoozeSeconds = SnoozeSeconds,
				MaxSnoozeCount = MaxSnoozeCount,
				MandatoryPause = MandatoryPause
			};
		}
	}
}
