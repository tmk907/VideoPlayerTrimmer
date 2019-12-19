using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressRing : ContentView
    {
        private const double FullyOpaque = 1;
        private const double FullyTransparent = 0;
        private const uint TogglingVisibilityAnimationDuration = 500;

        private static readonly SemaphoreSlim ToggleVisibilityAnimationSemaphoreSlim = new SemaphoreSlim(1);
        
        public ProgressRing()
        {
            InitializeComponent();
        }


        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
                                    typeof(string),
                                    typeof(ProgressRing),
                                    default(string));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor),
                                    typeof(Color),
                                    typeof(ProgressRing),
                                    default(Color));
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize),
                                    typeof(double),
                                    typeof(ProgressRing),
                                    18.0);
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color),
                                    typeof(Color),
                                    typeof(ProgressRing),
                                    Color.Blue);
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        public static readonly BindableProperty SizeProperty =
            BindableProperty.Create(nameof(Size),
                                    typeof(double),
                                    typeof(ProgressRing),
                                    50.0);
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }


        public static readonly BindableProperty IsLoadingProperty =
            BindableProperty.Create(nameof(IsLoading),
                                    typeof(bool),
                                    typeof(ProgressRing),
                                    default(bool),
                                    propertyChanged: IsLoadingPropertyChanged);
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        private static async void IsLoadingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ProgressRing progressRing) || !(newValue is bool isLoading))
            {
                return;
            }

            await ToggleVisibility(progressRing);
        }

        private static async Task ToggleVisibility(ProgressRing progressRing)
        {
            try
            {
                ViewExtensions.CancelAnimations(progressRing);

                await ToggleVisibilityAnimationSemaphoreSlim.WaitAsync();
                if (progressRing.IsLoading)
                {
                    progressRing.ActivityIndicator.IsRunning = true;
                    progressRing.IsVisible = true;
                    await progressRing.FadeTo(FullyOpaque, TogglingVisibilityAnimationDuration, Easing.CubicInOut);
                }
                else
                {
                    await progressRing.FadeTo(FullyTransparent, TogglingVisibilityAnimationDuration, Easing.CubicInOut);
                    progressRing.ActivityIndicator.IsRunning = false;
                    progressRing.IsVisible = false;
                }
            }
            finally
            {
                ToggleVisibilityAnimationSemaphoreSlim.Release();
            }
        }
    }
}