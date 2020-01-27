using DotNetMissionSDK;
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
	}
}
