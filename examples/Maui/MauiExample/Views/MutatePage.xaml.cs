using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Core.Cadence;
using MauiExample.Helpers;

namespace MauiExample.Views;

public partial class MutatePage : ContentPage
{
    public MutatePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private const string script = @"
transaction(test: String, testInt: Int) {
    prepare(signer: AuthAccount) {
        log(signer.address)
        log(test)
        log(testInt)
    }
}";
    public string MutationScript
    {
        get { 
            return @$"
Script:{script}"; 
        }
    }

    private string progress;
    public string Progress
    {
        get { return progress; }
        set
        {
            progress = value;
            OnPropertyChanged(nameof(Progress));
        }
    }

    public string Params
    {
        get
        {
            return @"
Parameters:
test = ""Hello World""
testInt = 8";
        }
    }    

    private async void Mutate_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Progress = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            var tx = new FclMutation
            {
                Script = script,
                Arguments = new List<ICadence>
                {
                    new CadenceString("Hello World"),
                    new CadenceNumber(CadenceNumberType.Int, "8")
                }
            };

            Progress = "Initializing...";
            var transactionId = await fcl.MutateAsync(tx);

            Progress = "Awaiting Sealing...";
            var result = await fcl.Sdk.WaitForSealAsync(transactionId);

            Progress = !string.IsNullOrEmpty(result.ErrorMessage) ? result.ErrorMessage : "Transaction Sealed!";
            button.IsEnabled = true;
        }        
    }
}