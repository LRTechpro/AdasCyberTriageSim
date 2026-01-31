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
    /// Custom panel for the lane runner game, supports background image.
    /// </summary>
    public class GamePanel : Panel
    {
        private Image? _backgroundImage;

        public void SetBackgroundImage(Image? img)
        {
            _backgroundImage = img;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_backgroundImage != null)
            {
                e.Graphics.DrawImage(_backgroundImage, this.ClientRectangle);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }
    }

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
        #region Resources (Assets)

        // ============================================================
        // ASSET MANAGER
        // ============================================================

        /// <summary>
        /// Manages loading and caching of game images to improve performance.
        /// Provides fallback rendering if images are not found.
        /// </summary>
        private class AssetManager
        {
            private readonly Dictionary<string, Image?> _imageCache = new Dictionary<string, Image?>();
            private readonly string _assetPath;

            public AssetManager()
            {
                _assetPath = Path.Combine(AppContext.BaseDirectory, "assets");
                Directory.CreateDirectory(_assetPath);
            }

            /// <summary>
            /// Loads an image from the assets folder with caching.
            /// Returns null if the image is not found (caller should use fallback rendering).
            /// </summary>
            public Image? LoadImage(string filename)
            {
                if (_imageCache.ContainsKey(filename))
                    return _imageCache[filename];

                try
                {
                    string imagePath = Path.Combine(_assetPath, filename);
                    if (File.Exists(imagePath))
                    {
                        Image img = Image.FromFile(imagePath);
                        _imageCache[filename] = img;
                        System.Diagnostics.Debug.WriteLine($"✓ Loaded asset: {filename}");
                        return img;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Error loading {filename}: {ex.Message}");
                }

                _imageCache[filename] = null;
                return null;
            }

            /// <summary>Disposes all cached images to free resources.</summary>
            public void Dispose()
            {
                foreach (var img in _imageCache.Values)
                    img?.Dispose();
                _imageCache.Clear();
            }
        }

        // ============================================================
        // REFERENCE TYPE EXAMPLE: AssetManager instance
        // ============================================================
        // EDUCATIONAL NOTE: AssetManager is a REFERENCE TYPE (class).
        // When we assign _assetManager, we're storing a REFERENCE (memory address) to the object,
        // not the object itself. Multiple variables can reference the same object in memory.
        // If we passed _assetManager to another method and that method modified it, 
        // our reference would see those changes because it points to the same object in heap memory.
        /// <summary>Manages loading and caching of game images</summary>
        private AssetManager? _assetManager;

        // ============================================================
        // REFERENCE TYPE EXAMPLES: Image objects (stored on heap)
        // ============================================================
        // EDUCATIONAL NOTE: Image is a REFERENCE TYPE (class from System.Drawing).
        // These variables store REFERENCES to Image objects in heap memory, not the images themselves.
        // When we pass an Image to a drawing method, we're passing the reference (pointer),
        // not copying the entire image data. This is memory-efficient for large objects.
        /// <summary>Player vehicle icon image</summary>
        private Image? _imgPlayerVehicle;

        /// <summary>Gate/checkpoint icon image</summary>
        private Image? _imgGate;

        /// <summary>Spinner/CAN Flood icon image</summary>
        private Image? _imgSpinner;

        // ============================================================
        // REFERENCE TYPE EXAMPLE: Dictionary collection
        // ============================================================
        // EDUCATIONAL NOTE: Dictionary is a REFERENCE TYPE (generic collection class).
        // It stores key-value pairs on the heap. The dictionary itself is referenced by this variable,
        // and modifications to the dictionary through any reference affect the same underlying object.
        /// <summary>Dictionary of threat images keyed by TokenType for efficient lookup during rendering</summary>
        private readonly Dictionary<TokenType, Image?> _threatImages = new Dictionary<TokenType, Image?>();

        /// <summary>Background image loaded from assets folder</summary>
        private Image? _backgroundImage;

        /// <summary>
        /// Generates game assets programmatically if they don't already exist.
        /// This is called once on application startup.
        /// </summary>
        private void GenerateAssetsIfNeeded()
        {
            string assetPath = Path.Combine(AppContext.BaseDirectory, "assets");

            // ============================================================
            // ARRAY TYPE EXAMPLE: Explicit string array for required assets
            // ============================================================
            // EDUCATIONAL NOTE: string[] is an ARRAY type (which is a REFERENCE TYPE).
            // Arrays in C# are reference types even when they contain value types.
            // The variable 'requiredAssets' stores a REFERENCE to an array object in heap memory.
            // The array has a fixed size (7 elements) determined at initialization.
            // Each element in the array is a string (also a reference type).
            // Arrays provide indexed access: requiredAssets[0], requiredAssets[1], etc.
            string[] requiredAssets =
            {
                "vehicle.png", "gate.png", "spinner.png",
                "threat_ota.png", "threat_gateway.png", "threat_uds.png", "threat_keys.png"
            };

            // EDUCATIONAL NOTE: 'allAssetsExist' is a VALUE TYPE (bool).
            // bool is a value type that stores true/false directly in stack memory.
            // When we assign or pass a bool, we're copying the actual value, not a reference.
            bool allAssetsExist = true;

            foreach (var asset in requiredAssets)
            {
                if (!File.Exists(Path.Combine(assetPath, asset)))
                {
                    allAssetsExist = false;
                    break;
                }
            }

            // Generate assets if any are missing
            if (!allAssetsExist)
            {
                try
                {
                    AppendLog("Generating game assets...");
                    var generator = new AssetGenerator(assetPath);
                    generator.GenerateAllAssets();
                    AppendLog("Game assets generated successfully!");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating assets: {ex.Message}");
                    AppendLog($"Warning: Could not generate assets, using fallback rendering");
                }
            }
        }

        /// <summary>
        /// Loads the background image from the assets folder.
        /// Logs to debug output if the image is found or missing.
        /// </summary>
        private void LoadBackgroundImage()
        {
            _backgroundImage = _assetManager?.LoadImage("bg_cyberlab.jpg");
            if (_backgroundImage == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠ Background image not found. Ensure bg_cyberlab.jpg exists in assets folder.");
            }
            else
            {
                try { AppendLog("Background image loaded successfully."); } catch { }
            }
        }

        /// <summary>
        /// Loads game asset images from the asset manager into memory.
        /// </summary>
        private void LoadGameAssets()
        {
            if (_assetManager == null) return;

            _imgPlayerVehicle = _assetManager.LoadImage("vehicle.png");
            _imgGate = _assetManager.LoadImage("gate.png");
            _imgSpinner = _assetManager.LoadImage("spinner.png");

            // ============================================================
            // ARRAY TYPE EXAMPLE: string array for threat filenames
            // ============================================================
            // EDUCATIONAL NOTE: Another string[] array demonstrating array usage.
            // This array has 4 elements containing threat image filenames.
            // We iterate through it to load each threat image into our dictionary,
            // keyed by TokenType for efficient lookup during rendering.
            
            // Map threat image files to their corresponding TokenTypes
            var threatImageMap = new (TokenType token, string filename)[]
            {
                (TokenType.ValidateOtaSignature, "threat_ota.png"),
                (TokenType.SegmentCanGateway, "threat_gateway.png"),
                (TokenType.LockUdsSession, "threat_uds.png"),
                (TokenType.RotateEcuKeys, "threat_keys.png")
            };

            foreach (var (token, filename) in threatImageMap)
            {
                _threatImages[token] = _assetManager.LoadImage(filename);
            }
        }

        #endregion

        #region Construction

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Initializes the main form with UI elements, background image, and the lane runner game panel.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            // Initialize asset manager
            _assetManager = new AssetManager();

            // Generate game assets on first run (if not already present)
            GenerateAssetsIfNeeded();

            // Load and display the cyberpunk background image
            LoadBackgroundImage();

            // Set the background image directly on the form
            if (_backgroundImage != null)
            {
                this.BackgroundImage = _backgroundImage;
                this.BackgroundImageLayout = ImageLayout.Stretch; // Fills entire form, stretching as needed
            }

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

            // Force initial repaint
            this.Invalidate();
        }

        #endregion

        #region State (Fields)

        // ============================================================
        // FIELD DECLARATIONS - DEMONSTRATING VALUE VS REFERENCE TYPES
        // ============================================================
        // This section demonstrates the fundamental difference between value types and reference types in C#.
        //
        // VALUE TYPES (stored on stack, contain actual data):
        //   - Primitives: int, float, double, bool, char, byte, etc.
        //   - Enums: ObjKind, GateMult, TokenType
        //   - Structs: RectangleF, Point, Size, Color (from System.Drawing)
        //   - When assigned or passed, the ENTIRE VALUE is COPIED
        //   - Each variable has its own independent copy of the data
        //
        // REFERENCE TYPES (stored on heap, variables hold memory addresses):
        //   - Classes: Button, Panel, Label, TextBox, List<T>, Dictionary<K,V>, string, Image
        //   - Arrays: string[], int[], LaneObj[]
        //   - When assigned or passed, only the REFERENCE (pointer) is copied
        //   - Multiple variables can reference the same object in memory
        // ============================================================

        // ---------- UI ELEMENTS (REFERENCE TYPES) ----------
        // EDUCATIONAL NOTE: All UI controls (Button, TextBox, Panel, Label) are REFERENCE TYPES (classes).
        // These variables store REFERENCES to objects created in heap memory.
        // When we add a control to this.Controls, we're passing the reference, not copying the control.

        /// <summary>Text box for displaying game event logs (REFERENCE TYPE)</summary>
        private TextBox? txtLog;

        /// <summary>Main panel where the lane runner game is rendered (REFERENCE TYPE)</summary>
        private Panel? pnlLaneRunner;

        /// <summary>HUD label displaying score, streak, posture, token, and remaining time (REFERENCE TYPE)</summary>
        private Label? lblRunnerHud;

        /// <summary>Button to start a new game run (REFERENCE TYPE)</summary>
        private Button? btnStartRun;

        // ---------- LANE RUNNER GAME STATE ----------
        // ============================================================
        // REFERENCE TYPE EXAMPLE: List<T> collection
        // ============================================================
        // EDUCATIONAL NOTE: List<LaneObj> is a REFERENCE TYPE (generic collection class).
        // _laneObjs stores a REFERENCE to a List object in heap memory.
        // The List itself contains references to LaneObj instances (also reference types).
        // When we pass _laneObjs to a method, we pass the reference, so modifications
        // in that method affect the same list we're referencing here.
        /// <summary>List of active game objects (Gates, Spinners, Threats) in the lane runner (REFERENCE TYPE)</summary>
        private readonly List<LaneObj> _laneObjs = new List<LaneObj>();

        // ============================================================
        // REFERENCE TYPE EXAMPLE: Random class instance
        // ============================================================
        // EDUCATIONAL NOTE: Random is a REFERENCE TYPE (class).
        // _runnerRng stores a reference to a Random object that maintains internal state
        // for pseudo-random number generation.
        /// <summary>Random number generator for spawning and game events (REFERENCE TYPE)</summary>
        private readonly Random _runnerRng = new Random();

        // ============================================================
        // REFERENCE TYPE EXAMPLE: Timer class
        // ============================================================
        // EDUCATIONAL NOTE: System.Windows.Forms.Timer is a REFERENCE TYPE (class).
        /// <summary>Game loop timer that ticks every 16ms for consistent gameplay (REFERENCE TYPE)</summary>
        private System.Windows.Forms.Timer? _tmrRun;

        // ============================================================
        // VALUE TYPE EXAMPLES: Primitive types (stored directly on stack)
        // ============================================================
        // EDUCATIONAL NOTE: bool, float, and int are all VALUE TYPES (primitives).
        // These variables store the ACTUAL DATA directly in stack memory.
        // When you assign one to another (e.g., int x = _score), the VALUE is COPIED.
        // Each variable has its own independent copy - changing one doesn't affect another.

        /// <summary>True when a game run is actively in progress (VALUE TYPE: bool)</summary>
        private bool _runActive = false;

        /// <summary>Objects scroll down the screen at this speed (pixels/frame); increases with difficulty (VALUE TYPE: float)</summary>
        private float _scrollSpeed = 4.0f;

        /// <summary>Time in milliseconds between each game tick (16ms = ~62.5 FPS) (VALUE TYPE: int)</summary>
        private int _tickMs = 16;

        /// <summary>Player's current score accumulated by collecting Gates and Threats with tokens (VALUE TYPE: int)</summary>
        private int _score = 0;

        /// <summary>Number of consecutive Gates collected without hitting a Threat (VALUE TYPE: int)</summary>
        private int _streak = 0;

        /// <summary>Vehicle health/integrity units (0 = game over); depleted by Spinners and unprotected Threats (VALUE TYPE: int)</summary>
        private int _postureUnits = 10;

        /// <summary>Milliseconds remaining in the current run (75 seconds = 75,000 ms to win) (VALUE TYPE: int)</summary>
        private int _runTimeLeftMs = 75_000;

        /// <summary>Countdown timer for spawning the next wave of objects (VALUE TYPE: int)</summary>
        private int _spawnCooldownMs = 0;

        /// <summary>Elapsed time in current run used to track difficulty progression (VALUE TYPE: int)</summary>
        private int _difficultyMs = 0;

        // ============================================================
        // VALUE TYPE EXAMPLE: Struct (composite value type)
        // ============================================================
        // EDUCATIONAL NOTE: RectangleF is a STRUCT, which is a VALUE TYPE.
        // Unlike classes, structs are stored directly on the stack (when local) or inline (when in a class).
        // When you pass a RectangleF to a method, the ENTIRE STRUCT is COPIED.
        // RectangleF contains four float values (X, Y, Width, Height) all stored together.
        /// <summary>Bounding rectangle of the player's vehicle at the bottom of the lane (VALUE TYPE: struct)</summary>
        private RectangleF _player;

        /// <summary>Target X coordinate for the player's smooth movement toward the mouse position (VALUE TYPE: float)</summary>
        private float _playerTargetX;

        /// <summary>True when the player is dragging the mouse to move the vehicle (VALUE TYPE: bool)</summary>
        private bool _dragging = false;

        // ============================================================
        // VALUE TYPE EXAMPLE: Nullable enum (TokenType?)
        // ============================================================
        // EDUCATIONAL NOTE: TokenType is an ENUM, which is a VALUE TYPE (based on int).
        // TokenType? is a nullable value type - it can hold a TokenType value OR null.
        // Nullable<T> is actually a struct that wraps a value type, making it nullable.
        // The actual TokenType value is still stored by value, not by reference.
        /// <summary>Currently active security token (if any) that protects against specific Threats (VALUE TYPE: nullable enum)</summary>
        private TokenType? _activeToken = null;

        /// <summary>Milliseconds remaining for the active token's protection (6 seconds when granted) (VALUE TYPE: int)</summary>
        private int _tokenTimeLeftMs = 0;

        // ============================================================
        // REFERENCE TYPE EXAMPLES: Font objects
        // ============================================================
        // EDUCATIONAL NOTE: Font is a REFERENCE TYPE (class from System.Drawing).
        // These variables store references to Font objects in heap memory.
        /// <summary>Font for HUD text (score, streak, time, etc.) (REFERENCE TYPE)</summary>
        private readonly Font _fHud = new Font("Segoe UI", 10, FontStyle.Bold);

        /// <summary>Font for drawing labels on game objects (REFERENCE TYPE)</summary>
        private readonly Font _fObj = new Font("Segoe UI", 9, FontStyle.Bold);

        // ============================================================
        // ARRAY TYPE EXAMPLE: String array for lane display names
        // ============================================================
        // EDUCATIONAL NOTE: This is an explicit ARRAY type demonstrating array usage in C#.
        // string[] is a REFERENCE TYPE (arrays are always reference types, even for value type elements).
        // The array object itself lives in heap memory, and this variable stores a reference to it.
        // This array has a fixed size of 3 elements, indexed from 0 to 2.
        // We use this array to provide readable names for the three lanes in our game.
        // Array syntax: Type[] variableName = new Type[size] or Type[] variableName = { element1, element2, ... }
        /// <summary>Display names for the three lanes (left, center, right) - demonstrates explicit array usage (REFERENCE TYPE: array)</summary>
        private readonly string[] _laneNames = { "Left Lane", "Center Lane", "Right Lane" };

        // ============================================================
        // UI LAYOUT CONSTANTS (VALUE TYPES)
        // ============================================================
        // EDUCATIONAL NOTE: These constants are all VALUE TYPES (int and float primitives).
        // const means the value is determined at compile-time and cannot be changed.

        /// <summary>Height of the HUD area at the top of the game panel (reserved for score, time, etc.) (VALUE TYPE: int)</summary>
        private const int HUD_HEIGHT = 52;

        /// <summary>Width of the legend box (VALUE TYPE: float)</summary>
        private const float LEGEND_WIDTH = 280f;

        /// <summary>Margin from right edge for legend (VALUE TYPE: float)</summary>
        private const float LEGEND_RIGHT_MARGIN = 12f;

        // ============================================================
        // PSEUDO-3D PERSPECTIVE CONSTANTS (VALUE TYPES)
        // ============================================================
        /// <summary>Y coordinate of the horizon line (where objects appear smallest) (VALUE TYPE: int)</summary>
        private const int HORIZON_Y = HUD_HEIGHT + 28;

        /// <summary>Y coordinate of the bottom of the road (player position area) (VALUE TYPE: int property)</summary>
        private int ROAD_BOTTOM_Y => pnlLaneRunner?.Height ?? 520;

        /// <summary>Left edge of road at bottom (widest point) (VALUE TYPE: int)</summary>
        private const int ROAD_LEFT_BOTTOM = 20;

        /// <summary>Right edge of road at bottom (widest point) (VALUE TYPE: int property)</summary>
        private int ROAD_RIGHT_BOTTOM => pnlLaneRunner?.Width - 20 ?? 520;

        /// <summary>Left edge of road at horizon (narrowest point) (VALUE TYPE: int)</summary>
        private const int ROAD_LEFT_TOP = 120;

        /// <summary>Right edge of road at horizon (narrowest point) (VALUE TYPE: int property)</summary>
        private int ROAD_RIGHT_TOP => pnlLaneRunner?.Width - 120 ?? 300;

        #endregion

        #region Model (Enums + LaneObj)

        // ============================================================
        // TYPE DEFINITIONS - ENUMS AND CLASSES
        // ============================================================

        // ============================================================
        // VALUE TYPE EXAMPLE: Enum (enumeration)
        // ============================================================
        // EDUCATIONAL NOTE: Enums are VALUE TYPES in C#.
        // They are stored as integers (int by default) and represent named constants.
        // ObjKind is based on int: Gate=0, Spinner=1, Threat=2.
        // When you assign or pass an enum, the INTEGER VALUE is copied, not a reference.
        // Enums prevent uncertainty and impossible values - only these three values are valid.
        /// <summary>Kind of lane object: Gate (collectible), Spinner (attack), or Threat (conditional attack) (VALUE TYPE: enum)</summary>
        private enum ObjKind { Gate, Spinner, Threat }

        // ============================================================
        // VALUE TYPE EXAMPLE: Enum with explicit integer values
        // ============================================================
        // EDUCATIONAL NOTE: GateMult is also a VALUE TYPE (enum).
        // Here we explicitly set the underlying integer values: X2=2, X3=3.
        // This enum demonstrates how enums can map meaningful names to specific numeric values.
        /// <summary>Point multiplier for Gates: X2 (65% chance) or X3 (35% chance) (VALUE TYPE: enum)</summary>
        private enum GateMult { X2 = 2, X3 = 3 }

        // ============================================================
        // VALUE TYPE EXAMPLE: Enum for security tokens
        // ============================================================
        /// <summary>
        /// Security tokens that protect the player from specific Threats (VALUE TYPE: enum).
        /// - ValidateOtaSignature: Protects against OTA downgrade attacks
        /// - SegmentCanGateway: Protects against gateway pivot/lateral ECU attacks
        /// - LockUdsSession: Protects against UDS brute force attacks
        /// - RotateEcuKeys: Protects against key reuse/replay attacks
        /// </summary>
        private enum TokenType
        {
            ValidateOtaSignature,    // = 0 (implicit)
            SegmentCanGateway,       // = 1
            LockUdsSession,          // = 2
            RotateEcuKeys            // = 3
        }

        // ============================================================
        // REFERENCE TYPE EXAMPLE: Class definition
        // ============================================================
        // EDUCATIONAL NOTE: LaneObj is a CLASS, which is a REFERENCE TYPE.
        // When we create a LaneObj instance with 'new LaneObj()', it's allocated in HEAP memory.
        // Variables of type LaneObj store REFERENCES (memory addresses) to these objects.
        // When we add a LaneObj to our _laneObjs list, we're adding a reference, not copying the object.
        // Multiple references can point to the same LaneObj instance in memory.
        //
        // INSIDE THE CLASS: This class contains BOTH value types AND reference types:
        //   - VALUE TYPES: ObjKind (enum), RectangleF (struct), float, int, GateMult (enum)
        //   - REFERENCE TYPES: string (Label field)
        //   - NULLABLE VALUE TYPES: TokenType? (nullable enum)
        //
        // When a LaneObj is created, its value type fields are stored INLINE with the object in heap memory,
        // while its string field stores a REFERENCE to a string object (also in heap).
        /// <summary>
        /// Represents a scrolling game object in the lanes (REFERENCE TYPE: class).
        /// Objects are Gates (grants points/tokens), Spinners (fleet-wide attacks),
        /// or Threats (lane-specific attacks that require specific tokens to neutralize).
        /// </summary>
        private sealed class LaneObj
        {
            /// <summary>Type of this object: Gate, Spinner, or Threat (VALUE TYPE: enum)</summary>
            public ObjKind Kind;

            /// <summary>Current position and size of the object on the screen (VALUE TYPE: struct)</summary>
            public RectangleF Rect;

            /// <summary>Vertical velocity (pixels per frame) — always positive (downward) (VALUE TYPE: float)</summary>
            public float Vy;

            /// <summary>Current rotation angle in radians (used for Spinners) (VALUE TYPE: float)</summary>
            public float Angle;

            /// <summary>Rotation speed in radians per frame (used for Spinners) (VALUE TYPE: float)</summary>
            public float AngVel;

            /// <summary>Label text displayed on the object (e.g., "x3", "CAN Flood", threat name) (REFERENCE TYPE: string)</summary>
            public string Label = "";

            // Gate-only fields
            /// <summary>[Gate only] Point multiplier for this Gate (X2 or X3) (VALUE TYPE: enum)</summary>
            public GateMult Mult;

            /// <summary>[Gate only] Security token granted when this Gate is collected (VALUE TYPE: nullable enum)</summary>
            public TokenType? TokenGranted;

            // Threat-only fields
            /// <summary>[Threat only] Required security token to safely neutralize this Threat (VALUE TYPE: nullable enum)</summary>
            public TokenType? TokenRequired;

            // Damage values
            /// <summary>Posture damage dealt when this object hits the player (0 for Gates) (VALUE TYPE: int)</summary>
            public int Damage;
        }

        #endregion

        #region UI Initialization

        // ============================================================
        // UI INITIALIZATION
        // ============================================================

        /// <summary>
        /// Creates and configures all UI elements for the lane runner game:
        /// - The main game panel with 3 lanes
        /// - The HUD label
        /// - The game loop timer
        /// - Loads game asset images
        /// </summary>
        private void BuildLaneRunnerUi()
        {
            // Load game asset images
            LoadGameAssets();

            // Create the main game panel with background image
            pnlLaneRunner = new GamePanel
            {
                Location = new Point(396, 78),
                Size = new Size(540, 520),
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

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(12, pnlLaneRunner.Bottom + 12),
                Size = new Size(pnlLaneRunner.Width, 80),
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(255, 18, 20, 28),
                ForeColor = HudTheme.C_TEXT
            };
            this.Controls.Add(txtLog);

            if (pnlLaneRunner is GamePanel gamePanel)
                gamePanel.SetBackgroundImage(_backgroundImage);

            // ===== CREATE LEGEND UI CONTAINER (OUTSIDE GAME CANVAS) =====
            var grpLegend = new GroupBox
            {
                Text = "LEGEND — What each object means",
                Location = new Point(pnlLaneRunner.Right + 12, pnlLaneRunner.Top),
                Size = new Size(320, 420),
                ForeColor = HudTheme.C_CYAN,
                BackColor = Color.FromArgb(220, 18, 20, 28),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            int legendY = 28;
            int legendItemHeight = 90;

            // Gate legend item
            var pnlGateIcon = new Panel
            {
                Location = new Point(16, legendY),
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(220, 30, 90, 140),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpLegend.Controls.Add(pnlGateIcon);

            var lblGateShort = new Label
            {
                Text = "Gate (Control): +Points +Token",
                Location = new Point(66, legendY),
                Size = new Size(240, 24),
                ForeColor = HudTheme.C_TEXT,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            grpLegend.Controls.Add(lblGateShort);

            var lblGateDesc = new Label
            {
                Text = "Deployed defense control. Grants tokens for threat mitigation (6s).",
                Location = new Point(66, legendY + 26),
                Size = new Size(240, 50),
                ForeColor = Color.FromArgb(200, HudTheme.C_TEXT),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9)
            };
            grpLegend.Controls.Add(lblGateDesc);

            // Threat legend item
            legendY += legendItemHeight;
            var pnlThreatIcon = new Panel
            {
                Location = new Point(16, legendY),
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(220, 120, 35, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpLegend.Controls.Add(pnlThreatIcon);

            var lblThreatShort = new Label
            {
                Text = "Threat (Attack): -Posture OR +Points",
                Location = new Point(66, legendY),
                Size = new Size(240, 24),
                ForeColor = HudTheme.C_TEXT,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            grpLegend.Controls.Add(lblThreatShort);

            var lblThreatDesc = new Label
            {
                Text = "Token-gated event. Neutralize with matching control; otherwise take damage.",
                Location = new Point(66, legendY + 26),
                Size = new Size(240, 50),
                ForeColor = Color.FromArgb(200, HudTheme.C_TEXT),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9)
            };
            grpLegend.Controls.Add(lblThreatDesc);

            // Spinner/CAN Flood legend item
            legendY += legendItemHeight;
            var pnlSpinnerIcon = new Panel
            {
                Location = new Point(16, legendY),
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(230, 180, 100, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpLegend.Controls.Add(pnlSpinnerIcon);

            var lblSpinnerShort = new Label
            {
                Text = "CAN Flood (DoS): -Posture",
                Location = new Point(66, legendY),
                Size = new Size(240, 24),
                ForeColor = HudTheme.C_TEXT,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            grpLegend.Controls.Add(lblSpinnerShort);

            var lblSpinnerDesc = new Label
            {
                Text = "Broadcast attack. Not blockable—avoid or absorb damage.",
                Location = new Point(66, legendY + 26),
                Size = new Size(240, 50),
                ForeColor = Color.FromArgb(200, HudTheme.C_TEXT),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9)
            };
            grpLegend.Controls.Add(lblSpinnerDesc);

            // Footer
            legendY += legendItemHeight;
            var lblFooter = new Label
            {
                Text = "Posture = vehicle security integrity. 0 = unsafe.",
                Location = new Point(16, legendY + 8),
                Size = new Size(280, 24),
                ForeColor = Color.FromArgb(180, HudTheme.C_TEXT),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };
            grpLegend.Controls.Add(lblFooter);

            this.Controls.Add(grpLegend);
        }
        #endregion

        #region Engine (Tick / Spawn / Collisions)

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

            // ============================================================
            // DEMONSTRATING VALUE TYPE ASSIGNMENT: Copying values directly
            // ============================================================
            // EDUCATIONAL NOTE: All these assignments involve VALUE TYPES (int, float, bool, nullable enum).
            // When we write "_score = 0", we're storing the actual integer value 0 in the _score variable.
            // There's no reference or pointer involved - the value itself is copied into the variable.
            // These operations are fast because they're just copying small amounts of data in stack memory.

            // Reset game state
            _laneObjs.Clear();
            _score = 0;              // VALUE TYPE assignment (int)
            _streak = 0;             // VALUE TYPE assignment (int)
            _postureUnits = 10;      // VALUE TYPE assignment (int)

            _runTimeLeftMs = 75_000; // VALUE TYPE assignment (int)
            _scrollSpeed = 4.0f;     // VALUE TYPE assignment (float)
            _difficultyMs = 0;       // VALUE TYPE assignment (int)

            _activeToken = null;     // VALUE TYPE assignment (nullable enum set to null)
            _tokenTimeLeftMs = 0;    // VALUE TYPE assignment (int)

            // ============================================================
            // DEMONSTRATING VALUE TYPE STRUCT: RectangleF
            // ============================================================
            // EDUCATIONAL NOTE: RectangleF is a STRUCT (VALUE TYPE).
            // When we create this rectangle, all four float values (X, Y, Width, Height) 
            // are stored directly in the _player variable's memory location.
            // No heap allocation or reference is involved.
            float w = 36, h = 42;  // VALUE TYPE local variables (float)
            _player = new RectangleF(pnlLaneRunner.Width / 2f - w / 2f, pnlLaneRunner.Height - 70, w, h);
            _playerTargetX = _player.X + _player.Width / 2f;

            _spawnCooldownMs = 0;
            _runActive = true;       // VALUE TYPE assignment (bool)

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
            // EDUCATIONAL NOTE: All parameters (score, posture, streak) are VALUE TYPES (int).
            // When this method is called, the VALUES are COPIED from the caller to these parameters.
            // The return value (string) is a REFERENCE TYPE, but the string itself is immutable.
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

            // EDUCATIONAL NOTE: dt is a VALUE TYPE (int) storing the time delta.
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

            // ============================================================
            // DEMONSTRATING REFERENCE TYPE ITERATION: List<LaneObj>
            // ============================================================
            // EDUCATIONAL NOTE: _laneObjs is a REFERENCE TYPE (List<LaneObj>).
            // 'o' in the loop is also a REFERENCE TYPE variable (LaneObj).
            // When we write "var o = _laneObjs[i]", we're copying the REFERENCE, not the object.
            // So 'o' points to the same LaneObj instance in memory as the one in the list.
            // Modifications to 'o' affect the actual object because they reference the same thing.

            // Update all objects (move them downward)
            for (int i = _laneObjs.Count - 1; i >= 0; i--)
            {
                var o = _laneObjs[i];  // o is a REFERENCE to a LaneObj
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
        /// Uses visual (perspective-scaled) rectangles for accurate collision detection.
        /// Handles three cases: Gates (always beneficial), Spinners (always harmful),
        /// and Threats (conditional based on token match).
        /// </summary>
        private void ResolveCollisions()
        {
            if (pnlLaneRunner == null) return;

            RectangleF visualPlayer = GetVisualRect(_player);

            for (int i = _laneObjs.Count - 1; i >= 0; i--)
            {
                var o = _laneObjs[i];
                RectangleF visualObj = GetVisualRect(o.Rect);

                if (!visualPlayer.IntersectsWith(visualObj)) continue;

                // ===== GATE COLLISION: Always grants points and possibly a token =====
                if (o.Kind == ObjKind.Gate)
                {
                    int mult = (int)o.Mult;
                    int gain = 25 * mult + (int)(_scrollSpeed * 2);
                    _score += gain;
                    _streak += 1;

                    if (o.TokenGranted.HasValue)
                    {
                        _activeToken = o.TokenGranted.Value;
                        _tokenTimeLeftMs = 6000;
                    }

                    System.Media.SystemSounds.Asterisk.Play();
                    _laneObjs.RemoveAt(i);
                    continue;
                }

                // ===== SPINNER COLLISION: Fleet-wide CAN flood attack, always harmful =====
                if (o.Kind == ObjKind.Spinner)
                {
                    _postureUnits -= o.Damage;
                    _streak = 0;
                    System.Media.SystemSounds.Exclamation.Play();

                    float push = (visualPlayer.X + visualPlayer.Width / 2f) < (visualObj.X + visualObj.Width / 2f) ? -18 : 18;
                    _player.X = Math.Clamp(_player.X + push, 18, pnlLaneRunner.Width - _player.Width - 18);

                    _laneObjs.RemoveAt(i);
                    continue;
                }

                // ===== THREAT COLLISION: Conditional damage based on token match =====
                if (o.Kind == ObjKind.Threat)
                {
                    bool protectedOk = o.TokenRequired.HasValue && _activeToken.HasValue && o.TokenRequired.Value == _activeToken.Value;

                    if (protectedOk)
                    {
                        int gain = 45 + (int)(_scrollSpeed * 4);
                        _score += gain;
                        _streak += 1;
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                    else
                    {
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

            // ============================================================
            // DEMONSTRATING ARRAY USAGE: float array for lane positions
            // ============================================================
            // EDUCATIONAL NOTE: This demonstrates explicit array usage with a numeric type.
            // float[] is an ARRAY type (REFERENCE TYPE containing value type elements).
            // The array object lives in heap memory, but each float element (lane0, lane1, lane2)
            // is stored by value inside the array. Array elements are accessed by index: lanes[0], lanes[1], lanes[2].

            // Calculate the three lane positions (divided into thirds with margins)
            float laneW = (pnlLaneRunner.Width - 60) / 3f;
            float lane0 = 20;
            float lane1 = lane0 + laneW;
            float lane2 = lane1 + laneW;

            float[] lanes = { lane0, lane1, lane2 };  // ARRAY TYPE: float[] (reference type with value type elements)

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

            // ============================================================
            // DEMONSTRATING REFERENCE TYPE CREATION: new LaneObj()
            // ============================================================
            // EDUCATIONAL NOTE: LaneObj is a REFERENCE TYPE (class).
            // 'new LaneObj()' allocates memory for a new LaneObj instance in the HEAP.
            // The variable 'o' stores a REFERENCE (memory address) to that object.
            // We then set its value type fields (Kind, Mult, Vy, Damage) directly by value.
            // When we add 'o' to _laneObjs, we're adding the REFERENCE, not copying the object.
            var o = new LaneObj
            {
                Kind = ObjKind.Gate,           // VALUE TYPE: enum
                Mult = mult,                    // VALUE TYPE: enum
                TokenGranted = token,           // VALUE TYPE: enum (in nullable wrapper)
                Label = $"{label}\n{control}",  // REFERENCE TYPE: string
                Rect = new RectangleF(laneX + 10, y, (pnlLaneRunner.Width - 60) / 3f - 20, 44),  // VALUE TYPE: struct
                Vy = _scrollSpeed,              // VALUE TYPE: float
                Damage = 0                      // VALUE TYPE: int
            };
            _laneObjs.Add(o);  // Adding a REFERENCE to the list
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
            // EDUCATIONAL NOTE: Explicit cast from int to enum (both value types)
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

        #endregion

        #region Rendering

        // ============================================================
        // LANE RUNNER: DRAWING
        // ============================================================

        /// <summary>
        /// Renders the entire game view: background, lane dividers, objects, player, and warnings.
        /// Includes pseudo-3D perspective rendering.
        /// </summary>
        private void DrawRunner(Graphics g)
        {
            if (pnlLaneRunner == null) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw semi-transparent overlay for readability (allows background to show through)
            using (var br = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillRectangle(br, pnlLaneRunner.ClientRectangle);

            // Draw HUD background (semi-transparent bar at top)
            using (var br = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                g.FillRectangle(br, 0, 0, pnlLaneRunner.Width, HUD_HEIGHT);

            // ===== DRAW PERSPECTIVE ROAD EDGES AND LANES =====
            using (var pen = new Pen(Color.FromArgb(50, HudTheme.C_CYAN), 2))
            {
                // Left road edge (converges to left-top)
                DrawPerspectiveLine(g, pen, ROAD_LEFT_TOP, ROAD_LEFT_BOTTOM);

                // Right road edge (converges to right-top)
                g.DrawLine(pen, ROAD_RIGHT_TOP, HORIZON_Y, ROAD_RIGHT_BOTTOM, ROAD_BOTTOM_Y);

                // Left lane divider (1/3 of road)
                float laneLeftBottom = ROAD_LEFT_BOTTOM + (ROAD_RIGHT_BOTTOM - ROAD_LEFT_BOTTOM) / 3f;
                float laneLeftTop = ROAD_LEFT_TOP + (ROAD_RIGHT_TOP - ROAD_LEFT_TOP) / 3f;
                g.DrawLine(pen, (int)laneLeftTop, HORIZON_Y, (int)laneLeftBottom, ROAD_BOTTOM_Y);

                // Right lane divider (2/3 of road)
                float laneRightBottom = ROAD_LEFT_BOTTOM + (ROAD_RIGHT_BOTTOM - ROAD_LEFT_BOTTOM) * 2f / 3f;
                float laneRightTop = ROAD_LEFT_TOP + (ROAD_RIGHT_TOP - ROAD_LEFT_TOP) * 2f / 3f;
                g.DrawLine(pen, (int)laneRightTop, HORIZON_Y, (int)laneRightBottom, ROAD_BOTTOM_Y);
            }

            // Draw all objects (skip any that are in the HUD reserved area)
            foreach (var o in _laneObjs)
            {
                // Skip objects entirely within or partially overlapping HUD area
                if (o.Rect.Bottom < HUD_HEIGHT)
                    continue;

                RectangleF visualRect = GetVisualRect(o.Rect);

                if (o.Kind == ObjKind.Gate)
                    DrawGateWithPerspective(g, o, visualRect);
                else if (o.Kind == ObjKind.Spinner)
                    DrawSpinnerWithPerspective(g, o, visualRect);
                else
                    DrawThreatWithPerspective(g, o, visualRect);
            }

            // Draw the player vehicle with perspective
            DrawPlayerVehicleWithPerspective(g);

            // Draw posture critical warning if health is low
            if (_postureUnits <= 3)
            {
                using var br = new SolidBrush(HudTheme.C_BAD);
                g.DrawString("POSTURE CRITICAL", _fHud, br, 12, pnlLaneRunner.Height - 32);
            }
        }

        /// <summary>
        /// Renders a Gate object with perspective scaling and shadow.
        /// </summary>
        private void DrawGateWithPerspective(Graphics g, LaneObj o, RectangleF visualRect)
        {
            if (_imgGate != null)
            {
                g.DrawImage(_imgGate, visualRect.X, visualRect.Y, visualRect.Width, visualRect.Height);
            }
            else
            {
                using var br = new SolidBrush(Color.FromArgb(220, 30, 90, 140));
                DrawRoundedRect(g, br, visualRect, 10);
                using var pen = new Pen(Color.FromArgb(200, HudTheme.C_CYAN), 2);
                DrawRoundedRectOutline(g, pen, visualRect, 10);
            }

            DrawWithDepthShadow(g, visualRect, Color.FromArgb(30, 90, 140));

            using var brText = new SolidBrush(HudTheme.C_TEXT);
            DrawCentered(g, o.Label, _fObj, brText, visualRect);
        }

        /// <summary>
        /// Renders a Threat object with perspective scaling and shadow.
        /// </summary>
        private void DrawThreatWithPerspective(Graphics g, LaneObj o, RectangleF visualRect)
        {
            // Lookup threat image using TokenRequired field instead of parsing Label
            // TokenRequired is always set when spawning threats and matches the dictionary key
            Image? threatImg = null;
            if (o.TokenRequired.HasValue && _threatImages.TryGetValue(o.TokenRequired.Value, out var img))
                threatImg = img;

            if (threatImg != null)
            {
                g.DrawImage(threatImg, visualRect.X, visualRect.Y, visualRect.Width, visualRect.Height);
            }
            else
            {
                using var br = new SolidBrush(Color.FromArgb(220, 120, 35, 35));
                DrawRoundedRect(g, br, visualRect, 12);
                using var pen = new Pen(Color.FromArgb(220, HudTheme.C_BAD), 2);
                DrawRoundedRectOutline(g, pen, visualRect, 12);
            }

            DrawWithDepthShadow(g, visualRect, Color.FromArgb(120, 35, 35));

            using var brText = new SolidBrush(HudTheme.C_TEXT);
            DrawCentered(g, o.Label, _fObj, brText, visualRect);
        }

        /// <summary>
        /// Renders a Spinner object with perspective scaling, rotation, and shadow.
        /// </summary>
        private void DrawSpinnerWithPerspective(Graphics g, LaneObj o, RectangleF visualRect)
        {
            // Calculate center point for rotation
            var cx = visualRect.X + visualRect.Width / 2f;
            var cy = visualRect.Y + visualRect.Height / 2f;

            // Apply rotation transformation
            g.TranslateTransform(cx, cy);
            g.RotateTransform(o.Angle * 57.2958f);
            g.TranslateTransform(-cx, -cy);

            if (_imgSpinner != null)
            {
                g.DrawImage(_imgSpinner, visualRect.X, visualRect.Y, visualRect.Width, visualRect.Height);
            }
            else
            {
                using var br = new SolidBrush(Color.FromArgb(230, 30, 30, 30));
                g.FillRectangle(br, visualRect.X, visualRect.Y, visualRect.Width, visualRect.Height);
                using var pen = new Pen(Color.FromArgb(220, HudTheme.C_WARN), 2);
                g.DrawRectangle(pen, visualRect.X, visualRect.Y, visualRect.Width, visualRect.Height);
            }

            g.ResetTransform();

            DrawWithDepthShadow(g, visualRect, Color.FromArgb(80, 80, 0));

            using var brText = new SolidBrush(HudTheme.C_WARN);
            g.DrawString("CAN FLOOD", _fObj, brText, visualRect.X + 10, visualRect.Y - 18);
        }

        /// <summary>
        /// Draws the player vehicle with perspective scaling and shadow.
        /// </summary>
        private void DrawPlayerVehicleWithPerspective(Graphics g)
        {
            RectangleF visualPlayer = GetVisualRect(_player);

            if (_imgPlayerVehicle != null)
            {
                g.DrawImage(_imgPlayerVehicle, visualPlayer.X, visualPlayer.Y, visualPlayer.Width, visualPlayer.Height);
            }
            else
            {
                using (var br = new SolidBrush(Color.FromArgb(220, 70, 130, 255)))
                    DrawRoundedRect(g, br, visualPlayer, 10);
                using (var pen = new Pen(Color.FromArgb(220, HudTheme.ColorYellow), 2))
                    DrawRoundedRectOutline(g, pen, visualPlayer, 10);
            }

            DrawWithDepthShadow(g, visualPlayer, Color.FromArgb(70, 130, 255));
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
        /// Draws a simple depth shadow under an object to give a sense of elevation.
        /// </summary>
        private static void DrawWithDepthShadow(Graphics g, RectangleF rect, Color color)
        {
            using var br = new SolidBrush(Color.FromArgb(60, color));
            var shadowRect = new RectangleF(rect.X + 3, rect.Y + 5, rect.Width, rect.Height);
            g.FillEllipse(br, shadowRect);
        }

        /// <summary>
        /// Draws a line from the top of the road (horizon) to the bottom of the road (player area) with perspective.
        /// </summary>
        private void DrawPerspectiveLine(Graphics g, Pen pen, int xTop, int xBottom)
        {
            g.DrawLine(pen, xTop, HORIZON_Y, xBottom, ROAD_BOTTOM_Y);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates the visual rectangle for an object based on depth perspective.
        /// Objects farther up (closer to horizon) appear smaller; objects at bottom appear larger.
        /// </summary>
        private RectangleF GetVisualRect(RectangleF baseRect)
        {
            if (pnlLaneRunner == null)
                return baseRect;

            // Calculate the vertical position as a ratio between horizon and road bottom
            float y = baseRect.Y + baseRect.Height / 2f;
            float t = (y - HORIZON_Y) / (ROAD_BOTTOM_Y - HORIZON_Y);
            t = Math.Clamp(t, 0f, 1f);

            // Perspective scale: objects at the horizon are 55% of their size, at bottom 100%
            float scale = 0.55f + 0.45f * t;

            // Center point
            float cx = baseRect.X + baseRect.Width / 2f;
            float cy = baseRect.Y + baseRect.Height / 2f;

            float newWidth = baseRect.Width * scale;
            float newHeight = baseRect.Height * scale;

            return new RectangleF(
                cx - newWidth / 2f,
                cy - newHeight / 2f,
                newWidth,
                newHeight
            );
        }

        /// <summary>
        /// Appends a message to the log TextBox, if available.
        /// </summary>
        private void AppendLog(string message)
        {
            if (txtLog == null) return;
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
        #endregion
    }
}