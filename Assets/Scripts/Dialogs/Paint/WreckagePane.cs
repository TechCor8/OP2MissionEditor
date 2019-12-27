using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for creating wreckage.
	/// </summary>
	public class WreckagePane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownWreckageTypes			= default;
		[SerializeField] private InputField	m_InputID						= default;
		[SerializeField] private Toggle	m_IsVisible							= default;

		private map_id[] m_WreckageTypes = new map_id[]
		{
			map_id.EDWARDSatellite,
			map_id.SolarSatellite,
			map_id.IonDriveModule,
			map_id.FusionDriveModule,
			map_id.CommandModule,
			map_id.FuelingSystems,
			map_id.HabitatRing,
			map_id.SensorPackage,
			map_id.Skydock,
			map_id.StasisSystems,
			map_id.OrbitalPackage,
			map_id.PhoenixModule,
			map_id.RareMetalsCargo,
			map_id.CommonMetalsCargo,
			map_id.FoodCargo,
			map_id.EvacuationModule,
			map_id.ChildrenModule,
		};

		private string GetWreckageName(map_id wreckageType)
		{
			switch (wreckageType)
			{
				case map_id.EDWARDSatellite:	return "EDWARD Satellite";
				case map_id.SolarSatellite:		return "Solar Satellite";
				case map_id.IonDriveModule:		return "Ion Drive Module";
				case map_id.FusionDriveModule:	return "Fusion Drive Module";
				case map_id.CommandModule:		return "Command Module";
				case map_id.FuelingSystems:		return "Fueling Systems";
				case map_id.HabitatRing:		return "Habitat Ring";
				case map_id.SensorPackage:		return "Sensor Package";
				case map_id.Skydock:			return "Skydock";
				case map_id.StasisSystems:		return "Stasis Systems";
				case map_id.OrbitalPackage:		return "Orbital Package";
				case map_id.PhoenixModule:		return "Phoenix Module";
				case map_id.RareMetalsCargo:	return "Rare Metals Cargo";
				case map_id.CommonMetalsCargo:	return "Common Metals Cargo";
				case map_id.FoodCargo:			return "Food Cargo";
				case map_id.EvacuationModule:	return "Evacuation Module";
				case map_id.ChildrenModule:		return "Children Module";
			}

			return wreckageType.ToString();
		}


		protected override void Awake()
		{
			base.Awake();

			RefreshWreckageTypes();
		}

		private void RefreshWreckageTypes()
		{
			List<string> wreckageNames = new List<string>();

			// Read wreckage names
			foreach (map_id wreckType in m_WreckageTypes)
				wreckageNames.Add(GetWreckageName(wreckType));

			// Update tileset dropdown options
			m_DropdownWreckageTypes.ClearOptions();
			m_DropdownWreckageTypes.AddOptions(wreckageNames);
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// If tile already contains wreckage, cancel
			if (UserData.current.mission.tethysGame.wreckage.Find((w) => w.position.x == tileXY.x && w.position.y == tileXY.y) != null)
				return;

			int id;
			int.TryParse(m_InputID.text, out id);

			// Create wreckage data
			GameData.Wreckage wreck = new GameData.Wreckage();
			wreck.id = id;
			wreck.techID = m_WreckageTypes[m_DropdownWreckageTypes.value];
			wreck.isVisible = m_IsVisible.isOn;
			wreck.position = new LOCATION(tileXY.x, tileXY.y);

			// Add wreckage to tile
			UserData.current.mission.tethysGame.wreckage.Add(wreck);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(wreck);
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// Find wreckage on tile
			int index = UserData.current.mission.tethysGame.wreckage.FindIndex((w) => w.position.x == tileXY.x && w.position.y == tileXY.y);
			if (index < 0)
				return;

			GameData.Wreckage wreckToRemove = UserData.current.mission.tethysGame.wreckage[index];

			// Remove wreckage from tile
			UserData.current.mission.tethysGame.wreckage.RemoveAt(index);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(wreckToRemove);
		}
	}
}
