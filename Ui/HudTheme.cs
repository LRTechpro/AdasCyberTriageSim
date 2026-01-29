using System;
using System.Drawing;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Centralized theme constants for the dark cyber lab HUD.
    /// </summary>
    public static class HudTheme
    {
        // Color palette - using old C_ names for backwards compatibility
        public static readonly Color C_BG = Color.FromArgb(14, 16, 20);
        public static readonly Color C_GLASS = Color.FromArgb(165, 0, 0, 0);
        public static readonly Color C_PANEL = Color.FromArgb(30, 34, 42);
        public static readonly Color C_PANEL_2 = Color.FromArgb(24, 28, 36);
        public static readonly Color C_CYAN = Color.FromArgb(50, 230, 255);
        public static readonly Color C_TEXT = Color.FromArgb(230, 235, 245);
        public static readonly Color C_MUTED = Color.FromArgb(160, 170, 190);
        public static readonly Color C_WARN = Color.FromArgb(255, 190, 60);
        public static readonly Color C_BAD = Color.FromArgb(255, 85, 85);
        public static readonly Color C_OK = Color.FromArgb(80, 220, 140);

        // Color palette - using PascalCase names for new code
        public static readonly Color ColorBg = C_BG;
        public static readonly Color ColorPanel = C_PANEL;
        public static readonly Color ColorPanelLight = C_PANEL_2;
        public static readonly Color ColorBorder = Color.FromArgb(80, 90, 110);
        public static readonly Color ColorGlass = C_GLASS;
        public static readonly Color ColorCyan = C_CYAN;
        public static readonly Color ColorCyanDim = Color.FromArgb(100, 150, 180);
        public static readonly Color ColorGreen = C_OK;
        public static readonly Color ColorYellow = C_WARN;
        public static readonly Color ColorRed = C_BAD;
        public static readonly Color ColorPurple = Color.FromArgb(180, 100, 200);
        public static readonly Color ColorText = C_TEXT;
        public static readonly Color ColorTextMuted = C_MUTED;
        public static readonly Color ColorTextDim = Color.FromArgb(100, 110, 130);

        // Fonts
        public static readonly Font FontTitle = new Font("Segoe UI", 18, FontStyle.Bold);
        public static readonly Font FontHeading = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font FontMono = new Font("Consolas", 9, FontStyle.Regular);
        public static readonly Font FontSmall = new Font("Segoe UI", 8, FontStyle.Regular);

        // Spacing & Layout
        public const int PaddingLarge = 16;
        public const int PaddingMed = 12;
        public const int PaddingSmall = 8;
        public const int CornerRadius = 10;
        public const int BorderWidth = 2;

        // Animation
        public const int AnimationFrameMs = 16;
        public const int MeterAnimationDurationMs = 300;
        public const int ToastDisplayDurationMs = 3500;

        /// <summary>
        /// Get a color based on a health/threat percentage (0-100).
        /// 0-33: green, 34-66: yellow, 67+: red
        /// </summary>
        public static Color GetStatusColor(int percent)
        {
            if (percent <= 33) return ColorGreen;
            if (percent <= 66) return ColorYellow;
            return ColorRed;
        }
    }

    /// <summary>
    /// Button styling options.
    /// </summary>
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info
    }
}