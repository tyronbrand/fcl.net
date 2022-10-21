using Fcl.Net.Core.Models;
using MauiExample.Helpers;

namespace MauiExample.Views;

public partial class SignUserMessagePage : ContentPage
{
    public SignUserMessagePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private FclCompositeSignature _signature;

    private const string message = "Hello World";
    public string Message
    {
        get { 
            return $"Message: {message}"; 
        }
    }

    private string signResult;
    public string SignResult
    {
        get { return signResult; }
        set
        {
            signResult = value;
            OnPropertyChanged(nameof(SignResult));
        }
    }

    private string verifyResult;
    public string VerifyResult
    {
        get { return verifyResult; }
        set
        {
            verifyResult = value;
            OnPropertyChanged(nameof(VerifyResult));
        }
    }

    private async void Sign_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            SignResult = "";
            VerifyResult = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            try
            {
                _signature = await fcl.SignUserMessageAsync(message);
                SignResult = "Message signed!";
            }
            catch (Exception)
            {
                SignResult = "Signing failed!";
            }

            button.IsEnabled = true;
        }
    }

    private async void Verify_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            VerifyResult = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            if (_signature != null)
            {
                try
                {
                    var verified = await fcl.VerifyUserSignatureAsync(message, new List<FclCompositeSignature> { _signature });

                    if(verified)
                        VerifyResult = "Verify successful!";
                    else
                        VerifyResult = "Verify failed!";
                }
                catch (Exception)
                {
                    VerifyResult = "Verify failed!";
                    button.IsEnabled = true;
                }
            }

            button.IsEnabled = true;
        }
    }    
}