using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	public class MapRenderer : MonoBehaviour
	{
		[SerializeField] private Tilemap m_GridOverlay			= default;
		[SerializeField] private Sprite m_GridOverlayTile		= default;

		private Tilemap m_Tilemap;

		public Texture2D minimapTexture			{ get; private set; }

		public delegate void OnMapProgressCallback(MapRenderer mapRenderer, string state, float progress);
		public delegate void OnMapCallback(MapRenderer mapRenderer);

		public event OnMapProgressCallback onMapRefreshProgressCB;
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
		public void Refresh(System.Action onCompleteCB=null)
		{
			StartCoroutine(_Refresh(onCompleteCB));
		}

		private IEnumerator _Refresh(System.Action onCompleteCB)
		{
			// Create grid overlay tile
			Tile gridOverlayTile = ScriptableObject.CreateInstance<Tile>();
			gridOverlayTile.sprite = m_GridOverlayTile;
			gridOverlayTile.color = Color.white;

			m_Tilemap.ClearAllTiles();
			m_GridOverlay.ClearAllTiles();

			uint mapWidth = UserData.current.map.GetWidthInTiles();
			uint mapHeight = UserData.current.map.GetHeightInTiles();

			minimapTexture = new Texture2D((int)mapWidth, (int)mapHeight, TextureFormat.ARGB32, false);

			Vector3Int[] cellPositions = new Vector3Int[(int)(mapWidth*mapHeight)];
			TileBase[] cellTiles = new TileBase[(int)(mapWidth*mapHeight)];
			int index = 0;

			int updateFrequency = (int)mapWidth / 100;

			for (uint x=0; x < mapWidth; ++x)
			{
				if (updateFrequency == 0 || x % updateFrequency == 0)
				{
					onMapRefreshProgressCB?.Invoke(this, "Reading tiles", (float)(x*mapHeight) / (mapWidth*mapHeight));
					yield return null;
				}

				for (uint y=0; y < mapHeight; ++y)
				{
					ulong tileSetIndex = UserData.current.map.GetTilesetIndex(x, y);
					//ulong tileMappingIndex = UserData.current.map.GetTileMappingIndex(x, y);
					int tileImageIndex = (int)UserData.current.map.GetImageIndex(x, y);

					string tileSetPath = UserData.current.map.GetTilesetSourceFilename(tileSetIndex);
					int tileSetNumTiles = (int)UserData.current.map.GetTilesetSourceNumTiles(tileSetIndex);

					// Get tile texture
					Texture2D texture = TextureManager.LoadTileset(tileSetPath);
					if (texture == null)
					{
						onCompleteCB?.Invoke();
						onMapRefreshedCB?.Invoke(this);
						throw new System.Exception("Could not find resource: " + tileSetPath);
					}

					// Get image offset
					int tileSize = texture.height / tileSetNumTiles;
					int inverseTileIndex = tileSetNumTiles-tileImageIndex-1;
					int texOffset = inverseTileIndex * tileSize;

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
					Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
					Color color = mTexture.GetPixel(0, inverseTileIndex);
					minimapTexture.SetPixel(cellPosition.x, cellPosition.y, color);
						
					++index;
				}
			}

			m_GridOverlay.color = UserPrefs.gridOverlayColor;

			onMapRefreshProgressCB?.Invoke(this, "Setting tiles", 1);
			yield return null;

			// Set tiles
			m_Tilemap.SetTiles(cellPositions, cellTiles);

			TileBase[] overlayTiles = new TileBase[cellPositions.Length];
			for (int i = 0; i < overlayTiles.Length; ++i)
				overlayTiles[i] = gridOverlayTile;

			m_GridOverlay.SetTiles(cellPositions, overlayTiles);

			// Refresh tiles
			//m_Tilemap.RefreshAllTiles();
			//m_GridOverlay.RefreshAllTiles();

			minimapTexture.Apply();

			onCompleteCB?.Invoke();
			onMapRefreshedCB?.Invoke(this);
		}

		public void RefreshTile(Vector3Int tileXY)
		{
			ulong x = (ulong)tileXY.x;
			ulong y = (ulong)tileXY.y;
			ulong tileSetIndex = UserData.current.map.GetTilesetIndex(x, y);
			//ulong tileMappingIndex = UserData.current.map.GetTileMappingIndex(x, y);
			int tileImageIndex = (int)UserData.current.map.GetImageIndex(x, y);

			string tileSetPath = UserData.current.map.GetTilesetSourceFilename(tileSetIndex);
			int tileSetNumTiles = (int)UserData.current.map.GetTilesetSourceNumTiles(tileSetIndex);

			// Get tile texture
			Texture2D texture = TextureManager.LoadTileset(tileSetPath);
			if (texture == null)
				throw new System.Exception("Could not find resource: " + tileSetPath);
			
			// Get image offset
			int tileSize = texture.height / tileSetNumTiles;
			int inverseTileIndex = tileSetNumTiles-tileImageIndex-1;
			int texOffset = inverseTileIndex * tileSize;

			// Create sprite
			Sprite tileSprite = Sprite.Create(texture, new Rect(0,texOffset, texture.width,texture.width), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
						
			// Load sprite into tile map
			Tile tile = ScriptableObject.CreateInstance<Tile>();
			tile.sprite = tileSprite;
			tile.color = Color.white;

			Vector3Int cellPosition = new Vector3Int((int)x,m_Tilemap.size.y-(int)y-1,0);

			// Set minimap pixel
			Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
			Color color = mTexture.GetPixel(0, inverseTileIndex);
			minimapTexture.SetPixel(cellPosition.x, cellPosition.y, color);

			// Set tiles
			m_Tilemap.SetTile(cellPosition, tile);
			minimapTexture.Apply();
		}
	}
}
