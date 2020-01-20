using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed structure on the map.
	/// </summary>
	public class StructureView : UnitView
	{
		[SerializeField] private SpriteRenderer m_Renderer		= default;
		[SerializeField] private SpriteRenderer m_HealthFrame	= default;
		[SerializeField] private SpriteRenderer m_HealthBar		= default;
		[SerializeField] private SpriteRenderer m_BarYield		= default;
		[SerializeField] private TextMesh m_txtTopLeft			= default;
		[SerializeField] private TextMesh m_txtBottomRight		= default;

		[SerializeField] private Sprite[] m_SpritesByHealth		= default;
		[SerializeField] private Sprite[] m_BarYieldSprites		= default;

		private bool m_IsEden;

		public PlayerData player	{ get; private set; }
		public UnitData unit		{ get; private set; }


		public void Initialize(PlayerData player, UnitData unit)
		{
			this.player = player;
			this.unit = unit;

			m_IsEden = player.isEden;

			UserData.current.onChangedValuesCB += OnChanged_MissionData;

			// Set displayed sprite based on health
			if (unit.health > 0.66f)
				m_Renderer.sprite = m_SpritesByHealth[0];
			else if (unit.health > 0.33f)
				m_Renderer.sprite = m_SpritesByHealth[1];
			else
				m_Renderer.sprite = m_SpritesByHealth[2];

			// Set health bar color based on health
			if (unit.health > 0.5f)
				m_HealthBar.color = Color.green;
			else if (unit.health > 0.25f)
				m_HealthBar.color = Color.yellow;
			else
				m_HealthBar.color = Color.red;

			m_Renderer.material.SetInt("_PaletteIndex", (int)player.color);
			m_HealthBar.transform.localScale = new Vector3(unit.health, 1, 1);

			// Set mine bar yield
			if (unit.typeID == map_id.CommonOreMine || unit.typeID == map_id.RareOreMine)
			{
				m_BarYield.gameObject.SetActive(true);
				m_BarYield.sprite = m_BarYieldSprites[GetBarYieldIndex(unit.barYield)];
			}
			else
			{
				m_BarYield.gameObject.SetActive(false);
			}

			// Add to minimap
			m_UnitMinimap.AddUnit(this, GetMapCoordinates(new Vector2Int(unit.position.x, unit.position.y)), 2, GetStructureArea(Vector2Int.zero, unit.typeID));

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

		private int GetBarYieldIndex(Yield barYield)
		{
			switch (barYield)
			{
				case Yield.Random:	return 0;
				case Yield.Bar1:	return 1;
				case Yield.Bar2:	return 2;
				case Yield.Bar3:	return 3;
			}

			return 0;
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (unit.id > 0)
				m_txtTopLeft.text = unit.id.ToString();
			
			m_txtTopLeft.gameObject.SetActive(unit.id > 0);

			if (unit.barVariant != Variant.Random)
				m_txtBottomRight.text = "v" + ((int)(unit.barVariant)+1).ToString();
			
			m_txtBottomRight.gameObject.SetActive(unit.barVariant != Variant.Random);

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

		private Vector2Int GetStructureSize(map_id type)
		{
			switch (type)
			{
				case map_id.CommonOreMine:			return new Vector2Int(2,1);
				case map_id.RareOreMine:			return new Vector2Int(2,1);
				case map_id.GuardPost:				return new Vector2Int(1,1);
				case map_id.LightTower:				return new Vector2Int(1,1);
				case map_id.CommonStorage:			return new Vector2Int(1,2);
				case map_id.RareStorage:			return new Vector2Int(1,2);
				case map_id.Forum:					return new Vector2Int(2,2);
				case map_id.CommandCenter:			return new Vector2Int(3,2);
				case map_id.MHDGenerator:			return new Vector2Int(2,2);
				case map_id.Residence:				return new Vector2Int(2,2);
				case map_id.RobotCommand:			return new Vector2Int(2,2);
				case map_id.TradeCenter:			return new Vector2Int(2,2);
				case map_id.BasicLab:				return new Vector2Int(2,2);
				case map_id.MedicalCenter:			return new Vector2Int(2,2);
				case map_id.Nursery:				return new Vector2Int(2,2);
				case map_id.SolarPowerArray:		return new Vector2Int(3,2);
				case map_id.RecreationFacility:		return new Vector2Int(2,2);
				case map_id.University:				return new Vector2Int(2,2);
				case map_id.Agridome:				return new Vector2Int(3,2);
				case map_id.DIRT:					return new Vector2Int(3,2);
				case map_id.Garage:					return new Vector2Int(3,2);
				case map_id.MagmaWell:				return new Vector2Int(2,1);
				case map_id.MeteorDefense:			return new Vector2Int(2,2);
				case map_id.GeothermalPlant:		return new Vector2Int(2,1);
				case map_id.ArachnidFactory:		return new Vector2Int(2,2);
				case map_id.ConsumerFactory:		return new Vector2Int(3,3);
				case map_id.StructureFactory:		return new Vector2Int(4,3);
				case map_id.VehicleFactory:			return new Vector2Int(4,3);
				case map_id.StandardLab:			return new Vector2Int(3,2);
				case map_id.AdvancedLab:			return new Vector2Int(3,3);
				case map_id.Observatory:			return new Vector2Int(2,2);
				case map_id.ReinforcedResidence:	return new Vector2Int(3,2);
				case map_id.AdvancedResidence:		return new Vector2Int(3,3);
				case map_id.CommonOreSmelter:		return new Vector2Int(4,3);
				case map_id.Spaceport:				return new Vector2Int(5,4);
				case map_id.RareOreSmelter:			return new Vector2Int(4,3);
				case map_id.GORF:					return new Vector2Int(3,2);
				case map_id.Tokamak:				return new Vector2Int(2,2);
			}

			return new Vector2Int(1,1);
		}

		private RectInt GetStructureArea(Vector2Int position, map_id unitType)
		{
			Vector2Int size = GetStructureSize(unitType);

			RectInt rect = new RectInt();
			rect.xMin = position.x - size.x / 2;
			rect.yMin = position.y - size.y / 2;
			rect.xMax = position.x + (size.x-1) / 2 + 1;
			rect.yMax = position.y + (size.y-1) / 2 + 1;

			return rect;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			UserData.current.onChangedValuesCB -= OnChanged_MissionData;
		}
	}
}
