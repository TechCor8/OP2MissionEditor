using B83.Image.BMP;
using OP2UtilityDotNet;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Systems
{
	/// <summary>
	/// Manages texture loading, processing, and caching.
	/// </summary>
	public class TextureManager
	{
		private static Dictionary<string, Texture2D> m_Tilesets = new Dictionary<string, Texture2D>();              // Key = Filename
		private static Dictionary<string, Texture2D> m_MinimapTilesets = new Dictionary<string, Texture2D>();       // Key = Filename

		private static string m_ArchiveDirectory;
		private static ResourceManager m_ResourceManager;


		/// <summary>
		/// Initializes the texture manager for use.
		/// </summary>
		public static void Initialize()
		{
			UserPrefs.onChangedPrefsCB += OnChangedPrefs;

			m_ArchiveDirectory = UserPrefs.gameDirectory;
			m_ResourceManager = new ResourceManager(m_ArchiveDirectory);
		}

		private static void OnChangedPrefs()
		{
			if (UserPrefs.gameDirectory == m_ArchiveDirectory)
				return;

			// Set resource manager to new game directory
			m_ResourceManager.Dispose();
			m_ArchiveDirectory = UserPrefs.gameDirectory;
			m_ResourceManager = new ResourceManager(m_ArchiveDirectory);
		}

		/// <summary>
		/// Returns the specified tileset texture, or null if not found.
		/// </summary>
		public static Texture2D GetTileset(string tilesetFileName)
		{
			Texture2D texture;
			if (m_Tilesets.TryGetValue(tilesetFileName, out texture))
				return texture;

			return null;
		}

		/// <summary>
		/// Returns the specified minimap tileset texture, or null if not found.
		/// </summary>
		public static Texture2D GetMinimapTileset(string tilesetFileName)
		{
			Texture2D texture;
			if (m_MinimapTilesets.TryGetValue(tilesetFileName, out texture))
				return texture;

			return null;
		}

		/// <summary>
		/// Returns the specified tileset texture from cache.
		/// If not found, attempts to load it from the archive directory.
		/// </summary>
		public static Texture2D LoadTileset(string tilesetFileName)
		{
			// Get tile bitmap data
			Texture2D texture;
			if (m_Tilesets.TryGetValue(tilesetFileName, out texture))
				return texture;

			// Image not found in cache. Fetch from archive.
			byte[] tileImageData = m_ResourceManager.GetResource(tilesetFileName + ".bmp", true);
			if (tileImageData == null)
			{
				Debug.LogError("Could not find resource: " + tilesetFileName);
				return null;
			}

			texture = GetTextureFromBMP(tileImageData);
			m_Tilesets.Add(tilesetFileName, texture);

			return texture;
		}

		private static Texture2D GetTextureFromBMP(byte[] bmpData)
		{
			BMPLoader bmpLoader = new BMPLoader();
			BMPImage bmpImage = bmpLoader.LoadBMP(bmpData);
			return bmpImage.ToTexture2D();
		}

		/// <summary>
		/// Returns the specified minimap tileset texture from cache.
		/// If not found, attempts to load the main tileset and generate a minimap tileset from it.
		/// </summary>
		public static Texture2D LoadMinimapTileset(string tilesetFileName, int tileSetNumTiles)
		{
			// Search cache for minimap texture
			Texture2D texture;
			if (m_MinimapTilesets.TryGetValue(tilesetFileName, out texture))
				return texture;

			// Load full tileset
			Texture2D tilesetTexture = LoadTileset(tilesetFileName);
			if (tilesetTexture == null)
				return null;

			// Create minimap tileset
			Texture2D minimapTileset = new Texture2D(1, tileSetNumTiles, TextureFormat.ARGB32, false);
			Color[] scaledTiles = new Color[tileSetNumTiles];

			int tileHeight = tilesetTexture.height / tileSetNumTiles;

			for (int i=0; i < tileSetNumTiles; ++i)
				scaledTiles[i] = GetTileColor(tilesetTexture, i * tileHeight, tilesetTexture.width, tileHeight);

			minimapTileset.SetPixels(scaledTiles);
			minimapTileset.Apply();
			m_MinimapTilesets.Add(tilesetFileName, minimapTileset);

			return minimapTileset;
		}

		private static Color GetTileColor(Texture2D tileTexture, int tileOffset, int tileWidth, int tileHeight)
		{
			Color[] tilePixels = tileTexture.GetPixels(0,tileOffset, tileWidth, tileHeight, 0);

			Color avgColor = Color.black;

			for (int i=0; i < tilePixels.Length; ++i)
				avgColor += tilePixels[i];

			avgColor /= tileWidth*tileHeight;

			return avgColor;
		}

		/// <summary>
		/// Gets the sprite for a tile.
		/// </summary>
		public static Sprite GetTileSprite(Texture2D tilesetTexture, int tilesetNumTiles, int tileGraphicIndex)
		{
			// Get image offset
			int tileSize = tilesetTexture.height / tilesetNumTiles;
			int inverseTileIndex = tilesetNumTiles-tileGraphicIndex-1;
			int texOffset = inverseTileIndex * tileSize;

			// Create sprite
			return Sprite.Create(tilesetTexture, new Rect(0,texOffset, tilesetTexture.width,tilesetTexture.width), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
		}

		/// <summary>
		/// Clears the texture cache.
		/// </summary>
		public static void ClearCache()
		{
			m_Tilesets.Clear();
			m_MinimapTilesets.Clear();
		}

		/// <summary>
		/// Releases the texture manager.
		/// </summary>
		public static void Release()
		{
			UserPrefs.onChangedPrefsCB -= OnChangedPrefs;

			if (m_ResourceManager != null)
				m_ResourceManager.Dispose();

			m_ResourceManager = null;

			ClearCache();
		}
	}
}
