using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	public class MapRenderer : MonoBehaviour
	{
		[SerializeField] private Tilemap m_CellTypeMap			= default;
		[SerializeField] private Tilemap m_GridOverlay			= default;
		[SerializeField] private Sprite m_GridOverlayTile		= default;
		[SerializeField] private UnitRenderer m_UnitRenderer	= default;

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

			HideCellTypeMap();
		}

		private void OnChangedPrefs()
		{
			// Refresh grid color
			m_GridOverlay.color = UserPrefs.gridOverlayColor;
			m_GridOverlay.gameObject.SetActive(UserPrefs.isGridOverlayVisible);
		}

		public void ShowCellTypeMap()
		{
			m_CellTypeMap.gameObject.SetActive(true);
			m_Tilemap.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		}

		public void HideCellTypeMap()
		{
			m_CellTypeMap.gameObject.SetActive(false);
			m_Tilemap.color = Color.white;
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
			m_CellTypeMap.ClearAllTiles();

			uint mapWidth = UserData.current.map.GetWidthInTiles();
			uint mapHeight = UserData.current.map.GetHeightInTiles();

			minimapTexture = new Texture2D((int)mapWidth, (int)mapHeight, TextureFormat.ARGB32, false);

			Vector3Int[] cellPositions = new Vector3Int[(int)(mapWidth*mapHeight)];
			TileBase[] cellTiles = new TileBase[(int)(mapWidth*mapHeight)];
			TileBase[] cellTypes = new TileBase[(int)(mapWidth*mapHeight)];
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
					cellTypes[index] = GetCellTypeTile(UserData.current.map.GetCellType(x, y));

					// Set minimap pixel
					Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
					Color color = mTexture.GetPixel(0, inverseTileIndex);
					minimapTexture.SetPixel(cellPosition.x, cellPosition.y, color);
						
					++index;
				}
			}

			m_GridOverlay.color = UserPrefs.gridOverlayColor;
			m_GridOverlay.gameObject.SetActive(UserPrefs.isGridOverlayVisible);

			onMapRefreshProgressCB?.Invoke(this, "Setting tiles", 1);
			yield return null;

			// Set tiles
			m_Tilemap.SetTiles(cellPositions, cellTiles);

			TileBase[] overlayTiles = new TileBase[cellPositions.Length];
			for (int i = 0; i < overlayTiles.Length; ++i)
				overlayTiles[i] = gridOverlayTile;

			m_GridOverlay.SetTiles(cellPositions, overlayTiles);

			m_CellTypeMap.SetTiles(cellPositions, cellTypes);

			// Apply minimap texture
			minimapTexture.Apply();

			onMapRefreshProgressCB?.Invoke(this, "Creating units", 1);
			yield return null;

			// Create units
			m_UnitRenderer.Refresh(() =>
			{
				// Inform listeners that we are done
				onCompleteCB?.Invoke();
				onMapRefreshedCB?.Invoke(this);
			});
		}

		public void RefreshTile(Vector2Int tileXY)
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
						
			// Load sprite into tile
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
			m_CellTypeMap.SetTile(cellPosition, GetCellTypeTile(UserData.current.map.GetCellType(x, y)));
			minimapTexture.Apply();
		}

		private TileBase GetCellTypeTile(int cellType)
		{
			Tile tile = ScriptableObject.CreateInstance<Tile>();
			tile.sprite = GetCellTypeSprite((OP2UtilityDotNet.CellType)cellType);
			tile.color = Color.white;

			return tile;
		}

		public Sprite GetCellTypeSprite(OP2UtilityDotNet.CellType cellType)
		{
			return Resources.Load<Sprite>("CellTypes/" + cellType.ToString());
		}
	}
}
