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
		[SerializeField] private Tilemap m_Tilemap			= default;

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
			{
				string prefabPath = null;

				switch (beacon.mapID)
				{
					case map_id.Fumarole:		prefabPath = "Resource/Fumarole";																		break;
					case map_id.MagmaVent:		prefabPath = "Resource/MagmaVent";																		break;
					case map_id.MiningBeacon:	prefabPath = "Resource/" + beacon.oreType.ToString() + "Beacon" + GetBarYieldSuffix(beacon.barYield);	break;
				}

				if (prefabPath == null)
					continue;

				CreateUnit(prefabPath, beacon.id, new Vector2Int(beacon.position.x,beacon.position.y));
			}

			// Create markers
			foreach (GameData.Marker marker in UserData.current.mission.tethysGame.markers)
				CreateUnit("Resource/" + marker.markerType.ToString(), marker.id, new Vector2Int(marker.position.x,marker.position.y));
			
			// Create wreckage
			foreach (GameData.Wreckage wreck in UserData.current.mission.tethysGame.wreckage)
				CreateUnit("Wreckage", wreck.id, new Vector2Int(wreck.position.x,wreck.position.y));

			// Create walls and tubes

			// Create units

			yield return 0;
			

			// Inform listeners that we are done
			onCompleteCB?.Invoke();
			onRefreshedCB?.Invoke();
		}

		public void AddUnit()
		{
		}

		public void RemoveUnit()
		{
		}

		private void CreateUnit(string resourcePath, int id, Vector2Int gridPosition)
		{
			// Remove game coordinates
			gridPosition -= Vector2Int.one;

			// Invert grid Y
			gridPosition.y = m_Tilemap.size.y-gridPosition.y-1;
			
			// Add half to center in the tile
			Vector3 position = new Vector3(gridPosition.x, gridPosition.y) + new Vector3(0.5f, 0.5f);

			GameObject goUnit = Instantiate(Resources.Load<GameObject>("Game/" + resourcePath));
			goUnit.transform.SetParent(transform);
			goUnit.transform.localScale = Vector3.one;
			goUnit.transform.position = Vector3.Scale(position, m_Tilemap.cellSize);
			goUnit.transform.localPosition = new Vector3(goUnit.transform.localPosition.x, goUnit.transform.localPosition.y, 0);
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
