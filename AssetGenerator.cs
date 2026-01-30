using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Generates ultra-high-quality professional game asset images using advanced GDI+ techniques.
    /// Features: Gradients, shadows, glows, bloom effects, detailed geometry, and cyberpunk aesthetics.
    /// </summary>
    public class AssetGenerator
    {
        private readonly string _assetPath;

        /// <summary>Professional cyberpunk color palette with extended variants</summary>
        private static class Colors
        {
            // Cyan/Blue family
            public static readonly Color CyanBright = Color.FromArgb(0, 255, 255);
            public static readonly Color CyanMid = Color.FromArgb(0, 200, 220);
            public static readonly Color CyanDark = Color.FromArgb(0, 120, 160);
            public static readonly Color CyanVeryDark = Color.FromArgb(0, 60, 100);
            public static readonly Color BlueBright = Color.FromArgb(120, 220, 255);
            public static readonly Color BlueMid = Color.FromArgb(60, 160, 255);
            public static readonly Color BlueDark = Color.FromArgb(20, 90, 180);
            public static readonly Color BlueVeryDark = Color.FromArgb(10, 40, 100);

            // Red/Orange/Yellow family
            public static readonly Color RedBright = Color.FromArgb(255, 100, 100);
            public static readonly Color RedMid = Color.FromArgb(255, 60, 60);
            public static readonly Color RedDark = Color.FromArgb(200, 20, 20);
            public static readonly Color OrangeBright = Color.FromArgb(255, 200, 0);
            public static readonly Color OrangeMid = Color.FromArgb(255, 160, 0);
            public static readonly Color OrangeDark = Color.FromArgb(200, 100, 0);
            public static readonly Color YellowWarn = Color.FromArgb(255, 255, 0);

            // Accent colors
            public static readonly Color GreenSafe = Color.FromArgb(100, 255, 140);
            public static readonly Color PurpleAccent = Color.FromArgb(180, 100, 255);
            public static readonly Color PinkAccent = Color.FromArgb(255, 100, 200);

            // Neutrals with depth
            public static readonly Color Black = Color.FromArgb(5, 10, 15);
            public static readonly Color DarkBg = Color.FromArgb(15, 20, 35);
            public static readonly Color DarkGray = Color.FromArgb(40, 50, 80);
            public static readonly Color MidGray = Color.FromArgb(80, 100, 140);
            public static readonly Color LightGray = Color.FromArgb(150, 170, 200);
            public static readonly Color White = Color.FromArgb(255, 255, 255);
        }

        public AssetGenerator(string assetPath)
        {
            _assetPath = assetPath;
            Directory.CreateDirectory(_assetPath);
        }

        /// <summary>Generates all game asset images with ultra-high quality</summary>
        public void GenerateAllAssets()
        {
            System.Diagnostics.Debug.WriteLine("=== Generating Ultra-High-Quality Game Assets ===");

            GenerateVehicleAsset();
            GenerateGateAsset();
            GenerateSpinnerAsset();
            GenerateThreatAssets();

            System.Diagnostics.Debug.WriteLine("=== Asset Generation Complete ===");
        }

        // ============================================================
        // PLAYER VEHICLE - Advanced Design
        // ============================================================

        private void GenerateVehicleAsset()
        {
            const int w = 72, h = 84;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // === Shadow Layer ===
                DrawDropShadow(g, new RectangleF(10, 16, 52, 52), 8, 3);

                // === Main Vehicle Body with Gradient ===
                var bodyGrad = new LinearGradientBrush(
                    new PointF(10, 20), new PointF(62, 70),
                    Colors.BlueMid, Colors.BlueVeryDark);
                var bodyRect = new RectangleF(10, 20, 52, 48);
                FillRoundedRect(g, bodyGrad, bodyRect, 10);
                bodyGrad.Dispose();

                // === Inner Glow ===
                using (var pen = new Pen(Colors.CyanBright, 1.5f))
                {
                    var glowRect = new RectangleF(11, 21, 50, 46);
                    DrawRoundedRectOutline(g, pen, glowRect, 9);
                }

                // === Windshield (Front Accent) ===
                var windshieldGrad = new LinearGradientBrush(
                    new PointF(14, 24), new PointF(58, 34),
                    Colors.CyanMid, Colors.BlueMid);
                var windshield = new RectangleF(14, 24, 44, 12);
                FillRoundedRect(g, windshieldGrad, windshield, 5);
                windshieldGrad.Dispose();

                // === Windshield Highlight ===
                using (var pen = new Pen(Color.FromArgb(120, Colors.CyanBright), 1))
                    DrawRoundedRectOutline(g, pen, new RectangleF(15, 25, 42, 10), 4);

                // === Engine/Reactor Core (center glow) ===
                using (var br = new SolidBrush(Color.FromArgb(200, Colors.CyanBright)))
                    g.FillEllipse(br, 30, 42, 12, 12);

                using (var br = new SolidBrush(Color.FromArgb(255, Colors.CyanBright)))
                    g.FillEllipse(br, 32, 44, 8, 8);

                // === Left & Right Energy Strips ===
                using (var br = new SolidBrush(Color.FromArgb(180, Colors.PurpleAccent)))
                {
                    g.FillRectangle(br, 12, 38, 3, 16); // Left strip
                    g.FillRectangle(br, 57, 38, 3, 16); // Right strip
                }

                // === Wheels/Stabilizers ===
                using (var br = new SolidBrush(Colors.CyanMid))
                {
                    g.FillEllipse(br, 14, 66, 8, 10);   // Left wheel
                    g.FillEllipse(br, 50, 66, 8, 10);   // Right wheel
                }

                using (var br = new SolidBrush(Colors.CyanDark))
                {
                    g.FillEllipse(br, 15, 67, 6, 8);    // Left inner
                    g.FillEllipse(br, 51, 67, 6, 8);    // Right inner
                }

                // === Outer Glow Effect ===
                DrawGlowBorder(g, new RectangleF(8, 18, 56, 52), Colors.CyanBright, 2);

                SaveImage(bmp, "vehicle.png");
            }
        }

        // ============================================================
        // GATE/CHECKPOINT - Shield Design
        // ============================================================

        private void GenerateGateAsset()
        {
            const int w = 100, h = 80;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(15, 15, 70, 50), 10, 4);

                // === Shield Body Gradient ===
                var shieldGrad = new LinearGradientBrush(
                    new PointF(20, 20), new PointF(80, 60),
                    Colors.BlueMid, Colors.BlueDark);
                var shieldPath = CreateShieldPath(new RectangleF(20, 15, 60, 55));
                g.FillPath(shieldGrad, shieldPath);
                shieldGrad.Dispose();

                // === Shield Border ===
                using (var pen = new Pen(Colors.CyanBright, 2.5f))
                    g.DrawPath(pen, shieldPath);

                // === Inner Safe Zone ===
                var innerGrad = new LinearGradientBrush(
                    new PointF(30, 25), new PointF(70, 50),
                    Color.FromArgb(150, Colors.CyanMid), Color.FromArgb(100, Colors.BlueMid));
                var innerPath = CreateShieldPath(new RectangleF(30, 22, 40, 40));
                g.FillPath(innerGrad, innerPath);
                innerGrad.Dispose();

                // === Security Lock Icon ===
                DrawLockIcon(g, 45, 35, 10, Colors.GreenSafe);

                // === Corner Accents ===
                using (var br = new SolidBrush(Color.FromArgb(200, Colors.PurpleAccent)))
                {
                    g.FillRectangle(br, 18, 18, 3, 3);
                    g.FillRectangle(br, 79, 18, 3, 3);
                    g.FillRectangle(br, 18, 63, 3, 3);
                    g.FillRectangle(br, 79, 63, 3, 3);
                }

                // === Glow Effect ===
                DrawGlowBorder(g, new RectangleF(18, 13, 64, 58), Colors.CyanBright, 3);

                shieldPath.Dispose();
                SaveImage(bmp, "gate.png");
            }
        }

        // ============================================================
        // SPINNER/CAN FLOOD - Hazard Design
        // ============================================================

        private void GenerateSpinnerAsset()
        {
            const int w = 350, h = 40;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(5, 6, 340, 28), 8, 2);

                // === Main Bar Gradient ===
                var barGrad = new LinearGradientBrush(
                    new PointF(0, 8), new PointF(350, 32),
                    Colors.RedBright, Colors.RedDark);
                g.FillRectangle(barGrad, 5, 8, 340, 24);
                barGrad.Dispose();

                // === Hazard Stripes Pattern ===
                using (var pen = new Pen(Colors.OrangeMid, 3))
                {
                    for (int i = 0; i < w; i += 25)
                    {
                        g.DrawLine(pen, i, 6, i + 15, 34);
                    }
                }

                // === Warning Circles ===
                for (int i = 0; i < w; i += 70)
                {
                    using (var br = new SolidBrush(Color.FromArgb(120, Colors.YellowWarn)))
                        g.FillEllipse(br, i + 8, 14, 12, 12);
                    using (var br = new SolidBrush(Color.FromArgb(200, Colors.OrangeBright)))
                        g.FillEllipse(br, i + 10, 16, 8, 8);
                }

                // === Border ===
                using (var pen = new Pen(Colors.OrangeBright, 2))
                    g.DrawRectangle(pen, 5, 8, 340, 24);

                // === Glow Effect ===
                DrawGlowBorder(g, new RectangleF(3, 6, 344, 28), Colors.RedBright, 2);

                SaveImage(bmp, "spinner.png");
            }
        }

        // ============================================================
        // THREAT ASSETS
        // ============================================================

        private void GenerateThreatAssets()
        {
            GenerateThreatOta();
            GenerateThreatGateway();
            GenerateThreatUds();
            GenerateThreatKeys();
        }

        /// <summary>OTA Downgrade: Cloud with warning symbol</summary>
        private void GenerateThreatOta()
        {
            const int w = 100, h = 90;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(12, 15, 76, 50), 8, 3);

                // === Cloud Shape ===
                var cloudPath = CreateCloudPath(new RectangleF(15, 20, 70, 35));
                var cloudGrad = new LinearGradientBrush(
                    new PointF(15, 20), new PointF(85, 55),
                    Colors.DarkGray, Colors.MidGray);
                g.FillPath(cloudGrad, cloudPath);
                cloudGrad.Dispose();

                using (var pen = new Pen(Color.FromArgb(150, Colors.LightGray), 1.5f))
                    g.DrawPath(pen, cloudPath);

                // === Warning Triangle ===
                var triangle = new PointF[]
                {
                    new PointF(50, 58),
                    new PointF(38, 80),
                    new PointF(62, 80)
                };
                using (var br = new SolidBrush(Colors.RedMid))
                    g.FillPolygon(br, triangle);
                using (var pen = new Pen(Colors.RedBright, 2))
                    g.DrawPolygon(pen, triangle);

                // === Exclamation Mark ===
                using (var br = new SolidBrush(Color.White))
                {
                    g.FillRectangle(br, 48, 62, 4, 10);
                    g.FillEllipse(br, 47, 75, 6, 3);
                }

                // === Glow ===
                DrawGlowBorder(g, new RectangleF(10, 13, 80, 75), Colors.RedMid, 2);

                cloudPath.Dispose();
                SaveImage(bmp, "threat_ota.png");
            }
        }

        /// <summary>Gateway Pivot: Network nodes with alert</summary>
        private void GenerateThreatGateway()
        {
            const int w = 100, h = 90;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(10, 10, 80, 70), 8, 3);

                // === Network Nodes ===
                var nodePositions = new[] {
                    new PointF(25, 20), new PointF(50, 20), new PointF(75, 20),
                    new PointF(37.5f, 45), new PointF(62.5f, 45), new PointF(50, 70)
                };

                foreach (var pos in nodePositions)
                {
                    // Node shadow
                    using (var br = new SolidBrush(Color.FromArgb(80, Colors.Black)))
                        g.FillEllipse(br, pos.X - 5.5f, pos.Y + 0.5f, 11, 11);

                    // Node glow
                    using (var br = new SolidBrush(Color.FromArgb(150, Colors.OrangeBright)))
                        g.FillEllipse(br, pos.X - 5, pos.Y - 1, 10, 10);

                    // Node core
                    using (var br = new SolidBrush(Colors.RedMid))
                        g.FillEllipse(br, pos.X - 3.5f, pos.Y + 0.5f, 7, 7);
                }

                // === Network Links ===
                using (var pen = new Pen(Colors.OrangeDark, 1.5f) { DashStyle = DashStyle.Solid })
                {
                    // Horizontal links
                    g.DrawLine(pen, 30, 20, 45, 20);
                    g.DrawLine(pen, 55, 20, 70, 20);

                    // Diagonal links
                    g.DrawLine(pen, 50, 25, 40, 42);
                    g.DrawLine(pen, 50, 25, 60, 42);
                    g.DrawLine(pen, 40, 50, 50, 65);
                    g.DrawLine(pen, 60, 50, 50, 65);
                }

                // === Alert Symbol ===
                using (var br = new SolidBrush(Colors.RedBright))
                {
                    g.FillEllipse(br, 70, 12, 14, 14);
                }
                using (var br = new SolidBrush(Color.White))
                {
                    g.FillRectangle(br, 75, 16, 2, 4);
                    g.FillEllipse(br, 74, 22, 4, 2);
                }

                SaveImage(bmp, "threat_gateway.png");
            }
        }

        /// <summary>UDS Bruteforce: Advanced padlock icon</summary>
        private void GenerateThreatUds()
        {
            const int w = 100, h = 90;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(25, 25, 50, 50), 8, 3);

                // === Lock Shackle (arc) ===
                var shackleRect = new RectangleF(32, 20, 36, 32);
                using (var pen = new Pen(Colors.RedMid, 4))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, shackleRect, 0, 180);
                }
                using (var pen = new Pen(Color.FromArgb(100, Colors.RedBright), 2))
                    g.DrawArc(pen, shackleRect, 0, 180);

                // === Lock Body ===
                var lockBodyGrad = new LinearGradientBrush(
                    new PointF(30, 45), new PointF(70, 70),
                    Colors.RedMid, Colors.RedDark);
                var lockBody = new RectangleF(30, 45, 40, 28);
                FillRoundedRect(g, lockBodyGrad, lockBody, 4);
                lockBodyGrad.Dispose();

                using (var pen = new Pen(Colors.RedBright, 2))
                    DrawRoundedRectOutline(g, pen, lockBody, 4);

                // === Keyhole ===
                using (var br = new SolidBrush(Colors.Black))
                {
                    g.FillEllipse(br, 46, 54, 8, 8);
                    g.FillRectangle(br, 48, 62, 4, 5);
                }
                using (var br = new SolidBrush(Color.FromArgb(100, Colors.DarkGray)))
                {
                    g.FillEllipse(br, 47, 55, 6, 6);
                }

                // === Security Bands ===
                using (var pen = new Pen(Colors.OrangeMid, 1))
                {
                    g.DrawLine(pen, 32, 48, 68, 48);
                    g.DrawLine(pen, 32, 65, 68, 65);
                }

                // === Glow ===
                DrawGlowBorder(g, new RectangleF(28, 18, 44, 60), Colors.RedBright, 2);

                SaveImage(bmp, "threat_uds.png");
            }
        }

        /// <summary>Key Reuse: Duplicate keys with warning</summary>
        private void GenerateThreatKeys()
        {
            const int w = 100, h = 90;
            using (var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // === Shadow ===
                DrawDropShadow(g, new RectangleF(15, 15, 70, 60), 8, 3);

                // === First Key ===
                DrawAdvancedKey(g, 15, 25, Colors.RedMid, Colors.RedDark);

                // === Second Key (overlapping) ===
                g.TranslateTransform(55, 50);
                g.RotateTransform(30);
                g.TranslateTransform(-55, -50);
                DrawAdvancedKey(g, 40, 35, Colors.OrangeMid, Colors.OrangeDark);
                g.ResetTransform();

                // === Warning X ===
                using (var pen = new Pen(Colors.RedBright, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, 70, 65, 82, 78);
                    g.DrawLine(pen, 82, 65, 70, 78);
                }

                // === Glow ===
                DrawGlowBorder(g, new RectangleF(13, 13, 74, 77), Colors.RedBright, 2);

                SaveImage(bmp, "threat_keys.png");
            }
        }

        // ============================================================
        // HELPER FUNCTIONS - Drawing Utilities
        // ============================================================

        private void DrawAdvancedKey(Graphics g, float x, float y, Color mainColor, Color darkColor)
        {
            // Key head (circle)
            var headGrad = new LinearGradientBrush(
                new PointF(x, y), new PointF(x + 10, y + 10),
                mainColor, darkColor);
            g.FillEllipse(headGrad, x, y, 10, 10);
            headGrad.Dispose();

            using (var pen = new Pen(Color.FromArgb(150, Color.White), 1))
                g.DrawEllipse(pen, x + 0.5f, y + 0.5f, 9, 9);

            // Key shaft
            var shaftGrad = new LinearGradientBrush(
                new PointF(x + 10, y + 3), new PointF(x + 22, y + 7),
                mainColor, darkColor);
            g.FillRectangle(shaftGrad, x + 10, y + 3, 12, 4);
            shaftGrad.Dispose();

            // Key teeth
            using (var br = new SolidBrush(mainColor))
            {
                g.FillRectangle(br, x + 20, y + 3, 3, 3);
                g.FillRectangle(br, x + 24, y + 3, 3, 3);
            }
        }

        private GraphicsPath CreateShieldPath(RectangleF rect)
        {
            var path = new GraphicsPath();
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;

            path.AddLine(x, y, x + w, y);
            path.AddLine(x + w, y, x + w, y + h * 0.7f);
            path.AddBezier(
                x + w, y + h * 0.7f,
                x + w, y + h,
                x + w / 2, y + h * 0.95f,
                x, y + h * 0.7f);
            path.AddLine(x, y + h * 0.7f, x, y);
            path.CloseFigure();

            return path;
        }

        private GraphicsPath CreateCloudPath(RectangleF rect)
        {
            var path = new GraphicsPath();
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;

            path.AddArc(x, y + h * 0.3f, w * 0.25f, h * 0.6f, 180, 180);
            path.AddArc(x + w * 0.2f, y, w * 0.3f, h * 0.5f, 180, 180);
            path.AddArc(x + w * 0.5f, y - h * 0.1f, w * 0.35f, h * 0.6f, 180, 180);
            path.AddArc(x + w * 0.75f, y + h * 0.2f, w * 0.25f, h * 0.5f, 180, 180);
            path.AddLine(x + w, y + h, x, y + h);
            path.CloseFigure();

            return path;
        }

        private void DrawLockIcon(Graphics g, float cx, float cy, float size, Color color)
        {
            // Lock body
            using (var br = new SolidBrush(color))
                FillRoundedRect(g, br, new RectangleF(cx - size * 0.6f, cy - size * 0.4f, size * 1.2f, size * 0.8f), 2);

            // Lock shackle
            using (var pen = new Pen(color, 2))
                g.DrawArc(pen, cx - size * 0.5f, cy - size * 1.2f, size, size, 0, 180);

            // Keyhole
            using (var br = new SolidBrush(Colors.Black))
                g.FillEllipse(br, cx - 1.5f, cy - 1, 3, 3);
        }

        private void DrawDropShadow(Graphics g, RectangleF rect, float blur, float offset)
        {
            using (var br = new SolidBrush(Color.FromArgb(60, Colors.Black)))
            {
                FillRoundedRect(g, br, new RectangleF(rect.X + offset, rect.Y + offset, rect.Width, rect.Height), 5);
            }
        }

        private void DrawGlowBorder(Graphics g, RectangleF rect, Color glowColor, float iterations)
        {
            for (float i = iterations; i > 0; i--)
            {
                using (var pen = new Pen(Color.FromArgb((int)(255 * (1 - i / iterations) * 0.6f), glowColor), 1))
                {
                    float inflate = i * 1.5f;
                    var glowRect = new RectangleF(rect.X - inflate, rect.Y - inflate, rect.Width + inflate * 2, rect.Height + inflate * 2);
                    DrawRoundedRectOutline(g, pen, glowRect, 8);
                }
            }
        }

        private void FillRoundedRect(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using (var path = CreateRoundedRectPath(rect, radius))
                g.FillPath(brush, path);
        }

        private void DrawRoundedRectOutline(Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using (var path = CreateRoundedRectPath(rect, radius))
                g.DrawPath(pen, path);
        }

        private GraphicsPath CreateRoundedRectPath(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void SaveImage(Bitmap bmp, string filename)
        {
            try
            {
                string filepath = Path.Combine(_assetPath, filename);
                bmp.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                System.Diagnostics.Debug.WriteLine($"✓ Generated: {filename} (Ultra-High Quality)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error saving {filename}: {ex.Message}");
            }
        }
    }
}