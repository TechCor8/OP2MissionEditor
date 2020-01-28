using DotNetMissionSDK.Json;
using OP2UtilityDotNet;
using UnityEngine;

namespace OP2MissionEditor.Data
{
	/// <summary>
	/// Contains useful information about the game tile map.
	/// </summary>
	public class TileMapData
	{
		public static bool IsTilePassable(Vector2Int tileXY)
		{
			// Out of bounds is not passable
			if (tileXY.x < 0 || tileXY.y < 0 || tileXY.x >= UserData.current.map.GetWidthInTiles() || tileXY.y >= UserData.current.map.GetHeightInTiles())
				return false;

			// Check for passable tile types
			CellType type = (CellType)UserData.current.map.GetCellType((ulong)tileXY.x, (ulong)tileXY.y);
			switch (type)
			{
				case CellType.FastPassible1:
				case CellType.SlowPassible1:
				case CellType.SlowPassible2:
				case CellType.MediumPassible1:
				case CellType.MediumPassible2:
				case CellType.FastPassible2:
				case CellType.DozedArea:
				case CellType.Rubble:
				case CellType.Tube0:
					return true;
			}

			return false;
		}

		public static bool IsAreaPassable(RectInt area)
		{
			for (int x=area.xMin; x < area.xMax; ++x)
			{
				for (int y=area.yMin; y < area.yMax; ++y)
				{
					if (!IsTilePassable(new Vector2Int(x,y)))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Gets the cell type for a tile at mission start.
		/// </summary>
		/// <param name="isAdjacent">If true, returns Tube0 for tiles underneath structures.</param>
		public static CellType GetMissionCellType(Vector2Int tileXY, bool isAdjacent=false)
		{
			// Out of bounds is not passable
			if (tileXY.x < 0 || tileXY.y < 0 || tileXY.x >= UserData.current.map.GetWidthInTiles() || tileXY.y >= UserData.current.map.GetHeightInTiles())
				return CellType.Impassible1;

			// Add game coordinates
			Vector2Int gameXY = tileXY + Vector2Int.one;

			foreach (PlayerData player in UserData.current.GetCombinedVariant().players)
			{
				PlayerData.ResourceData resData = UserData.current.GetCombinedResourceData(player);

				// Get wall tube modifications
				foreach (WallTubeData wallTube in resData.wallTubes)
				{
					if (wallTube.position.x == gameXY.x && wallTube.position.y == gameXY.y)
					{
						switch (wallTube.typeID)
						{
							case DotNetMissionSDK.map_id.Tube:			return CellType.Tube0;
							case DotNetMissionSDK.map_id.Wall:			return CellType.NormalWall;
							case DotNetMissionSDK.map_id.LavaWall:		return CellType.LavaWall;
							case DotNetMissionSDK.map_id.MicrobeWall:	return CellType.MicrobeWall;
						}
					}
				}

				// Get structure modifications
				foreach (UnitData unit in resData.units)
				{
					if (!StructureData.IsStructure(unit.typeID))
						continue;

					RectInt area = StructureData.GetStructureArea(new Vector2Int(unit.position.x, unit.position.y), unit.typeID);

					if (isAdjacent && area.Contains(gameXY))
						return CellType.Tube0;

					// Add bulldozed area
					area.min -= Vector2Int.one;
					area.max += Vector2Int.one;

					if (StructureData.NeedsTube(unit.typeID))
					{
						if (gameXY.x == unit.position.x && gameXY.y == area.max.y-1)
							return CellType.Tube0;
						else if (gameXY.x == area.max.x-1 && gameXY.y == unit.position.y)
							return CellType.Tube0;
					}

					if (area.Contains(gameXY))
						return CellType.DozedArea;
				}
			}

			return (CellType)UserData.current.map.GetCellType((ulong)tileXY.x, (ulong)tileXY.y);
		}

		private static CellType GetMissionCellType_ForceTubesToZero(Vector2Int tileXY, bool isAdjacent=false)
		{
			CellType cellType = GetMissionCellType(tileXY, isAdjacent);
			switch (cellType)
			{
				case CellType.Tube1:
				case CellType.Tube2:
				case CellType.Tube3:
				case CellType.Tube4:
				case CellType.Tube5:
					cellType = CellType.Tube0;
					break;
			}

			return cellType;
		}

		/// <summary>
		/// Returns the CellType with the correct wall/tube index for a tile based on its adjacent tiles.
		/// </summary>
		public static CellType GetWallTubeIndexForTile(Vector2Int tileXY, out int wallTubeIndex)
		{
			wallTubeIndex = 0;

			CellType cellType = GetMissionCellType_ForceTubesToZero(tileXY);
			switch (cellType)
			{
				// Valid types to check
				case CellType.Tube0:
				case CellType.NormalWall:
				case CellType.LavaWall:
				case CellType.MicrobeWall:
					break;

				default:
					return cellType;
			}

			CellType cellTypeUp = GetMissionCellType_ForceTubesToZero(tileXY + new Vector2Int(0,-1), true);
			CellType cellTypeDown = GetMissionCellType_ForceTubesToZero(tileXY + new Vector2Int(0,1), true);
			CellType cellTypeLeft = GetMissionCellType_ForceTubesToZero(tileXY + new Vector2Int(-1,0), true);
			CellType cellTypeRight = GetMissionCellType_ForceTubesToZero(tileXY + new Vector2Int(1,0), true);

			bool up = cellType == cellTypeUp;
			bool down = cellType == cellTypeDown;
			bool left = cellType == cellTypeLeft;
			bool right = cellType == cellTypeRight;

			if (left && up && right && down)		wallTubeIndex = 8;
			else if (left && up && right)			wallTubeIndex = 6;
			else if (left && down && right)			wallTubeIndex = 7;
			else if (left && up && down)			wallTubeIndex = 9;
			else if (right && up && down)			wallTubeIndex = 10;
			else if (left && right)					wallTubeIndex = 0;
			else if (up && down)					wallTubeIndex = 1;
			else if (left && down)					wallTubeIndex = 2;
			else if (right && down)					wallTubeIndex = 3;
			else if (up && left)					wallTubeIndex = 4;
			else if (up && right)					wallTubeIndex = 5;
			else if (down)							wallTubeIndex = 11;
			else if (up)							wallTubeIndex = 12;
			else if (right)							wallTubeIndex = 13;
			else if (left)							wallTubeIndex = 14;
			else									wallTubeIndex = 15;

			return cellType;
		}
	}
}
