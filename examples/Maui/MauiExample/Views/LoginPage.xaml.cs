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
        var selectedProvider = ((ListView)sender).SelectedItem as FclWalletProvider;

        var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

        if (fcl.User == null || !fcl.User.LoggedIn)
        {
            fcl.SetWalletProvider(new Fcl.Net.Core.Models.FclWalletDiscovery
            {
                Wallet = new Uri(selectedProvider.Endpoint),
                WalletMethod = selectedProvider.Method
            });
            await fcl.AuthenticateAsync();
        }

        if (fcl.User.LoggedIn)
        {
            await Shell.Current.GoToAsync($"//main");
        }
    }
}