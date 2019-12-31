using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed wreckage on the map.
	/// </summary>
	public class WreckageView : UnitView
	{
		[SerializeField] private TextMesh m_txtTopLeft		= default;
		[SerializeField] private TextMesh m_txtBottomRight	= default;
		[SerializeField] private GameObject m_goNotVisible	= default;

		public GameData.Wreckage wreck { get; private set; }


		public void Initialize(GameData.Wreckage wreck)
		{
			this.wreck = wreck;

			OnShowTextOverlay();
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (wreck.id > 0)
				m_txtTopLeft.text = wreck.id.ToString();

			m_txtTopLeft.gameObject.SetActive(wreck.id > 0);

			m_txtBottomRight.text = GetShortName(wreck.techID);
			m_goNotVisible.SetActive(!wreck.isVisible);
		}

		protected override void OnHideTextOverlay()
		{
			base.OnHideTextOverlay();

			m_txtTopLeft.gameObject.SetActive(false);
			m_txtBottomRight.gameObject.SetActive(false);
			m_goNotVisible.SetActive(false);
		}

		private string GetShortName(map_id wreckageType)
		{
			switch (wreckageType)
			{
				case map_id.EDWARDSatellite:	return "ED";
				case map_id.SolarSatellite:		return "Solar";
				case map_id.IonDriveModule:		return "Ion";
				case map_id.FusionDriveModule:	return "Fuse";
				case map_id.CommandModule:		return "Comm";
				case map_id.FuelingSystems:		return "Fuel";
				case map_id.HabitatRing:		return "Ring";
				case map_id.SensorPackage:		return "Sensr";
				case map_id.Skydock:			return "Dock";
				case map_id.StasisSystems:		return "Stasis";
				case map_id.OrbitalPackage:		return "Orbital";
				case map_id.PhoenixModule:		return "Fenix";
				case map_id.RareMetalsCargo:	return "Rare";
				case map_id.CommonMetalsCargo:	return "Metal";
				case map_id.FoodCargo:			return "Food";
				case map_id.EvacuationModule:	return "Evac";
				case map_id.ChildrenModule:		return "Child";
			}

			return wreckageType.ToString();
		}
	}
}
