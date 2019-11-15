using Xamarin.Forms;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseContentPage<T> : ContentPage
        where T : BaseViewModel
    {
        private bool firstTimeAppearing = true;
        private bool firstTimeDisappearing = true;

        protected virtual T ViewModel { get; }

        public BaseContentPage()
        {
            App.DebugLog("");
            ViewModel = App.DIContainer.Resolve<T>();
            BindingContext = ViewModel;
            ViewModel.OnNavigating(App.NavigationService.GetNavigationParameters());
        }

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
