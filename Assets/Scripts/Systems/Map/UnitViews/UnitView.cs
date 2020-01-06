using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed unit on the map.
	/// </summary>
	public abstract class UnitView : MonoBehaviour
	{
		private Tilemap m_Tilemap;
		protected UnitMinimap m_UnitMinimap;
		protected UnitRenderer m_UnitRenderer;

		private bool m_CanShowTextOverlay = true;
		private bool m_IsUnitOverlayVisible;

		
		private void Awake()
		{
			UserPrefs.onChangedPrefsCB += OnChangedPrefs;
		}

		public void Initialize(Tilemap tilemap, UnitRenderer unitRenderer)
		{
			m_Tilemap = tilemap;
			m_UnitMinimap = unitRenderer.unitMinimap;
			m_UnitRenderer = unitRenderer;
		}

		public void SetPosition(Vector2Int gridPosition)
		{
			gridPosition = GetMapCoordinates(gridPosition);
			
			// Add half to center in the tile
			Vector3 position = new Vector3(gridPosition.x, gridPosition.y) + new Vector3(0.5f, 0.5f);

			transform.position = Vector3.Scale(position, m_Tilemap.cellSize);
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);

			m_UnitMinimap.MoveUnit(this, gridPosition);
		}

		protected Vector2Int GetMapCoordinates(Vector2Int gridPosition)
		{
			// Remove game coordinates
			gridPosition -= Vector2Int.one;

			// Invert grid Y
			gridPosition.y = m_Tilemap.size.y-gridPosition.y-1;

			return gridPosition;
		}

		public virtual Color GetMinimapColor()
		{
			return Color.white;
		}

		private void OnChangedPrefs()
		{
			// Don't do anything if the overlay pref did not change.
			if (m_IsUnitOverlayVisible == UserPrefs.isUnitOverlayVisible)
				return;

			if (!m_CanShowTextOverlay)
				return;

			RefreshOverlay();
		}

		protected void RefreshOverlay()
		{
			m_IsUnitOverlayVisible = UserPrefs.isUnitOverlayVisible;

			if (UserPrefs.isUnitOverlayVisible)
				OnShowTextOverlay();
			else
				OnHideTextOverlay();
		}

		public void SetCanShowTextOverlay(bool canShow)
		{
			m_CanShowTextOverlay = canShow;

			if (m_CanShowTextOverlay)
				RefreshOverlay();
			else
				OnHideTextOverlay();
		}

		protected virtual void OnShowTextOverlay() { }
		protected virtual void OnHideTextOverlay() { }


		protected virtual void OnDestroy()
		{
			UserPrefs.onChangedPrefsCB -= OnChangedPrefs;

			m_UnitMinimap.RemoveUnit(this);
		}
	}
}
