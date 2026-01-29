using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Custom HUD panel that displays a network map with stations and animates a hacker icon
    /// moving between them. Fully double-buffered for smooth 60 FPS animation.
    ///
    /// Animation Math:
    /// - Hacker position interpolates smoothly from current station to target station
    /// - Uses linear interpolation over 600ms (configurable)
    /// - Easing function: ease-in-out cubic for natural motion
    /// - Only invalidates when animating, for performance
    /// </summary>
    public class HudMapPanel : HudPanel
    {
        /// <summary>Represents a station (node) on the network map.</summary>
        public class Station
        {
            public string Name { get; set; } = "";
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsCompromised { get; set; }
        }

        private Dictionary<string, Station> _stations = new();
        private string _currentStation = "";
        private string _targetStation = "";
        private PointF _hackerPos = PointF.Empty;
        private PointF _targetPos = PointF.Empty;
        private DateTime _animationStart = DateTime.Now;
        private bool _isMoving = false;
        private const int AnimationDurationMs = 600;
        private Image? _hackerImage;
        private int _glowPulse = 0;

        public HudMapPanel()
        {
            Title = "NETWORK MAP";
            Height = 400;
            Width = 500;
        }

        /// <summary>
        /// Define a station on the map.
        /// Call this during initialization.
        /// </summary>
        public void AddStation(string name, int x, int y)
        {
            _stations[name] = new Station { Name = name, X = x, Y = y };

            if (string.IsNullOrEmpty(_currentStation))
            {
                SetInitialStation(name);
            }

            Invalidate();
        }

        /// <summary>
        /// Set the starting position for the hacker.
        /// </summary>
        public void SetInitialStation(string stationName)
        {
            if (!_stations.ContainsKey(stationName)) return;

            _currentStation = stationName;
            _targetStation = stationName;
            var station = _stations[stationName];
            _hackerPos = new PointF(station.X, station.Y);
            _targetPos = _hackerPos;
            _isMoving = false;

            Invalidate();
        }

        /// <summary>
        /// Smoothly animate the hacker to a target station.
        /// Does nothing if target doesn't exist or is current position.
        /// </summary>
        public void MoveToStation(string stationName)
        {
            if (!_stations.ContainsKey(stationName)) return;
            if (stationName == _currentStation && !_isMoving) return;

            _targetStation = stationName;
            var station = _stations[stationName];
            _targetPos = new PointF(station.X, station.Y);
            _animationStart = DateTime.Now;
            _isMoving = true;

            Invalidate();
        }

        /// <summary>
        /// Call this from a timer tick (e.g., every 16ms) to update animation.
        /// </summary>
        public void TickAnimation()
        {
            if (!_isMoving) return;

            double elapsed = (DateTime.Now - _animationStart).TotalMilliseconds;
            double progress = Math.Min(1.0, elapsed / AnimationDurationMs);

            // Easing: ease-in-out cubic
            double t = progress < 0.5
                ? 4 * progress * progress * progress
                : 1 - Math.Pow(-2 * progress + 2, 3) / 2;

            _hackerPos.X = _hackerPos.X + (float)((_targetPos.X - _hackerPos.X) * t);
            _hackerPos.Y = _hackerPos.Y + (float)((_targetPos.Y - _hackerPos.Y) * t);

            if (progress >= 1.0)
            {
                _hackerPos = _targetPos;
                _currentStation = _targetStation;
                _isMoving = false;
            }

            _glowPulse = (_glowPulse + 1) % 360;
            Invalidate();
        }

        /// <summary>
        /// Mark a station as compromised (turns red).
        /// </summary>
        public void CompromiseStation(string stationName)
        {
            if (_stations.ContainsKey(stationName))
            {
                _stations[stationName].IsCompromised = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Recover a station (turns green again).
        /// </summary>
        public void RecoverStation(string stationName)
        {
            if (_stations.ContainsKey(stationName))
            {
                _stations[stationName].IsCompromised = false;
                Invalidate();
            }
        }

        /// <summary>
        /// Load a custom hacker image.
        /// Falls back to drawn icon if not available.
        /// </summary>
        public void SetHackerImage(Image? image)
        {
            _hackerImage = image;
            Invalidate();
        }

        protected override void OnPaintContent(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid background
            DrawGrid(e.Graphics);

            // Draw station connections
            DrawConnections(e.Graphics);

            // Draw stations
            DrawStations(e.Graphics);

            // Draw hacker icon
            DrawHacker(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            using var gridPen = new Pen(Color.FromArgb(15, HudTheme.ColorCyan), 0.5f);

            int gridSize = 50;
            for (int x = HudTheme.PaddingMed; x < Width; x += gridSize)
                g.DrawLine(gridPen, x, HudTheme.PaddingMed, x, Height - HudTheme.PaddingMed);

            for (int y = HudTheme.PaddingMed; y < Height; y += gridSize)
                g.DrawLine(gridPen, HudTheme.PaddingMed, y, Width - HudTheme.PaddingMed, y);
        }

        private void DrawConnections(Graphics g)
        {
            var stations = _stations.Values.ToList();

            using var connPen = new Pen(Color.FromArgb(60, HudTheme.ColorCyanDim), 1);
            connPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            for (int i = 0; i < stations.Count - 1; i++)
            {
                for (int j = i + 1; j < stations.Count; j++)
                {
                    g.DrawLine(connPen, stations[i].X, stations[i].Y, stations[j].X, stations[j].Y);
                }
            }
        }

        private void DrawStations(Graphics g)
        {
            foreach (var station in _stations.Values)
            {
                bool isActive = (station.Name == _currentStation);
                bool isTarget = (station.Name == _targetStation);
                Color stationColor = station.IsCompromised ? HudTheme.ColorRed : HudTheme.ColorGreen;

                // Station node
                int nodeRadius = isActive ? 12 : 8;
                using var nodeBrush = new SolidBrush(Color.FromArgb(80, stationColor));
                g.FillEllipse(nodeBrush, station.X - nodeRadius, station.Y - nodeRadius,
                    nodeRadius * 2, nodeRadius * 2);

                // Node border
                using var nodePen = new Pen(stationColor, 2);
                g.DrawEllipse(nodePen, station.X - nodeRadius, station.Y - nodeRadius,
                    nodeRadius * 2, nodeRadius * 2);

                // Glow if active or target
                if (isActive || isTarget)
                {
                    int glowRadius = nodeRadius + 6 + (int)(3 * Math.Sin(_glowPulse * 0.05));
                    using var glowPen = new Pen(Color.FromArgb(100, stationColor), 1)
                    { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
                    g.DrawEllipse(glowPen, station.X - glowRadius, station.Y - glowRadius,
                        glowRadius * 2, glowRadius * 2);
                }

                // Label
                var labelSize = g.MeasureString(station.Name, HudTheme.FontSmall);
                using var labelBrush = new SolidBrush(HudTheme.ColorText);
                g.DrawString(station.Name, HudTheme.FontSmall, labelBrush,
                    station.X - labelSize.Width / 2, station.Y + nodeRadius + 8);
            }
        }

        private void DrawHacker(Graphics g)
        {
            int iconSize = 24;

            if (_hackerImage != null)
            {
                // Draw image with slight glow
                using var glowPen = new Pen(Color.FromArgb(150, HudTheme.ColorYellow), 2);
                g.DrawEllipse(glowPen, _hackerPos.X - iconSize / 2 - 3,
                    _hackerPos.Y - iconSize / 2 - 3, iconSize + 6, iconSize + 6);

                g.DrawImage(_hackerImage,
                    _hackerPos.X - iconSize / 2, _hackerPos.Y - iconSize / 2,
                    iconSize, iconSize);
            }
            else
            {
                // Fallback: drawn skull/hacker icon
                DrawHackerIcon(g, (int)_hackerPos.X, (int)_hackerPos.Y, iconSize);
            }

            // Trail indicator
            if (_isMoving)
            {
                using var trailPen = new Pen(Color.FromArgb(120, HudTheme.ColorYellow), 1)
                { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                g.DrawLine(trailPen, _hackerPos.X, _hackerPos.Y, _targetPos.X, _targetPos.Y);
            }
        }

        private void DrawHackerIcon(Graphics g, int cx, int cy, int size)
        {
            // Simple hacker skull symbol
            using var skullBrush = new SolidBrush(HudTheme.ColorYellow);
            using var skullPen = new Pen(HudTheme.ColorYellow, 2);

            // Head
            g.FillEllipse(skullBrush, cx - size / 2, cy - size / 2, size, size);
            g.DrawEllipse(skullPen, cx - size / 2, cy - size / 2, size, size);

            // Eye sockets
            using var eyeBrush = new SolidBrush(Color.Black);
            g.FillEllipse(eyeBrush, cx - size / 4, cy - size / 6, size / 6, size / 6);
            g.FillEllipse(eyeBrush, cx + size / 6, cy - size / 6, size / 6, size / 6);

            // Crossbones
            g.DrawLine(skullPen, cx - size, cy + size / 2, cx + size, cy + size / 2);
        }
    }
}