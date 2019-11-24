using B83.Image.BMP;
using OP2UtilityDotNet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	public class MapRenderer : MonoBehaviour
	{
		[SerializeField] private Tilemap m_GridOverlay			= default;
		[SerializeField] private Sprite m_GridOverlayTile		= default;

		private Tilemap m_Tilemap;

		private Dictionary<string, Texture2D> m_TextureCache = new Dictionary<string, Texture2D>();		// Key = Filename

		public Texture2D minimapTexture			{ get; private set; }

		public delegate void OnMapCallback(MapRenderer mapRenderer);

		public event OnMapCallback onMapRefreshedCB;


		private void Awake()
		{
			m_Tilemap = GetComponent<Tilemap>();

			UserPrefs.onChangedPrefsCB += OnChangedPrefs;
		}

		private void OnChangedPrefs()
		{
			// Refresh grid color
			m_GridOverlay.color = UserPrefs.gridOverlayColor;
		}

		/// <summary>
		/// Refreshes the map view with data from UserData.
		/// </summary>
		public void Refresh()
		{
			// Create grid overlay tile
			Tile gridOverlayTile = ScriptableObject.CreateInstance<Tile>();
			gridOverlayTile.sprite = m_GridOverlayTile;
			gridOverlayTile.color = Color.white;

			m_Tilemap.ClearAllTiles();
			m_GridOverlay.ClearAllTiles();

			//using (ResourceManager resourceManager = new ResourceManager(UserPrefs.GameDirectory))
			using (VolFile volFile = new VolFile(UserPrefs.GameDirectory + "/" + "art.vol"))
			{
				uint mapWidth = UserData.current.map.GetWidthInTiles();
				uint mapHeight = UserData.current.map.GetHeightInTiles();

				minimapTexture = new Texture2D((int)mapWidth, (int)mapHeight, TextureFormat.ARGB32, false);

				Vector3Int[] cellPositions = new Vector3Int[(int)(mapWidth*mapHeight)];
				TileBase[] cellTiles = new TileBase[(int)(mapWidth*mapHeight)];
				int index = 0;

				for (uint x=0; x < mapWidth; ++x)
				{
					for (uint y=0; y < mapHeight; ++y)
					{
						ulong tileSetIndex = UserData.current.map.GetTilesetIndex(x, y);
						//ulong tileMappingIndex = UserData.current.map.GetTileMappingIndex(x, y);
						ulong tileImageIndex = UserData.current.map.GetImageIndex(x, y);

						string tileSetPath = UserData.current.map.GetTilesetSourceFilename(tileSetIndex);
						ulong tileSetNumTiles = UserData.current.map.GetTilesetSourceNumTiles(tileSetIndex);
						
						// Get tile bitmap data
						Texture2D texture;
						if (!m_TextureCache.TryGetValue(tileSetPath, out texture))
						{
							// Image not found in cache. Fetch from archive.
							//ulong mysize = resourceManager.GetResourceSize(tileSetPath + ".bmp", true);
							byte[] tileImageData = volFile.ReadFileByName(tileSetPath + ".bmp");//resourceManager.GetResource(tileSetPath + ".bmp", true);
							if (tileImageData == null)
								throw new System.Exception("Could not find resource: " + tileSetPath);

							texture = GetTextureFromBMP(tileImageData);
							m_TextureCache.Add(tileSetPath, texture);
						}
						
						// Get image offset
						int tileSize = texture.height / (int)tileSetNumTiles;
						int texOffset = (int)(tileSetNumTiles-tileImageIndex-1) * tileSize;

						// Create sprite
						Sprite tileSprite = Sprite.Create(texture, new Rect(0,texOffset, texture.width,texture.width), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
						
						// Load sprite into tile map
						Tile tile = ScriptableObject.CreateInstance<Tile>();
						tile.sprite = tileSprite;
						tile.color = Color.white;

						Vector3Int cellPosition = new Vector3Int((int)x,(int)(mapHeight-y-1),0);

						cellPositions[index] = cellPosition;
						cellTiles[index] = tile;

						// Set minimap pixel
						Color color = GetTileColor(tileSprite, texOffset, tileSize);
						minimapTexture.SetPixel(cellPosition.x, cellPosition.y, color);
						
						++index;
					}
				}

				m_GridOverlay.color = UserPrefs.gridOverlayColor;

				// Set tiles
				m_Tilemap.SetTiles(cellPositions, cellTiles);

				TileBase[] overlayTiles = new TileBase[cellPositions.Length];
				for (int i = 0; i < overlayTiles.Length; ++i)
					overlayTiles[i] = gridOverlayTile;

				m_GridOverlay.SetTiles(cellPositions, overlayTiles);
				
				// Refresh tiles
				m_Tilemap.RefreshAllTiles();
				m_GridOverlay.RefreshAllTiles();
				minimapTexture.Apply();
			}

			onMapRefreshedCB?.Invoke(this);
		}

		private Texture2D GetTextureFromBMP(byte[] bmpData)
		{
			BMPLoader bmpLoader = new BMPLoader();
			BMPImage bmpImage = bmpLoader.LoadBMP(bmpData);
			return bmpImage.ToTexture2D();
		}

		private Color GetTileColor(Sprite tileSprite, int tileOffset, int tileSize)
		{
			Color[] tilePixels = tileSprite.texture.GetPixels(0,tileOffset, tileSize, tileSize, 0);

			Color avgColor = Color.black;

			for (int i=0; i < tilePixels.Length; ++i)
				avgColor += tilePixels[i];

			avgColor /= tileSize*tileSize;

			return avgColor;
		}
	}
}
