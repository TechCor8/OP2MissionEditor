﻿using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Systems.TechTree;
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
				wreckageNames.Add(wreckType.ToString());

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