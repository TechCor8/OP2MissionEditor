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
		protected Tilemap m_Tilemap					{ get; private set; }
		protected MapRenderer m_MapRenderer			{ get; private set; }
		protected UnitRenderer m_UnitRenderer		{ get; private set; }
		protected OverlayRenderer m_OverlayRenderer { get; private set; }

		[System.NonSerialized] private bool m_IsPainting;
		[System.NonSerialized] private bool m_IsErasing;


		protected virtual void Awake()
		{
			// Find the primary tile map
			List<Tilemap> maps = new List<Tilemap>();
			Camera.main.transform.parent.GetComponentsInChildren(maps);
			m_Tilemap = maps.Find(map => map.name == "Tilemap");
			m_MapRenderer = m_Tilemap.GetComponent<MapRenderer>();

			// Find unit renderer
			List<UnitRenderer> unitRenderers = new List<UnitRenderer>();
			Camera.main.transform.parent.GetComponentsInChildren(unitRenderers);
			m_UnitRenderer = unitRenderers.Find(rend => rend.name == "Units");

			// Find overlay renderer
			m_OverlayRenderer = Camera.main.transform.parent.GetComponentInChildren<OverlayRenderer>(true);
		}

		protected virtual void Update()
		{
			UpdatePaint();
		}

		private void UpdatePaint()
		{
			// If mouse button is up, always stop painting
			if (!Input.GetMouseButton(0))
				m_IsPainting = false;

			if (!Input.GetMouseButton(1))
				m_IsErasing = false;

			// If mouse is over UI, we are not painting
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			// If we mouse down, and we are not over the UI, start painting
			if (Input.GetMouseButtonDown(0))
				m_IsPainting = true;

			if (Input.GetMouseButtonDown(1))
				m_IsErasing = true;

			// Get the tile that was clicked on
			Vector2 worldMousePt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cell = m_Tilemap.WorldToCell(worldMousePt);

			// Don't paint out of bounds
			if (cell.x < 0 || cell.y < 0 || cell.x >= m_Tilemap.size.x || cell.y >= m_Tilemap.size.y)
				return;

			// Invert Y to match data storage instead of render value
			cell.y = m_Tilemap.size.y-(cell.y+1);

			// Draw overlay
			OnOverTile((Vector2Int)cell);

			// Paint or erase
			if (m_IsPainting)
				OnPaintTile((Vector2Int)cell);
			else if (m_IsErasing)
				OnEraseTile((Vector2Int)cell);
		}

		protected virtual void OnPaintTile(Vector2Int tileXY)
		{
		}

		protected virtual void OnEraseTile(Vector2Int tileXY)
		{
		}

		protected virtual void OnOverTile(Vector2Int tileXY)
		{
			// Invert Y to match data storage instead of render value
			tileXY.y = m_Tilemap.size.y-tileXY.y-1;

			Vector3 worldPt = m_Tilemap.CellToWorld((Vector3Int)tileXY);

			// Center point in tile
			worldPt.x += m_Tilemap.cellSize.x / 2.0f;
			worldPt.y += m_Tilemap.cellSize.y / 2.0f;
			
			m_OverlayRenderer.SetPosition(worldPt);
		}
	}
}
