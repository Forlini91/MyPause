using MyPause.Models;
using MyPause.Helpers;
using MyPause.Resources;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;

namespace MyPause.Views
{
	/// <summary>
	/// Modal editor window used to create or update an alert configuration.
	/// </summary>
	public partial class EditPauseWindow : Window
	{
		/// <summary>Configuration being edited by this window.</summary>
		public AlertConfig Config { get; private set; }
		private bool _edit;
		private Predicate<string> _nameValidator;
		private string _origName;

		/// <summary>
		/// Initializes a new editor window from an existing alert configuration.
		/// </summary>
		/// <param name="config">Source configuration to clone and edit.</param>
		public EditPauseWindow(AlertConfig config, bool edit, Predicate<string> nameValidator)
		{
			InitializeComponent();
			_nameValidator = nameValidator;
			_edit = edit;
			_origName = config.Name;
			Config = config;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"[EditPauseWindow] Loaded for alert: {Config.Name}");

			ScheduleTabControl.SelectedIndex = Config.Type == AlertType.FixedTime ? 0 : 1;

			IsActiveCheckBox.IsChecked = Config.IsActive;
			NameTextBox.Text = Config.Name;

			LunCheckBox.IsChecked = Config.ActiveDays.Contains(1);
			MarCheckBox.IsChecked = Config.ActiveDays.Contains(2);
			MerCheckBox.IsChecked = Config.ActiveDays.Contains(3);
			GioCheckBox.IsChecked = Config.ActiveDays.Contains(4);
			VenCheckBox.IsChecked = Config.ActiveDays.Contains(5);
			SabCheckBox.IsChecked = Config.ActiveDays.Contains(6);
			DomCheckBox.IsChecked = Config.ActiveDays.Contains(0);

			FixedTimeHourBox.Text = Config.FixedTimeHour.ToString();
			FixedTimeMinuteBox.Text = Config.FixedTimeMinute.ToString("D2");

			ResetTimerForEveryPauseCheckBox.IsChecked = Config.ResetTimerForEveryPause;
			MandatoryPauseCheckBox.IsChecked = Config.SnoozeConfig.MandatoryPause;
			MaxSnoozeCountTextBox.Text = Config.SnoozeConfig.MaxSnoozeCount.ToString();

			UIHelper.InitializeTimeComboAndText(TimerUnitBox, TimerValueTextBox, Config.TimerSeconds);
			UIHelper.InitializeTimeComboAndText(PauseDurationUnitBox, PauseDurationTextBox, Config.PauseDurationSeconds);
			UIHelper.InitializeTimeComboAndText(SnoozeUnitBox, SnoozeMinutesTextBox, Config.SnoozeConfig.SnoozeSeconds);

