using Mobile.Data;

namespace Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App(ILocalDatabase database)
	{
		InitializeComponent();

		MainPage = new AppShell();
		
		// Initialize database asynchronously
		Task.Run(async () => 
		{
			try
			{
				await database.InitializeDatabaseAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DB init error: {ex.Message}");
			}
		});
	}
}
