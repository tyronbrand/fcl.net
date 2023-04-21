using Fcl.Net.Maui.WalletProviders;
using MauiExample.Helpers;
using System.Collections.ObjectModel;

namespace MauiExample.Views;

public partial class LoginPage : ContentPage
{
    ObservableCollection<FclWalletProvider> providers = new();
    public ObservableCollection<FclWalletProvider> Providers { get { return providers; } }

    public LoginPage()
    {
        InitializeComponent();

        LoginView.ItemsSource = providers;
        var walletProviders = WalletProviders.Providers(chainId: Fcl.Net.Core.ChainId.Testnet);
        foreach (var provider in walletProviders)
            providers.Add(provider);
    }

    async void ProviderSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

        if (fcl.User == null || !fcl.User.LoggedIn)
        {
            var discoveryServices = await fcl.DiscoveryServicesAsync();

            if(discoveryServices == null || !discoveryServices.Any())
                throw new Exception("Failed to find any services");

            // selecting blocto for example only
            var authnService = discoveryServices.FirstOrDefault(f => f.Uid == "blocto#authn") ?? throw new Exception("Failed to find blocto service");
            
            await fcl.AuthenticateAsync(authnService);
        }

        if (fcl.User.LoggedIn)
        {
            await Shell.Current.GoToAsync($"//main");
        }
    }
}