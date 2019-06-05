using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    public class FoldersContentPage : BaseContentPage<FoldersViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FoldersPage : FoldersContentPage
    {
        public FoldersPage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, true);
        }

        private void AutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            ViewModel.Search(((AutoSuggestBox)sender).Text);
            ((AutoSuggestBox)sender).ItemsSource = ViewModel.Results.ToList();
        }

        private async void AutoSuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                await ViewModel.OnVideoSelected(e.ChosenSuggestion);
            }
            else
            {
                await ViewModel.OnVideoSelected(e.QueryText);
            }
            ((AutoSuggestBox)sender).Text = "";
        }

        private async void SearchButton_Clicked(object sender, EventArgs e)
        {
            await ShowSearchBox();
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            ((AutoSuggestBox)sender).Text = "";
            await HideSearchBox();
        }

        private async Task ShowSearchBox()
        {
            var a1 = SearchBox.FadeTo(1,200,Easing.SpringIn);
            var a2 = SearchButton.FadeTo(0, 200, Easing.SpringIn);
            var a3 = CancelButton.FadeTo(1, 200, Easing.SpringIn);
            await Task.WhenAll(a1, a2, a3);
            SearchBox.IsVisible = true;
            SearchButton.IsVisible = false;
            CancelButton.IsVisible = true;
        }

        private async Task HideSearchBox()
        {
            var a1 = SearchBox.FadeTo(0, 200, Easing.SpringOut);
            var a2 = SearchButton.FadeTo(1, 200, Easing.SpringOut);
            var a3 = CancelButton.FadeTo(0, 200, Easing.SpringOut);
            await Task.WhenAll(a1, a2, a3);
            SearchBox.IsVisible = false;
            SearchButton.IsVisible = true;
            CancelButton.IsVisible = false;
        }

        private async void SearchBox_Unfocused(object sender, FocusEventArgs e)
        {
            await HideSearchBox();
        }
    }
}