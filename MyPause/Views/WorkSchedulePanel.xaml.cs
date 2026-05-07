using MyPause.Helpers;
using MyPause.Models;
using MyPause.Resources;
using System.Windows;
using System.Windows.Controls;

namespace MyPause.Views
{
	/// <summary>
	/// Reusable panel for editing work schedule start/end time.
	/// </summary>
	public partial class WorkSchedulePanel : UserControl
	{
		/// <summary>Shared work schedule instance edited by this panel.</summary>
		public WorkSchedule WorkSchedule { get; }

		/// <summary>Raised when the work schedule values are changed.</summary>
		public event EventHandler? ConfigurationChanged;

		/// <summary>
		/// Initializes the panel with an existing work schedule object.
		/// </summary>
		/// <param name="workSchedule">Schedule instance to edit.</param>
		public WorkSchedulePanel(WorkSchedule workSchedule)
		{
			InitializeComponent();
			WorkSchedule = workSchedule;
		}

		private void OnLoaded(object? sender, RoutedEventArgs e)
		{
			WorkScheduleStartHourBox.Text = WorkSchedule.WorkStart.Hours.ToString("D2");
			WorkScheduleStartMinuteBox.Text = WorkSchedule.WorkStart.Minutes.ToString("D2");
			WorkScheduleEndHourBox.Text = WorkSchedule.WorkEnd.Hours.ToString("D2");
			WorkScheduleEndMinuteBox.Text = WorkSchedule.WorkEnd.Minutes.ToString("D2");

			UIHelper.InitializeTimeComboAndText(CooldownUnitBox, CooldownSecondsBox, WorkSchedule.PauseCooldownSeconds);
		}

		private void FixedRangeStartHourUp_Click(object sender, RoutedEventArgs e)
		{
			int hour = UIHelper.EvaluateTextBoxInt(WorkScheduleStartHourBox, false, 1, 0, 23);
			WorkSchedule?.UpdateWorkStartHour(hour);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeStartHourDown_Click(object sender, RoutedEventArgs e)
		{
			int hour = UIHelper.EvaluateTextBoxInt(WorkScheduleStartHourBox, false, -1, 0, 23);
			WorkSchedule?.UpdateWorkStartHour(hour);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeStartMinuteUp_Click(object sender, RoutedEventArgs e)
		{
			int minute = UIHelper.EvaluateTextBoxInt(WorkScheduleStartMinuteBox, true, 1, 0, 59);
			WorkSchedule?.UpdateWorkStartMinute(minute);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeStartMinuteDown_Click(object sender, RoutedEventArgs e)
		{
			int minute = UIHelper.EvaluateTextBoxInt(WorkScheduleStartMinuteBox, true, -1, 0, 59);
			WorkSchedule?.UpdateWorkStartMinute(minute);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeEndHourUp_Click(object sender, RoutedEventArgs e)
		{
			int hour = UIHelper.EvaluateTextBoxInt(WorkScheduleEndHourBox, false, 1, 0, 23);
			WorkSchedule?.UpdateWorkEndHour(hour);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeEndHourDown_Click(object sender, RoutedEventArgs e)
		{
			int hour = UIHelper.EvaluateTextBoxInt(WorkScheduleEndHourBox, false, -1, 0, 23);
			WorkSchedule?.UpdateWorkEndHour(hour);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeEndMinuteUp_Click(object sender, RoutedEventArgs e)
		{
			int minute = UIHelper.EvaluateTextBoxInt(WorkScheduleEndMinuteBox, true, 1, 0, 59);
			WorkSchedule?.UpdateWorkEndMinute(minute);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeEndMinuteDown_Click(object sender, RoutedEventArgs e)
		{
			int minute = UIHelper.EvaluateTextBoxInt(WorkScheduleEndMinuteBox, true, -1, 0, 59);
			WorkSchedule?.UpdateWorkEndMinute(minute);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void FixedRangeTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (WorkSchedule == null)
				return;

			int startHour = UIHelper.EvaluateTextBoxInt(WorkScheduleStartHourBox, false, 0, 0, 23);
			int startMinute = UIHelper.EvaluateTextBoxInt(WorkScheduleStartMinuteBox, true, 0, 0, 59);
			int endHour = UIHelper.EvaluateTextBoxInt(WorkScheduleEndHourBox, false, 0, 0, 23);
			int endMinute = UIHelper.EvaluateTextBoxInt(WorkScheduleEndMinuteBox, true, 0, 0, 59);
			WorkSchedule.WorkStart = new TimeSpan(startHour, startMinute, 0);
			WorkSchedule.WorkEnd = new TimeSpan(endHour, endMinute, 0);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void CooldownUp_Click(object sender, RoutedEventArgs e)
		{
			WorkSchedule.PauseCooldownSeconds = UIHelper.EvaluateTextBoxSeconds(CooldownSecondsBox, CooldownUnitBox, 1);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void CooldownDown_Click(object sender, RoutedEventArgs e)
		{
			WorkSchedule.PauseCooldownSeconds = UIHelper.EvaluateTextBoxSeconds(CooldownSecondsBox, CooldownUnitBox, -1);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void CooldownTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			WorkSchedule.PauseCooldownSeconds = UIHelper.EvaluateTextBoxSeconds(CooldownSecondsBox, CooldownUnitBox);
			ConfigurationChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
