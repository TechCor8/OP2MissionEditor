using OP2MissionEditor.Systems;
using OP2MissionEditor.Systems.Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for editing map terrain.
	/// </summary>
	public class TerrainPane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownTileset			= default;

		[SerializeField] private Transform m_TileButtonContainer		= default;
		[SerializeField] private GameObject	m_TileButtonPrefab			= default;

		private bool m_IsPainting;
		private ulong m_SelectedMappingIndex;

		
		protected override void Awake()
		{
			base.Awake();

			UserData.current.onChangedValuesCB += OnChangedUserData;

			RefreshTilesets();
		}

		private void OnChangedUserData(UserData src)
		{
			RefreshTilesets();
		}

		private void RefreshTilesets()
		{
			List<string> tilesetNames = new List<string>();

			ulong tilesetSourceCount = UserData.current.map.GetTilesetSourceCount();

			// Load all tilesets in map
			for (ulong i=0; i < tilesetSourceCount; ++i)
			{
				string tilesetFileName = UserData.current.map.GetTilesetSourceFilename(i);

				if (string.IsNullOrEmpty(tilesetFileName))
					continue;

				Texture2D tileset = TextureManager.LoadTileset(tilesetFileName);
				if (tileset == null)
					continue;

				tilesetNames.Add(tilesetFileName);
			}

			// Update tileset dropdown options
			m_DropdownTileset.ClearOptions();
			m_DropdownTileset.AddOptions(tilesetNames);

			// Refresh displayed tiles
			OnChanged_Tileset();
		}

		/// <summary>
		/// Called when tileset dropdown changes.
		/// </summary>
		public void OnChanged_Tileset()
		{
			// Clear old tile mappings
			foreach (Transform t in m_TileButtonContainer)
				Destroy(t.gameObject);

			if (m_DropdownTileset.options.Count == 0)
				return;

			SetTileset(m_DropdownTileset.options[m_DropdownTileset.value].text);
		}

		private void SetTileset(string tilesetName)
		{
			// Get tileset index
			ulong tilesetIndex = GetTilesetIndex(tilesetName);
			if (tilesetIndex == UserData.current.map.GetTilesetSourceCount())
			{
				Debug.LogError("Could not find tileset: " + tilesetName);
				return;
			}

			int numTiles = (int)UserData.current.map.GetTilesetSourceNumTiles(tilesetIndex);

			// Get tileset texture
			Texture2D tileset = TextureManager.GetTileset(tilesetName);
			if (tileset == null)
			{
				Debug.LogError("Tileset not loaded: " + tilesetName);
				return;
			}

			// Get all mappings for tileset
			ulong mappingCount = UserData.current.map.GetTileMappingCount();

			for (ulong i=0; i < mappingCount; ++i)
			{
				OP2UtilityDotNet.TileMapping mapping = UserData.current.map.GetTileMapping(i);

				if (mapping.tilesetIndex != tilesetIndex)
					continue;

				Sprite tileSprite = TextureManager.GetTileSprite(tileset, numTiles, mapping.tileGraphicIndex);

				// Create button for tile
				GameObject goTile = Instantiate(m_TileButtonPrefab);
				PaintButton button = goTile.GetComponent<PaintButton>();
				button.Initialize(m_TileButtonContainer, tileSprite, OnClick_Button, i);
			}
		}

		private ulong GetTilesetIndex(string tilesetName)
		{
			ulong tilesetSourceCount = UserData.current.map.GetTilesetSourceCount();
			
			for (ulong i=0; i < tilesetSourceCount; ++i)
			{
				if (UserData.current.map.GetTilesetSourceFilename(i) == tilesetName)
					return i;
			}

			return tilesetSourceCount;
		}

		private void OnClick_Button(object data)
		{
			m_SelectedMappingIndex = (ulong)data;
			m_IsPainting = true;
		}

		protected override void OnPaintTile(MapRenderer mapRenderer, Vector3Int tileXY)
		{
			if (!m_IsPainting)
				return;

			// Paint tile with mapping index
			UserData.current.map.SetTileMappingIndex((ulong)tileXY.x, (ulong)tileXY.y, m_SelectedMappingIndex);
			UserData.current.SetUnsaved();

			mapRenderer.RefreshTile(tileXY);
		}

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChangedUserData;
		}
	}
}
