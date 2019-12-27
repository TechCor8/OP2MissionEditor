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
		[SerializeField] private Transform m_StructureContainer	= default;
		[SerializeField] private Transform m_UnitContainer		= default;

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

			// Create units

			yield return 0;
			

			// Inform listeners that we are done
			onCompleteCB?.Invoke();
			onRefreshedCB?.Invoke();
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
	}
}
