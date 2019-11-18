using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	public class MapScroller : MonoBehaviour
	{
		[SerializeField] private Tilemap m_Tilemap			= default;

		private void Awake()
		{
		}

		private void Update()
		{
			// Apply user input to move camera
			float speed = 300;

			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				speed *= 3;

			if (Input.GetKey(KeyCode.UpArrow))		{ Camera.main.transform.position += new Vector3(0, speed * Time.deltaTime, 0);			}
			if (Input.GetKey(KeyCode.DownArrow))	{ Camera.main.transform.position -= new Vector3(0, speed * Time.deltaTime, 0);			}
			if (Input.GetKey(KeyCode.LeftArrow))	{ Camera.main.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);			}
			if (Input.GetKey(KeyCode.RightArrow))	{ Camera.main.transform.position += new Vector3(speed * Time.deltaTime, 0, 0);			}

			// Keep camera in bounds
			Vector2 mapCenter = new Vector2(m_Tilemap.cellBounds.center.x*m_Tilemap.cellSize.x, m_Tilemap.cellBounds.center.y*m_Tilemap.cellSize.y);
			Vector2 mapSize = new Vector2(m_Tilemap.cellBounds.size.x*m_Tilemap.cellSize.x, m_Tilemap.cellBounds.size.y*m_Tilemap.cellSize.y);
			Bounds mapBounds = new Bounds(mapCenter, mapSize);

			Vector3 camPosition = Camera.main.transform.position;
			Vector2 screenSize = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);

			if (camPosition.x-screenSize.x < mapBounds.min.x)	camPosition.x = mapBounds.min.x+screenSize.x;
			if (camPosition.y-screenSize.y < mapBounds.min.y)	camPosition.y = mapBounds.min.y+screenSize.y;
			if (camPosition.x+screenSize.x > mapBounds.max.x)	camPosition.x = mapBounds.max.x-screenSize.x;
			if (camPosition.y+screenSize.y > mapBounds.max.y)	camPosition.y = mapBounds.max.y-screenSize.y;

			Camera.main.transform.position = camPosition;
		}
	}
}
