﻿using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for creating structures.
	/// </summary>
	public class StructurePane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownPlayer				= default;
		[SerializeField] private Dropdown	m_DropdownVariant				= default;
		[SerializeField] private InputField	m_InputID						= default;
		[SerializeField] private Slider		m_SliderHealth					= default;
		[SerializeField] private Text		m_txtSliderHealth				= default;
		[SerializeField] private Transform	m_ButtonContainerEden			= default;
		[SerializeField] private Transform	m_ButtonContainerPlymouth		= default;
		[SerializeField] private GameObject m_ScrollViewEden				= default;
		[SerializeField] private GameObject m_ScrollViewPlymouth			= default;

		private string m_SelectedButtonName;


		protected override void Awake()
		{
			base.Awake();

			UserData.current.onChangedValuesCB += OnChangedUserData;

			// Default variant to "Random"
			m_DropdownVariant.value = m_DropdownVariant.options.Count-1;

			OnChanged_Player(m_DropdownPlayer.value);

			// Assign button listeners
			foreach (PaintButton btn in m_ButtonContainerEden.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_StructureButton, btn.name);

			foreach (PaintButton btn in m_ButtonContainerPlymouth.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_StructureButton, btn.name);
		}

		private void OnChangedUserData(UserData src)
		{
			RefreshPlayerDropdown();
		}

		private void RefreshPlayerDropdown()
		{
			List<string> options = new List<string>();

			for (int i=0; i < UserData.current.mission.players.Length; ++i)
				options.Add("Player " + (i+1));

			m_DropdownPlayer.ClearOptions();
			m_DropdownPlayer.AddOptions(options);
		}

		public void OnChanged_Player(int index)
		{
			// Toggle which structure buttons are visible based on the current player's colony type.
			bool isEden = UserData.current.mission.players[index].isEden;
			m_ScrollViewEden.SetActive(isEden);
			m_ScrollViewPlymouth.SetActive(!isEden);

			// Update selected button on new scroll view to currently selected name
			foreach (Transform t in m_ButtonContainerEden)
			{
				if (t.name == m_SelectedButtonName)
					t.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
			}

			foreach (Transform t in m_ButtonContainerPlymouth)
			{
				if (t.name == m_SelectedButtonName)
					t.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
			}
		}

		public void OnChanged_SliderHealth(float value)
		{
			m_txtSliderHealth.text = (value*100).ToString("N2") + "%";
		}

		private void OnClick_StructureButton(object data)
		{
			m_SelectedButtonName = (string)data;

			// Only mines have variants
			m_DropdownVariant.interactable = IsOreMineName(m_SelectedButtonName);
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			map_id selectedTypeID = GetMapIDFromName(m_SelectedButtonName);
			map_id selectedCargoTypeID = GetCargoMapIDFromName(m_SelectedButtonName);
			RectInt structureArea = GetStructureArea(tileXY, selectedTypeID);

			// Check if area is not buildable
			if (!IsAreaPassable(structureArea))
				return;

			// Check if area is blocked by units or structures
			if (AreUnitsInArea(structureArea))
				return;

			int id;
			int.TryParse(m_InputID.text, out id);

			// Create structure data
			UnitData structure = new UnitData();
			
			// Standard info
			structure.id = id;
			structure.typeID = selectedTypeID;
			structure.cargoType = (int)selectedCargoTypeID;
			structure.health = m_SliderHealth.value;
			structure.barVariant = Variant.Random;
			structure.position = new LOCATION(tileXY.x, tileXY.y);

			// Used for ore mines
			if (IsOreMineName(m_SelectedButtonName))
			{
				structure.barYield = GetYieldFromName(m_SelectedButtonName);
				structure.barVariant = GetVariant();
			}

			// Add structure to tile
			PlayerData player = UserData.current.mission.players[m_DropdownPlayer.value];
			player.units.Add(structure);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(player, structure);
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// Find structure on tile
			int playerIndex = 0;
			int unitIndex = -1;

			for (playerIndex=0; playerIndex < UserData.current.mission.players.Length; ++playerIndex)
			{
				PlayerData player = UserData.current.mission.players[playerIndex];

				for (int i=0; i < player.units.Count; ++i)
				{
					UnitData unit = player.units[i];

					RectInt structureArea = GetStructureArea(new Vector2Int(unit.position.x, unit.position.y), unit.typeID);
					if (structureArea.Contains(tileXY))
					{
						unitIndex = i;
						break;
					}
				}

				if (unitIndex >= 0)
					break;
			}

			if (unitIndex < 0)
				return;
			
			UnitData structureToRemove = UserData.current.mission.players[playerIndex].units[unitIndex];

			// Remove structure from tile
			UserData.current.mission.players[playerIndex].units.RemoveAt(unitIndex);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(structureToRemove);
		}

		private bool IsOreMineName(string name)
		{
			switch (name)
			{
				case "CommonOreMine0":	return true;
				case "CommonOreMine1":	return true;
				case "CommonOreMine2":	return true;
				case "CommonOreMine3":	return true;
				case "RareOreMine0":	return true;
				case "RareOreMine1":	return true;
				case "RareOreMine2":	return true;
				case "RareOreMine3":	return true;
			}

			return false;
		}

		private map_id GetMapIDFromName(string name)
		{
			// If underscore present, only use part before it
			name = name.Split('_')[0];

			// If name is an ore mine, remove the yield suffix
			if (IsOreMineName(name))
				name = name.Substring(0, name.Length-1);

			// Parse name with map_id enum
			map_id typeID;
			if (!System.Enum.TryParse(name, out typeID))
				throw new System.ArgumentException("Name is not a valid map_id: " + name);

			return typeID;
		}

		private map_id GetCargoMapIDFromName(string name)
		{
			// Part after underscore is the cargo name
			string[] split = name.Split('_');
			if (split.Length < 2)
				return map_id.None;

			name = split[1];

			// Parse name with map_id enum
			map_id typeID;
			if (!System.Enum.TryParse(name, out typeID))
				throw new System.ArgumentException("Name is not a valid map_id: " + name);

			return typeID;
		}

		private Yield GetYieldFromName(string name)
		{
			string suffix = name[name.Length-1].ToString();
			switch (suffix)
			{
				case "1": return Yield.Bar1;
				case "2": return Yield.Bar2;
				case "3": return Yield.Bar3;
			}

			return Yield.Random;
		}

		private Variant GetVariant()
		{
			switch (m_DropdownVariant.value)
			{
				case 0: return Variant.Variant1;
				case 1: return Variant.Variant2;
				case 2: return Variant.Variant3;
			}

			return Variant.Random;
		}

		private Vector2Int GetStructureSize(map_id type)
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

		private RectInt GetStructureArea(Vector2Int position, map_id unitType)
		{
			Vector2Int size = GetStructureSize(unitType);

			RectInt rect = new RectInt();
			rect.xMin = position.x - size.x / 2;
			rect.yMin = position.y - size.y / 2;
			rect.xMax = position.x + (size.x-1) / 2 + 1;
			rect.yMax = position.y + (size.y-1) / 2 + 1;

			return rect;
		}

		private bool AreUnitsInArea(RectInt area)
		{
			foreach (PlayerData player in UserData.current.mission.players)
			{
				foreach (UnitData unit in player.units)
				{
					RectInt otherArea = GetStructureArea(new Vector2Int(unit.position.x, unit.position.y), unit.typeID);

					if (RectIntersect(area, otherArea))
						return true;
				}
			}

			return false;
		}

		private bool RectIntersect(RectInt a1, RectInt a2)
		{
			return !(a1.xMin >= a2.xMax || a1.xMax <= a2.xMin || a1.yMin >= a2.yMax || a1.yMax <= a2.yMin);
		}

		private bool IsAreaPassable(RectInt area)
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

		private bool IsTilePassable(Vector2Int tileXY)
		{
			// Remove game coordinates
			tileXY -= Vector2Int.one;

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

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChangedUserData;
		}
	}
}