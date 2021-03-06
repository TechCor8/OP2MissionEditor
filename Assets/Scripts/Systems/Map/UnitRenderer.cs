﻿using DotNetMissionSDK;
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
		[SerializeField] private Transform m_MarkerContainer	= default;
		[SerializeField] private Transform m_StartContainer		= default;

		public UnitMinimap unitMinimap			{ get; private set; } = new UnitMinimap();

		public delegate void OnProgressCallback(float progress);
		public delegate void OnCallback();

		public event OnProgressCallback onRefreshProgressCB;
		public event OnCallback onRefreshedCB;


		private void Start()
		{
			unitMinimap.CreateMinimap();
		}

		/// <summary>
		/// Refreshes the map units with data from UserData.
		/// </summary>
		public void Refresh(System.Action onCompleteCB=null)
		{
			StartCoroutine(_Refresh(onCompleteCB));
		}

		private IEnumerator _Refresh(System.Action onCompleteCB)
		{
			// Delete everything
			foreach (Transform t in m_ResourceContainer)
				Destroy(t.gameObject);

			foreach (Transform t in m_UnitContainer)
				Destroy(t.gameObject);

			foreach (Transform t in m_MarkerContainer)
				Destroy(t.gameObject);

			foreach (Transform t in m_StartContainer)
				Destroy(t.gameObject);

			// Create minimap texture
			unitMinimap.CreateMinimap();

			// Create units for master and selected variants
			if (UserData.current.selectedVariantIndex < 0)
				CreateUnitsForVariant(UserData.current.mission.masterVariant, true);
			else
			{
				CreateUnitsForVariant(UserData.current.mission.masterVariant, false);
				CreateUnitsForVariant(UserData.current.selectedVariant, true);
			}

			yield return 0;
			

			// Inform listeners that we are done
			onCompleteCB?.Invoke();
			onRefreshedCB?.Invoke();
		}

		private void CreateUnitsForVariant(MissionVariant variant, bool isActiveVariant)
		{
			bool isDifficultySelected = UserData.current.selectedDifficultyIndex >= 0;

			// Get gaia unit tint
			Color multiplier = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			Color variantColor = Color.white;
			if (!isActiveVariant) variantColor = multiplier;
			if (isDifficultySelected) variantColor = multiplier;

			Color difficultyColor = Color.white;
			if (!isActiveVariant) difficultyColor = multiplier;

			// Create variant master gaia units
			CreateGameData(variant.tethysGame, variantColor);

			// Create variant difficulty gaia units
			if (isDifficultySelected) CreateGameData(variant.tethysDifficulties[UserData.current.selectedDifficultyIndex], difficultyColor);

			// Get player unit tint
			multiplier = new Color(0.49f, 0.49f, 0.49f);
			variantColor = Color.white;
			if (!isActiveVariant) variantColor = multiplier;
			if (isDifficultySelected) variantColor = multiplier;

			difficultyColor = Color.white;
			if (!isActiveVariant) difficultyColor = multiplier;

			// Player data
			for (int i=0; i < variant.players.Count; ++i)
			{
				PlayerData player = variant.players[i];

				// Set start location
				SetStartLocation(i, player);

				// Create units (difficulty invariant)
				foreach (UnitData unit in player.resources.units)
					AddUnit(player, unit, variantColor);

				// Create units for selected difficulty
				if (isDifficultySelected)
				{
					foreach (UnitData unit in player.difficulties[UserData.current.selectedDifficultyIndex].units)
						AddUnit(player, unit, difficultyColor);
				}

				// Create walls and tubes
			}
		}

		private void CreateGameData(GameData tethysGame, Color tint)
		{
			// Create beacons
			foreach (GameData.Beacon beacon in tethysGame.beacons)
				AddUnit(beacon, tint);

			// Create markers
			foreach (GameData.Marker marker in tethysGame.markers)
				AddUnit(marker, tint);

			// Create wreckage
			foreach (GameData.Wreckage wreck in tethysGame.wreckage)
				AddUnit(wreck, tint);
		}

		public void SetStartLocation(int playerIndex, PlayerData player)
		{
			// Remove old start location
			List<StartLocationView> views = new List<StartLocationView>(m_StartContainer.GetComponentsInChildren<StartLocationView>());
			StartLocationView view = views.Find((v) => v.player == player);
			if (view != null)
				Destroy(view.gameObject);

			// Create new start location
			DataLocation centerView = UserData.current.GetPlayerResourceData(player).centerView;
			GameObject goUnit = CreateUnit("StartLocation", m_StartContainer, 0, new Vector2Int(centerView.x, centerView.y));
			view = goUnit.GetComponent<StartLocationView>();
			view.Initialize(player);
		}

		public UnitView AddUnit(GameData.Beacon beacon)				{ return AddUnit(beacon, Color.white);			}
		public UnitView AddUnit(GameData.Beacon beacon, Color tint)
		{
			string prefabPath = null;

			switch (beacon.mapID)
			{
				case map_id.Fumarole:		prefabPath = "Resource/Fumarole";																		break;
				case map_id.MagmaVent:		prefabPath = "Resource/MagmaVent";																		break;
				case map_id.MiningBeacon:	prefabPath = "Resource/" + beacon.oreType.ToString() + "Beacon" + GetBarYieldSuffix(beacon.barYield);	break;
			}

			if (prefabPath == null)
				return null;

			GameObject goUnit = CreateUnit(prefabPath, m_ResourceContainer, beacon.id, new Vector2Int(beacon.position.x,beacon.position.y));
			BeaconView view = goUnit.GetComponent<BeaconView>();
			view.Initialize(beacon, tint);
			return view;
		}

		public UnitView AddUnit(GameData.Marker marker)				{ return AddUnit(marker, Color.white);			}
		public UnitView AddUnit(GameData.Marker marker, Color tint)
		{
			GameObject goUnit = CreateUnit("Resource/" + marker.markerType.ToString(), m_MarkerContainer, marker.id, new Vector2Int(marker.position.x,marker.position.y));
			MarkerView view = goUnit.GetComponent<MarkerView>();
			view.Initialize(marker, tint);
			return view;
		}

		public UnitView AddUnit(GameData.Wreckage wreck)			{ return AddUnit(wreck, Color.white);			}
		public UnitView AddUnit(GameData.Wreckage wreck, Color tint)
		{
			GameObject goUnit = CreateUnit("Wreckage", m_ResourceContainer, wreck.id, new Vector2Int(wreck.position.x,wreck.position.y));
			WreckageView view = goUnit.GetComponent<WreckageView>();
			view.Initialize(wreck, tint);
			return view;
		}

		public UnitView AddUnit(PlayerData player, UnitData unit)	{ return AddUnit(player, unit, Color.white);	}
		public UnitView AddUnit(PlayerData player, UnitData unit, Color tint)
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
				view.Initialize(player, unit, tint);
				return view;
			}
			else
			{
				// Add vehicle
				GameObject goUnit = CreateUnit("Vehicles/" + edenPath + unit.typeID.ToString() + weaponType, m_UnitContainer, unit.id, new Vector2Int(unit.position.x, unit.position.y));
				VehicleView view = goUnit.GetComponent<VehicleView>();
				view.Initialize(player, unit, tint);
				return view;
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
			GameObject goUnit = Instantiate(Resources.Load<GameObject>("Game/" + resourcePath));
			goUnit.transform.SetParent(parent);
			goUnit.transform.localScale = Vector3.one;
			UnitView unit = goUnit.GetComponent<UnitView>();
			unit.Initialize(m_Tilemap, this);
			unit.SetPosition(gridPosition);

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
