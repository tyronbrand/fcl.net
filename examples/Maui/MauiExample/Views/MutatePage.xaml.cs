using Fcl.Net.Core.Models;
using MauiExample.Helpers;

namespace MauiExample.Views;

public partial class MutatePage : ContentPage
{
	public MutatePage()
	{
		InitializeComponent();
	}

    private async void Mutate_Clicked(object sender, EventArgs e)
    {
        var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

        var tx = new FclMutation
        {
            Script = "transaction() { prepare(signer: AuthAccount) { log(signer.address) } }"
        };
        
        var transactionId = await fcl.MutateAsync(tx);        
        var result = await fcl.Sdk.WaitForSealAsync(transactionId);
    }
}