﻿using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed vehicle on the map.
	/// </summary>
	public class VehicleView : UnitView
	{
		[SerializeField] private SpriteRenderer m_Renderer			= default;
		[SerializeField] private SpriteRenderer m_WeaponRenderer	= default;
		[SerializeField] private SpriteRenderer m_HealthFrame		= default;
		[SerializeField] private SpriteRenderer m_HealthBar			= default;
		[SerializeField] private TextMesh m_txtTopLeft				= default;
		[SerializeField] private TextMesh m_txtBottomRight			= default;

		[SerializeField] private Sprite[] m_BodySprites				= default;
		[SerializeField] private Sprite[] m_WeaponSprites			= default;

		private bool m_IsEden;

		public PlayerData player	{ get; private set; }
		public UnitData unit		{ get; private set; }


		public void Initialize(PlayerData player, UnitData unit, Color tint)
		{
			this.player = player;
			this.unit = unit;

			m_IsEden = player.isEden;

			UserData.current.onChangedValuesCB += OnChanged_MissionData;

			// Set displayed sprite based on direction
			m_Renderer.sprite = m_BodySprites[(int)unit.direction];
			if (m_WeaponRenderer != null && m_WeaponSprites.Length > 0)
				m_WeaponRenderer.sprite = m_WeaponSprites[(int)unit.direction];

			// Set health bar color based on health
			if (unit.health > 0.5f)
				m_HealthBar.color = Color.green;
			else if (unit.health > 0.25f)
				m_HealthBar.color = Color.yellow;
			else
				m_HealthBar.color = Color.red;

			m_Renderer.material.SetInt("_PaletteIndex", (int)player.color);
			m_WeaponRenderer.material.SetInt("_PaletteIndex", (int)player.color);
			m_HealthBar.transform.localScale = new Vector3(unit.health, 1, 1);

			m_Renderer.material.SetColor("_Color", tint);
			m_WeaponRenderer.material.SetColor("_Color", tint);
			
			// Add to minimap
			m_UnitMinimap.AddUnit(this, GetMapCoordinates(new Vector2Int(unit.position.x, unit.position.y)), 2);

			RefreshOverlay();
		}

		private void OnChanged_MissionData(UserData src)
		{
			// Check if player was destroyed
			bool foundPlayer = false;
			foreach (PlayerData pData in UserData.current.selectedVariant.players)
			{
				if (pData == player)
				{
					foundPlayer = true;
					break;
				}
			}

			if (!foundPlayer)
			{
				// Unit is no longer tied to a player. Destroy it.
				Destroy(gameObject);
				return;
			}

			if (player.isEden != m_IsEden)
			{
				// Player colony changed. Need to reinstantiate the unit.
				Destroy(gameObject);
				m_UnitRenderer.AddUnit(player, unit);
				return;
			}

			// Update player color
			m_Renderer.material.SetInt("_PaletteIndex", (int)player.color);
			m_WeaponRenderer.material.SetInt("_PaletteIndex", (int)player.color);
			m_UnitMinimap.MoveUnit(this, GetMapCoordinates(new Vector2Int(unit.position.x, unit.position.y)));
		}

		public override Color GetMinimapColor()
		{
			return GetPlayerColor();
		}

		private Color GetPlayerColor()
		{
			switch (player.color)
			{
				case PlayerColor.Blue:		return Color.blue;
				case PlayerColor.Red:		return Color.red;
				case PlayerColor.Green:		return Color.green;
				case PlayerColor.Yellow:	return Color.yellow;
				case PlayerColor.Cyan:		return Color.cyan;
				case PlayerColor.Magenta:	return Color.magenta;
				case PlayerColor.Black:		return Color.black;
			}

			return Color.white;
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (unit.id > 0)
				m_txtTopLeft.text = unit.id.ToString();

			m_txtTopLeft.gameObject.SetActive(unit.id > 0);

			if (unit.cargoType != 0)
			{
				switch (unit.typeID)
				{
					case map_id.CargoTruck:
						// Show cargo and amount, or starship module
						if (unit.cargoType >= 1 && unit.cargoType <= 7)
							m_txtBottomRight.text = ((TruckCargo)unit.cargoType).ToString() + ": " + unit.cargoAmount;
						else if (unit.cargoType >= 8 && unit.cargoType <= 9)
							m_txtBottomRight.text = ((map_id)unit.cargoAmount).ToString();
						else
							m_txtBottomRight.text = "Gene Bank";

						m_txtBottomRight.gameObject.SetActive(true);
						break;

					case map_id.ConVec:
						// Show structure kit
						m_txtBottomRight.text = ((map_id)unit.cargoType).ToString();
						m_txtBottomRight.gameObject.SetActive(true);
						break;

					default:
						m_txtBottomRight.gameObject.SetActive(false);
						break;
				}
			}
			else
			{
				m_txtBottomRight.gameObject.SetActive(false);
			}

			m_HealthFrame.gameObject.SetActive(true);
			m_HealthBar.gameObject.SetActive(true);
		}

		protected override void OnHideTextOverlay()
		{
			base.OnHideTextOverlay();

			m_txtTopLeft.gameObject.SetActive(false);
			m_txtBottomRight.gameObject.SetActive(false);

			m_HealthFrame.gameObject.SetActive(false);
			m_HealthBar.gameObject.SetActive(false);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			UserData.current.onChangedValuesCB -= OnChanged_MissionData;
		}
	}
}
