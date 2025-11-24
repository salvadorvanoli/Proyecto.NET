using Mobile.ViewModels;

namespace Mobile.Pages;

public partial class RedeemBenefitPage : ContentPage
{
	private readonly RedeemBenefitViewModel _viewModel;

	public RedeemBenefitPage(RedeemBenefitViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.InitializeAsync();
	}
}
