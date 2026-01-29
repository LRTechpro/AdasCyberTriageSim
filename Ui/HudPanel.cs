using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Base panel with built-in double buffering and HUD styling.
    /// Derives from this to create custom HUD panels.
    /// </summary>
    public class HudPanel : Panel
    {
        private string _title = "";
        protected bool _drawTitle = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Invalidate();
            }
        }

        public HudPanel()
        {
            // Performance: enable double buffering
            DoubleBuffered = true;
            ControlStyles styles = ControlStyles.AllPaintingInWmPaint |
                                   ControlStyles.UserPaint |
                                   ControlStyles.Opaque;
            SetStyle(styles, true);

            BackColor = HudTheme.ColorPanel;
            ForeColor = HudTheme.ColorText;
            Font = HudTheme.FontBody;
            Margin = new Padding(0);
            Padding = new Padding(HudTheme.PaddingMed);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw background
            e.Graphics.Clear(BackColor);

            // Draw border + glow
            DrawBorder(e.Graphics);

            // Draw title if set
            if (_drawTitle && !string.IsNullOrEmpty(_title))
            {
                DrawTitle(e.Graphics);
            }

            // Subclass override point
            OnPaintContent(e);
        }

        /// <summary>
        /// Override this in subclasses to draw custom content.
        /// </summary>
        protected virtual void OnPaintContent(PaintEventArgs e)
        {
        }

        protected virtual void DrawBorder(Graphics g)
        {
            // Outer border
            using var borderPen = new Pen(HudTheme.ColorBorder, HudTheme.BorderWidth);
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

            // Inner highlight (subtle glow)
            using var glowPen = new Pen(Color.FromArgb(40, HudTheme.ColorCyan), 1);
            g.DrawRectangle(glowPen, 2, 2, Width - 5, Height - 5);
        }

        protected virtual void DrawTitle(Graphics g)
        {
            using var brush = new SolidBrush(HudTheme.ColorTextMuted);
            g.DrawString(_title, HudTheme.FontSmall, brush,
                new PointF(HudTheme.PaddingMed, HudTheme.PaddingSmall));
        }
    }
}