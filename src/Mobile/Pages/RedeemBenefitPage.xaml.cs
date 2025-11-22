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

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.LoadBenefitsCommand.Execute(null);
	}
}
