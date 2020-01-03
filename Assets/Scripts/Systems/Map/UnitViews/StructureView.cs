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

		public PlayerData player	{ get; private set; }
		public UnitData unit		{ get; private set; }


		public void Initialize(PlayerData player, UnitData unit)
		{
			this.player = player;
			this.unit = unit;

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

			OnShowTextOverlay();
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
	}
}
