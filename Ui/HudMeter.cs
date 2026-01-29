using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Reusable HUD meter control with animated bar fill.
    /// Displays: label | current value | smooth animated progress bar | status color
    /// </summary>
    public class HudMeter : HudPanel
    {
        private string _label = "METER";
        private int _value = 50;
        private int _maxValue = 100;
        private int _displayValue = 50;  // Animated interpolation
        private DateTime _animationStart = DateTime.Now;
        private bool _isAnimating = false;
        private bool _pulseOnCritical = false;
        private int _pulsePhase = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public int Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = Math.Clamp(value, 0, _maxValue);
                    _animationStart = DateTime.Now;
                    _isAnimating = true;
                    _pulseOnCritical = _value >= 80;  // Pulse at critical
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Math.Max(1, value);
                Invalidate();
            }
        }

        public HudMeter()
        {
            _drawTitle = false;
            Height = 70;
            Width = 300;
        }

        protected override void OnPaintContent(PaintEventArgs e)
        {
            // Animation frame: interpolate display value towards actual value
            UpdateAnimation();

            int labelY = HudTheme.PaddingMed;
            int barY = labelY + 20;
            int barHeight = 16;

            // Draw label + value on same line
            using var labelBrush = new SolidBrush(HudTheme.ColorTextMuted);
            e.Graphics.DrawString(_label, HudTheme.FontSmall, labelBrush,
                new PointF(HudTheme.PaddingMed, labelY));

            // Value text (right-aligned)
            string valueText = $"{_displayValue}%";
            var textSize = e.Graphics.MeasureString(valueText, HudTheme.FontSmall);
            using var valueBrush = new SolidBrush(HudTheme.ColorText);
            e.Graphics.DrawString(valueText, HudTheme.FontSmall, valueBrush,
                new PointF(Width - textSize.Width - HudTheme.PaddingMed, labelY));

            // Progress bar background
            int barX = HudTheme.PaddingMed;
            int barWidth = Width - (2 * HudTheme.PaddingMed);
            var barRect = new Rectangle(barX, barY, barWidth, barHeight);

            using var barBgBrush = new SolidBrush(HudTheme.ColorPanelLight);
            e.Graphics.FillRectangle(barBgBrush, barRect);
            using var barBgPen = new Pen(HudTheme.ColorBorder, 1);
            e.Graphics.DrawRectangle(barBgPen, barRect);

            // Progress bar fill (animated)
            int fillWidth = (int)((double)_displayValue / _maxValue * barWidth);
            var fillRect = new Rectangle(barX, barY, fillWidth, barHeight);

            Color fillColor = HudTheme.GetStatusColor(_displayValue);

            // Pulsing effect at critical
            if (_pulseOnCritical)
            {
                int pulseAlpha = 200 + (int)(50 * Math.Sin(_pulsePhase * 0.1));
                fillColor = Color.FromArgb(pulseAlpha, fillColor);
                _pulsePhase++;
            }

            using var fillBrush = new SolidBrush(fillColor);
            e.Graphics.FillRectangle(fillBrush, fillRect);
        }

        private void UpdateAnimation()
        {
            if (!_isAnimating) return;

            double elapsed = (DateTime.Now - _animationStart).TotalMilliseconds;
            double progress = Math.Min(1.0, elapsed / HudTheme.MeterAnimationDurationMs);

            // Easing: ease-out cubic
            progress = 1 - Math.Pow(1 - progress, 3);

            _displayValue = (int)(_displayValue + (_value - _displayValue) * progress);

            if (progress >= 1.0)
            {
                _displayValue = _value;
                _isAnimating = false;
            }

            Invalidate();
        }
    }
}