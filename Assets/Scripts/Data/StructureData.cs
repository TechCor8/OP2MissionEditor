using DotNetMissionSDK;
using UnityEngine;

namespace OP2MissionEditor.Data
{
	/// <summary>
	/// Contains useful information about game buildings.
	/// </summary>
	public class StructureData
	{
		public static Vector2Int GetStructureSize(map_id type)
		{
			switch (type)
			{
				case map_id.CommonOreMine:			return new Vector2Int(2,1);
				case map_id.RareOreMine:			return new Vector2Int(2,1);
				case map_id.GuardPost:				return new Vector2Int(1,1);
				case map_id.LightTower:				return new Vector2Int(1,1);
				case map_id.CommonStorage:			return new Vector2Int(1,2);
				case map_id.RareStorage:			return new Vector2Int(1,2);
				case map_id.Forum:					return new Vector2Int(2,2);
				case map_id.CommandCenter:			return new Vector2Int(3,2);
				case map_id.MHDGenerator:			return new Vector2Int(2,2);
				case map_id.Residence:				return new Vector2Int(2,2);
				case map_id.RobotCommand:			return new Vector2Int(2,2);
				case map_id.TradeCenter:			return new Vector2Int(2,2);
				case map_id.BasicLab:				return new Vector2Int(2,2);
				case map_id.MedicalCenter:			return new Vector2Int(2,2);
				case map_id.Nursery:				return new Vector2Int(2,2);
				case map_id.SolarPowerArray:		return new Vector2Int(3,2);
				case map_id.RecreationFacility:		return new Vector2Int(2,2);
				case map_id.University:				return new Vector2Int(2,2);
				case map_id.Agridome:				return new Vector2Int(3,2);
				case map_id.DIRT:					return new Vector2Int(3,2);
				case map_id.Garage:					return new Vector2Int(3,2);
				case map_id.MagmaWell:				return new Vector2Int(2,1);
				case map_id.MeteorDefense:			return new Vector2Int(2,2);
				case map_id.GeothermalPlant:		return new Vector2Int(2,1);
				case map_id.ArachnidFactory:		return new Vector2Int(2,2);
				case map_id.ConsumerFactory:		return new Vector2Int(3,3);
				case map_id.StructureFactory:		return new Vector2Int(4,3);
				case map_id.VehicleFactory:			return new Vector2Int(4,3);
				case map_id.StandardLab:			return new Vector2Int(3,2);
				case map_id.AdvancedLab:			return new Vector2Int(3,3);
				case map_id.Observatory:			return new Vector2Int(2,2);
				case map_id.ReinforcedResidence:	return new Vector2Int(3,2);
				case map_id.AdvancedResidence:		return new Vector2Int(3,3);
				case map_id.CommonOreSmelter:		return new Vector2Int(4,3);
				case map_id.Spaceport:				return new Vector2Int(5,4);
				case map_id.RareOreSmelter:			return new Vector2Int(4,3);
				case map_id.GORF:					return new Vector2Int(3,2);
				case map_id.Tokamak:				return new Vector2Int(2,2);
			}

			return new Vector2Int(1,1);
		}

		public static RectInt GetStructureArea(Vector2Int position, map_id unitType)
		{
			Vector2Int size = GetStructureSize(unitType);

			RectInt rect = new RectInt();
			rect.xMin = position.x - size.x / 2;
			rect.yMin = position.y - size.y / 2;
			rect.xMax = position.x + (size.x-1) / 2 + 1;
			rect.yMax = position.y + (size.y-1) / 2 + 1;

			return rect;
		}

		public static RectInt GetBulldozedStructureArea(Vector2Int position, map_id unitType)
		{
			RectInt bulldozedArea = GetStructureArea(position, unitType);
			bulldozedArea.min -= Vector2Int.one;
			bulldozedArea.max += Vector2Int.one;

			return bulldozedArea;
		}

		public static bool NeedsTube(map_id type)
		{
			switch (type)
			{
				case map_id.CommandCenter:
				case map_id.LightTower:
				case map_id.CommonOreMine:
				case map_id.RareOreMine:
				case map_id.MagmaWell:
				case map_id.Tokamak:
				case map_id.SolarPowerArray:
				case map_id.MHDGenerator:
				case map_id.GeothermalPlant:
					return false;
			}

			return IsStructure(type);
		}

		public static bool HasTubes(map_id type)
		{
			switch (type)
			{
				case map_id.LightTower:
				case map_id.CommonOreMine:
				case map_id.RareOreMine:
				case map_id.MagmaWell:
				case map_id.Tokamak:
				case map_id.SolarPowerArray:
				case map_id.MHDGenerator:
				case map_id.GeothermalPlant:
					return false;
			}

			return IsStructure(type);
		}

		public static bool IsStructure(map_id type)
		{
			return (int)type >= 21 && (int)type <= 58;
		}
	}
}
