using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    public enum ToastSeverity { Info, Warning, Critical }

    /// <summary>
    /// Non-blocking toast notification overlay.
    /// Manages a queue of alerts that appear top-right and auto-dismiss.
    /// </summary>
    public class ToastManager
    {
        private class Toast
        {
            public string Message { get; set; } = "";
            public ToastSeverity Severity { get; set; }
            public DateTime CreatedAt { get; set; }
            public int LayoutY { get; set; }
        }

        private readonly List<Toast> _toasts = new();
        private readonly Label _toastContainer;
        private readonly System.Windows.Forms.Timer _toastTimer;

        public ToastManager(Form parentForm)
        {
            // Create invisible container for toast rendering
            _toastContainer = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = Color.Transparent,
                Text = ""
            };
            parentForm.Controls.Add(_toastContainer);
            _toastContainer.BringToFront();

            // Hook paint event for custom rendering
            _toastContainer.Paint += ToastContainer_Paint;

            // Timer to update and dismiss toasts
            _toastTimer = new System.Windows.Forms.Timer
            {
                Interval = 100  // Check 10x per second
            };
            _toastTimer.Tick += (_, __) =>
            {
                RemoveExpiredToasts();
                _toastContainer.Invalidate();
            };
            _toastTimer.Start();
        }

        public void ShowToast(string message, ToastSeverity severity = ToastSeverity.Info)
        {
            lock (_toasts)
            {
                _toasts.Add(new Toast
                {
                    Message = message,
                    Severity = severity,
                    CreatedAt = DateTime.Now
                });

                LayoutToasts();
            }
        }

        private void ToastContainer_Paint(object? sender, PaintEventArgs e)
        {
            lock (_toasts)
            {
                foreach (var toast in _toasts)
                {
                    DrawToast(e.Graphics, toast);
                }
            }
        }

        private void DrawToast(Graphics g, Toast toast)
        {
            int x = _toastContainer.Width - 320;
            int y = toast.LayoutY;
            int w = 300;
            int h = 60;

            // Background color based on severity
            Color bgColor = toast.Severity switch
            {
                ToastSeverity.Info => Color.FromArgb(40, HudTheme.ColorCyan),
                ToastSeverity.Warning => Color.FromArgb(60, HudTheme.ColorYellow),
                ToastSeverity.Critical => Color.FromArgb(80, HudTheme.ColorRed),
                _ => Color.FromArgb(40, HudTheme.ColorCyan)
            };

            Color borderColor = toast.Severity switch
            {
                ToastSeverity.Info => HudTheme.ColorCyan,
                ToastSeverity.Warning => HudTheme.ColorYellow,
                ToastSeverity.Critical => HudTheme.ColorRed,
                _ => HudTheme.ColorCyan
            };

            // Draw rounded background
            using var bgBrush = new SolidBrush(bgColor);
            using var path = GetRoundedPath(new Rectangle(x, y, w, h), 8);
            g.FillPath(bgBrush, path);

            // Draw border
            using var borderPen = new Pen(borderColor, 2);
            g.DrawPath(borderPen, path);

            // Draw icon (severity symbol)
            string icon = toast.Severity switch
            {
                ToastSeverity.Info => "ℹ",
                ToastSeverity.Warning => "⚠",
                ToastSeverity.Critical => "⚠",
                _ => "•"
            };

            using var iconBrush = new SolidBrush(borderColor);
            g.DrawString(icon, HudTheme.FontHeading, iconBrush, x + 12, y + 8);

            // Draw message
            using var textBrush = new SolidBrush(HudTheme.ColorText);
            var textRect = new Rectangle(x + 40, y + 8, w - 50, h - 16);
            g.DrawString(toast.Message, HudTheme.FontSmall, textBrush, textRect);
        }

        private void RemoveExpiredToasts()
        {
            lock (_toasts)
            {
                var now = DateTime.Now;
                _toasts.RemoveAll(t =>
                    (now - t.CreatedAt).TotalMilliseconds > HudTheme.ToastDisplayDurationMs);

                LayoutToasts();
            }
        }

        private void LayoutToasts()
        {
            int y = 20;
            foreach (var toast in _toasts)
            {
                toast.LayoutY = y;
                y += 80;
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        public void Cleanup()
        {
            _toastTimer?.Stop();
            _toastTimer?.Dispose();
        }
    }
}