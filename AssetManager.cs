using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AdasCyberTriageSim
{
    /// <summary>
    /// Manages loading and caching of game images to improve performance.
    /// Provides fallback rendering if images are not found.
    /// </summary>
    public class AssetManager
    {
        private readonly Dictionary<string, Image?> _imageCache = new Dictionary<string, Image?>();
        private readonly string _assetPath;

        public AssetManager()
        {
            _assetPath = Path.Combine(AppContext.BaseDirectory, "assets");
            Directory.CreateDirectory(_assetPath);
        }

        /// <summary>Path to the last successfully loaded asset image</summary>
        public string? LastLoadedPath { get; private set; }

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
                // Try primary assets path first
                string imagePath = Path.Combine(_assetPath, filename);
                if (File.Exists(imagePath))
                {
                    Image img = Image.FromFile(imagePath);
                    _imageCache[filename] = img;
                    LastLoadedPath = imagePath;
                    System.Diagnostics.Debug.WriteLine($"✓ Loaded asset: {filename} from {imagePath}");
                    return img;
                }

                // Try current directory/assets
                string currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", filename);
                if (File.Exists(currentDirPath))
                {
                    Image img = Image.FromFile(currentDirPath);
                    _imageCache[filename] = img;
                    LastLoadedPath = currentDirPath;
                    System.Diagnostics.Debug.WriteLine($"✓ Loaded asset: {filename} from {currentDirPath}");
                    return img;
                }

                // Walk up parent directories (up to 6 levels) to find assets folder
                DirectoryInfo? currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                for (int level = 0; level < 6 && currentDir != null; level++)
                {
                    string parentAssetPath = Path.Combine(currentDir.FullName, "assets", filename);
                    if (File.Exists(parentAssetPath))
                    {
                        Image img = Image.FromFile(parentAssetPath);
                        _imageCache[filename] = img;
                        LastLoadedPath = parentAssetPath;
                        System.Diagnostics.Debug.WriteLine($"✓ Loaded asset: {filename} from {parentAssetPath}");
                        return img;
                    }
                    currentDir = currentDir.Parent;
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
}