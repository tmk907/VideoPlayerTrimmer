using Xamarin.Forms;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseContentPage<T> : ContentPage
        where T : BaseViewModel
    {
        private bool firstTimeAppearing = true;
        private bool firstTimeDisappearing = true;

        protected virtual T ViewModel => BindingContext as T;

        protected override async void OnAppearing()
        {
            App.DebugLog("");
            base.OnAppearing();

            await ViewModel.OnAppearingAsync(firstTimeAppearing);

            if (firstTimeAppearing)
            {
                
                firstTimeAppearing = false;
            }
        }

        protected override void OnDisappearing()
        {
            App.DebugLog("");
            base.OnDisappearing();

            ViewModel.OnDisappearingAsync(firstTimeDisappearing);

            if (firstTimeDisappearing)
            {
                firstTimeDisappearing = true;
            }
        }
    }
}
