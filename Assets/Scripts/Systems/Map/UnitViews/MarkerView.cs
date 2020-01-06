using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed marker on the map.
	/// </summary>
	public class MarkerView : UnitView
	{
		[SerializeField] private TextMesh m_txtTopLeft		= default;

		public GameData.Marker marker { get; private set; }


		public void Initialize(GameData.Marker marker)
		{
			this.marker = marker;

			// Add to minimap
			m_UnitMinimap.AddUnit(this, GetMapCoordinates(new Vector2Int(marker.position.x, marker.position.y)), 3);

			RefreshOverlay();
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (marker.id > 0)
				m_txtTopLeft.text = marker.id.ToString();
			
			m_txtTopLeft.gameObject.SetActive(marker.id > 0);
		}

		protected override void OnHideTextOverlay()
		{
			base.OnHideTextOverlay();

			m_txtTopLeft.gameObject.SetActive(false);
		}
	}
}
