using System.Globalization;
using System.Resources;

namespace MyPause.Resources;

public static class Strings
{
	private static readonly ResourceManager ResourceManager = new("MyPause.Resources.Strings", typeof(Strings).Assembly);

	private static string Get(string key) => ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

	public static string MainWindow_AppTitle => Get(nameof(MainWindow_AppTitle));
	public static string MainWindow_ConfiguredBreaksHeader => Get(nameof(MainWindow_ConfiguredBreaksHeader));
	public static string MainWindow_NewBreakButton => Get(nameof(MainWindow_NewBreakButton));
	public static string MainWindow_StatusConfiguredBreaksDefault => Get(nameof(MainWindow_StatusConfiguredBreaksDefault));
	public static string MainWindow_StartupWithWindows => Get(nameof(MainWindow_StartupWithWindows));
	public static string MainWindow_MinimizeToTray => Get(nameof(MainWindow_MinimizeToTray));
	public static string MainWindow_Start => Get(nameof(MainWindow_Start));
	public static string MainWindow_Stop => Get(nameof(MainWindow_Stop));

	public static string MainWindow_UserIdlePaused => Get(nameof(MainWindow_UserIdlePaused));
	public static string MainWindow_UserActiveRestarted => Get(nameof(MainWindow_UserActiveRestarted));
	public static string MainWindow_AppActiveMonitoring => Get(nameof(MainWindow_AppActiveMonitoring));
	public static string MainWindow_AppStopped => Get(nameof(MainWindow_AppStopped));
	public static string MainWindow_StartupEnabled => Get(nameof(MainWindow_StartupEnabled));
	public static string MainWindow_StartupDisabled => Get(nameof(MainWindow_StartupDisabled));
	public static string MainWindow_StatusConfiguredBreaksFormat(int count) => string.Format(CultureInfo.CurrentCulture, Get(nameof(MainWindow_StatusConfiguredBreaksFormat)), count);
	public static string MainWindow_AlertUpdatedFormat(string name) => string.Format(CultureInfo.CurrentCulture, Get(nameof(MainWindow_AlertUpdatedFormat)), name);
	public static string MainWindow_AlertDeletedFormat(string name) => string.Format(CultureInfo.CurrentCulture, Get(nameof(MainWindow_AlertDeletedFormat)), name);
	public static string MainWindow_AlertAddedFormat(string name) => string.Format(CultureInfo.CurrentCulture, Get(nameof(MainWindow_AlertAddedFormat)), name);
	public static string MainWindow_StartupUpdateErrorFormat(string error) => string.Format(CultureInfo.CurrentCulture, Get(nameof(MainWindow_StartupUpdateErrorFormat)), error);
	public static string Common_InfoTitle => Get(nameof(Common_InfoTitle));
	public static string Common_ErrorTitle => Get(nameof(Common_ErrorTitle));

	public static string EditPause_Title => Get(nameof(EditPause_Title));
	public static string EditPause_Active => Get(nameof(EditPause_Active));
	public static string EditPause_Name => Get(nameof(EditPause_Name));
	public static string EditPause_Days => Get(nameof(EditPause_Days));
	public static string EditPause_DayMon => Get(nameof(EditPause_DayMon));
	public static string EditPause_DayTue => Get(nameof(EditPause_DayTue));
	public static string EditPause_DayWed => Get(nameof(EditPause_DayWed));
	public static string EditPause_DayThu => Get(nameof(EditPause_DayThu));
	public static string EditPause_DayFri => Get(nameof(EditPause_DayFri));
	public static string EditPause_DaySat => Get(nameof(EditPause_DaySat));
	public static string EditPause_DaySun => Get(nameof(EditPause_DaySun));
	public static string EditPause_TabFixedTime => Get(nameof(EditPause_TabFixedTime));
	public static string EditPause_Time => Get(nameof(EditPause_Time));
	public static string EditPause_TabTimer => Get(nameof(EditPause_TabTimer));
	public static string EditPause_Every => Get(nameof(EditPause_Every));
	public static string EditPause_Mandatory => Get(nameof(EditPause_Mandatory));
	public static string EditPause_PauseDuration => Get(nameof(EditPause_PauseDuration));
	public static string EditPause_Snooze => Get(nameof(EditPause_Snooze));
	public static string EditPause_SnoozeDuration => Get(nameof(EditPause_SnoozeDuration));
	public static string EditPause_MaxSnoozes => Get(nameof(EditPause_MaxSnoozes));
	public static string EditPause_NotificationSound => Get(nameof(EditPause_NotificationSound));
	public static string EditPause_NoSoundSelected => Get(nameof(EditPause_NoSoundSelected));
	public static string EditPause_SelectAudioFile => Get(nameof(EditPause_SelectAudioFile));
	public static string EditPause_ResetDefault => Get(nameof(EditPause_ResetDefault));
	public static string EditPause_Cancel => Get(nameof(EditPause_Cancel));
	public static string EditPause_Save => Get(nameof(EditPause_Save));
	public static string EditPause_UnitSeconds => Get(nameof(EditPause_UnitSeconds));
	public static string EditPause_UnitMinutes => Get(nameof(EditPause_UnitMinutes));
	public static string EditPause_UnitHours => Get(nameof(EditPause_UnitHours));
	public static string EditPause_NoSoundSelectedWindowsAlert => Get(nameof(EditPause_NoSoundSelectedWindowsAlert));
	public static string EditPause_SoundFileNotFound => Get(nameof(EditPause_SoundFileNotFound));
	public static string EditPause_OpenFileFilter => Get(nameof(EditPause_OpenFileFilter));
	public static string EditPause_OpenFileTitle => Get(nameof(EditPause_OpenFileTitle));
	public static string EditPause_ErrorPauseDurationPositive => Get(nameof(EditPause_ErrorPauseDurationPositive));
	public static string EditPause_ErrorTimerPositive => Get(nameof(EditPause_ErrorTimerPositive));
	public static string EditPause_ErrorSnoozePositive => Get(nameof(EditPause_ErrorSnoozePositive));
	public static string EditPause_SoundFileFormat(string fileName) => string.Format(CultureInfo.CurrentCulture, Get(nameof(EditPause_SoundFileFormat)), fileName);
	public static string EditPause_ErrorInputDataFormat(string error) => string.Format(CultureInfo.CurrentCulture, Get(nameof(EditPause_ErrorInputDataFormat)), error);
	public static string EditPause_ErrorNameEmpty => Get(nameof(EditPause_ErrorNameEmpty));
	public static string EditPause_ErrorNameDuplicate => Get(nameof(EditPause_ErrorNameDuplicate));

