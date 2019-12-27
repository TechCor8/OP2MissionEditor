﻿using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for creating resources.
	/// </summary>
	public class ResourcePane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownVariant				= default;
		[SerializeField] private InputField	m_InputID						= default;
		[SerializeField] private Transform m_ButtonContainer				= default;

		private string m_SelectedButtonName;


		protected override void Awake()
		{
			base.Awake();

			// Default variant to "Random"
			m_DropdownVariant.value = m_DropdownVariant.options.Count-1;

			// Assign button listeners
			foreach (PaintButton btn in m_ButtonContainer.GetComponentsInChildren<PaintButton>())
				btn.Initialize(OnClick_ResourceButton, btn.name);
		}

		private void OnClick_ResourceButton(object data)
		{
			m_SelectedButtonName = (string)data;

			// Markers do not have variants
			m_DropdownVariant.interactable = !IsMarkerName(m_SelectedButtonName);
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			if (IsMarkerName(m_SelectedButtonName))
				AddMarker(tileXY);
			else
				AddBeacon(tileXY);
		}

		private void AddMarker(Vector2Int tileXY)
		{
			// If tile already contains marker, cancel
			if (UserData.current.mission.tethysGame.markers.Find((m) => m.position.x == tileXY.x && m.position.y == tileXY.y) != null)
				return;

			int id;
			int.TryParse(m_InputID.text, out id);

			// Create marker data
			GameData.Marker marker = new GameData.Marker();
			marker.id = id;
			MarkerType mType;
			System.Enum.TryParse(m_SelectedButtonName, out mType);
			marker.markerType = mType;
			marker.position = new LOCATION(tileXY.x, tileXY.y);

			// Add marker to tile
			UserData.current.mission.tethysGame.markers.Add(marker);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(marker);
		}

		private void AddBeacon(Vector2Int tileXY)
		{
			// If tile already contains beacon, cancel
			if (UserData.current.mission.tethysGame.beacons.Find((b) => b.position.x == tileXY.x && b.position.y == tileXY.y) != null)
				return;

			int id;
			int.TryParse(m_InputID.text, out id);

			// Create beacon data
			GameData.Beacon beacon = new GameData.Beacon();
			beacon.id = id;
			beacon.mapID = GetMapIDFromName(m_SelectedButtonName);
			beacon.oreType = GetOreTypeFromName(m_SelectedButtonName);
			beacon.barYield = GetYieldFromName(m_SelectedButtonName);
			beacon.barVariant = GetVariant();
			beacon.position = new LOCATION(tileXY.x, tileXY.y);

			// Add beacon to tile
			UserData.current.mission.tethysGame.beacons.Add(beacon);
			UserData.current.SetUnsaved();

			m_UnitRenderer.AddUnit(beacon);
		}

		protected override void OnEraseTile(Vector2Int tileXY)
		{
			// Add game coordinates
			tileXY += Vector2Int.one;

			if (IsMarkerName(m_SelectedButtonName))
				RemoveMarker(tileXY);
			else
				RemoveBeacon(tileXY);
		}

		private void RemoveMarker(Vector2Int tileXY)
		{
			// Find marker on tile
			int index = UserData.current.mission.tethysGame.markers.FindIndex((m) => m.position.x == tileXY.x && m.position.y == tileXY.y);
			if (index < 0)
				return;

			GameData.Marker markerToRemove = UserData.current.mission.tethysGame.markers[index];

			// Remove marker from tile
			UserData.current.mission.tethysGame.markers.RemoveAt(index);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(markerToRemove);
		}

		private void RemoveBeacon(Vector2Int tileXY)
		{
			// Find beacon on tile
			int index = UserData.current.mission.tethysGame.beacons.FindIndex((b) => b.position.x == tileXY.x && b.position.y == tileXY.y);
			if (index < 0)
				return;

			GameData.Beacon beaconToRemove = UserData.current.mission.tethysGame.beacons[index];

			// Remove beacon from tile
			UserData.current.mission.tethysGame.beacons.RemoveAt(index);
			UserData.current.SetUnsaved();

			m_UnitRenderer.RemoveUnit(beaconToRemove);
		}

		private bool IsMarkerName(string name)
		{
			return System.Enum.IsDefined(typeof(MarkerType), name);
		}

		private map_id GetMapIDFromName(string name)
		{
			switch (name)
			{
				case "Fumarole":	return map_id.Fumarole;
				case "MagmaVent":	return map_id.MagmaVent;
			}

			return map_id.MiningBeacon;
		}

		private BeaconType GetOreTypeFromName(string name)
		{
			if (name.Contains("Common"))
				return BeaconType.Common;

			if (name.Contains("Rare"))
				return BeaconType.Rare;

			return BeaconType.Random;
		}

		private Yield GetYieldFromName(string name)
		{
			string suffix = name[name.Length-1].ToString();
			switch (suffix)
			{
				case "1": return Yield.Bar1;
				case "2": return Yield.Bar2;
				case "3": return Yield.Bar3;
			}

			return Yield.Random;
		}

		private Variant GetVariant()
		{
			switch (m_DropdownVariant.value)
			{
				case 0: return Variant.Variant1;
				case 1: return Variant.Variant2;
				case 2: return Variant.Variant3;
			}

			return Variant.Random;
		}
	}
}