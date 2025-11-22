using Mobile.Pages;

namespace Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Registrar rutas de navegación
		Routing.RegisterRoute("LoginPage", typeof(LoginPage));
	}
}
