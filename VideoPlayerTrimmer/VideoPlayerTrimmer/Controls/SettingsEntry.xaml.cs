using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsEntry : ContentView
    {
        public SettingsEntry()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header),
            typeof(string),
            typeof(SettingsEntry),
            "");

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description),
                                    typeof(string),
                                    typeof(SettingsEntry),
                                    "");
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }


        public static readonly BindableProperty IsLineVisibleProperty =
            BindableProperty.Create(nameof(IsLineVisible),
                                    typeof(bool),
                                    typeof(SettingsEntry),
                                    true);
        public bool IsLineVisible
        {
            get { return (bool)GetValue(IsLineVisibleProperty); }
            set { SetValue(IsLineVisibleProperty, value); }
        }


        public static readonly BindableProperty ControlWidthProperty =
            BindableProperty.Create(nameof(ControlWidth),
                                    typeof(GridLength),
                                    typeof(SettingsEntry),
                                    new GridLength(100, GridUnitType.Absolute));
        public GridLength ControlWidth
        {
            get { return (GridLength)GetValue(ControlWidthProperty); }
            set { SetValue(ControlWidthProperty, value); }
        }


        public static readonly BindableProperty HeaderStyleProperty =
            BindableProperty.Create(nameof(HeaderStyle),
                                    typeof(Style),
                                    typeof(SettingsEntry),
                                    DefaultHeaderStyle);
        public Style HeaderStyle
        {
            get { return (Style)GetValue(HeaderStyleProperty); }
            set { SetValue(HeaderStyleProperty, value); }
        }


        public static readonly BindableProperty DescriptionStyleProperty =
            BindableProperty.Create(nameof(DescriptionStyle),
                                    typeof(Style),
                                    typeof(SettingsEntry),
                                    DefaultDescriptionStyle);
        public Style DescriptionStyle
        {
            get { return (Style)GetValue(DescriptionStyleProperty); }
            set { SetValue(DescriptionStyleProperty, value); }
        }

        private static Style DefaultHeaderStyle
        {
            get
            {
                return new Style(typeof(Label))
                {
                    Setters =
                    {
                        new Setter()
                        {
                            Property = Label.FontSizeProperty, Value = 15
                        },
                        new Setter()
                        {
                            Property = Label.LineBreakModeProperty, Value = LineBreakMode.CharacterWrap
                        },
                        new Setter()
                        {
                            Property = Label.MaxLinesProperty, Value = 1
                        }
                    }
                };
            }
        }

        private static Style DefaultDescriptionStyle
        {
            get
            {
                return new Style(typeof(Label))
                {
                    Setters =
                    {
                        new Setter()
                        {
                            Property = Label.FontSizeProperty, Value = 14
                        },
                        new Setter()
                        {
                            Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap
                        }
                    }
                };
            }
        }
    }
}