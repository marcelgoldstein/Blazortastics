using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Blazortastics.Core.Base
{
    /// <summary>
    /// Implements INotifyPropertyChanging/Changed and IDisposable.
    /// Provides virtual methods to hook up. 
    /// Automatically calls 'StateHasChanged' on 'PropertyChanged'.
    /// </summary>
    public abstract class BindableBlazorComponent : BlazorComponent, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            var oldValue = storage;
            var newValue = value;

            if (!this.disposedValue)
            {
                this.OnPropertyChanging(propertyName, oldValue, newValue);
                this.RaiseOnPropertyChangingEvent(propertyName);
            }
            storage = value;
            if (!this.disposedValue)
            {
                this.OnPropertyChanged(propertyName, oldValue, newValue);
                this.RaiseOnPropertyChangedEvent(propertyName);
                this.StateHasChanged();
            }
            return true;
        }

        protected void RaiseOnPropertyChangingEvent([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected void RaiseOnPropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets executed right before the property value gets set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnPropertyChanging<T>(string propertyName, T oldValue, T newValue)
        {

        }

        /// <summary>
        /// Gets executed right after the property value gets set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
        {

        }

        #region IDisposable Support
        protected bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.DisposeManagedCode();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BindableBlazorComponent() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManagedCode()
        {

        }
        #endregion IDisposable Support
    }
}
