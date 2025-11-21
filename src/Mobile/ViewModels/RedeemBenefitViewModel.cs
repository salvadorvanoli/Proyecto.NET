using Mobile.Services;
using Mobile.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mobile.ViewModels;

public class RedeemBenefitViewModel : BaseViewModel
{
    private readonly IBenefitService _benefitService;
    private readonly IAuthService _authService;
    
    private ObservableCollection<RedeemableBenefitDto> _benefits = new();
    private RedeemableBenefitDto? _selectedBenefit;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _showConfirmation;
    private string _successMessage = string.Empty;
    private bool _hasSuccess;

    public ObservableCollection<RedeemableBenefitDto> Benefits
    {
        get => _benefits;
        set => SetProperty(ref _benefits, value);
    }

    public RedeemableBenefitDto? SelectedBenefit
    {
        get => _selectedBenefit;
        set => SetProperty(ref _selectedBenefit, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public bool ShowConfirmation
    {
        get => _showConfirmation;
        set => SetProperty(ref _showConfirmation, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public bool HasSuccess
    {
        get => _hasSuccess;
        set => SetProperty(ref _hasSuccess, value);
    }

    public ICommand LoadBenefitsCommand { get; }
    public ICommand SelectBenefitCommand { get; }
    public ICommand ConfirmSelectionCommand { get; }
    public ICommand RedeemBenefitCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BackToListCommand { get; }

    public RedeemBenefitViewModel(IBenefitService benefitService, IAuthService authService)
    {
        _benefitService = benefitService;
        _authService = authService;
        
        LoadBenefitsCommand = new Command(async () => await LoadBenefitsAsync());
        SelectBenefitCommand = new Command<RedeemableBenefitDto>(SelectBenefit);
        ConfirmSelectionCommand = new Command(ConfirmSelection);
        RedeemBenefitCommand = new Command(async () => await RedeemBenefitAsync());
        CancelCommand = new Command(Cancel);
        BackToListCommand = new Command(BackToList);
    }

    private async Task LoadBenefitsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        HasSuccess = false;

        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                ErrorMessage = "Usuario no autenticado";
                HasError = true;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Loading benefits for user {currentUser.UserId}");
            
            var benefits = await _benefitService.GetRedeemableBenefitsAsync(currentUser.UserId);
            
            Benefits.Clear();
            foreach (var benefit in benefits)
            {
                Benefits.Add(benefit);
            }

            System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Loaded {Benefits.Count} benefits");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Error loading benefits: {ex.Message}");
            ErrorMessage = $"Error al cargar beneficios: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectBenefit(RedeemableBenefitDto benefit)
    {
        SelectedBenefit = benefit;
        System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Selected benefit: {benefit.BenefitTypeName}");
    }

    private void ConfirmSelection()
    {
        if (SelectedBenefit == null)
        {
            ErrorMessage = "Por favor selecciona un beneficio";
            HasError = true;
            return;
        }

        HasError = false;
        ShowConfirmation = true;
        System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Showing confirmation for benefit {SelectedBenefit.Id}");
    }

    private async Task RedeemBenefitAsync()
    {
        if (IsBusy) return;

        HasError = false;
        ErrorMessage = string.Empty;

        if (SelectedBenefit == null)
        {
            ErrorMessage = "No hay beneficio seleccionado";
            HasError = true;
            return;
        }

        IsBusy = true;

        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                ErrorMessage = "Usuario no autenticado";
                HasError = true;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Redeeming benefit {SelectedBenefit.BenefitId}");
            
            var result = await _benefitService.RedeemBenefitAsync(
                currentUser.UserId,
                SelectedBenefit.BenefitId
            );

            if (result.Success)
            {
                SuccessMessage = result.Message;
                HasSuccess = true;
                ShowConfirmation = false;
                
                // Recargar beneficios
                await LoadBenefitsAsync();
                
                System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Benefit redeemed successfully");
            }
            else
            {
                ErrorMessage = result.Message;
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RedeemBenefitViewModel] Error redeeming benefit: {ex.Message}");
            ErrorMessage = $"Error: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        ShowConfirmation = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void BackToList()
    {
        SelectedBenefit = null;
        ShowConfirmation = false;
        HasError = false;
        HasSuccess = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
