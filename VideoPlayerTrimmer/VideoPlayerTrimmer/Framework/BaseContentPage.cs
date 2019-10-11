using Xamarin.Forms;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseContentPage<T> : ContentPage
        where T : BaseViewModel
    {
        private bool firstTimeAppearing = true;
        private bool firstTimeDisappearing = true;

        protected virtual T ViewModel => BindingContext as T;

        protected override void OnAppearing()
        {
            App.DebugLog("");
            base.OnAppearing();

            ViewModel.OnAppearing(firstTimeAppearing);

            if (firstTimeAppearing)
            {
                
                firstTimeAppearing = false;
            }
        }

        protected override void OnDisappearing()
        {
            App.DebugLog("");
            base.OnDisappearing();

            ViewModel.OnDisappearing(firstTimeDisappearing);

            if (firstTimeDisappearing)
            {
                firstTimeDisappearing = true;
            }
        }
    }
}
