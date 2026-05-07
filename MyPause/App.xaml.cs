using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace MyPause;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		// Debug: Allow temporary language override via environment variable
		// Usage: $env:MYPAUSE_LANG = "ar-SA"; dotnet run
		// if (Environment.GetEnvironmentVariable("MYPAUSE_LANG") is string debugLang)
		// {
		// 	try
		// 	{
		// 		Thread.CurrentThread.CurrentUICulture = new CultureInfo(debugLang);
		// 		CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(debugLang);
		// 	}
		// 	catch { /* Invalid culture code, proceed with OS default */ }
		// }

		ApplyCultureUiSettings();
		base.OnStartup(e);
	}

	private static void ApplyCultureUiSettings()
	{
		var uiCulture = CultureInfo.CurrentUICulture;
		FrameworkElement.LanguageProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(uiCulture.IetfLanguageTag)));

		var flowDirection = uiCulture.TextInfo.IsRightToLeft
			? FlowDirection.RightToLeft
			: FlowDirection.LeftToRight;

		FrameworkElement.FlowDirectionProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(flowDirection));
	}

	/// <summary>
	/// Helper method to dynamically switch UI culture and reapply RTL settings.
	/// Used for testing different languages without restarting the app.
	/// Example: App.SwitchUICulture("ar-SA") for Arabic testing.
	/// </summary>
	public static void SwitchUICulture(string cultureCode)
	{
		var newCulture = new CultureInfo(cultureCode);
		Thread.CurrentThread.CurrentUICulture = newCulture;
		Thread.CurrentThread.CurrentCulture = newCulture;
		CultureInfo.DefaultThreadCurrentUICulture = newCulture;
		CultureInfo.DefaultThreadCurrentCulture = newCulture;

		// Reapply RTL metadata
		ApplyCultureUiSettings();
	}
}