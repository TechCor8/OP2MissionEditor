using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed beacon on the map.
	/// </summary>
	public class BeaconView : UnitView
	{
		[SerializeField] private TextMesh m_txtTopLeft		= default;
		[SerializeField] private TextMesh m_txtBottomRight	= default;

		public GameData.Beacon beacon { get; private set; }


		public void Initialize(GameData.Beacon beacon)
		{
			this.beacon = beacon;

			OnShowTextOverlay();
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (beacon.id > 0)
				m_txtTopLeft.text = beacon.id.ToString();

			m_txtTopLeft.gameObject.SetActive(beacon.id > 0);

			if (beacon.barVariant != DotNetMissionSDK.Variant.Random)
				m_txtBottomRight.text = "v" + ((int)(beacon.barVariant)+1).ToString();

			m_txtBottomRight.gameObject.SetActive(beacon.barVariant != DotNetMissionSDK.Variant.Random);
		}

		protected override void OnHideTextOverlay()
		{
			base.OnHideTextOverlay();

			m_txtTopLeft.gameObject.SetActive(false);
			m_txtBottomRight.gameObject.SetActive(false);
		}
	}
}
