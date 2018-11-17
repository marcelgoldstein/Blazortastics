using Microsoft.JSInterop;
using System;

namespace Blazortastics.App.Services
{
    public class PlacementCheckService
    {
        /// <summary>
        /// Detects whether the app is hosted directly in the browser or embedded in another.
        /// </summary>
        /// <returns>true if embedded in another site, else false.</returns>
        public static bool IsEmbedded()
        {
            try
            {
                return (JSRuntime.Current as IJSInProcessRuntime).Invoke<bool>("isEmbedded");
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
