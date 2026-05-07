using System.Windows.Controls;
using MyPause.Resources;

namespace MyPause.Helpers
{
	public static class UIHelper
	{

		public static void InitializeTimeComboAndText(ComboBox comboBox, TextBox textBox, int seconds)
		{
			comboBox.Items.Clear();
			comboBox.Items.Add(Strings.EditPause_UnitSeconds);
			comboBox.Items.Add(Strings.EditPause_UnitMinutes);
			comboBox.Items.Add(Strings.EditPause_UnitHours);

			var timerDuration = TimeFormatter.ToBestUnit(seconds);
			comboBox.SelectedIndex = (int)timerDuration.Unit;
			textBox.Text = timerDuration.Value.ToString();
		}

		public static int EvaluateTextBoxInt(TextBox textBox, bool doubleDigit = false, int delta = 0, int min = 0, int max = int.MaxValue)
		{
			var oldValue = ParseInt(textBox.Text, min, max);
			var newValue = Math.Clamp(oldValue + delta, min, max);
			textBox.Text = newValue.ToString(doubleDigit ? "D2" : null);
			return newValue;
		}

		public static int EvaluateTextBoxSeconds(TextBox textBox, ComboBox comboBox, int delta = 0, int min = 0, int max = int.MaxValue)
		{
			var newValue = EvaluateTextBoxInt(textBox, false, delta, min, max);
			return TimeFormatter.ToSeconds(newValue, (TimeUnit)comboBox.SelectedIndex);
		}

		private static int ParseInt(string text, int min = 0, int max = int.MaxValue)
		{
			if (!int.TryParse(text, out var value))
				return min;
			if (value < min)
				return min;
			if (value > max)
				return max;
			return value;
		}
	}
}