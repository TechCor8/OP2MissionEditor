using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Handles rendering units on the minimap.
	/// </summary>
	public class UnitMinimap
	{
		private class UnitMapState
		{
			public UnitView view;
			public Vector2Int currentTileXY;
			public RectInt localBounds;
			public int sortOrder;

			public UnitMapState(UnitView view, Vector2Int currentTileXY, int sortOrder, RectInt localBounds)
			{
				this.view = view;
				this.currentTileXY = currentTileXY;
				this.sortOrder = sortOrder;
				this.localBounds = localBounds;
			}
		}

		private List<UnitMapState> m_Units = new List<UnitMapState>();
		private List<UnitMapState>[,] m_UnitMap;

		public Texture2D minimapTexture			{ get; private set; }


		/// <summary>
		/// Creates the minimap. Must be called whenever the map changes.
		/// </summary>
		public void CreateMinimap()
		{
			m_Units.Clear();

			// Create texture
			if (UserData.current.map.WidthInTiles() > 0)
			{
				minimapTexture = new Texture2D((int)UserData.current.map.WidthInTiles(), (int)UserData.current.map.HeightInTiles(), TextureFormat.ARGB32, false);
				minimapTexture.SetPixels32(new Color32[UserData.current.map.WidthInTiles() * UserData.current.map.HeightInTiles()]);
				minimapTexture.Apply();
			}

			// Create state map
			m_UnitMap = new List<UnitMapState>[UserData.current.map.WidthInTiles(), UserData.current.map.HeightInTiles()];
			for (int x=0; x < m_UnitMap.GetLength(0); ++x)
			{
				for (int y=0; y < m_UnitMap.GetLength(1); ++y)
					m_UnitMap[x,y] = new List<UnitMapState>();
			}
		}

		public void AddUnit(UnitView unitView, Vector2Int tileXY, int sortOrder)
		{
			AddUnit(unitView, tileXY, sortOrder, new RectInt(0,0,1,1));
		}

		public void AddUnit(UnitView unitView, Vector2Int tileXY, int sortOrder, RectInt localBounds)
		{
			UnitMapState state = new UnitMapState(unitView, tileXY, sortOrder, localBounds);

			RectInt worldBounds = new RectInt();
			worldBounds.min = tileXY + localBounds.min;
			worldBounds.max = tileXY + localBounds.max;

			// Add to list
			m_Units.Add(state);

			// Add to map
			for (int x=worldBounds.min.x; x < worldBounds.max.x; ++x)
			{
				if (x < 0 || x >= m_UnitMap.GetLength(0)) continue;

				for (int y=worldBounds.min.y; y < worldBounds.max.y; ++y)
				{
					if (y < 0 || y >= m_UnitMap.GetLength(1)) continue;
					m_UnitMap[x,y].Add(state);
					m_UnitMap[x,y].Sort((a,b) => a.sortOrder.CompareTo(b.sortOrder));
				}
			}

			// Refresh minimap
			RefreshMinimapTiles(worldBounds.min, worldBounds.max);
		}


		public void RemoveUnit(UnitView unitView)
		{
			_RemoveUnit(unitView);
		}

		private UnitMapState _RemoveUnit(UnitView unitView)
		{
			// Find unit in list
			int index = m_Units.FindIndex((state2) => state2.view == unitView);
			if (index < 0)
				return null;

			UnitMapState state = m_Units[index];

			// Remove from list
			m_Units.RemoveAt(index);

			RectInt worldBounds = new RectInt();
			worldBounds.min = state.currentTileXY + state.localBounds.min;
			worldBounds.max = state.currentTileXY + state.localBounds.max;

			// Remove from map
			for (int x=worldBounds.min.x; x < worldBounds.max.x; ++x)
			{
				if (x < 0 || x >= m_UnitMap.GetLength(0)) continue;

				for (int y=worldBounds.min.y; y < worldBounds.max.y; ++y)
				{
					if (y < 0 || y >= m_UnitMap.GetLength(1)) continue;
					m_UnitMap[x, y].Remove(state);
				}
			}
			

			// Refresh minimap
			RefreshMinimapTiles(worldBounds.min, worldBounds.max);

			return state;
		}

		public void MoveUnit(UnitView unitView, Vector2Int tileXY)
		{
			UnitMapState state = _RemoveUnit(unitView);
			if (state != null)
				AddUnit(unitView, tileXY, state.sortOrder, state.localBounds);
		}

		private void RefreshMinimapTiles(Vector2Int tileMin, Vector2Int tileMax)
		{
			for (int x=tileMin.x; x < tileMax.x; ++x)
			{
				if (x < 0 || x >= m_UnitMap.GetLength(0)) continue;

				for (int y=tileMin.y; y < tileMax.y; ++y)
				{
					if (y < 0 || y >= m_UnitMap.GetLength(1)) continue;
					RefreshMinimapTile(x,y);
				}
			}
		}

		private void RefreshMinimapTile(int x, int y)
		{
			List<UnitMapState> statesOnTile = m_UnitMap[x, y];
			if (statesOnTile.Count == 0)
			{
				// No units left on tile, clear it
				minimapTexture.SetPixel(x, y, Color.clear);
				minimapTexture.Apply();
			}
			else
			{
				// Use color of last unit in list
				UnitMapState lastState = statesOnTile[statesOnTile.Count-1];
				minimapTexture.SetPixel(x, y, lastState.view.GetMinimapColor());
				minimapTexture.Apply();
			}
		}
	}
}
