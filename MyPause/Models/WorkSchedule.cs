using MyPause.Services;

namespace MyPause.Models
{
	/// <summary>
	/// Represents the user's work schedule (start/end time) for break rules.
	/// </summary>
	public class WorkSchedule
	{
		/// <summary>Start of the workday (inclusive).</summary>
		public TimeSpan WorkStart { get; set; } = new TimeSpan(8, 0, 0);
		/// <summary>End of the workday (exclusive).</summary>
		public TimeSpan WorkEnd { get; set; } = new TimeSpan(17, 0, 0);
		/// <summary>Minutes to block new pauses after one is triggered (0 = disabled).</summary>
		public int PauseCooldownSeconds { get; set; } = 30;

		/// <summary>
		/// Initializes the schedule from a configuration object.
		/// </summary>
		/// <param name="configuration">Work schedule configuration</param>
		public void Initialize(WorkScheduleConfiguration? configuration)
		{
			if (configuration == null)
				return;

			WorkStart = new TimeSpan(configuration.WorkScheduleStartHour, configuration.WorkScheduleStartMinute, 0);
			WorkEnd = new TimeSpan(configuration.WorkScheduleEndHour, configuration.WorkScheduleEndMinute, 0);
			PauseCooldownSeconds = configuration.PauseCooldownSeconds;
		}

		/// <summary>
		/// Returns a configuration object representing the current schedule.
		/// </summary>
		public WorkScheduleConfiguration GetConfiguration()
		{
			return new WorkScheduleConfiguration
			{
				WorkScheduleStartHour = WorkStart.Hours,
				WorkScheduleStartMinute = WorkStart.Minutes,
				WorkScheduleEndHour = WorkEnd.Hours,
				WorkScheduleEndMinute = WorkEnd.Minutes,
				PauseCooldownSeconds = PauseCooldownSeconds,
			};
		}

		/// <summary>
		/// Updates the workday start hour.
		/// </summary>
		public void UpdateWorkStartHour(int hour)
		{
			WorkStart = new TimeSpan(hour, WorkStart.Minutes, WorkStart.Seconds);
		}

		/// <summary>
		/// Updates the workday start minute.
		/// </summary>
		public void UpdateWorkStartMinute(int minute)
		{
			WorkStart = new TimeSpan(WorkStart.Hours, minute, WorkStart.Seconds);
		}

		/// <summary>
		/// Updates the workday end hour.
		/// </summary>
		public void UpdateWorkEndHour(int hour)
		{
			WorkEnd = new TimeSpan(hour, WorkEnd.Minutes, WorkEnd.Seconds);
		}

		/// <summary>
		/// Updates the workday end minute.
		/// </summary>
		public void UpdateWorkEndMinute(int minute)
		{
			WorkEnd = new TimeSpan(WorkEnd.Hours, minute, WorkEnd.Seconds);
		}

		/// <summary>
		/// Returns the effective day of week for the current time, handling overnight schedules.
		/// </summary>
		/// <param name="now">Current date and time</param>
		public DayOfWeek GetEffectiveDayOfWeek(DateTime now)
		{
			if (WorkStart <= WorkEnd)
				return now.DayOfWeek;
			if (now.TimeOfDay > WorkEnd)
				return now.DayOfWeek;
			return now.AddDays(-1).DayOfWeek;
		}


		public bool IsWithinWorkHours(TimeSpan time)
		{
			if (WorkStart == WorkEnd)
				return true;

			if (WorkStart < WorkEnd)
				return WorkStart <= time && time <= WorkEnd;  //include time between start and end (start < end)
			else
				return time <= WorkEnd || WorkStart <= time;  //exclude time between end and start (end < start)
		}

		public bool IsBeforeWorkHour(TimeSpan time)
		{
			if (WorkStart == WorkEnd)
				return false;

			if (WorkStart < WorkEnd)
				return time < WorkStart;
			else
				return WorkEnd < time && time < WorkStart;
		}

		public bool IsAfterWorkHour(TimeSpan time)
		{
			if (WorkStart == WorkEnd)
				return false;

			if (WorkStart < WorkEnd)
				return time > WorkEnd;
			else
				return WorkEnd < time && time < WorkStart;
		}
	}
}