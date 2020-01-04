using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace OP2MissionEditor.Menu
{
	public class StatusBarController : MonoBehaviour
	{
		[SerializeField] private Text m_txtStatus				= default;
		[SerializeField] private Text m_txtCoordinates			= default;

		private Tilemap m_Tilemap;


		private void Awake()
		{
			Application.logMessageReceived += OnLogMessageReceived;

			// Find the primary tile map
			List<Tilemap> maps = new List<Tilemap>();
			Camera.main.transform.parent.GetComponentsInChildren(maps);
			m_Tilemap = maps.Find(map => map.name == "Tilemap");
		}

		private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			m_txtStatus.text = condition;
		}

		private void Update()
		{
			m_txtCoordinates.text = "";

			// If mouse is over UI, we should not show coordinates
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

			m_txtCoordinates.text = cell.x.ToString() + ", " + cell.y.ToString();
		}
	}
}
