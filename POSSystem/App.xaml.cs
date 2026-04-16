using POSSystem.Data;

using System.Windows;

namespace POSSystem
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			POSDbContext.Initialize();
        }
    }

}
