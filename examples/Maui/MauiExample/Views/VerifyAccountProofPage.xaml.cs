using MauiExample.Helpers;

namespace MauiExample.Views;

public partial class VerifyAccountProofPage : ContentPage
{
    public VerifyAccountProofPage()
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

    private async void Verify_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Result = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            var response = await fcl.VerifyAccountProofAsync();

            if (response)
                Result = "Success!";
            else
                Result = "Error!";

            button.IsEnabled = true;
        }        
    }
}