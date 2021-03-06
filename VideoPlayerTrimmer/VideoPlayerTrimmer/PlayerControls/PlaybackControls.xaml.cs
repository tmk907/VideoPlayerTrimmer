﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.PlayerControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaybackControls : ContentView
    {
        private const string EpPlayIcon = "\ue864";
        private const string EpPauseIcon = "\ue863";
        private const string EpHeart = "\ue8bf";
        private const string EpHeartOutlined = "\ue8c0";

        private const string FasPlay = "\uf04b";
        private const string FasPause = "\uf04c";

        private const string FasHeart = "\uf004";
        private const string FarHeart = "\uf004";

        private const string FontAwesomeRegular = "FontAwesome5Regular.otf#Regular";
        private const string FontAwesomeSolid = "FontAwesome5Solid.otf#Regular";

        private bool isSliderDragging = false;


        public PlaybackControls()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty IsFavoriteProperty =
            BindableProperty.Create(nameof(IsFavorite),
                                    typeof(bool),
                                    typeof(PlaybackControls),
                                    default(bool),
                                    propertyChanged: IsFavoritePropertyChanged);
        public bool IsFavorite
        {
            get { return (bool)GetValue(IsFavoriteProperty); }
            set { SetValue(IsFavoriteProperty, value); }
        }

        private static void IsFavoritePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)newValue)
            {
                ((PlaybackControls)bindable).FavoriteIconFontFamily = FontAwesomeSolid;
                ((PlaybackControls)bindable).FavoriteIcon = FasHeart;
            }
            else
            {
                ((PlaybackControls)bindable).FavoriteIconFontFamily = FontAwesomeRegular;
                ((PlaybackControls)bindable).FavoriteIcon = FarHeart;
            }
        }


        public static readonly BindableProperty FavoriteIconFontFamilyProperty =
            BindableProperty.Create(nameof(FavoriteIconFontFamily),
                                    typeof(string),
                                    typeof(PlaybackControls),
                                    FontAwesomeRegular);
        public string FavoriteIconFontFamily
        {
            get { return (string)GetValue(FavoriteIconFontFamilyProperty); }
            set { SetValue(FavoriteIconFontFamilyProperty, value); }
        }

        public static readonly BindableProperty FavoriteIconProperty =
            BindableProperty.Create(nameof(FavoriteIcon),
                                    typeof(string),
                                    typeof(PlaybackControls),
                                    FarHeart);
        public string FavoriteIcon
        {
            get { return (string)GetValue(FavoriteIconProperty); }
            set { SetValue(FavoriteIconProperty, value); }
        }

        public static readonly BindableProperty IsPlayingProperty =
            BindableProperty.Create(nameof(IsPlaying),
                                    typeof(bool),
                                    typeof(PlaybackControls),
                                    default(bool),
                                    propertyChanged: IsPlayingPropertyChanged);
        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        private static void IsPlayingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)newValue)
            {
                ((PlaybackControls)bindable).PlayIcon = FasPause;
            }
            else
            {
                ((PlaybackControls)bindable).PlayIcon = FasPlay;
            }
        }


        public static readonly BindableProperty PlayIconProperty =
            BindableProperty.Create(nameof(PlayIcon),
                                    typeof(string),
                                    typeof(PlaybackControls),
                                    FasPlay);
        public string PlayIcon
        {
            get { return (string)GetValue(PlayIconProperty); }
            set { SetValue(PlayIconProperty, value); }
        }

        public static readonly BindableProperty ElapsedTimeProperty =
            BindableProperty.Create(nameof(ElapsedTime),
                                    typeof(TimeSpan),
                                    typeof(PlaybackControls),
                                    default(TimeSpan),
                                    propertyChanged: ElapsedTimePropertyChanged);
        public TimeSpan ElapsedTime
        {
            get { return (TimeSpan)GetValue(ElapsedTimeProperty); }
            set { SetValue(ElapsedTimeProperty, value); }
        }

        private static void ElapsedTimePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!((PlaybackControls)bindable).isSliderDragging)
            {
                var elapsedTime = ((PlaybackControls)bindable).ElapsedTime;
                var totalTime = ((PlaybackControls)bindable).TotalTime;
                var sliderMaxValue = ((PlaybackControls)bindable).SliderMaxValue;
                var progress = elapsedTime.TotalMilliseconds / totalTime.TotalMilliseconds * sliderMaxValue;
                ((PlaybackControls)bindable).SliderValue = progress;

            }
        }

        public static readonly BindableProperty TotalTimeProperty =
            BindableProperty.Create(nameof(TotalTime),
                                    typeof(TimeSpan),
                                    typeof(PlaybackControls),
                                    default(TimeSpan));
        public TimeSpan TotalTime
        {
            get { return (TimeSpan)GetValue(TotalTimeProperty); }
            set { SetValue(TotalTimeProperty, value); }
        }


        public static readonly BindableProperty SliderValueProperty =
            BindableProperty.Create(nameof(SliderValue),
                                    typeof(double),
                                    typeof(PlaybackControls),
                                    default(double));
        public double SliderValue
        {
            get { return (double)GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }


        public static readonly BindableProperty SliderMaxValueProperty =
            BindableProperty.Create(nameof(SliderMaxValue),
                                    typeof(double),
                                    typeof(PlaybackControls),
                                    1000.0);
        public double SliderMaxValue
        {
            get { return (double)GetValue(SliderMaxValueProperty); }
            set { SetValue(SliderMaxValueProperty, value); }
        }


        public static readonly BindableProperty IsCastButtonVisibleProperty =
            BindableProperty.Create(nameof(IsCastButtonVisible),
                                    typeof(bool),
                                    typeof(PlaybackControls),
                                    default(bool));
        public bool IsCastButtonVisible
        {
            get { return (bool)GetValue(IsCastButtonVisibleProperty); }
            set { SetValue(IsCastButtonVisibleProperty, value); }
        }



        public static readonly BindableProperty SubtitlesClickedCommandProperty =
            BindableProperty.Create(nameof(SubtitlesClickedCommand), typeof(ICommand), typeof(PlaybackControls), null);

        public ICommand SubtitlesClickedCommand
        {
            get { return (ICommand)GetValue(SubtitlesClickedCommandProperty); }
            set { SetValue(SubtitlesClickedCommandProperty, value); }
        }


        public static readonly BindableProperty AudioTracksClickedCommandProperty =
            BindableProperty.Create(nameof(AudioTracksClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand AudioTracksClickedCommand
        {
            get { return (ICommand)GetValue(AudioTracksClickedCommandProperty); }
            set { SetValue(AudioTracksClickedCommandProperty, value); }
        }


        public static readonly BindableProperty VideoDetailsClickedCommandProperty =
            BindableProperty.Create(nameof(VideoDetailsClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand VideoDetailsClickedCommand
        {
            get { return (ICommand)GetValue(VideoDetailsClickedCommandProperty); }
            set { SetValue(VideoDetailsClickedCommandProperty, value); }
        }


        public static readonly BindableProperty FavoriteClickedCommandProperty =
            BindableProperty.Create(nameof(FavoriteClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand FavoriteClickedCommand
        {
            get { return (ICommand)GetValue(FavoriteClickedCommandProperty); }
            set { SetValue(FavoriteClickedCommandProperty, value); }
        }


        public static readonly BindableProperty AspectRatioClickedCommandProperty =
            BindableProperty.Create(nameof(AspectRatioClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand AspectRatioClickedCommand
        {
            get { return (ICommand)GetValue(AspectRatioClickedCommandProperty); }
            set { SetValue(AspectRatioClickedCommandProperty, value); }
        }


        public static readonly BindableProperty CastClickedCommandProperty =
            BindableProperty.Create(nameof(CastClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand CastClickedCommand
        {
            get { return (ICommand)GetValue(CastClickedCommandProperty); }
            set { SetValue(CastClickedCommandProperty, value); }
        }


        public static readonly BindableProperty PreviousClickedCommandProperty =
            BindableProperty.Create(nameof(PreviousClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand PreviousClickedCommand
        {
            get { return (ICommand)GetValue(PreviousClickedCommandProperty); }
            set { SetValue(PreviousClickedCommandProperty, value); }
        }

        public static readonly BindableProperty PlayPauseClickedCommandProperty =
           BindableProperty.Create(nameof(PlayPauseClickedCommand),
                                   typeof(ICommand),
                                   typeof(PlaybackControls),
                                   default(ICommand));
        public ICommand PlayPauseClickedCommand
        {
            get { return (ICommand)GetValue(PlayPauseClickedCommandProperty); }
            set { SetValue(PlayPauseClickedCommandProperty, value); }
        }

        public static readonly BindableProperty NextClickedCommandProperty =
            BindableProperty.Create(nameof(NextClickedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand NextClickedCommand
        {
            get { return (ICommand)GetValue(NextClickedCommandProperty); }
            set { SetValue(NextClickedCommandProperty, value); }
        }



        public static readonly BindableProperty SliderValueChangedCommandProperty =
            BindableProperty.Create(nameof(SliderValueChangedCommand),
                                    typeof(ICommand),
                                    typeof(PlaybackControls),
                                    default(ICommand));
        public ICommand SliderValueChangedCommand
        {
            get { return (ICommand)GetValue(SliderValueChangedCommandProperty); }
            set { SetValue(SliderValueChangedCommandProperty, value); }
        }


        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (isSliderDragging)
            {
                var newPosition = TimeSpan.FromMilliseconds(e.NewValue / SliderMaxValue * TotalTime.TotalMilliseconds);
                Execute(SliderValueChangedCommand, newPosition);
            }
        }

        private void Slider_DragStarted(object sender, EventArgs e)
        {
            isSliderDragging = true;
        }

        private void Slider_DragCompleted(object sender, EventArgs e)
        {
            isSliderDragging = false;
        }

        private void Execute(ICommand command)
        {
            if (command == null) return;
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }

        }
        private void Execute<T>(ICommand command, T parameter)
        {
            if (command == null) return;
            if (command.CanExecute(null))
            {
                command.Execute(parameter);
            }
        }

        private bool isAnimating = false;

        private async Task FadeOut()
        {
            await this.FadeTo(0, 300, Easing.CubicInOut);
            IsControlVisible = false;
        }

        private async Task FadeIn()
        {
            await this.FadeTo(1, 300, Easing.CubicInOut);
            IsControlVisible = true;
        }


        public static readonly BindableProperty IsControlVisibleProperty =
            BindableProperty.Create(nameof(IsControlVisible),
                                    typeof(bool),
                                    typeof(PlaybackControls),
                                    true,
                                    defaultBindingMode: BindingMode.TwoWay,
                                    propertyChanged: IsControlVisiblePropertyChanged);
        public bool IsControlVisible
        {
            get { return (bool)GetValue(IsControlVisibleProperty); }
            set { SetValue(IsControlVisibleProperty, value); }
        }

        private static void IsControlVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!((PlaybackControls)bindable).isAnimating)
            {
                ((PlaybackControls)bindable).AnimateVisibility((bool)newValue);
            }
        }


        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            AnimateVisibility(!IsControlVisible);
        }

        private async void AnimateVisibility(bool targetVisibility)
        {
            if (targetVisibility)
            {
                isAnimating = true;
                await FadeIn();
                isAnimating = false;
            }
            else
            {
                isAnimating = true;
                await FadeOut();
                isAnimating = false;
            }
        }

        public static readonly BindableProperty BrightnessViewModelProperty =
            BindableProperty.Create(nameof(BrightnessViewModel),
                                    typeof(IBrightnessViewModel),
                                    typeof(PlaybackControls),
                                    default(IBrightnessViewModel));
        public IBrightnessViewModel BrightnessViewModel
        {
            get { return (IBrightnessViewModel)GetValue(BrightnessViewModelProperty); }
            set { SetValue(BrightnessViewModelProperty, value); }
        }

        public static readonly BindableProperty VolumeViewModelProperty =
            BindableProperty.Create(nameof(VolumeViewModel),
                            typeof(IVolumeViewModel),
                            typeof(PlaybackControls),
                            default(IVolumeViewModel));
        public IVolumeViewModel VolumeViewModel
        {
            get { return (IVolumeViewModel)GetValue(VolumeViewModelProperty); }
            set { SetValue(VolumeViewModelProperty, value); }
        }

        private Direction leftGridDirection = Direction.None;
        private Direction rightGridDirection = Direction.None;
        private double gridWidth = 100;
        private double initialSliderPosition = 0;

        private void LeftGrid_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    gridWidth = GesturesGrid.Width;
                    brightnessControl.GridHeight = GesturesGrid.Height;
                    brightnessControl.BrightnessPanUpdated(e);
                    initialSliderPosition = SliderValue;
                    break;
                case GestureStatus.Running:
                    if (leftGridDirection == Direction.None)
                    {
                        leftGridDirection = ToDirection(e.TotalX, e.TotalY);
                    }
                    if (leftGridDirection == Direction.Horizontal && ToDirection(e.TotalX, e.TotalY) == Direction.Horizontal)
                    {
                        SwipePositionChange(e.TotalX);
                    }
                    else if (leftGridDirection == Direction.Vertical && ToDirection(e.TotalX, e.TotalY) == Direction.Vertical)
                    {
                        brightnessControl.BrightnessPanUpdated(e);
                    }
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    brightnessControl.BrightnessPanUpdated(e);
                    leftGridDirection = Direction.None;
                    break;
            }
        }

        private void RightGrid_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    gridWidth = GesturesGrid.Width;
                    volumeControl.GridHeight = GesturesGrid.Height;
                    volumeControl.VolumePanUpdated(e);
                    initialSliderPosition = SliderValue;
                    break;
                case GestureStatus.Running:
                    if (rightGridDirection == Direction.None)
                    {
                        rightGridDirection = ToDirection(e.TotalX, e.TotalY);
                    }
                    if (rightGridDirection == Direction.Horizontal && ToDirection(e.TotalX, e.TotalY) == Direction.Horizontal)
                    {
                        SwipePositionChange(e.TotalX);
                    }
                    else if (rightGridDirection == Direction.Vertical && ToDirection(e.TotalX, e.TotalY) == Direction.Vertical)
                    {
                        volumeControl.VolumePanUpdated(e);
                    }
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    volumeControl.VolumePanUpdated(e);
                    rightGridDirection = Direction.None;
                    break;
            }
        }

        private Direction ToDirection(double totalX, double totalY)
        {
            return Math.Abs(totalX) > Math.Abs(totalY) ? Direction.Horizontal : Direction.Vertical;
        }

        private void SwipePositionChange(double totalX)
        {
            var a = totalX / (gridWidth * 2);
            var b = a * SliderMaxValue;
            var c = initialSliderPosition + b;
            var d = Math.Max(0, Math.Min(c, SliderMaxValue));
            var newPosition = TimeSpan.FromMilliseconds(d / SliderMaxValue * TotalTime.TotalMilliseconds);
            App.DebugLog($"Swipe {a} {b} {c} {d} {newPosition}");
            Execute(SliderValueChangedCommand, newPosition);
        }

        private void TapGestureRecognizer_Tapped2(object sender, EventArgs e)
        {
            Execute(FavoriteClickedCommand);
        }

        private enum Direction
        {
            None,
            Horizontal,
            Vertical
        }
    }
}