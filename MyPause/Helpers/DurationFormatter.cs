namespace MyPause.Helpers
{
	using MyPause.Resources;

	/// <summary>
	/// Time unit for formatting durations.
	/// </summary>
	public enum TimeUnit
	{
		Seconds,
		Minutes,
		Hours
	}

	/// <summary>
	/// Helper class for formatting and converting time durations.
	/// </summary>
	public static class TimeFormatter
	{
		/// <summary>
		/// Converts a number of seconds to the most appropriate time unit (hours, minutes, or seconds).
		/// </summary>
		/// <param name="totalSeconds">Total seconds</param>
		/// <returns>Tuple of value and unit</returns>
		public static (int Value, TimeUnit Unit) ToBestUnit(int totalSeconds)
		{
			if (totalSeconds % 3600 == 0)
			{
				return (totalSeconds / 3600, TimeUnit.Hours);
			}

			if (totalSeconds % 60 == 0)
			{
				return (totalSeconds / 60, TimeUnit.Minutes);
			}

			return (totalSeconds, TimeUnit.Seconds);
		}

		/// <summary>
		/// Converts a value and unit to seconds.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="unit">The time unit</param>
		/// <returns>Total seconds</returns>
		public static int ToSeconds(int value, TimeUnit unit)
		{
			return unit switch
			{
				TimeUnit.Hours => value * 3600,
				TimeUnit.Minutes => value * 60,
				_ => value
			};
		}

		/// <summary>
		/// Formats a duration in seconds as a long string (e.g. "2 hours", "5 minutes").
		/// </summary>
		/// <param name="totalSeconds">Total seconds</param>
		/// <returns>Formatted string</returns>
		public static string FormatSecondsLong(int totalSeconds)
		{
			var (value, unit) = ToBestUnit(totalSeconds);
			return unit switch
			{
				TimeUnit.Hours => $"{value} {Strings.Duration_LongHour(value == 1)}",
				TimeUnit.Minutes => $"{value} {Strings.Duration_LongMinute(value == 1)}",
				_ => $"{value} {Strings.Duration_LongSecond(value == 1)}"
			};
		}

		/// <summary>
		/// Formats a duration in seconds as a short string (e.g. "2h", "5 min").
		/// </summary>
		/// <param name="totalSeconds">Total seconds</param>
		/// <returns>Formatted string</returns>
		public static string FormatSecondsShort(int totalSeconds)
		{
			var (value, unit) = ToBestUnit(totalSeconds);
			return unit switch
			{
				TimeUnit.Hours => $"{value}h",
				TimeUnit.Minutes => $"{value} min",
				_ => $"{value}s"
			};
		}

		/// <summary>
		/// Formats a countdown TimeSpan as a string (e.g. "01:23:45").
		/// </summary>
		/// <param name="remaining">Time remaining</param>
		/// <returns>Formatted string</returns>
		public static string FormatCountdown(TimeSpan remaining)
		{
			if (remaining.TotalDays >= 1)
			{
				int days = (int)Math.Ceiling(remaining.TotalDays);
				return $"{days} {Strings.Duration_Day(days == 1)}";
			}

			if (remaining.TotalHours >= 1)
			{
				int totalHours = (int)remaining.TotalHours;
				return $"{totalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
			}

			if (remaining.TotalMinutes >= 1)
			{
				int totalMinutes = (int)remaining.TotalMinutes;
				return $"{totalMinutes:D2}:{remaining.Seconds:D2}";
			}

			int seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
			return $"{seconds}s";
		}
	}
}