using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Manages the display of the mission's units on the map.
	/// </summary>
	public class UnitRenderer : MonoBehaviour
	{
		[SerializeField] private Tilemap m_Tilemap				= default;

		[SerializeField] private Transform m_ResourceContainer	= default;
		[SerializeField] private Transform m_UnitContainer		= default;
		[SerializeField] private Transform m_StartContainer		= default;

		public delegate void OnProgressCallback(float progress);
		public delegate void OnCallback();

		public event OnProgressCallback onRefreshProgressCB;
		public event OnCallback onRefreshedCB;


		/// <summary>
		/// Refreshes the map units with data from UserData.
		/// </summary>
		public void Refresh(System.Action onCompleteCB=null)
		{
			StartCoroutine(_Refresh(onCompleteCB));
		}

		private IEnumerator _Refresh(System.Action onCompleteCB)
		{
			// Create beacons
			foreach (GameData.Beacon beacon in UserData.current.mission.tethysGame.beacons)
				AddUnit(beacon);

			// Create markers
			foreach (GameData.Marker marker in UserData.current.mission.tethysGame.markers)
				AddUnit(marker);

			// Create wreckage
			foreach (GameData.Wreckage wreck in UserData.current.mission.tethysGame.wreckage)
				AddUnit(wreck);

			// Create walls and tubes

			// Player data
			for (int i=0; i < UserData.current.mission.players.Length; ++i)
			{
				PlayerData player = UserData.current.mission.players[i];

				// Set start location
				SetStartLocation(i, player);

				// Create units
				foreach (UnitData unit in player.units)
					AddUnit(player, unit);
			}

			yield return 0;
			

			// Inform listeners that we are done
			onCompleteCB?.Invoke();
			onRefreshedCB?.Invoke();
		}

		public void SetStartLocation(int playerIndex, PlayerData player)
		{
			// Remove old start location
			List<StartLocationView> views = new List<StartLocationView>(m_StartContainer.GetComponentsInChildren<StartLocationView>());
			StartLocationView view = views.Find((v) => v.player == player);
			if (view != null)
				Destroy(view.gameObject);

			// Create new start location
			GameObject goUnit = CreateUnit("StartLocation", m_StartContainer, 0, new Vector2Int(player.centerView.x, player.centerView.y));
			view = goUnit.GetComponent<StartLocationView>();
			view.Initialize(player);
		}

		public void AddUnit(GameData.Beacon beacon)
		{
			string prefabPath = null;

			switch (beacon.mapID)
			{
				case map_id.Fumarole:		prefabPath = "Resource/Fumarole";																		break;
				case map_id.MagmaVent:		prefabPath = "Resource/MagmaVent";																		break;
				case map_id.MiningBeacon:	prefabPath = "Resource/" + beacon.oreType.ToString() + "Beacon" + GetBarYieldSuffix(beacon.barYield);	break;
			}

			if (prefabPath == null)
				return;

			GameObject goUnit = CreateUnit(prefabPath, m_ResourceContainer, beacon.id, new Vector2Int(beacon.position.x,beacon.position.y));
			BeaconView view = goUnit.GetComponent<BeaconView>();
			view.Initialize(beacon);
		}

		public void AddUnit(GameData.Marker marker)
		{
			GameObject goUnit = CreateUnit("Resource/" + marker.markerType.ToString(), m_ResourceContainer, marker.id, new Vector2Int(marker.position.x,marker.position.y));
			MarkerView view = goUnit.GetComponent<MarkerView>();
			view.Initialize(marker);
		}

		public void AddUnit(GameData.Wreckage wreck)
		{
			GameObject goUnit = CreateUnit("Wreckage", m_ResourceContainer, wreck.id, new Vector2Int(wreck.position.x,wreck.position.y));
			WreckageView view = goUnit.GetComponent<WreckageView>();
			view.Initialize(wreck);
		}

		public void AddUnit(PlayerData player, UnitData unit)
		{
			string edenPath = player.isEden ? "Eden/" : "Plymouth/";
			string weaponType = "";

			if (unit.typeID == map_id.GuardPost || unit.typeID == map_id.Lynx || unit.typeID == map_id.Panther || unit.typeID == map_id.Tiger)
				weaponType = "_" + ((map_id)unit.cargoType).ToString();
			else if (unit.typeID == map_id.CargoTruck && unit.cargoType != 0)
			{
				// Cargo truck type
				if (unit.cargoType >= 1 && unit.cargoType <= 7)
					weaponType = "_" + ((TruckCargo)unit.cargoType).ToString();
				else if (unit.cargoType == 8)
					weaponType = "_Starship";
				else if (unit.cargoType == 9)
					weaponType = "_Wreckage";
				else
					weaponType = "_GeneBank";
			}

			if (IsStructure(unit.typeID))
			{
				// Add structure
				GameObject goUnit = CreateUnit("Structures/" + edenPath + unit.typeID.ToString() + weaponType, m_UnitContainer, unit.id, new Vector2Int(unit.position.x, unit.position.y));
				StructureView view = goUnit.GetComponent<StructureView>();
				view.Initialize(player, unit);
			}
			else
			{
				// Add vehicle
				GameObject goUnit = CreateUnit("Vehicles/" + edenPath + unit.typeID.ToString() + weaponType, m_UnitContainer, unit.id, new Vector2Int(unit.position.x, unit.position.y));
				VehicleView view = goUnit.GetComponent<VehicleView>();
				view.Initialize(player, unit);
			}
		}

		public void RemoveUnit(GameData.Beacon beacon)
		{
			List<BeaconView> views = new List<BeaconView>(m_ResourceContainer.GetComponentsInChildren<BeaconView>());
			BeaconView view = views.Find((v) => v.beacon == beacon);
			Destroy(view.gameObject);
		}

		public void RemoveUnit(GameData.Marker marker)
		{
			List<MarkerView> views = new List<MarkerView>(m_ResourceContainer.GetComponentsInChildren<MarkerView>());
			MarkerView view = views.Find((v) => v.marker == marker);
			Destroy(view.gameObject);
		}

		public void RemoveUnit(GameData.Wreckage wreck)
		{
			List<WreckageView> views = new List<WreckageView>(m_ResourceContainer.GetComponentsInChildren<WreckageView>());
			WreckageView view = views.Find((v) => v.wreck == wreck);
			Destroy(view.gameObject);
		}

		public void RemoveUnit(UnitData unit)
		{
			if (IsStructure(unit.typeID))
			{
				// Remove structure
				List<StructureView> views = new List<StructureView>(m_UnitContainer.GetComponentsInChildren<StructureView>());
				StructureView view = views.Find((v) => v.unit == unit);
				Destroy(view.gameObject);
			}
			else
			{
				// Remove vehicle
				List<VehicleView> views = new List<VehicleView>(m_UnitContainer.GetComponentsInChildren<VehicleView>());
				VehicleView view = views.Find((v) => v.unit == unit);
				Destroy(view.gameObject);
			}
		}

		private GameObject CreateUnit(string resourcePath, Transform parent, int id, Vector2Int gridPosition)
		{
			// Remove game coordinates
			gridPosition -= Vector2Int.one;

			// Invert grid Y
			gridPosition.y = m_Tilemap.size.y-gridPosition.y-1;
			
			// Add half to center in the tile
			Vector3 position = new Vector3(gridPosition.x, gridPosition.y) + new Vector3(0.5f, 0.5f);

			GameObject goUnit = Instantiate(Resources.Load<GameObject>("Game/" + resourcePath));
			goUnit.transform.SetParent(parent);
			goUnit.transform.localScale = Vector3.one;
			goUnit.transform.position = Vector3.Scale(position, m_Tilemap.cellSize);
			goUnit.transform.localPosition = new Vector3(goUnit.transform.localPosition.x, goUnit.transform.localPosition.y, 0);

			return goUnit;
		}

		private int GetBarYieldSuffix(Yield barYield)
		{
			switch (barYield)
			{
				case Yield.Random:		return 0;
				case Yield.Bar1:		return 1;
				case Yield.Bar2:		return 2;
				case Yield.Bar3:		return 3;
			}

			return 0;
		}

		private bool IsVehicle(map_id unitType)							{ return (int)unitType >= 1 && (int)unitType <= 15;					}
		private bool IsStructure(map_id unitType)						{ return (int)unitType >= 21 && (int)unitType <= 58;				}
	}
}
