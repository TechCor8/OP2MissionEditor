using OP2MissionEditor.Data;
using OP2UtilityDotNet;
using System.Collections;
using System.Collections.Generic;
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
		private Dictionary<string, Dictionary<int, TileBase>> m_TileCache = new Dictionary<string, Dictionary<int, TileBase>>();
		private TileBase[] m_CellTypeCache;

		public Texture2D minimapTexture			{ get; private set; }

		public delegate void OnMapProgressCallback(MapRenderer mapRenderer, string state, float progress);
		public delegate void OnMapCallback(MapRenderer mapRenderer);

		public event OnMapProgressCallback onMapRefreshProgressCB;
		public event OnMapCallback onMapRefreshedCB;


		private void Awake()
		{
			m_Tilemap = GetComponent<Tilemap>();

			UserPrefs.onChangedPrefsCB += OnChangedPrefs;

			InitializeCellTypeCache();
			HideCellTypeMap();
		}

		private void InitializeCellTypeCache()
		{
			m_CellTypeCache = new TileBase[32];

			for (int i=0; i < m_CellTypeCache.Length; ++i)
				m_CellTypeCache[i] = GetCellTypeTile(i);
		}

		private TileBase GetCellTypeTile(int cellType)
		{
			Tile tile = ScriptableObject.CreateInstance<Tile>();
			tile.sprite = GetCellTypeSprite((OP2UtilityDotNet.OP2Map.CellType)cellType);
			tile.color = Color.white;

			return tile;
		}

		public Sprite GetCellTypeSprite(OP2UtilityDotNet.OP2Map.CellType cellType)
		{
			return Resources.Load<Sprite>("CellTypes/" + cellType.ToString());
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

			m_TileCache.Clear();

			// Cache calculated CellTypes
			onMapRefreshProgressCB?.Invoke(this, "Calculating Cell Types", 0.0f);
			yield return null;

			CellTypeMap cellTypeMap = new CellTypeMap(UserData.current.map, UserData.current.GetCombinedMission());

			int mapWidth = (int)UserData.current.map.WidthInTiles();
			int mapHeight = (int)UserData.current.map.HeightInTiles();

			minimapTexture = new Texture2D((int)mapWidth*TextureManager.minimapScale, (int)mapHeight*TextureManager.minimapScale, TextureFormat.ARGB32, false);

			Vector3Int[] cellPositions = new Vector3Int[(int)(mapWidth*mapHeight)];
			TileBase[] cellTiles = new TileBase[(int)(mapWidth*mapHeight)];
			TileBase[] cellTypes = new TileBase[(int)(mapWidth*mapHeight)];
			int index = 0;

			int updateFrequency = 5;

			for (int x=0; x < mapWidth; ++x)
			{
				if (index % updateFrequency == 0)
				{
					onMapRefreshProgressCB?.Invoke(this, "Reading tiles", (float)(x*mapHeight) / (mapWidth*mapHeight));
					yield return null;
				}

				for (int y=0; y < mapHeight; ++y)
				{
					int tileMappingIndex = GetTileMappingIndex(cellTypeMap, new Vector2Int(x,y));
					OP2UtilityDotNet.OP2Map.TileMapping mapping = UserData.current.map.tileMappings[tileMappingIndex];
			
					int tileSetIndex = mapping.tilesetIndex;
					int tileImageIndex = mapping.tileGraphicIndex;

					string tileSetPath = UserData.current.map.tilesetSources[tileSetIndex].tilesetFilename;
					int tileSetNumTiles = (int)UserData.current.map.tilesetSources[tileSetIndex].numTiles;

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

					// Load tile into tile map
					TileBase tile = LoadTile(tileSetPath, texOffset);
					Vector3Int cellPosition = new Vector3Int((int)x,(int)(mapHeight-y-1),0);

					cellPositions[index] = cellPosition;
					cellTiles[index] = tile;
					cellTypes[index] = m_CellTypeCache[(int)UserData.current.map.GetCellType(x, y)];

					++cellPosition.y;

					// Set minimap pixel
					Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
					for (int my=0; my < TextureManager.minimapScale; ++my)
					{
						for (int mx=0; mx < TextureManager.minimapScale; ++mx)
						{
							Color color = mTexture.GetPixel(mx, inverseTileIndex*TextureManager.minimapScale + my);
							minimapTexture.SetPixel(cellPosition.x*TextureManager.minimapScale + mx, cellPosition.y*TextureManager.minimapScale + my - 1, color);
						}
					}
						
					++index;
				}
			}

			m_GridOverlay.color = UserPrefs.gridOverlayColor;
			m_GridOverlay.gameObject.SetActive(UserPrefs.isGridOverlayVisible);

			onMapRefreshProgressCB?.Invoke(this, "Setting tiles", 1);
			yield return null;

			// Set tiles
			m_Tilemap.SetTiles(cellPositions, cellTiles);
			
			// Create cell types
			onMapRefreshProgressCB?.Invoke(this, "Setting cell types", 1);
			yield return null;

			m_CellTypeMap.SetTiles(cellPositions, cellTypes);

			// Create grid
			onMapRefreshProgressCB?.Invoke(this, "Creating grid", 1);
			yield return null;

			TileBase[] overlayTiles = new TileBase[cellPositions.Length];
			for (int i = 0; i < overlayTiles.Length; ++i)
				overlayTiles[i] = gridOverlayTile;

			m_GridOverlay.SetTiles(cellPositions, overlayTiles);

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

		public void RefreshTiles(RectInt area)
		{
			// Cache calculated CellTypes
			CellTypeMap cellTypeMap = new CellTypeMap(UserData.current.map, UserData.current.GetCombinedMission());

			for (int x=area.xMin; x < area.xMax; ++x)
			{
				for (int y=area.yMin; y < area.yMax; ++y)
					RefreshTile(cellTypeMap, new Vector2Int(x,y));
			}
		}

		public void RefreshTile(Vector2Int tileXY)
		{
			// Cache calculated CellTypes
			CellTypeMap cellTypeMap = new CellTypeMap(UserData.current.map, UserData.current.GetCombinedMission());

			RefreshTile(cellTypeMap, tileXY);
		}

		private void RefreshTile(CellTypeMap cellTypeMap, Vector2Int tileXY)
		{
			int x = tileXY.x;
			int y = tileXY.y;

			int tileMappingIndex = GetTileMappingIndex(cellTypeMap, tileXY);
			OP2UtilityDotNet.OP2Map.TileMapping mapping = UserData.current.map.tileMappings[tileMappingIndex];
			
			int tileSetIndex = mapping.tilesetIndex;
			int tileImageIndex = mapping.tileGraphicIndex;

			string tileSetPath = UserData.current.map.tilesetSources[tileSetIndex].tilesetFilename;
			int tileSetNumTiles = (int)UserData.current.map.tilesetSources[tileSetIndex].numTiles;

			// Get tile texture
			Texture2D texture = TextureManager.LoadTileset(tileSetPath);
			if (texture == null)
				throw new System.Exception("Could not find resource: " + tileSetPath);
			
			// Get image offset
			int tileSize = texture.height / tileSetNumTiles;
			int inverseTileIndex = tileSetNumTiles-tileImageIndex-1;
			int texOffset = inverseTileIndex * tileSize;

			// Load tile into tile map
			TileBase tile = LoadTile(tileSetPath, texOffset);
			Vector3Int cellPosition = new Vector3Int((int)x,m_Tilemap.size.y-(int)y-1,0);

			// Set minimap pixel
			Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
			Color color = mTexture.GetPixel(0, inverseTileIndex);
			minimapTexture.SetPixel(cellPosition.x, cellPosition.y, color);

			// Set tiles
			m_Tilemap.SetTile(cellPosition, tile);
			m_CellTypeMap.SetTile(cellPosition, m_CellTypeCache[(int)UserData.current.map.GetCellType(x, y)]);
			minimapTexture.Apply();
		}

		private int GetTileMappingIndex(CellTypeMap cellTypeMap, Vector2Int tileXY)
		{
			int x = tileXY.x;
			int y = tileXY.y;

			// Get default mapping index
			int mappingIndex = UserData.current.map.GetTileMappingIndex(x,y);

			// Get TerrainType for mapping index, if available
			OP2UtilityDotNet.OP2Map.TerrainType terrainType;
			if (!GetTerrainTypeForMappingIndex(mappingIndex, out terrainType))
				return mappingIndex;

			// Predict starting CellType and remap to terrain type
			int wallTubeIndex;
			switch (TileMapData.GetWallTubeIndexForTile(cellTypeMap, tileXY, out wallTubeIndex))
			{
				case OP2UtilityDotNet.OP2Map.CellType.DozedArea:			mappingIndex = terrainType.bulldozedTileMappingIndex;						break;
				case OP2UtilityDotNet.OP2Map.CellType.NormalWall:			mappingIndex = terrainType.wallTileMappingIndexes[2, wallTubeIndex];		break;
				case OP2UtilityDotNet.OP2Map.CellType.LavaWall:				mappingIndex = terrainType.wallTileMappingIndexes[0, wallTubeIndex];		break;
				case OP2UtilityDotNet.OP2Map.CellType.MicrobeWall:			mappingIndex = terrainType.wallTileMappingIndexes[1, wallTubeIndex];		break;
				case OP2UtilityDotNet.OP2Map.CellType.Tube0:
				case OP2UtilityDotNet.OP2Map.CellType.Tube1:
				case OP2UtilityDotNet.OP2Map.CellType.Tube2:
				case OP2UtilityDotNet.OP2Map.CellType.Tube3:
				case OP2UtilityDotNet.OP2Map.CellType.Tube4:
				case OP2UtilityDotNet.OP2Map.CellType.Tube5:
					mappingIndex = terrainType.tubeTileMappingIndexes[wallTubeIndex];
					break;
			}

			return mappingIndex;
		}

		private bool GetTerrainTypeForMappingIndex(int mappingIndex, out OP2UtilityDotNet.OP2Map.TerrainType terrainType)
		{
			int count = UserData.current.map.terrainTypes.Count;

			// Search terrain types for associated mapping index
			for (int i=0; i < count; ++i)
			{
				OP2UtilityDotNet.OP2Map.TerrainType type = UserData.current.map.terrainTypes[i];
				if (type.tileMappingRange.start <= mappingIndex && mappingIndex <= type.tileMappingRange.end)
				{
					terrainType = type;
					return true;
				}
			}

			terrainType = new OP2UtilityDotNet.OP2Map.TerrainType();
			return false;
		}

		private TileBase LoadTile(string tileSetPath, int texOffset)
		{
			// Get tile texture
			Texture2D texture = TextureManager.LoadTileset(tileSetPath);
			if (texture == null)
				return null;

			// Get tileset tile cache
			Dictionary<int, TileBase> tileCache;
			if (!m_TileCache.TryGetValue(tileSetPath, out tileCache))
			{
				tileCache = new Dictionary<int, TileBase>();
				m_TileCache.Add(tileSetPath, tileCache);
			}

			// Get tile from cache
			TileBase tile;
			if (!tileCache.TryGetValue(texOffset, out tile))
			{
				// Create new tile
				Sprite tileSprite = Sprite.Create(texture, new UnityEngine.Rect(0,texOffset, texture.width,texture.width), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
				Tile tile2 = ScriptableObject.CreateInstance<Tile>();
				tile2.sprite = tileSprite;
				tile2.color = Color.white;
				tileCache.Add(texOffset, tile2);
				tile = tile2;
			}

			return tile;
		}
	}
}
