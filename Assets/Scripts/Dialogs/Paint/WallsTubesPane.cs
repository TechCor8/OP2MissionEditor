using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Data;
using OP2MissionEditor.Systems.Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for creating walls and tubes.
	/// </summary>
	public class WallsTubesPane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownPlayer				= default;
		[SerializeField] private Transform	m_ButtonContainer				= default;

		[SerializeField] private Sprite[]	m_IconSprites					= default;

		private string m_SelectedButtonName;


		protected override void Awake()
		{
			base.Awake();

			UserData.current.onChangedValuesCB += OnChangedUserData;
			UserData.current.onSelectVariantCB += OnChangedUserData;

			RefreshPlayerDropdown();
			OnChanged_Player(m_DropdownPlayer.value);

			// Assign button listeners
			foreach (PaintButton btn in m_ButtonContainer.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_ItemButton, btn.name);
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
			RefreshOverlay();
		}

		private void OnClick_ItemButton(object data)
		{
			m_SelectedButtonName = (string)data;

			RefreshOverlay();
		}

		public void RefreshOverlay()
		{
			if (!gameObject.activeSelf) return;

			if (m_SelectedButtonName == null)
				return;

			m_OverlayRenderer.SetOverlay(GetSpriteFromName(m_SelectedButtonName));
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			// Check if tile is passable
			if (!TileMapData.IsTilePassable(tileXY))
				return;

			// Check if area is blocked by units or structures
			if (AreUnitsOnTile(tileXY))
				return;

			if (IsWallOnTile(tileXY))
				return;

			if (IsTubeOnTile(tileXY))
				return;

			// Add game coordinates
			tileXY += Vector2Int.one;

			// Create data
			WallTubeData wallTube = new WallTubeData();
			wallTube.typeID = GetMapIDFromName(m_SelectedButtonName);
			wallTube.position = new LOCATION(tileXY.x, tileXY.y);

			// Add wallTube to tile
			PlayerData player = UserData.current.selectedVariant.players[m_DropdownPlayer.value];
			UserData.current.GetPlayerResourceData(player).wallTubes.Add(wallTube);
			UserData.current.SetUnsaved();

			// Remove game coordinates
			tileXY -= Vector2Int.one;

			m_MapRenderer.RefreshTile(tileXY);
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			// Find wall or tube on tile
			int playerIndex = 0;
			int wallTubeIndex = -1;

			for (playerIndex=0; playerIndex < UserData.current.selectedVariant.players.Count; ++playerIndex)
			{
				PlayerData.ResourceData playerResData = UserData.current.GetPlayerResourceData(playerIndex);

				for (int i=0; i < playerResData.wallTubes.Count; ++i)
				{
					WallTubeData wallTube = playerResData.wallTubes[i];

					if (wallTube.position.x == tileXY.x && wallTube.position.y == tileXY.y)
					{
						wallTubeIndex = i;
						break;
					}
				}

				if (wallTubeIndex >= 0)
					break;
			}

			if (wallTubeIndex < 0)
				return;
			
			// Remove wall or tube from tile
			UserData.current.GetPlayerResourceData(playerIndex).wallTubes.RemoveAt(wallTubeIndex);
			UserData.current.SetUnsaved();

			// Remove game coordinates
			tileXY -= Vector2Int.one;

			m_MapRenderer.RefreshTile(tileXY);
		}

		protected override void OnOverTile(Vector2Int tileXY)
		{
			base.OnOverTile(tileXY);

			bool canPlace = TileMapData.IsTilePassable(tileXY) && !AreUnitsOnTile(tileXY);
			m_OverlayRenderer.SetOverlayColor(canPlace ? Color.white : Color.red);
		}

		private map_id GetMapIDFromName(string name)
		{
			// Parse name with map_id enum
			map_id typeID;
			if (!System.Enum.TryParse(name, out typeID))
				throw new System.ArgumentException("Name is not a valid map_id: " + name);

			return typeID;
		}

		private Sprite GetSpriteFromName(string name)
		{
			switch (GetMapIDFromName(name))
			{
				case map_id.Tube:			return m_IconSprites[0];
				case map_id.Wall:			return m_IconSprites[1];
				case map_id.LavaWall:		return m_IconSprites[2];
				case map_id.MicrobeWall:	return m_IconSprites[3];
			}

			return null;
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
					if (wallTube.typeID != map_id.Wall || wallTube.typeID != map_id.LavaWall || wallTube.typeID != map_id.MicrobeWall)
						continue;

					if (wallTube.position.x == tileXY.x && wallTube.position.y == tileXY.y)
						return true;
				}
			}

			return false;
		}

		private bool IsTubeOnTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			MissionVariant variant = UserData.current.GetCombinedVariant();

			foreach (PlayerData player in variant.players)
			{
				foreach (WallTubeData wallTube in UserData.current.GetCombinedResourceData(player).wallTubes)
				{
					if (wallTube.typeID != map_id.Tube)
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
