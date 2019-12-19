using System;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    public class TrimmerContentPage : BaseContentPage<TrimmerViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrimmerPage : TrimmerContentPage
    {
        public TrimmerPage()
        {
            InitializeComponent();
            //SizeChanged += TrimmerPage_SizeChanged;
        }

        //private void TrimmerPage_SizeChanged(object sender, EventArgs e)
        //{
        //    bool isPortrait = this.Height > this.Width;
        //    if (isPortrait)
        //    {
        //        flexLayout.Direction = FlexDirection.Column;
        //        videoView.HeightRequest = 200;
        //        videoView.WidthRequest = Width;
        //    }
        //    else
        //    {
        //        flexLayout.Direction = FlexDirection.Row;
        //        videoView.HeightRequest = Height / 2;
        //        videoView.WidthRequest = Width / 2;
        //    }
        //}


        private async void Open_Clicked(object sender, EventArgs e)
        {
            await ViewModel.ChooseVideoAsync();
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            await ViewModel.ConvertVideoAsync();
        }
    }
}