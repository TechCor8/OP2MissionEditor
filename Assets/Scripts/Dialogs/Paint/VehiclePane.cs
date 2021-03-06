﻿using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Data;
using OP2MissionEditor.Systems.Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for creating vehicles.
	/// </summary>
	public class VehiclePane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownPlayer				= default;
		[SerializeField] private Dropdown	m_DropdownDirection				= default;
		[SerializeField] private InputField	m_InputID						= default;
		[SerializeField] private Slider		m_SliderHealth					= default;
		[SerializeField] private Text		m_txtSliderHealth				= default;
		[SerializeField] private Dropdown	m_DropdownCargoType				= default;
		[SerializeField] private Dropdown	m_DropdownCargoSubtype			= default;
		[SerializeField] private InputField	m_InputCargoAmount				= default;
		[SerializeField] private Toggle		m_ToggleLights					= default;
		[SerializeField] private Transform	m_ButtonContainerEden			= default;
		[SerializeField] private Transform	m_ButtonContainerPlymouth		= default;
		[SerializeField] private GameObject m_ScrollViewEden				= default;
		[SerializeField] private GameObject m_ScrollViewPlymouth			= default;

		private string m_SelectedButtonName;


		protected override void Awake()
		{
			base.Awake();

			UserData.current.onChangedValuesCB += OnChangedUserData;
			UserData.current.onSelectVariantCB += OnChangedUserData;

			RefreshPlayerDropdown();
			OnChanged_Player(m_DropdownPlayer.value);

			m_DropdownCargoSubtype.gameObject.SetActive(false);

			// Assign button listeners
			foreach (PaintButton btn in m_ButtonContainerEden.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_VehicleButton, btn.name);

			foreach (PaintButton btn in m_ButtonContainerPlymouth.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_VehicleButton, btn.name);
		}

		private void OnEnable()
		{
			RefreshOverlay();
		}

		private void OnChangedUserData(UserData src)
		{
			RefreshPlayerDropdown();
		}

		private void RefreshPlayerDropdown()
		{
			List<string> options = new List<string>();

			for (int i=0; i < UserData.current.selectedVariant.players.Count; ++i)
				options.Add("Player " + (i+1));

			m_DropdownPlayer.ClearOptions();
			m_DropdownPlayer.AddOptions(options);

			OnChanged_Player(0);
		}

		public void OnChanged_Player(int index)
		{
			// Toggle which structure buttons are visible based on the current player's colony type.
			bool isEden = UserData.current.selectedVariant.players[index].isEden;
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

			RefreshIconColors();

			RefreshOverlay();
		}

		private void RefreshIconColors()
		{
			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];

			// Refresh Eden icons
			foreach (Image button in GetIcons(m_ButtonContainerEden))
			{
				Material mat = Instantiate(button.material);
				mat.name = "UnitIconMaterial(clone)";
				mat.SetInt("_PaletteIndex", (int)player.color);
				button.material = mat;
			}

			// Refresh Plymouth icons
			foreach (Image button in GetIcons(m_ButtonContainerPlymouth))
			{
				Material mat = Instantiate(button.material);
				mat.name = "UnitIconMaterial(clone)";
				mat.SetInt("_PaletteIndex", (int)player.color);
				button.material = mat;
			}
		}

		private List<Image> GetIcons(Transform container)
		{
			List<Image> icons = new List<Image>();

			Image[] buttons = container.GetComponentsInChildren<Image>(true);
			foreach (Image button in buttons)
			{
				if (button.name == "TileImage" || button.name == "WeaponImage")
					icons.Add(button);
			}

			return icons;
		}

		public void OnChanged_SliderHealth(float value)
		{
			m_txtSliderHealth.text = (value*100).ToString("N2") + "%";
		}

		private void OnClick_VehicleButton(object data)
		{
			m_SelectedButtonName = (string)data;

			// Default cargo type selection does not use a subtype or amount
			m_DropdownCargoSubtype.interactable = false;
			m_InputCargoAmount.interactable = false;

			// Start location has no adjustable parameters
			bool isStartLocation = m_SelectedButtonName == "StartLocation";

			m_DropdownCargoType.interactable = false;
			m_InputID.interactable = !isStartLocation;
			m_DropdownDirection.interactable = !isStartLocation;
			m_SliderHealth.interactable = !isStartLocation;
			m_ToggleLights.interactable = !isStartLocation;

			RefreshOverlay();

			if (isStartLocation)
				return;


			// Refresh cargo type dropdown
			switch (GetMapIDFromName(m_SelectedButtonName))
			{
				case map_id.CargoTruck:	RefreshCargoTypeAsTruckOptions();	m_DropdownCargoType.interactable = true;	break;
				case map_id.ConVec:		RefreshCargoTypeAsConvecOptions();	m_DropdownCargoType.interactable = true;	break;
			}
		}

		public void RefreshOverlay()
		{
			if (!gameObject.activeSelf) return;

			if (m_SelectedButtonName == null)
				return;

			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];

			if (m_SelectedButtonName == "StartLocation")
			{
				StartLocationView startLocationView = Instantiate(Resources.Load<GameObject>("Game/StartLocation")).GetComponent<StartLocationView>();
				startLocationView.Initialize(m_Tilemap, m_UnitRenderer);
				startLocationView.Initialize(player);
				m_OverlayRenderer.SetOverlay(startLocationView, Vector2Int.zero, Vector2.zero);
			}
			else
			{
				m_OverlayRenderer.SetOverlay(m_UnitRenderer.AddUnit(player, GetVehicleData()), Vector2Int.one, Vector2.zero);
			}
		}

		public void OnChanged_CargoType()
		{
			switch (GetMapIDFromName(m_SelectedButtonName))
			{
				case map_id.ConVec:
					map_id convecCargoID = GetCargoTypeForConvecOption(m_DropdownCargoType.value);
					if (convecCargoID == map_id.GuardPost)
					{
						// Show weapon options for guard post
						m_InputCargoAmount.gameObject.SetActive(false);
						m_DropdownCargoSubtype.gameObject.SetActive(true);
						m_DropdownCargoSubtype.interactable = true;
						RefreshSubtypeAsWeaponOptions();
						RefreshOverlay();
						return;
					}
					break;

				case map_id.CargoTruck:
					int truckCargoID = GetCargoTypeForTruckOption(m_DropdownCargoType.value);
					if (IsQuantifiedCargo(truckCargoID))
					{
						// Show cargo amount
						m_DropdownCargoSubtype.gameObject.SetActive(false);
						m_InputCargoAmount.gameObject.SetActive(true);
						m_InputCargoAmount.interactable = true;
						RefreshOverlay();
						return;
					}
					else if (truckCargoID == 8 || truckCargoID == 9)
					{
						// Show starship modules
						m_InputCargoAmount.gameObject.SetActive(false);
						m_DropdownCargoSubtype.gameObject.SetActive(true);
						m_DropdownCargoSubtype.interactable = true;
						RefreshSubtypeAsStarshipOptions();
						RefreshOverlay();
						return;
					}
					break;
			}

			// No subtype or amount to change
			m_DropdownCargoSubtype.interactable = false;
			m_InputCargoAmount.interactable = false;
			RefreshOverlay();
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			if (m_SelectedButtonName == "StartLocation")
			{
				// Add game coordinates
				tileXY += Vector2Int.one;

				// Set player start location
				PlayerData player1 = UserData.current.selectedVariant.players[m_DropdownPlayer.value];
				DataLocation centerView = new DataLocation();
				centerView.x = tileXY.x;
				centerView.y = tileXY.y;
				UserData.current.GetPlayerResourceData(player1).centerView = centerView;
				UserData.current.SetUnsaved();

				m_UnitRenderer.SetStartLocation(m_DropdownPlayer.value, player1);
				return;
			}

			// Check if tile is passable
			if (!TileMapData.IsTilePassable(tileXY))
				return;

			// Check if area is blocked by units or structures
			if (AreUnitsOnTile(tileXY))
				return;

			// Check if area is blocked by wall
			if (IsWallOnTile(tileXY))
				return;

			// Add game coordinates
			tileXY += Vector2Int.one;

			UnitData vehicle = GetVehicleData();
			vehicle.position = new LOCATION(tileXY.x, tileXY.y);

			// Add vehicle to tile
			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];
			UserData.current.GetPlayerResourceData(player).units.Add(vehicle);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(player, vehicle);
		}

		private UnitData GetVehicleData()
		{
			// Get unit type
			map_id selectedTypeID = GetMapIDFromName(m_SelectedButtonName);

			// Get cargo type
			int selectedCargoTypeID = 0;

			if (selectedTypeID == map_id.CargoTruck)
				selectedCargoTypeID = GetCargoTypeForTruckOption(m_DropdownCargoType.value);
			else if (selectedTypeID == map_id.ConVec)
				selectedCargoTypeID = (int)GetCargoTypeForConvecOption(m_DropdownCargoType.value);
			else
				selectedCargoTypeID = (int)GetCargoMapIDFromName(m_SelectedButtonName);

			int id;
			int.TryParse(m_InputID.text, out id);

			int cargoAmount = 0;

			// Cargo amount represents different things depending on cargo type.
			switch (selectedTypeID)
			{
				case map_id.ConVec:
					// Get guard post weapon for convec
					if ((map_id)selectedCargoTypeID == map_id.GuardPost)
						cargoAmount = GetSubtypeForWeaponOption(m_DropdownCargoSubtype.value);
					break;

				case map_id.CargoTruck:
					// Get cargo amount for quantified cargo type, or starship module for starship cargo type
					if (IsQuantifiedCargo(selectedCargoTypeID))
						int.TryParse(m_InputCargoAmount.text, out cargoAmount);
					else if (selectedCargoTypeID == 8 || selectedCargoTypeID == 9) // Starship || Wreckage
						cargoAmount = GetSubtypeForStarshipOption(m_DropdownCargoSubtype.value);
					else if (selectedCargoTypeID == (int)map_id.IonDriveModule) // Gene bank
						cargoAmount = 5; // Dunno why this is 5.
					break;
			}

			// Create vehicle data
			UnitData vehicle = new UnitData();
			
			// Standard info
			vehicle.id = id;
			vehicle.typeID = selectedTypeID;
			vehicle.health = m_SliderHealth.value;
			vehicle.lights = m_ToggleLights.isOn;
			vehicle.cargoType = selectedCargoTypeID;
			vehicle.cargoAmount = cargoAmount;
			vehicle.direction = (UnitDirection)m_DropdownDirection.value;
			vehicle.position = new DataLocation(new LOCATION(1,1));

			return vehicle;
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// Find vehicle on tile
			int playerIndex = 0;
			int unitIndex = -1;

			for (playerIndex=0; playerIndex < UserData.current.selectedVariant.players.Count; ++playerIndex)
			{
				PlayerData.ResourceData playerResData = UserData.current.GetPlayerResourceData(playerIndex);

				for (int i=0; i < playerResData.units.Count; ++i)
				{
					UnitData unit = playerResData.units[i];

					if (unit.position.x == tileXY.x && unit.position.y == tileXY.y)
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
			
			UnitData vehicleToRemove = UserData.current.GetPlayerResourceData(playerIndex).units[unitIndex];

			// Remove vehicle from tile
			UserData.current.GetPlayerResourceData(playerIndex).units.RemoveAt(unitIndex);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(vehicleToRemove);
		}

		protected override void OnOverTile(Vector2Int tileXY)
		{
			base.OnOverTile(tileXY);

			if (m_SelectedButtonName != "StartLocation")
			{
				bool canPlace = TileMapData.IsTilePassable(tileXY) && !AreUnitsOnTile(tileXY) && !IsWallOnTile(tileXY);
				m_OverlayRenderer.SetTileStatus(Vector2Int.zero, canPlace ? Color.green : Color.red);
			}
		}

		private map_id GetMapIDFromName(string name)
		{
			// If underscore present, only use part before it
			name = name.Split('_')[0];

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

		private map_id GetCargoTypeForConvecOption(int index)
		{
			switch (index)
			{
				case 0:		return map_id.None;
				case 1:		return map_id.Agridome;
				case 2:		return map_id.AdvancedLab;
				case 3:		return map_id.AdvancedResidence;
				case 4:		return map_id.ArachnidFactory;
				case 5:		return map_id.BasicLab;
				case 6:		return map_id.CommandCenter;
				case 7:		return map_id.CommonOreSmelter;
				case 8:		return map_id.CommonStorage;
				case 9:		return map_id.ConsumerFactory;
				case 10:	return map_id.DIRT;
				case 11:	return map_id.Forum;
				case 12:	return map_id.Garage;
				case 13:	return map_id.GORF;
				case 14:	return map_id.GuardPost;
				case 15:	return map_id.LightTower;
				case 16:	return map_id.MedicalCenter;
				case 17:	return map_id.MeteorDefense;
				case 18:	return map_id.MHDGenerator;
				case 19:	return map_id.Nursery;
				case 20:	return map_id.Observatory;
				case 21:	return map_id.RareOreSmelter;
				case 22:	return map_id.RareStorage;
				case 23:	return map_id.RecreationFacility;
				case 24:	return map_id.ReinforcedResidence;
				case 25:	return map_id.Residence;
				case 26:	return map_id.RobotCommand;
				case 27:	return map_id.SolarPowerArray;
				case 28:	return map_id.Spaceport;
				case 29:	return map_id.StandardLab;
				case 30:	return map_id.StructureFactory;
				case 31:	return map_id.Tokamak;
				case 32:	return map_id.TradeCenter;
				case 33:	return map_id.University;
				case 34:	return map_id.VehicleFactory;
			}

			return map_id.None;
		}

		private void RefreshCargoTypeAsConvecOptions()
		{
			List<string> cargoOptions = new List<string>();
			cargoOptions.Add("Empty");
			cargoOptions.Add("Agridome");
			cargoOptions.Add("Advanced Lab");
			cargoOptions.Add("Advanced Residence");
			cargoOptions.Add("Arachnid Factory");
			cargoOptions.Add("Basic Lab");
			cargoOptions.Add("Command Center");
			cargoOptions.Add("Common Ore Smelter");
			cargoOptions.Add("Common Storage");
			cargoOptions.Add("Consumer Factory");
			cargoOptions.Add("DIRT");
			cargoOptions.Add("Forum");
			cargoOptions.Add("Garage");
			cargoOptions.Add("GORF");
			cargoOptions.Add("Guard Post");
			cargoOptions.Add("Light Tower");
			cargoOptions.Add("Medical Center");
			cargoOptions.Add("Meteor Defense");
			cargoOptions.Add("MHD Generator");
			cargoOptions.Add("Nursery");
			cargoOptions.Add("Observatory");
			cargoOptions.Add("Rare Ore Smelter");
			cargoOptions.Add("Rare Storage");
			cargoOptions.Add("Recreation Facility");
			cargoOptions.Add("Reinforced Residence");
			cargoOptions.Add("Residence");
			cargoOptions.Add("Robot Command");
			cargoOptions.Add("Solar Power Array");
			cargoOptions.Add("Spaceport");
			cargoOptions.Add("Standard Lab");
			cargoOptions.Add("Structure Factory");
			cargoOptions.Add("Tokamak");
			cargoOptions.Add("Trade Center");
			cargoOptions.Add("University");
			cargoOptions.Add("Vehicle Factory");

			m_DropdownCargoType.ClearOptions();
			m_DropdownCargoType.AddOptions(cargoOptions);
		}

		private bool IsQuantifiedCargo(int value)
		{
			return value >= 1 && value <= 7;
		}

		private int GetCargoTypeForTruckOption(int index)
		{
			switch (index)
			{
				case 0:		return (int)TruckCargo.Empty;
				case 1:		return (int)TruckCargo.Food;
				case 2:		return (int)TruckCargo.CommonOre;
				case 3:		return (int)TruckCargo.RareOre;
				case 4:		return (int)TruckCargo.CommonMetal;
				case 5:		return (int)TruckCargo.RareMetal;
				case 6:		return (int)TruckCargo.CommonRubble;
				case 7:		return (int)TruckCargo.RareRubble;
				case 8:		return (int)TruckCargo.Spaceport;	// Starship
				case 9:		return (int)TruckCargo.Garbage;		// Wreckage
				case 10:	return (int)map_id.IonDriveModule;	// Gene Bank
			}

			return 0;
		}

		private void RefreshCargoTypeAsTruckOptions()
		{
			List<string> cargoOptions = new List<string>();
			cargoOptions.Add("Empty");
			cargoOptions.Add("Food");
			cargoOptions.Add("Common Ore");
			cargoOptions.Add("Rare Ore");
			cargoOptions.Add("Common Metal");
			cargoOptions.Add("Rare Metal");
			cargoOptions.Add("Common Rubble");
			cargoOptions.Add("Rare Rubble");
			cargoOptions.Add("Starship");
			cargoOptions.Add("Wreckage");
			cargoOptions.Add("Gene Bank");

			m_DropdownCargoType.ClearOptions();
			m_DropdownCargoType.AddOptions(cargoOptions);
		}

		private int GetSubtypeForStarshipOption(int index)
		{
			switch (index)
			{
				case 0:		return (int)map_id.EDWARDSatellite;
				case 1:		return (int)map_id.SolarSatellite;
				case 2:		return (int)map_id.IonDriveModule;
				case 3:		return (int)map_id.FusionDriveModule;
				case 4:		return (int)map_id.CommandModule;
				case 5:		return (int)map_id.FuelingSystems;
				case 6:		return (int)map_id.HabitatRing;
				case 7:		return (int)map_id.SensorPackage;
				case 8:		return (int)map_id.Skydock;
				case 9:		return (int)map_id.StasisSystems;
				case 10:	return (int)map_id.OrbitalPackage;
				case 11:	return (int)map_id.PhoenixModule;
				case 12:	return (int)map_id.RareMetalsCargo;
				case 13:	return (int)map_id.CommonMetalsCargo;
				case 14:	return (int)map_id.FoodCargo;
				case 15:	return (int)map_id.EvacuationModule;
				case 16:	return (int)map_id.ChildrenModule;
			}

			return 0;
		}

		private void RefreshSubtypeAsStarshipOptions()
		{
			List<string> options = new List<string>();
			options.Add("EDWARD Satellite");
			options.Add("Solar Satellite");
			options.Add("Ion Drive Module");
			options.Add("Fusion Drive Module");
			options.Add("Command Module");
			options.Add("Fueling Systems");
			options.Add("Habitat Ring");
			options.Add("Sensor Package");
			options.Add("Skydock");
			options.Add("Stasis Systems");
			options.Add("Orbital Package");
			options.Add("Phoenix Module");
			options.Add("Rare Metals Cargo");
			options.Add("Common Metals Cargo");
			options.Add("Food Cargo");
			options.Add("Evacuation Module");
			options.Add("Children Module");

			m_DropdownCargoSubtype.ClearOptions();
			m_DropdownCargoSubtype.AddOptions(options);
		}

		private int GetSubtypeForWeaponOption(int index)
		{
			switch (index)
			{
				case 0:		return (int)map_id.AcidCloud;
				case 1:		return (int)map_id.EMP;
				case 2:		return (int)map_id.Laser;
				case 3:		return (int)map_id.Microwave;
				case 4:		return (int)map_id.RailGun;
				case 5:		return (int)map_id.RPG;
				case 6:		return (int)map_id.Starflare2;
				case 7:		return (int)map_id.Supernova2;
				case 8:		return (int)map_id.ESG;
				case 9:		return (int)map_id.Stickyfoam;
				case 10:	return (int)map_id.ThorsHammer;
			}

			return 0;
		}

		private void RefreshSubtypeAsWeaponOptions()
		{
			List<string> options = new List<string>();
			options.Add("Acid Cloud");
			options.Add("EMP");
			options.Add("Laser");
			options.Add("Microwave");
			options.Add("Rail Gun");
			options.Add("RPG");
			options.Add("Starflare");
			options.Add("Supernova");
			options.Add("ESG");
			options.Add("Stickyfoam");
			options.Add("Thor's Hammer");

			m_DropdownCargoSubtype.ClearOptions();
			m_DropdownCargoSubtype.AddOptions(options);
		}

		private bool AreUnitsOnTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			MissionVariant variant = UserData.current.GetCombinedVariant();

			foreach (PlayerData player in variant.players)
			{
				foreach (UnitData unit in UserData.current.GetCombinedResourceData(player).units)
				{
					RectInt otherArea = StructureData.GetStructureArea(new Vector2Int(unit.position.x, unit.position.y), unit.typeID);
					if (otherArea.Contains(tileXY))
						return true;
				}
			}

			return false;
		}

		private bool IsWallOnTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			MissionVariant variant = UserData.current.GetCombinedVariant();

			foreach (PlayerData player in variant.players)
			{
				foreach (WallTubeData wallTube in UserData.current.GetCombinedResourceData(player).wallTubes)
				{
					if (wallTube.typeID != map_id.Wall && wallTube.typeID != map_id.LavaWall && wallTube.typeID != map_id.MicrobeWall)
						continue;

					if (wallTube.position.x == tileXY.x && wallTube.position.y == tileXY.y)
						return true;
				}
			}

			return false;
		}

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChangedUserData;
			UserData.current.onSelectVariantCB -= OnChangedUserData;
		}
	}
}
