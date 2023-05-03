using MauiExample.Helpers;
using Newtonsoft.Json;

namespace MauiExample.Views;

public partial class DiscoveryPage : ContentPage
{
    public DiscoveryPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private string result;
    public string Result
    {
        get { return result; }
        set
        {
            result = value;
            OnPropertyChanged(nameof(Result));
        }
    }

    private async void Discovery_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Result = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            var response = await fcl.DiscoveryServicesAsync();

            Result = JsonConvert.SerializeObject(response, Formatting.Indented);
            button.IsEnabled = true;
        }        
    }
}