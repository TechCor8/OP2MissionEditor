using OP2MissionEditor.Systems.Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Base class for a PaintDialog pane.
	/// Determines if a tile has been painted. If so, child panes will receive the tile to paint.
	/// </summary>
	public class PaintPane : MonoBehaviour
	{
		private Tilemap m_Tilemap;
		protected MapRenderer m_MapRenderer { get; private set; }

		[System.NonSerialized] private bool m_IsPainting;


		protected virtual void Awake()
		{
			// Find the primary tile map
			List<Tilemap> maps = new List<Tilemap>();
			Camera.main.transform.parent.GetComponentsInChildren(maps);
			m_Tilemap = maps.Find(map => map.name == "Tilemap");
			m_MapRenderer = m_Tilemap.GetComponent<MapRenderer>();
		}

		protected virtual void Update()
		{
			UpdatePaint();
		}

		private void UpdatePaint()
		{
			// If mouse button is up, always stop painting
			if (Input.GetMouseButtonUp(0))
				m_IsPainting = false;

			// If mouse is over UI, we are not painting
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			// If we mouse down, and we are not over the UI, start painting
			if (Input.GetMouseButtonDown(0))
				m_IsPainting = true;

			if (!m_IsPainting)
				return;

			// Get the tile that was clicked on
			Vector2 worldMousePt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cell = m_Tilemap.WorldToCell(worldMousePt);

			// Don't paint out of bounds
			if (cell.x < 0 || cell.y < 0 || cell.x >= m_Tilemap.size.x || cell.y >= m_Tilemap.size.y)
				return;

			// Invert Y to match data storage instead of render value
			cell.y = m_Tilemap.size.y-(cell.y+1);

			OnPaintTile(cell);
		}

		protected virtual void OnPaintTile(Vector3Int tileXY)
		{
		}
	}
}
