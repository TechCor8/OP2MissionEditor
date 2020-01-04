using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace OP2MissionEditor.UserInterface
{
	/// <summary>
	/// Displays info about a unit when the cursor is placed over the unit's tile.
	/// </summary>
	public class UnitInfoText : MonoBehaviour
	{
		[SerializeField] private Text m_txtHeaders			= default;

		private Text m_txtInfo;
		private Tilemap m_Tilemap;

		private bool m_IsUnitInfoVisible;
		

		private void Awake()
		{
			UserPrefs.onChangedPrefsCB += OnChangedPrefs;

			m_txtInfo = GetComponent<Text>();

			// Find the primary tile map
			List<Tilemap> maps = new List<Tilemap>();
			Camera.main.transform.parent.GetComponentsInChildren(maps);
			m_Tilemap = maps.Find(map => map.name == "Tilemap");

			// Show/Hide info text
			m_IsUnitInfoVisible = UserPrefs.isUnitInfoVisible;
			gameObject.SetActive(UserPrefs.isUnitInfoVisible);
		}

		private void OnChangedPrefs()
		{
			// Don't do anything if the info pref did not change.
			if (m_IsUnitInfoVisible == UserPrefs.isUnitInfoVisible)
				return;

			// Show/Hide info text
			m_IsUnitInfoVisible = UserPrefs.isUnitInfoVisible;
			gameObject.SetActive(UserPrefs.isUnitInfoVisible);
		}

		private void Update()
		{
			m_txtHeaders.text = "";
			m_txtInfo.text = "";

			// If mouse is over UI, we should not get unit info
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			// Get the tile that we are over
			Vector2 worldMousePt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cell = m_Tilemap.WorldToCell(worldMousePt);

			// Don't do anything out of bounds
			if (cell.x < 0 || cell.y < 0 || cell.x >= m_Tilemap.size.x || cell.y >= m_Tilemap.size.y)
				return;

			// Invert Y to match data storage instead of render value
			cell.y = m_Tilemap.size.y-(cell.y+1);

			// Add game coordinates
			cell += Vector3Int.one;

			// Get unit on tile
			UnitData unitOnTile = GetUnitOnTile(cell.x, cell.y);
			if (unitOnTile != null)
			{
				// Display info for unit on tile
				System.Text.StringBuilder headers = new System.Text.StringBuilder();
				System.Text.StringBuilder info = new System.Text.StringBuilder();

				string type = unitOnTile.typeID.ToString();

				// Add weapon to type
				if (unitOnTile.typeID == map_id.GuardPost || unitOnTile.typeID == map_id.Lynx || unitOnTile.typeID == map_id.Panther || unitOnTile.typeID == map_id.Tiger)
					type += " " + ((map_id)unitOnTile.cargoType).ToString();

				headers.AppendLine("ID:");		info.AppendLine(unitOnTile.id.ToString());
				headers.AppendLine("Type:");	info.AppendLine(type);
				if (IsVehicle(unitOnTile.typeID) || IsStructure(unitOnTile.typeID))		{ headers.AppendLine("Health:");	info.AppendLine((unitOnTile.health * 100).ToString("N2") + "%");	}
				if (IsVehicle(unitOnTile.typeID))										{ headers.AppendLine("Direction:");	info.AppendLine(unitOnTile.direction.ToString());					}
				if (IsVehicle(unitOnTile.typeID))										{ headers.AppendLine("Lights:");	info.AppendLine(unitOnTile.lights ? "On" : "Off");					}
				if (IsMine(unitOnTile.typeID) || Isbeacon(unitOnTile.typeID))			{ headers.AppendLine("Yield:");		info.AppendLine(unitOnTile.barYield.ToString());					}
				if (IsMine(unitOnTile.typeID) || Isbeacon(unitOnTile.typeID))			{ headers.AppendLine("Variant:");	info.AppendLine(unitOnTile.barVariant.ToString());					}
				string cargoText = GetCargoText(unitOnTile);
				if (cargoText != null)
				{
					string[] typeAmount = cargoText.Split(':');

					headers.AppendLine("Cargo:");		info.AppendLine(typeAmount[0]);
					if (typeAmount.Length >= 2)		{	headers.AppendLine("Amount:");		info.AppendLine(typeAmount[1]);		}
				}
				headers.AppendLine("Position:");	info.AppendLine(unitOnTile.position.x.ToString() + ", " + unitOnTile.position.y.ToString());

				m_txtHeaders.text = headers.ToString();
				m_txtInfo.text = info.ToString();
				return;
			}

			// Get beacon on tile
			GameData.Beacon beacon = UserData.current.mission.tethysGame.beacons.Find((b) => b.position.x == cell.x && b.position.y == cell.y);
			if (beacon != null)
			{
				// Display info for beacon on tile
				System.Text.StringBuilder headers = new System.Text.StringBuilder();
				System.Text.StringBuilder info = new System.Text.StringBuilder();

				headers.AppendLine("ID:");			info.AppendLine(beacon.id.ToString());
				headers.AppendLine("Type:");		info.AppendLine(beacon.mapID.ToString());
				if (beacon.mapID == map_id.MiningBeacon)
				{
					headers.AppendLine("Ore Type:");	info.AppendLine(beacon.oreType.ToString());
					headers.AppendLine("Yield:");		info.AppendLine(beacon.barYield.ToString());
					headers.AppendLine("Variant:");		info.AppendLine(beacon.barVariant.ToString());
				}
				headers.AppendLine("Position:");	info.AppendLine(beacon.position.x.ToString() + ", " + beacon.position.y.ToString());
				
				m_txtHeaders.text = headers.ToString();
				m_txtInfo.text = info.ToString();
				return;
			}

			// Get marker on tile
			GameData.Marker marker = UserData.current.mission.tethysGame.markers.Find((m) => m.position.x == cell.x && m.position.y == cell.y);
			if (marker != null)
			{
				// Display info for marker on tile
				System.Text.StringBuilder headers = new System.Text.StringBuilder();
				System.Text.StringBuilder info = new System.Text.StringBuilder();

				headers.AppendLine("ID:");			info.AppendLine(marker.id.ToString());
				headers.AppendLine("Type:");		info.AppendLine(marker.markerType.ToString());
				headers.AppendLine("Position:");	info.AppendLine(marker.position.x.ToString() + ", " + marker.position.y.ToString());
				
				m_txtHeaders.text = headers.ToString();
				m_txtInfo.text = info.ToString();
				return;
			}

			// Get wreckage on tile
			GameData.Wreckage wreckage = UserData.current.mission.tethysGame.wreckage.Find((w) => w.position.x == cell.x && w.position.y == cell.y);
			if (wreckage != null)
			{
				// Display info for wreckage on tile
				System.Text.StringBuilder headers = new System.Text.StringBuilder();
				System.Text.StringBuilder info = new System.Text.StringBuilder();

				headers.AppendLine("ID:");			info.AppendLine(wreckage.id.ToString());
				headers.AppendLine("Type:");		info.AppendLine(wreckage.techID.ToString());
				headers.AppendLine("Visible:");		info.AppendLine(wreckage.isVisible ? "Yes" : "No");
				headers.AppendLine("Position:");	info.AppendLine(wreckage.position.x.ToString() + ", " + wreckage.position.y.ToString());
				
				m_txtHeaders.text = headers.ToString();
				m_txtInfo.text = info.ToString();
				return;
			}

			// Get start location on tile
			foreach (PlayerData player in UserData.current.mission.players)
			{
				if (player.centerView.x == cell.x && player.centerView.y == cell.y)
				{
					// Display info for start location
					System.Text.StringBuilder headers = new System.Text.StringBuilder();
					System.Text.StringBuilder info = new System.Text.StringBuilder();

					headers.AppendLine("Player:");		info.AppendLine(player.id.ToString());
					headers.AppendLine("Type:");		info.AppendLine("Start Location");
					headers.AppendLine("Position:");	info.AppendLine(player.centerView.x.ToString() + ", " + player.centerView.y.ToString());
				
					m_txtHeaders.text = headers.ToString();
					m_txtInfo.text = info.ToString();
					return;
				}
			}
		}

		private UnitData GetUnitOnTile(int tileX, int tileY)
		{
			// Search for a unit on this tile
			foreach (PlayerData player in UserData.current.mission.players)
			{
				foreach (UnitData unit in player.units)
				{
					if (unit.position.x == tileX && unit.position.y == tileY)
						return unit;
				}
			}

			return null;
		}

		private string GetCargoText(UnitData unit)
		{
			switch (unit.typeID)
			{
				case map_id.CargoTruck:
					// Show cargo and amount, or starship module
					if (unit.cargoType >= 1 && unit.cargoType <= 7)
						return ((TruckCargo)unit.cargoType).ToString() + ":" + unit.cargoAmount;
					else if (unit.cargoType >= 8 && unit.cargoType <= 9)
						return ((map_id)unit.cargoAmount).ToString();

					return "Gene Bank";

				case map_id.ConVec:
					// Show structure kit
					map_id kit = (map_id)unit.cargoType;
					if (kit != map_id.GuardPost)
						return kit.ToString();
					else
						return kit.ToString() + ":" + ((map_id)unit.cargoAmount).ToString();
			}

			return null;
		}

		private bool IsVehicle(map_id unitType)							{ return (int)unitType >= 1 && (int)unitType <= 15;								}
		private bool IsStructure(map_id unitType)						{ return (int)unitType >= 21 && (int)unitType <= 58;							}
		private bool IsMine(map_id unitType)							{ return unitType == map_id.CommonOreMine || unitType == map_id.RareOreMine;	}
		private bool Isbeacon(map_id unitType)							{ return unitType == map_id.MiningBeacon;										}
	}
}
