using DotNetMissionSDK.Json;
using OP2UtilityDotNet.OP2Map;
using System;
using UnityEngine;

namespace OP2MissionEditor.Data
{
	/// <summary>
	/// Represents a map of the starting mission cell types after structures, walls, and tubes are applied.
	/// </summary>
	public class CellTypeMap
	{
		private struct Tile
		{
			public bool isStructureTile;
			public CellType cellType;
		}

		private Tile[,] m_Grid;


		/// <summary>
		/// Initializes the CellType map with mission data.
		/// </summary>
		public CellTypeMap(Map map, MissionVariant variant)
		{
			uint mapWidth = map.WidthInTiles();
			uint mapHeight = map.HeightInTiles();

			m_Grid = new Tile[mapWidth, mapHeight];

			// Default CellTypes to map CellTypes
			for (int x=0; x < mapWidth; ++x)
			{
				for (int y=0; y < mapHeight; ++y)
				{
					m_Grid[x,y].cellType = (CellType)map.GetCellType(x,y);
				}
			}

			// Loop through all units, walls, and tubes
			foreach (PlayerData player in variant.players)
			{
				// Get structure modifications
				foreach (UnitData unit in player.resources.units)
				{
					if (!StructureData.IsStructure(unit.typeID))
						continue;

					RectInt area = StructureData.GetStructureArea(new Vector2Int(unit.position.x-1, unit.position.y-1), unit.typeID);

					// Add bulldozed area
					area.min -= Vector2Int.one;
					area.max += Vector2Int.one;

					SetAreaCellType(area, CellType.DozedArea, false);

					// Add structure tubes
					if (StructureData.HasTubes(unit.typeID))
					{
						m_Grid[unit.position.x-1, area.max.y-1].cellType = CellType.Tube0;
						m_Grid[area.max.x-1, unit.position.y-1].cellType = CellType.Tube0;
					}

					// Remove bulldozed area
					area.min += Vector2Int.one;
					area.max -= Vector2Int.one;

					SetAreaCellType(area, CellType.DozedArea, true);
				}

				// Get wall tube modifications
				foreach (WallTubeData wallTube in player.resources.wallTubes)
				{
					CellType cellType = CellType.zPad20;

					switch (wallTube.typeID)
					{
						case DotNetMissionSDK.map_id.Tube:			cellType = CellType.Tube0;			break;
						case DotNetMissionSDK.map_id.Wall:			cellType = CellType.NormalWall;		break;
						case DotNetMissionSDK.map_id.LavaWall:		cellType = CellType.LavaWall;		break;
						case DotNetMissionSDK.map_id.MicrobeWall:	cellType = CellType.MicrobeWall;	break;
					}

					m_Grid[wallTube.position.x-1, wallTube.position.y-1].cellType = cellType;
				}
			} // foreach playerStates
		}

		private void SetAreaCellType(RectInt area, CellType cellType, bool isStructureTile)
		{
			for (int x=area.xMin; x < area.xMax; ++x)
			{
				for (int y=area.yMin; y < area.yMax; ++y)
				{
					m_Grid[x,y].cellType = cellType;
					m_Grid[x,y].isStructureTile = isStructureTile;
				}
			}
		}

		/// <summary>
		/// Returns the CellType for the specified tile.
		/// </summary>
		public CellType GetCellType(Vector2Int tileXY)
		{
			return m_Grid[tileXY.x, tileXY.y].cellType;
		}

		/// <summary>
		/// Does the specified tile contain a structure?
		/// </summary>
		public bool IsStructureTile(Vector2Int tileXY)
		{
			return m_Grid[tileXY.x, tileXY.y].isStructureTile;
		}

		/// <summary>
		/// Clears the map.
		/// </summary>
		private void Clear()
		{
			Array.Clear(m_Grid, 0, m_Grid.Length);
		}
	}
}
