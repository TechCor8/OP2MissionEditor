using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Data;
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
			UserData.current.onSelectVariantCB += OnChangedUserData;

			// Default variant to "Random"
			m_DropdownVariant.value = m_DropdownVariant.options.Count-1;

			RefreshPlayerDropdown();
			OnChanged_Player(m_DropdownPlayer.value);

			// Assign button listeners
			foreach (PaintButton btn in m_ButtonContainerEden.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_StructureButton, btn.name);

			foreach (PaintButton btn in m_ButtonContainerPlymouth.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_StructureButton, btn.name);
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

		private void OnClick_StructureButton(object data)
		{
			m_SelectedButtonName = (string)data;

			// Only mines have variants
			m_DropdownVariant.interactable = IsOreMineName(m_SelectedButtonName);

			RefreshOverlay();
		}

		public void RefreshOverlay()
		{
			if (!gameObject.activeSelf) return;

			if (m_SelectedButtonName == null)
				return;

			UnitData structure = GetStructureData();
			Vector2Int structureSize = StructureData.GetStructureSize(structure.typeID);
			structureSize += new Vector2Int(2, 2); // Bulldozed area
			Vector2 offset = new Vector2(-structureSize.x / 2, -structureSize.y / 2);
			offset *= m_Tilemap.cellSize;

			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];
			m_OverlayRenderer.SetOverlay(m_UnitRenderer.AddUnit(player, structure), structureSize, offset);
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			UnitData structure = GetStructureData();
			structure.position = new LOCATION(tileXY.x + 1, tileXY.y + 1); // Add game coordinates

			RectInt structureArea = StructureData.GetStructureArea(tileXY, structure.typeID);
			RectInt bulldozedArea = structureArea;
			bulldozedArea.min -= Vector2Int.one;
			bulldozedArea.max += Vector2Int.one;

			// Check if area is not buildable
			if (!TileMapData.IsAreaPassable(bulldozedArea))
				return;

			// Check if area is blocked by units or structures
			if (AreUnitsInArea(structureArea))
				return;

			// Check if area is blocked by walls
			if (AreWallsInArea(structureArea))
				return;

			// Add structure to tile
			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];
			UserData.current.GetPlayerResourceData(player.id).units.Add(structure);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(player, structure);
			m_MapRenderer.RefreshTiles(bulldozedArea);
		}

		private UnitData GetStructureData()
		{
			int id;
			int.TryParse(m_InputID.text, out id);

			// Create structure data
			UnitData structure = new UnitData();
			
			// Standard info
			structure.id = id;
			structure.typeID = GetMapIDFromName(m_SelectedButtonName);
			structure.cargoType = (int)GetCargoMapIDFromName(m_SelectedButtonName);
			structure.health = m_SliderHealth.value;
			structure.barVariant = Variant.Random;
			
			// Used for ore mines
			if (IsOreMineName(m_SelectedButtonName))
			{
				structure.barYield = GetYieldFromName(m_SelectedButtonName);
				structure.barVariant = GetVariant();
			}

			structure.position = new DataLocation(new LOCATION(1,1));

			return structure;
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// Find structure on tile
			int playerIndex = 0;
			int unitIndex = -1;

			for (playerIndex=0; playerIndex < UserData.current.selectedVariant.players.Count; ++playerIndex)
			{
				PlayerData.ResourceData resData = UserData.current.GetPlayerResourceData(playerIndex);

				for (int i=0; i < resData.units.Count; ++i)
				{
					UnitData unit = resData.units[i];

					RectInt structureArea = StructureData.GetStructureArea(new Vector2Int(unit.position.x, unit.position.y), unit.typeID);
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

			PlayerData.ResourceData playerResData = UserData.current.GetPlayerResourceData(playerIndex);
			
			UnitData structureToRemove = playerResData.units[unitIndex];

			// Remove structure from tile
			playerResData.units.RemoveAt(unitIndex);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(structureToRemove);
			m_MapRenderer.RefreshTiles(StructureData.GetBulldozedStructureArea(new Vector2Int(structureToRemove.position.x-1, structureToRemove.position.y-1), structureToRemove.typeID));
		}

		protected override void OnOverTile(Vector2Int tileXY)
		{
			base.OnOverTile(tileXY);

			UnitData structure = GetStructureData();
			Vector2Int structureSize = StructureData.GetStructureSize(structure.typeID);
			structureSize += new Vector2Int(2,2); // Bulldozed area
			Vector2Int minOffset = new Vector2Int(-structureSize.x / 2, -structureSize.y / 2);

			// Set each tile status based on collision within structure area
			for (int x=0; x < structureSize.x; ++x)
			{
				for (int y=0; y < structureSize.y; ++y)
				{
					Vector2Int localTileXY = new Vector2Int(x,y);
					Vector2Int curTileXY = tileXY + localTileXY + minOffset;

					bool isInBulldozedArea = x == 0 || y ==0 || x == structureSize.x-1 || y == structureSize.y-1;

					// Structures can't be placed on impassable terrain, including bulldozed area
					bool canPlace = TileMapData.IsTilePassable(curTileXY);

					// Structures can have units in bulldozed area, but not in building area
					if (canPlace && !isInBulldozedArea)
						canPlace = !AreUnitsInArea(new RectInt(curTileXY, Vector2Int.one));

					// Structures can have walls in bulldozed area, but not in building area
					if (canPlace && !isInBulldozedArea)
						canPlace = !AreWallsInArea(new RectInt(curTileXY, Vector2Int.one));

					Color color = Color.red;
					if (canPlace)
						color = isInBulldozedArea ? Color.yellow : Color.green;

					m_OverlayRenderer.SetTileStatus(localTileXY, color);
				}
			}
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

		private bool AreUnitsInArea(RectInt area)
		{
			MissionVariant variant = UserData.current.GetCombinedVariant();

			foreach (PlayerData player in variant.players)
			{
				foreach (UnitData unit in UserData.current.GetCombinedResourceData(player).units)
				{
					// Subtract game coordinates
					RectInt otherArea = StructureData.GetStructureArea(new Vector2Int(unit.position.x-1, unit.position.y-1), unit.typeID);

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

		private bool AreWallsInArea(RectInt area)
		{
			for (int x=area.xMin; x < area.xMax; ++x)
			{
				for (int y=area.yMin; y < area.yMax; ++y)
				{
					if (!IsWallOnTile(new Vector2Int(x,y)))
						return false;
				}
			}

			return true;
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
