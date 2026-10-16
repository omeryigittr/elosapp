namespace ElosApp;

public partial class MainPage : ContentPage
{
	private bool _permissionsRequested;

	public MainPage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (_permissionsRequested)
			return;

		_permissionsRequested = true;
		await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		await Permissions.RequestAsync<Permissions.Camera>();
		await Permissions.RequestAsync<Permissions.Photos>();
	}
}
