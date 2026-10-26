using MudBlazor;

namespace Enfilade;

internal static class EnfiladeTheme
{
    public const string Night = "#080C1C";
    public const string Deep = "#0F1530";
    public const string Rule = "#1E2648";
    public const string Mist = "#8A90AB";
    public const string Light = "#EEEBE5";
    public const string Klein = "#002FA7";
    public const string KleinGlow = "#4A6CFF";
    public const string Vermilion = "#FF5A3C";
    public const string Ochre = "#E3A63E";
    public const string Paper = "#F6F4EF";
    public const string Sand = "#ECE9E2";
    public const string Seam = "#D9D5CC";
    public const string Slate = "#5A607A";
    public const string Ink = "#0F1530";
    public const string Cobalt = "#2442D6";
    public const string Rust = "#C7381D";
    public const string Umber = "#8A5A00";

    private static readonly string[] Sans = ["Onest", "ui-sans-serif", "system-ui", "sans-serif"];

    public static MudTheme For(string? accent) => new()
    {
        PaletteDark = new PaletteDark
        {
            Black = Night,
            White = "#FFFFFF",
            Primary = accent ?? Klein,
            PrimaryContrastText = "#FFFFFF",
            Secondary = Mist,
            SecondaryContrastText = Night,
            Tertiary = Light,
            TertiaryContrastText = Night,
            Info = KleinGlow,
            InfoContrastText = Night,
            Success = Light,
            SuccessContrastText = Night,
            Warning = Ochre,
            WarningContrastText = Night,
            Error = Vermilion,
            ErrorContrastText = Night,
            Dark = Deep,
            DarkContrastText = Light,
            TextPrimary = Light,
            TextSecondary = Mist,
            TextDisabled = "rgba(138, 144, 171, 0.5)",
            ActionDefault = Mist,
            ActionDisabled = "rgba(138, 144, 171, 0.5)",
            ActionDisabledBackground = Rule,
            Background = Night,
            BackgroundGray = Deep,
            Surface = Deep,
            DrawerBackground = Night,
            DrawerText = Mist,
            DrawerIcon = Mist,
            AppbarBackground = Night,
            AppbarText = Light,
            LinesDefault = Rule,
            LinesInputs = Rule,
            TableLines = Rule,
            TableStriped = Deep,
            TableHover = Deep,
            Divider = Rule,
            DividerLight = Rule,
            Skeleton = Deep,
            OverlayDark = "rgba(8, 12, 28, 0.6)",
            HoverOpacity = 0.06,
        },
        PaletteLight = new PaletteLight
        {
            Black = Ink,
            White = "#FFFFFF",
            Primary = accent ?? Klein,
            PrimaryContrastText = "#FFFFFF",
            Secondary = Slate,
            SecondaryContrastText = Paper,
            Tertiary = Ink,
            TertiaryContrastText = Paper,
            Info = Cobalt,
            InfoContrastText = Paper,
            Success = Ink,
            SuccessContrastText = Paper,
            Warning = Umber,
            WarningContrastText = Paper,
            Error = Rust,
            ErrorContrastText = Paper,
            Dark = Ink,
            DarkContrastText = Paper,
            TextPrimary = Ink,
            TextSecondary = Slate,
            TextDisabled = "rgba(90, 96, 122, 0.5)",
            ActionDefault = Slate,
            ActionDisabled = "rgba(90, 96, 122, 0.5)",
            ActionDisabledBackground = Seam,
            Background = Paper,
            BackgroundGray = Sand,
            Surface = Sand,
            DrawerBackground = Paper,
            DrawerText = Slate,
            DrawerIcon = Slate,
            AppbarBackground = Paper,
            AppbarText = Ink,
            LinesDefault = Seam,
            LinesInputs = Seam,
            TableLines = Seam,
            TableStriped = Sand,
            TableHover = Sand,
            Divider = Seam,
            DividerLight = Seam,
            Skeleton = Sand,
            OverlayDark = "rgba(15, 21, 48, 0.35)",
            HoverOpacity = 0.06,
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = Sans, FontSize = "15px", FontWeight = "400", LineHeight = "24px", LetterSpacing = "normal" },
            H1 = new H1Typography { FontFamily = Sans, FontSize = "52px", FontWeight = "200", LineHeight = "60px", LetterSpacing = "-0.035em" },
            H6 = new H6Typography { FontFamily = Sans, FontSize = "20px", FontWeight = "500", LineHeight = "28px", LetterSpacing = "-0.01em" },
            Body1 = new Body1Typography { FontFamily = Sans, FontSize = "15px", FontWeight = "400", LineHeight = "24px", LetterSpacing = "normal" },
            Body2 = new Body2Typography { FontFamily = Sans, FontSize = "13px", FontWeight = "500", LineHeight = "20px", LetterSpacing = "normal" },
            Button = new ButtonTypography { FontFamily = Sans, FontSize = "13px", FontWeight = "600", LineHeight = "20px", LetterSpacing = "normal", TextTransform = "none" },
            Caption = new CaptionTypography { FontFamily = Sans, FontSize = "13px", FontWeight = "500", LineHeight = "20px", LetterSpacing = "normal" },
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "2px" },
        Shadows = new Shadow { Elevation = Enumerable.Repeat("none", new Shadow().Elevation.Length).ToArray() },
    };
}
