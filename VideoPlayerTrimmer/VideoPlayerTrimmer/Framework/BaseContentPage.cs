using Xamarin.Forms;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseContentPage<T> : ContentPage
        where T : BaseViewModel
    {
        private bool isAlreadyInitialized;
        private bool isAlreadyUninitialized;

        protected virtual T ViewModel => BindingContext as T;

        protected override async void OnAppearing()
        {
            App.DebugLog("");
            base.OnAppearing();

            if (!isAlreadyInitialized)
            {
                await ViewModel.InitializeAsync();
                isAlreadyInitialized = true;
            }
        }

        protected override void OnDisappearing()
        {
            App.DebugLog("");
            base.OnDisappearing();

            if (!isAlreadyUninitialized)
            {
                ViewModel.UninitializeAsync();
                isAlreadyUninitialized = true;
            }
        }
    }
}
