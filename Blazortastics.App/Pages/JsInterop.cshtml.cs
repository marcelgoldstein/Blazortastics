using Blazortastics.App.Services;
using Blazortastics.Core.Base;
using Microsoft.AspNetCore.Blazor.Components;

namespace Blazortastics.App.Pages
{
    public class JsInteropModel : BindableBlazorComponent
    {
        #region Injects
        [Inject] protected MessageBoxService MsgService { get; set; }
        #endregion Injects

        #region Properties
        private string messageResult;
        protected string MessageResult
        {
            get { return messageResult; }
            set { this.SetProperty(ref this.messageResult, value); }
        }
        #endregion Properties

        #region Methods
        protected async void Alert()
        {
            this.MessageResult = null;
            await this.MsgService.Alert("alert message");
        }

        protected async void Confirm()
        {
            this.MessageResult = null;
            var r = await this.MsgService.Confirm("confirm message");
            this.MessageResult = $"u clicked '{(r ? "ok" : "cancel")}'";
        }

        protected async void Prompt()
        {
            this.MessageResult = null;
            var r = await this.MsgService.Prompt("prompt message");
            var btnResult = (r != null);
            this.MessageResult = $"u clicked '{(btnResult ? "ok" : "cancel")}'{(string.IsNullOrEmpty(r) ? string.Empty : $" and inputted '{r}'")}";
        }
        #endregion Methods
    }
}