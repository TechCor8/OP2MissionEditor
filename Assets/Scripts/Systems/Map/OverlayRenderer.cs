﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OP2MissionEditor.Systems.Map
{
	public class OverlayRenderer : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer m_SpriteOverlay			= default;
		[SerializeField] private GameObject m_StatusTilePrefab			= default;

		private UnitView m_UnitView;
		private SpriteRenderer[,] m_StatusTiles;
		private GameObject m_StatusTileContainer;


		/// <summary>
		/// Sets a sprite (tile) overlay.
		/// </summary>
		public void SetOverlay(Sprite overlaySprite)
		{
			// Disable unit overlay
			if (m_UnitView != null)
				Destroy(m_UnitView.gameObject);

			ClearStatusOverlay();

			// Enable sprite overlay
			m_SpriteOverlay.enabled = true;

			m_SpriteOverlay.color = Color.white;
			m_SpriteOverlay.sprite = overlaySprite;
		}

		public void SetOverlayColor(Color color)
		{
			m_SpriteOverlay.color = color;
		}

		/// <summary>
		/// Sets a unit overlay.
		/// </summary>
		public void SetOverlay(UnitView overlayView, Vector2Int sizeInTiles, Vector2 minTileOffset)
		{
			// Remove previous overlay
			if (overlayView != m_UnitView && m_UnitView != null)
				Destroy(m_UnitView.gameObject);

			ClearStatusOverlay();

			// Disable sprite overlay
			m_SpriteOverlay.enabled = false;

			// Initialize view for overlay
			overlayView.transform.SetParent(transform);
			overlayView.SetCanShowTextOverlay(false);

			m_UnitView = overlayView;

			// Initialize tile status overlay
			m_StatusTiles = new SpriteRenderer[sizeInTiles.x, sizeInTiles.y];
			m_StatusTileContainer = new GameObject("StatusTileContainer");
			m_StatusTileContainer.transform.SetParent(transform);

			minTileOffset.y *= -1;

			for (int x=0; x < sizeInTiles.x; ++x)
			{
				for (int y=0; y < sizeInTiles.y; ++y)
				{
					m_StatusTiles[x,y] = Instantiate(m_StatusTilePrefab).GetComponent<SpriteRenderer>();
					m_StatusTiles[x,y].transform.SetParent(m_StatusTileContainer.transform);
					m_StatusTiles[x,y].transform.localPosition = new Vector3(m_StatusTiles[x,y].sprite.rect.width * x, m_StatusTiles[x,y].sprite.rect.height * -y) + (Vector3)minTileOffset;
				}
			}
		}

		private void ClearStatusOverlay()
		{
			if (m_StatusTileContainer == null)
				return;

			Destroy(m_StatusTileContainer);
			m_StatusTiles = new SpriteRenderer[0,0];
		}

		/// <summary>
		/// Sets the position of the overlay.
		/// </summary>
		public void SetPosition(Tilemap tilemap, Vector2Int tileXY)
		{
			// Set unit overlay position
			if (m_UnitView != null)
				m_UnitView.SetPosition(tileXY);

			// Remove game coordinates
			tileXY -= Vector2Int.one;

			// Invert Y to match render value instead of data storage
			tileXY.y = tilemap.size.y-tileXY.y-1;

			Vector3 worldPt = tilemap.CellToWorld((Vector3Int)tileXY);

			// Center point in tile
			worldPt.x += tilemap.cellSize.x / 2.0f;
			worldPt.y += tilemap.cellSize.y / 2.0f;

			// Set sprite overlay position
			m_SpriteOverlay.transform.localPosition = worldPt;

			// Set status tile container position
			if (m_StatusTileContainer != null)
				m_StatusTileContainer.transform.localPosition = worldPt;
		}

		/// <summary>
		/// Sets the collision status of a tile in the overlay.
		/// </summary>
		public void SetTileStatus(Vector2Int localTileXY, Color statusColor)
		{
			if (m_StatusTiles == null)
				return;

			Color color = statusColor;
			color.a = 0.5f;

			m_StatusTiles[localTileXY.x,localTileXY.y].color = color;
		}
	}
}