			UpdateSoundLabel();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"[EditPauseWindow] Unloaded for alert: {Config.Name}");
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Debug.WriteLine($"[EditPauseWindow] Escape key pressed for alert: {Config.Name}");
				DialogResult = false;
				Close();
				e.Handled = true;
			}
		}

		private void HourUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(FixedTimeHourBox, false, 1, 0, 23);
		}

		private void HourDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(FixedTimeHourBox, false, -1, 0, 23);
		}

		private void MinuteUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(FixedTimeMinuteBox, true, 1, 0, 59);
		}

		private void MinuteDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(FixedTimeMinuteBox, true, -1, 0, 59);
		}

		private void TimerValueUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(TimerValueTextBox, false, 1, 1);
		}

		private void TimerValueDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(TimerValueTextBox, false, -1, 1);
		}

		private void PauseDurationUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(PauseDurationTextBox, false, 1, 1);
		}

		private void PauseDurationDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(PauseDurationTextBox, false, -1, 1);
		}

		private void SnoozeMinutesUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(SnoozeMinutesTextBox, false, 1, 1);
		}

		private void SnoozeMinutesDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(SnoozeMinutesTextBox, false, -1, 1);
		}

		private void MaxSnoozeCountUp_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(MaxSnoozeCountTextBox, false, 1, 1);
		}

		private void MaxSnoozeCountDown_Click(object sender, RoutedEventArgs e)
		{
			UIHelper.EvaluateTextBoxInt(MaxSnoozeCountTextBox, false, -1, 1);
		}

		private void UpdateSoundLabel()
		{
			if (string.IsNullOrEmpty(Config.NotificationSoundPath))
			{
				SoundFileLabel.Text = Strings.EditPause_NoSoundSelectedWindowsAlert;
			}
			else if (File.Exists(Config.NotificationSoundPath))
			{
				SoundFileLabel.Text = Strings.EditPause_SoundFileFormat(Path.GetFileName(Config.NotificationSoundPath));
			}
			else
			{
				SoundFileLabel.Text = Strings.EditPause_SoundFileNotFound;
			}
		}

		private void SelectSoundButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Filter = Strings.EditPause_OpenFileFilter,
				Title = Strings.EditPause_OpenFileTitle
			};

			if (openFileDialog.ShowDialog() == true)
			{
				Config.NotificationSoundPath = openFileDialog.FileName;
				UpdateSoundLabel();
				Debug.WriteLine($"[EditPauseWindow] Sound selected: {openFileDialog.FileName}");
			}
		}

		private void ClearSoundButton_Click(object sender, RoutedEventArgs e)
		{
			Config.NotificationSoundPath = null;
			UpdateSoundLabel();
			Debug.WriteLine($"[EditPauseWindow] Sound reset to default for alert: {Config.Name}");
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// Validate pause name
			if (string.IsNullOrWhiteSpace(NameTextBox.Text))
			{
				MessageBox.Show(Strings.EditPause_ErrorNameEmpty, Strings.Common_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			else if (!_edit || NameTextBox.Text != _origName)
			{
				if (!_nameValidator(NameTextBox.Text))
				{
					MessageBox.Show(Strings.EditPause_ErrorNameDuplicate, Strings.Common_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			Debug.WriteLine($"[EditPauseWindow] Save clicked for alert: {NameTextBox.Text}");
			try
			{
				Config.IsActive = IsActiveCheckBox.IsChecked == true;
				Config.Name = NameTextBox.Text;

				Config.ActiveDays.Clear();
				if (LunCheckBox.IsChecked == true) Config.ActiveDays.Add(1);
				if (MarCheckBox.IsChecked == true) Config.ActiveDays.Add(2);
				if (MerCheckBox.IsChecked == true) Config.ActiveDays.Add(3);
				if (GioCheckBox.IsChecked == true) Config.ActiveDays.Add(4);
				if (VenCheckBox.IsChecked == true) Config.ActiveDays.Add(5);
				if (SabCheckBox.IsChecked == true) Config.ActiveDays.Add(6);
				if (DomCheckBox.IsChecked == true) Config.ActiveDays.Add(0);

				Config.Type = ScheduleTabControl.SelectedIndex == 0 ? AlertType.FixedTime : AlertType.Timer;
				Config.FixedTimeHour = UIHelper.EvaluateTextBoxInt(FixedTimeHourBox, false, 0, 0, 23);
				Config.FixedTimeMinute = UIHelper.EvaluateTextBoxInt(FixedTimeMinuteBox, true, 0, 0, 59);
				Config.TimerSeconds = UIHelper.EvaluateTextBoxSeconds(TimerValueTextBox, TimerUnitBox, 0, 1);
				Config.ResetTimerForEveryPause = ResetTimerForEveryPauseCheckBox.IsChecked == true;
				Config.PauseDurationSeconds = UIHelper.EvaluateTextBoxSeconds(PauseDurationTextBox, PauseDurationUnitBox, 0, 1);
				Config.SnoozeConfig.SnoozeSeconds = UIHelper.EvaluateTextBoxSeconds(SnoozeMinutesTextBox, SnoozeUnitBox, 0, 1);
				Config.SnoozeConfig.MaxSnoozeCount = UIHelper.EvaluateTextBoxInt(MaxSnoozeCountTextBox);
				Config.SnoozeConfig.MandatoryPause = MandatoryPauseCheckBox.IsChecked == true;


				DialogResult = true;
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Strings.EditPause_ErrorInputDataFormat(ex.Message), Strings.Common_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine($"[EditPauseWindow] Cancel clicked for alert: {Config.Name}");
			DialogResult = false;
			Close();
		}
	}
}
