using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazortastics.App.Services
{
    public class MessageBoxService
    {
        public async ValueTask Alert(string message)
        {
            await JSRuntime.Current.InvokeAsync<dynamic>("alert", message);
        }

        public async ValueTask<bool> Confirm(string message)
        {
            return await JSRuntime.Current.InvokeAsync<bool>("confirm", message);
        }

        public async ValueTask<string> Prompt(string text, string defaultText = null)
        {
            // remarks: when c# null gets passed to an optional javascript parameter it gets translated into an string of 'null'.

            if (defaultText == null)
            {
                return await JSRuntime.Current.InvokeAsync<string>("prompt", text);
            }
            else
            {
                return await JSRuntime.Current.InvokeAsync<string>("prompt", text, defaultText);
            }
        }
    }
}