	public static string PauseNotification_Title => Get(nameof(PauseNotification_Title));
	public static string PauseNotification_Header => Get(nameof(PauseNotification_Header));
	public static string PauseNotification_Skip => Get(nameof(PauseNotification_Skip));
	public static string PauseNotification_Snooze => Get(nameof(PauseNotification_Snooze));
	public static string PauseNotification_DurationFormat(string duration) => string.Format(CultureInfo.CurrentCulture, Get(nameof(PauseNotification_DurationFormat)), duration);
	public static string PauseNotification_SnoozeButtonFormat(string duration) => string.Format(CultureInfo.CurrentCulture, Get(nameof(PauseNotification_SnoozeButtonFormat)), duration);
	public static string PauseNotification_SnoozeAvailableFormat(int remaining, int max) => string.Format(CultureInfo.CurrentCulture, Get(nameof(PauseNotification_SnoozeAvailableFormat)), remaining, max);

	public static string WorkSchedule_Label => Get(nameof(WorkSchedule_Label));
	public static string WorkSchedule_From => Get(nameof(WorkSchedule_From));
	public static string WorkSchedule_To => Get(nameof(WorkSchedule_To));
	public static string WorkSchedule_Cooldown_Label => Get(nameof(WorkSchedule_Cooldown_Label));
	public static string WorkSchedule_Cooldown_Unit => Get(nameof(WorkSchedule_Cooldown_Unit));

	public static string AlertCard_ConfirmTitle => Get(nameof(AlertCard_ConfirmTitle));
	public static string AlertCard_DescriptionFixedTimeFormat(int hour, int minute, string pause) => string.Format(CultureInfo.CurrentCulture, Get(nameof(AlertCard_DescriptionFixedTimeFormat)), hour, minute, pause);
	public static string AlertCard_DescriptionTimerFormat(string timer, string pause) => string.Format(CultureInfo.CurrentCulture, Get(nameof(AlertCard_DescriptionTimerFormat)), timer, pause);
	public static string AlertCard_DeleteConfirmFormat(string name) => string.Format(CultureInfo.CurrentCulture, Get(nameof(AlertCard_DeleteConfirmFormat)), name);

	public static string TrayMenu_Open => Get(nameof(TrayMenu_Open));
	public static string TrayMenu_Exit => Get(nameof(TrayMenu_Exit));

	public static string Alerts_Default_Morning => Get(nameof(Alerts_Default_Morning));
	public static string Alerts_Default_Afternoon => Get(nameof(Alerts_Default_Afternoon));
	public static string Alerts_Default_HourlyTimer => Get(nameof(Alerts_Default_HourlyTimer));

	public static string Duration_LongHour(bool singular) => singular ? Get("Duration_LongHourSingular") : Get("Duration_LongHourPlural");
	public static string Duration_LongMinute(bool singular) => singular ? Get("Duration_LongMinuteSingular") : Get("Duration_LongMinutePlural");
	public static string Duration_LongSecond(bool singular) => singular ? Get("Duration_LongSecondSingular") : Get("Duration_LongSecondPlural");
	public static string Duration_Day(bool singular) => singular ? Get("Duration_DaySingular") : Get("Duration_DayPlural");
}
