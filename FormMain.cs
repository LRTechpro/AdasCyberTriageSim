using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// ADAS Cyber Triage Simulator - An arcade-style lane-runner game that teaches 
    /// automotive cybersecurity concepts through interactive gameplay.
    /// 
    /// Core Mechanics:
    /// - Player controls a vehicle at the bottom of the screen
    /// - Security objects (Gates, Threats, Spinners) scroll down from the top
    /// - Colliding with Gates grants points and security tokens
    /// - Colliding with Threats/Spinners damages posture unless protected by a token
    /// - Game lasts 75 seconds with increasing difficulty
    /// - Difficulty increases every 3 seconds, speeding up object scrolling and damage
    /// </summary>
    public partial class FormMain : Form
    {
        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Initializes the main form with UI elements, background image, and the lane runner game panel.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            
            // Load and display the cyberpunk background image
            LoadBackgroundImage();
            this.Paint += (_, e) => DrawBackgroundImage(e.Graphics);
            this.DoubleBuffered = true;
            
            // Build the lane runner game UI (panel, HUD, timer)
            BuildLaneRunnerUi();
            
            // Add the lane runner panel to the form
            this.Controls.Add(pnlLaneRunner);
            
            // Add and configure the "Start Run" button to begin a game session
            btnStartRun = new Button
            {
                Text = "Start Run",
                Location = new Point(12, 12),
                Size = new Size(100, 40),
                BackColor = HudTheme.ColorCyan,
                ForeColor = HudTheme.ColorBg,
                Font = HudTheme.FontHeading,
                FlatStyle = FlatStyle.Flat
            };
            btnStartRun.Click += (_, __) => StartRun();
            this.Controls.Add(btnStartRun);
        }

        /// <summary>
        /// Loads the background image from the assets folder.
        /// Logs to debug output if the image is found or missing.
        /// </summary>
        private void LoadBackgroundImage()
        {
            try
            {
                string imagePath = Path.Combine(AppContext.BaseDirectory, "assets", "bg_cyberlab.jpg");
                System.Diagnostics.Debug.WriteLine($"Looking for image at: {imagePath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(imagePath)}");
                
                if (File.Exists(imagePath))
                {
                    _backgroundImage = Image.FromFile(imagePath);
                    System.Diagnostics.Debug.WriteLine("Background image loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Background image NOT FOUND");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading background: {ex.Message}");
            }
        }

        /// <summary>
        /// Draws the background image to fill the entire form.
        /// </summary>
        private void DrawBackgroundImage(Graphics g)
        {
            if (_backgroundImage != null)
            {
                g.DrawImage(_backgroundImage, 0, 0, this.Width, this.Height);
            }
        }

        /// <summary>
        /// Draws a cyberpunk-style background with gradient, grid pattern, and accent lines.
        /// (Currently unused but available for alternative background styling)
        /// </summary>
        private void DrawCyberpunkBackground(Graphics g)
        {
            // Create a dark gradient background (dark blue to darker blue)
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(10, 12, 16),
                Color.FromArgb(20, 25, 35),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }

            // Draw a grid pattern for a cyberpunk aesthetic
            using (var pen = new Pen(Color.FromArgb(30, 100, 150, 200), 1))
            {
                int gridSize = 40;
                for (int x = 0; x < this.Width; x += gridSize)
                    g.DrawLine(pen, x, 0, x, this.Height);
                for (int y = 0; y < this.Height; y += gridSize)
                    g.DrawLine(pen, 0, y, this.Width, y);
            }

            // Draw side accent lines in cyan
            using (var pen = new Pen(Color.FromArgb(80, HudTheme.C_CYAN), 2))
            {
                g.DrawLine(pen, 0, 0, 0, this.Height);
                g.DrawLine(pen, this.Width - 1, 0, this.Width - 1, this.Height);
            }
        }

        // ============================================================
        // FIELD DECLARATIONS
        // ============================================================

        // ---------- UI ELEMENTS ----------
        /// <summary>Text box for displaying game event logs</summary>
        private TextBox? txtLog;
        
        /// <summary>Main panel where the lane runner game is rendered</summary>
        private Panel? pnlLaneRunner;
        
        /// <summary>HUD label displaying score, streak, posture, token, and remaining time</summary>
        private Label? lblRunnerHud;
        
        /// <summary>Button to start a new game run</summary>
        private Button? btnStartRun;

        // ---------- LANE RUNNER GAME STATE ----------
        /// <summary>List of active game objects (Gates, Spinners, Threats) in the lane runner</summary>
        private readonly List<LaneObj> _laneObjs = new List<LaneObj>();
        
        /// <summary>Random number generator for spawning and game events</summary>
        private readonly Random _runnerRng = new Random();
        
        /// <summary>Game loop timer that ticks every 16ms for consistent gameplay</summary>
        private System.Windows.Forms.Timer? _tmrRun;

        /// <summary>True when a game run is actively in progress</summary>
        private bool _runActive = false;
        
        /// <summary>Objects scroll down the screen at this speed (pixels/frame); increases with difficulty</summary>
        private float _scrollSpeed = 4.0f;
        
        /// <summary>Time in milliseconds between each game tick (16ms = ~62.5 FPS)</summary>
        private int _tickMs = 16;

        /// <summary>Player's current score accumulated by collecting Gates and Threats with tokens</summary>
        private int _score = 0;
        
        /// <summary>Number of consecutive Gates collected without hitting a Threat</summary>
        private int _streak = 0;
        
        /// <summary>Vehicle health/integrity units (0 = game over); depleted by Spinners and unprotected Threats</summary>
        private int _postureUnits = 10;
        
        /// <summary>Milliseconds remaining in the current run (75 seconds = 75,000 ms to win)</summary>
        private int _runTimeLeftMs = 75_000;

        /// <summary>Countdown timer for spawning the next wave of objects</summary>
        private int _spawnCooldownMs = 0;
        
        /// <summary>Elapsed time in current run used to track difficulty progression</summary>
        private int _difficultyMs = 0;

        /// <summary>Bounding rectangle of the player's vehicle at the bottom of the lane</summary>
        private RectangleF _player;
        
        /// <summary>Target X coordinate for the player's smooth movement toward the mouse position</summary>
        private float _playerTargetX;
        
        /// <summary>True when the player is dragging the mouse to move the vehicle</summary>
        private bool _dragging = false;

        /// <summary>Currently active security token (if any) that protects against specific Threats</summary>
        private TokenType? _activeToken = null;
        
        /// <summary>Milliseconds remaining for the active token's protection (6 seconds when granted)</summary>
        private int _tokenTimeLeftMs = 0;

        /// <summary>Font for HUD text (score, streak, time, etc.)</summary>
        private readonly Font _fHud = new Font("Segoe UI", 10, FontStyle.Bold);
        
        /// <summary>Font for drawing labels on game objects</summary>
        private readonly Font _fObj = new Font("Segoe UI", 9, FontStyle.Bold);

        // ============================================================
        // TYPE DEFINITIONS
        // ============================================================

        /// <summary>Kind of lane object: Gate (collectible), Spinner (attack), or Threat (conditional attack)</summary>
        private enum ObjKind { Gate, Spinner, Threat }
        
        /// <summary>Point multiplier for Gates: X2 (65% chance) or X3 (35% chance)</summary>
        private enum GateMult { X2 = 2, X3 = 3 }

        /// <summary>
        /// Security tokens that protect the player from specific Threats.
        /// - ValidateOtaSignature: Protects against OTA downgrade attacks
        /// - SegmentCanGateway: Protects against gateway pivot/lateral ECU attacks
        /// - LockUdsSession: Protects against UDS brute force attacks
        /// - RotateEcuKeys: Protects against key reuse/replay attacks
        /// </summary>
        private enum TokenType
        {
            ValidateOtaSignature,
            SegmentCanGateway,
            LockUdsSession,
            RotateEcuKeys
        }

        /// <summary>
        /// Represents a scrolling game object in the lanes.
        /// Objects are Gates (grants points/tokens), Spinners (fleet-wide attacks),
        /// or Threats (lane-specific attacks that require specific tokens to neutralize).
        /// </summary>
        private sealed class LaneObj
        {
            /// <summary>Type of this object: Gate, Spinner, or Threat</summary>
            public ObjKind Kind;
            
            /// <summary>Current position and size of the object on the screen</summary>
            public RectangleF Rect;
            
            /// <summary>Vertical velocity (pixels per frame) — always positive (downward)</summary>
            public float Vy;
            
            /// <summary>Current rotation angle in radians (used for Spinners)</summary>
            public float Angle;
            
            /// <summary>Rotation speed in radians per frame (used for Spinners)</summary>
            public float AngVel;
            
            /// <summary>Label text displayed on the object (e.g., "x3", "CAN Flood", threat name)</summary>
            public string Label = "";

            // Gate-only fields
            /// <summary>[Gate only] Point multiplier for this Gate (X2 or X3)</summary>
            public GateMult Mult;
            
            /// <summary>[Gate only] Security token granted when this Gate is collected</summary>
            public TokenType? TokenGranted;

            // Threat-only fields
            /// <summary>[Threat only] Required security token to safely neutralize this Threat</summary>
            public TokenType? TokenRequired;

            // Damage values
            /// <summary>Posture damage dealt when this object hits the player (0 for Gates)</summary>
            public int Damage;
        }

        // ============================================================
        // UI INITIALIZATION
        // ============================================================

        /// <summary>
        /// Creates and configures all UI elements for the lane runner game:
        /// - The main game panel with 3 lanes
        /// - The HUD label
        /// - The game loop timer
        /// </summary>
        private void BuildLaneRunnerUi()
        {
            // Create the main game panel with dark background
            pnlLaneRunner = new Panel
            {
                Location = new Point(396, 78),
                Size = new Size(540, 520),
                BackColor = Color.FromArgb(220, 10, 12, 16),
            };
            pnlLaneRunner.Paint += (_, e) => DrawRunner(e.Graphics);
            
            // Handle mouse drag for player movement
            pnlLaneRunner.MouseDown += (_, e) => { _dragging = true; _playerTargetX = e.X; };
            pnlLaneRunner.MouseMove += (_, e) => { if (_dragging) _playerTargetX = e.X; };
            pnlLaneRunner.MouseUp += (_, __) => { _dragging = false; };
            
            // Enable double buffering to reduce flicker
            typeof(Control).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(pnlLaneRunner, true);

            // Create the HUD label for displaying game stats
            lblRunnerHud = new Label
            {
                AutoSize = false,
                Location = new Point(12, 10),
                Size = new Size(pnlLaneRunner.Width - 24, 24),
                ForeColor = HudTheme.C_TEXT,
                BackColor = Color.Transparent,
                Font = _fHud,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlLaneRunner.Controls.Add(lblRunnerHud);

            // Keep the panel hidden until a run starts
            pnlLaneRunner.Visible = false;
            pnlLaneRunner.BringToFront();

            // Create the game loop timer (ticks every 16ms for ~62.5 FPS)
            _tmrRun = new System.Windows.Forms.Timer();
            _tmrRun.Interval = _tickMs;
            _tmrRun.Tick += (_, __) => TickRun();
        }

        // ============================================================
        // LANE RUNNER: CONTROL
        // ============================================================

        /// <summary>
        /// Starts a new game run. Resets all game state and begins the game loop.
        /// </summary>
        private void StartRun()
        {
            if (pnlLaneRunner == null || _tmrRun == null) return;
            
            // Show the game panel and set focus
            pnlLaneRunner.Visible = true;
            pnlLaneRunner.Focus();

            // Reset game state
            _laneObjs.Clear();
            _score = 0;
            _streak = 0;
            _postureUnits = 10;

            _runTimeLeftMs = 75_000;
            _scrollSpeed = 4.0f;
            _difficultyMs = 0;

            _activeToken = null;
            _tokenTimeLeftMs = 0;

            // Initialize the player vehicle (36x42 pixels, centered horizontally at bottom)
            float w = 36, h = 42;
            _player = new RectangleF(pnlLaneRunner.Width / 2f - w / 2f, pnlLaneRunner.Height - 70, w, h);
            _playerTargetX = _player.X + _player.Width / 2f;

            _spawnCooldownMs = 0;
            _runActive = true;

            // Log the start event
            try { AppendLog("ARCADE RUN START → Maintain integrity. Collect controls. Avoid attacks."); } catch { }
            
            // Start the game loop
            _tmrRun.Start();
            pnlLaneRunner.Invalidate();
        }

        /// <summary>
        /// Ends the current game run and displays results.
        /// Calculates the player's grade based on score, posture, and streak.
        /// </summary>
        private void EndRun(string reason)
        {
            if (_tmrRun == null || pnlLaneRunner == null) return;
            
            // Stop the game loop
            _tmrRun.Stop();
            _runActive = false;

            // Hide the game panel
            pnlLaneRunner.Visible = false;

            // Log the end event
            try { AppendLog($"ARCADE RUN END → {reason} | Score={_score} Streak={_streak} Posture={_postureUnits}"); } catch { }

            // Calculate and display results
            string grade = GradeRun(_score, _postureUnits, _streak);
            MessageBox.Show(
                $"Run Complete\n\nReason: {reason}\n\nScore: {_score}\nStreak: {_streak}\nPosture Units: {_postureUnits}\nGrade: {grade}",
                "Arcade Result",
                MessageBoxButtons.OK,
                reason.Contains("Failed", StringComparison.OrdinalIgnoreCase) ? MessageBoxIcon.Warning : MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Calculates a letter grade for the run based on total performance.
        /// Grade formula: score + (posture × 40) + (streak × 15)
        /// </summary>
        private static string GradeRun(int score, int posture, int streak)
        {
            int g = score + posture * 40 + streak * 15;
            if (g >= 2200) return "S";
            if (g >= 1600) return "A";
            if (g >= 1100) return "B";
            if (g >= 700) return "C";
            return "D";
        }

        // ============================================================
        // LANE RUNNER: GAME LOOP
        // ============================================================

        /// <summary>
        /// Main game loop called every 16ms.
        /// Handles: object movement, collision detection, difficulty scaling, token expiration, 
        /// spawning, and win/lose conditions.
        /// </summary>
        private void TickRun()
        {
            if (!_runActive || pnlLaneRunner == null || lblRunnerHud == null) return;

            int dt = _tickMs;
            _runTimeLeftMs -= dt;
            _difficultyMs += dt;

            // Increase difficulty (scroll speed) every 3 seconds, capping at 9.5
            if (_difficultyMs % 3000 == 0) _scrollSpeed = Math.Min(_scrollSpeed + 0.35f, 9.5f);

            // Decrease active token duration
            if (_tokenTimeLeftMs > 0)
            {
                _tokenTimeLeftMs -= dt;
                if (_tokenTimeLeftMs <= 0)
                {
                    _tokenTimeLeftMs = 0;
                    _activeToken = null;
                }
            }

            // Spawn waves of objects at adaptive intervals (shorter at higher speeds)
            _spawnCooldownMs -= dt;
            if (_spawnCooldownMs <= 0)
            {
                SpawnWave();
                int baseCd = 900;
                int cd = (int)Math.Max(350, baseCd - (_scrollSpeed - 4f) * 80);
                _spawnCooldownMs = cd;
            }

            // Smoothly move the player toward the target X position (mouse-following behavior)
            float targetLeft = _playerTargetX - _player.Width / 2f;
            targetLeft = Math.Clamp(targetLeft, 18, pnlLaneRunner.Width - _player.Width - 18);
            float lerp = 0.22f;
            _player.X = _player.X + (targetLeft - _player.X) * lerp;

            // Update all objects (move them downward)
            for (int i = _laneObjs.Count - 1; i >= 0; i--)
            {
                var o = _laneObjs[i];
                o.Rect = new RectangleF(o.Rect.X, o.Rect.Y + o.Vy, o.Rect.Width, o.Rect.Height);

                // Rotate Spinners
                if (o.Kind == ObjKind.Spinner) o.Angle += o.AngVel;

                // Remove objects that have scrolled off screen
                if (o.Rect.Y > pnlLaneRunner.Height + 80)
                    _laneObjs.RemoveAt(i);
            }

            // Check for collisions between player and objects
            ResolveCollisions();

            // Check for losing conditions
            if (_postureUnits <= 0)
            {
                EndRun("Battle Failed: Vehicle Unsafe (posture depleted)");
                return;
            }
            if (_runTimeLeftMs <= 0)
            {
                EndRun("Incident Contained: Run Time Complete");
                return;
            }

            // Update HUD display
            lblRunnerHud.Text =
                $"Score: {_score}   Streak: {_streak}   Posture: {_postureUnits}   " +
                $"Token: {(_activeToken?.ToString() ?? "None")}   Time: {Math.Max(0, _runTimeLeftMs / 1000)}s";

            // Trigger redraw
            pnlLaneRunner.Invalidate();
        }

        /// <summary>
        /// Checks for collisions between the player and all objects.
        /// Handles three cases: Gates (always beneficial), Spinners (always harmful),
        /// and Threats (conditional based on token match).
        /// </summary>
        private void ResolveCollisions()
        {
            if (pnlLaneRunner == null) return;
            
            RectangleF p = _player;

            for (int i = _laneObjs.Count - 1; i >= 0; i--)
            {
                var o = _laneObjs[i];
                if (!p.IntersectsWith(o.Rect)) continue;

                // ===== GATE COLLISION: Always grants points and possibly a token =====
                if (o.Kind == ObjKind.Gate)
                {
                    // Award points based on multiplier (x2 or x3) and current scroll speed
                    int mult = (int)o.Mult;
                    int gain = 25 * mult + (int)(_scrollSpeed * 2);
                    _score += gain;
                    _streak += 1;

                    // Grant the associated security token (6-second duration)
                    if (o.TokenGranted.HasValue)
                    {
                        _activeToken = o.TokenGranted.Value;
                        _tokenTimeLeftMs = 6000;
                    }

                    // Play success sound
                    System.Media.SystemSounds.Asterisk.Play();
                    _laneObjs.RemoveAt(i);
                    continue;
                }

                // ===== SPINNER COLLISION: Fleet-wide CAN flood attack, always harmful =====
                if (o.Kind == ObjKind.Spinner)
                {
                    // Damage posture and reset streak
                    _postureUnits -= o.Damage;
                    _streak = 0;
                    System.Media.SystemSounds.Exclamation.Play();

                    // Push the player away from the Spinner
                    float push = (p.X + p.Width / 2f) < (o.Rect.X + o.Rect.Width / 2f) ? -18 : 18;
                    _player.X = Math.Clamp(_player.X + push, 18, pnlLaneRunner.Width - _player.Width - 18);

                    _laneObjs.RemoveAt(i);
                    continue;
                }

                // ===== THREAT COLLISION: Conditional damage based on token match =====
                if (o.Kind == ObjKind.Threat)
                {
                    // Check if player has the required token to neutralize this Threat
                    bool protectedOk = o.TokenRequired.HasValue && _activeToken.HasValue && o.TokenRequired.Value == _activeToken.Value;

                    if (protectedOk)
                    {
                        // Token matches: safe neutralization, grant points
                        int gain = 45 + (int)(_scrollSpeed * 4);
                        _score += gain;
                        _streak += 1;
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                    else
                    {
                        // No token or wrong token: take damage and lose streak
                        _postureUnits -= o.Damage;
                        _streak = 0;
                        System.Media.SystemSounds.Hand.Play();
                    }

                    _laneObjs.RemoveAt(i);
                    continue;
                }
            }
        }

        /// <summary>
        /// Spawns a wave of objects based on difficulty level.
        /// At higher speeds (late game), spawning patterns shift toward more dangerous combinations.
        /// </summary>
        private void SpawnWave()
        {
            if (pnlLaneRunner == null) return;
            
            // Calculate the three lane positions (divided into thirds with margins)
            float laneW = (pnlLaneRunner.Width - 60) / 3f;
            float lane0 = 20;
            float lane1 = lane0 + laneW;
            float lane2 = lane1 + laneW;

            float[] lanes = { lane0, lane1, lane2 };
            int lanePickA = _runnerRng.Next(0, 3);
            int lanePickB = _runnerRng.Next(0, 3);

            int t = _runnerRng.Next(100);
            bool late = _scrollSpeed >= 6.2f;

            // Spawn patterns based on difficulty and randomness
            if (!late && t < 55)
            {
                // Early game: mostly Gates with occasional Threats
                SpawnGate(lanes[lanePickA], y: -60);
                if (_runnerRng.Next(100) < 30) SpawnThreat(lanes[lanePickB], y: -140);
            }
            else if (t < 35)
            {
                // Mid game: double Gates
                SpawnGate(lanes[lanePickA], y: -60);
                SpawnGate(lanes[lanePickB], y: -140);
            }
            else if (t < 70)
            {
                // Mid-late: Threats with Spinners
                SpawnThreat(lanes[lanePickA], y: -60);
                if (_runnerRng.Next(100) < 40) SpawnSpinner(y: -160);
            }
            else
            {
                // Late game: dangerous combinations
                SpawnSpinner(y: -60);
                if (_runnerRng.Next(100) < 45) SpawnThreat(lanes[lanePickB], y: -170);
            }
        }

        /// <summary>
        /// Spawns a Gate object in a specific lane.
        /// Gates are beneficial: they grant points and a security token.
        /// </summary>
        private void SpawnGate(float laneX, float y)
        {
            if (pnlLaneRunner == null) return;

            // Randomly assign multiplier (65% chance for x2, 35% for x3)
            var mult = _runnerRng.Next(100) < 65 ? GateMult.X2 : GateMult.X3;
            TokenType token = PickTokenForGate();
            string label = mult == GateMult.X3 ? "x3" : "x2";
            
            // Get a display name for the token
            string control = token switch
            {
                TokenType.ValidateOtaSignature => "Validate OTA Signature",
                TokenType.SegmentCanGateway => "Segment CAN Gateway",
                TokenType.LockUdsSession => "Lock UDS Session",
                TokenType.RotateEcuKeys => "Rotate ECU Keys",
                _ => "Control"
            };

            var o = new LaneObj
            {
                Kind = ObjKind.Gate,
                Mult = mult,
                TokenGranted = token,
                Label = $"{label}\n{control}",
                Rect = new RectangleF(laneX + 10, y, (pnlLaneRunner.Width - 60) / 3f - 20, 44),
                Vy = _scrollSpeed,
                Damage = 0
            };
            _laneObjs.Add(o);
        }

        /// <summary>
        /// Spawns a Spinner object (CAN flood attack) that spans the full width.
        /// Damage increases at higher difficulties (2-3 damage points).
        /// </summary>
        private void SpawnSpinner(float y)
        {
            if (pnlLaneRunner == null) return;
            
            float w = pnlLaneRunner.Width - 120;
            float x = 60;
            var o = new LaneObj
            {
                Kind = ObjKind.Spinner,
                Label = "CAN Flood",
                Rect = new RectangleF(x, y, w, 20),
                Vy = _scrollSpeed + 0.2f,
                Damage = _scrollSpeed >= 7.5f ? 3 : 2,
                AngVel = 0.22f + (float)_runnerRng.NextDouble() * 0.15f,
                Angle = 0
            };
            _laneObjs.Add(o);
        }

        /// <summary>
        /// Spawns a Threat object in a specific lane.
        /// Threats require a matching security token to neutralize safely.
        /// </summary>
        private void SpawnThreat(float laneX, float y)
        {
            if (pnlLaneRunner == null) return;
            
            (TokenType req, string label) = PickThreat();
            var o = new LaneObj
            {
                Kind = ObjKind.Threat,
                TokenRequired = req,
                Label = label,
                Rect = new RectangleF(laneX + 18, y, (pnlLaneRunner.Width - 60) / 3f - 36, 54),
                Vy = _scrollSpeed + 0.1f,
                Damage = _scrollSpeed >= 7.5f ? 4 : 3
            };
            _laneObjs.Add(o);
        }

        /// <summary>
        /// Randomly picks a security token for a Gate (uniform distribution across 4 types).
        /// </summary>
        private TokenType PickTokenForGate()
        {
            int r = _runnerRng.Next(4);
            return (TokenType)r;
        }

        /// <summary>
        /// Randomly selects a Threat type and returns the required token and display label.
        /// Maps threat types to automotive security vulnerabilities.
        /// </summary>
        private (TokenType req, string label) PickThreat()
        {
            int r = _runnerRng.Next(4);
            return r switch
            {
                0 => (TokenType.ValidateOtaSignature, "OTA Downgrade\n(Hash mismatch)"),
                1 => (TokenType.SegmentCanGateway, "Gateway Pivot\n(ECU lateral)"),
                2 => (TokenType.LockUdsSession, "UDS Bruteforce\n(0x27 seed/key)"),
                _ => (TokenType.RotateEcuKeys, "Key Reuse\n(Replay risk)"),
            };
        }

        // ============================================================
        // LANE RUNNER: DRAWING
        // ============================================================

        /// <summary>
        /// Renders the entire game view: background, lane dividers, objects, player, and warnings.
        /// </summary>
        private void DrawRunner(Graphics g)
        {
            if (pnlLaneRunner == null) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw dark background
            using (var br = new SolidBrush(Color.FromArgb(255, 12, 14, 18)))
                g.FillRectangle(br, pnlLaneRunner.ClientRectangle);

            // Draw lane divider lines to show the three lanes
            using (var pen = new Pen(Color.FromArgb(50, HudTheme.C_CYAN), 2))
            {
                int top = 40;
                int bottom = pnlLaneRunner.Height;
                int left = 20;
                int right = pnlLaneRunner.Width - 20;
                g.DrawLine(pen, left, top, left, bottom);
                g.DrawLine(pen, right, top, right, bottom);

                float laneW = (pnlLaneRunner.Width - 60) / 3f;
                g.DrawLine(pen, 20 + laneW, top, 20 + laneW, bottom);
                g.DrawLine(pen, 20 + laneW * 2, top, 20 + laneW * 2, bottom);
            }

            // Draw all objects
            foreach (var o in _laneObjs)
            {
                if (o.Kind == ObjKind.Gate)
                    DrawGate(g, o);
                else if (o.Kind == ObjKind.Spinner)
                    DrawSpinner(g, o);
                else
                    DrawThreat(g, o);
            }

            // Draw the player vehicle
            using (var br = new SolidBrush(Color.FromArgb(220, 70, 130, 255)))
                DrawRoundedRect(g, br, _player, 10);
            using (var pen = new Pen(Color.FromArgb(220, HudTheme.C_CYAN), 2))
                DrawRoundedRectOutline(g, pen, _player, 10);

            // Draw posture critical warning if health is low
            if (_postureUnits <= 3)
            {
                using var br = new SolidBrush(HudTheme.C_BAD);
                g.DrawString("POSTURE CRITICAL", _fHud, br, 12, pnlLaneRunner.Height - 32);
            }
        }

        /// <summary>
        /// Renders a Gate object with a rounded rectangle and centered label.
        /// </summary>
        private void DrawGate(Graphics g, LaneObj o)
        {
            using var br = new SolidBrush(Color.FromArgb(220, 30, 90, 140));
            DrawRoundedRect(g, br, o.Rect, 10);
            using var pen = new Pen(Color.FromArgb(200, HudTheme.C_CYAN), 2);
            DrawRoundedRectOutline(g, pen, o.Rect, 10);

            using var brText = new SolidBrush(HudTheme.C_TEXT);
            DrawCentered(g, o.Label, _fObj, brText, o.Rect);
        }

        /// <summary>
        /// Renders a Threat object with a rounded rectangle and centered label.
        /// </summary>
        private void DrawThreat(Graphics g, LaneObj o)
        {
            using var br = new SolidBrush(Color.FromArgb(220, 120, 35, 35));
            DrawRoundedRect(g, br, o.Rect, 12);
            using var pen = new Pen(Color.FromArgb(220, HudTheme.C_BAD), 2);
            DrawRoundedRectOutline(g, pen, o.Rect, 12);

            using var brText = new SolidBrush(HudTheme.C_TEXT);
            DrawCentered(g, o.Label, _fObj, brText, o.Rect);
        }

        /// <summary>
        /// Renders a Spinner object with rotation animation and centered label.
        /// </summary>
        private void DrawSpinner(Graphics g, LaneObj o)
        {
            // Calculate center point for rotation
            var cx = o.Rect.X + o.Rect.Width / 2f;
            var cy = o.Rect.Y + o.Rect.Height / 2f;

            // Apply rotation transformation
            g.TranslateTransform(cx, cy);
            g.RotateTransform(o.Angle * 57.2958f);
            g.TranslateTransform(-cx, -cy);

            // Draw rotating rectangle
            using var br = new SolidBrush(Color.FromArgb(230, 30, 30, 30));
            g.FillRectangle(br, o.Rect.X, o.Rect.Y, o.Rect.Width, o.Rect.Height);
            using var pen = new Pen(Color.FromArgb(220, HudTheme.C_WARN), 2);
            g.DrawRectangle(pen, o.Rect.X, o.Rect.Y, o.Rect.Width, o.Rect.Height);

            // Reset transformation
            g.ResetTransform();

            // Draw label above the spinner
            using var brText = new SolidBrush(HudTheme.C_WARN);
            g.DrawString("CAN FLOOD", _fObj, brText, o.Rect.X + 10, o.Rect.Y - 18);
        }

        /// <summary>
        /// Draws centered text within a rectangle with proper formatting.
        /// </summary>
        private static void DrawCentered(Graphics g, string text, Font font, Brush brush, RectangleF rect)
        {
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(text, font, brush, rect, sf);
        }

        /// <summary>
        /// Draws a filled rounded rectangle.
        /// </summary>
        private static void DrawRoundedRect(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using var path = RoundedPath(rect, radius);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Draws a rounded rectangle outline.
        /// </summary>
        private static void DrawRoundedRectOutline(Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using var path = RoundedPath(rect, radius);
            g.DrawPath(pen, path);
        }

        /// <summary>
        /// Creates a GraphicsPath for a rounded rectangle.
        /// </summary>
        private static GraphicsPath RoundedPath(RectangleF r, float radius)
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

        /// <summary>
        /// Appends a timestamped message to the log text box.
        /// </summary>
        private void AppendLog(string msg)
        {
            if (txtLog != null)
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
        }

        /// <summary>Background image loaded from assets folder</summary>
        private Image? _backgroundImage;
    }
}