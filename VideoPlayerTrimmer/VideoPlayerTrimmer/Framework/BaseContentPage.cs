﻿using Xamarin.Forms;

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
            App.DebugLog(this.GetType().ToString());
            ViewModel = App.DIContainer.Resolve<T>();
            BindingContext = ViewModel;
            ViewModel.OnNavigating(App.NavigationService.GetNavigationParameters());
        }

        protected override void OnAppearing()
        {
            App.DebugLog(this.GetType().ToString());

            base.OnAppearing();

            ViewModel.OnAppearing(firstTimeAppearing);

            if (firstTimeAppearing)
            {
                firstTimeAppearing = false;
            }
        }

        protected override void OnDisappearing()
        {
            App.DebugLog(this.GetType().ToString());
            base.OnDisappearing();

            ViewModel.OnDisappearing(firstTimeDisappearing);

            if (firstTimeDisappearing)
            {
                firstTimeDisappearing = true;
            }
        }
    }
}
