using OP2MissionEditor.Systems;
using OP2UtilityDotNet.OP2Map;
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
		private int m_SelectedMappingIndex;

		
		protected override void Awake()
		{
			base.Awake();

			UserData.current.onChangedValuesCB += OnChangedUserData;

			RefreshTilesets();
		}

		private void OnEnable()
		{
			RefreshOverlay();
		}

		private void OnChangedUserData(UserData src)
		{
			RefreshTilesets();
		}

		private void RefreshTilesets()
		{
			List<string> tilesetNames = new List<string>();

			int tilesetSourceCount = UserData.current.map.tilesetSources.Count;

			// Load all tilesets in map
			for (int i=0; i < tilesetSourceCount; ++i)
			{
				string tilesetFileName = UserData.current.map.tilesetSources[i].tilesetFilename;

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
			int tilesetIndex = GetTilesetIndex(tilesetName);
			if (tilesetIndex == UserData.current.map.tilesetSources.Count)
			{
				Debug.LogError("Could not find tileset: " + tilesetName);
				return;
			}

			int numTiles = (int)UserData.current.map.tilesetSources[tilesetIndex].numTiles;

			// Get tileset texture
			Texture2D tileset = TextureManager.GetTileset(tilesetName);
			if (tileset == null)
			{
				Debug.LogError("Tileset not loaded: " + tilesetName);
				return;
			}

			// Get all mappings for tileset
			int mappingCount = UserData.current.map.tileMappings.Count;

			for (int i=0; i < mappingCount; ++i)
			{
				TileMapping mapping = UserData.current.map.tileMappings[i];

				if (mapping.tilesetIndex != tilesetIndex)
					continue;

				Sprite tileSprite = TextureManager.GetTileSprite(tileset, numTiles, mapping.tileGraphicIndex);

				// Create button for tile
				GameObject goTile = Instantiate(m_TileButtonPrefab);
				PaintButton button = goTile.GetComponent<PaintButton>();
				button.Initialize(m_TileButtonContainer, tileSprite, OnClick_Button, i);
			}
		}

		private int GetTilesetIndex(string tilesetName)
		{
			int tilesetSourceCount = UserData.current.map.tilesetSources.Count;
			
			for (int i=0; i < tilesetSourceCount; ++i)
			{
				if (UserData.current.map.tilesetSources[i].tilesetFilename == tilesetName)
					return i;
			}

			return tilesetSourceCount;
		}

		private void OnClick_Button(object data)
		{
			m_SelectedMappingIndex = (int)data;
			m_IsPainting = true;

			RefreshOverlay();
		}

		private void RefreshOverlay()
		{
			if (!gameObject.activeSelf) return;

			if (m_SelectedMappingIndex < 0 || m_SelectedMappingIndex >= UserData.current.map.tileMappings.Count)
				return;

			// Get mapping and tileset data from mapping index
			TileMapping mapping = UserData.current.map.tileMappings[m_SelectedMappingIndex];
			string tilesetName = UserData.current.map.tilesetSources[mapping.tilesetIndex].tilesetFilename;
			Texture2D tileset = TextureManager.GetTileset(tilesetName);
			int numTiles = (int)UserData.current.map.tilesetSources[mapping.tilesetIndex].numTiles;

			// Set overlay sprite to mapping
			m_OverlayRenderer.SetOverlay(TextureManager.GetTileSprite(tileset, numTiles, mapping.tileGraphicIndex));
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			if (!m_IsPainting)
				return;

			// Paint tile with mapping index
			UserData.current.map.SetTileMappingIndex(tileXY.x, tileXY.y, (uint)m_SelectedMappingIndex);
			UserData.current.SetUnsaved();

			m_MapRenderer.RefreshTile(tileXY);
		}

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChangedUserData;
		}
	}
}
