using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using MauiExample.Helpers;

namespace MauiExample.Views;

public partial class QueryPage : ContentPage
{
    public QueryPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private const string script = @"
pub fun main(a: Int): Int {
    return a + 10
}";
    public string QueryScript
    {
        get { 
            return @$"
Script:{script}"; 
        }
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

    public string Params
    {
        get
        {
            return @"
Parameters:
a = 5";
        }
    }    

    private async void Query_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Result = "";
            button.IsEnabled = false;

            var fcl = ServiceHelper.GetService<Fcl.Net.Core.Fcl>();

            var arguments = new List<ICadence>
            {
                new CadenceNumber(CadenceNumberType.Int, "5")
            };

            var response = await fcl.QueryAsync(
                new FlowScript
                {
                    Script = script,
                    Arguments = arguments
                });

            Result = $"SignResult: {response.As<CadenceNumber>().Value}";
            button.IsEnabled = true;
        }        
    }
}