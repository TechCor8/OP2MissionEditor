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
		[SerializeField] private SpriteRenderer m_ColorOverlay	= default;
		[SerializeField] private SpriteRenderer m_HealthFrame	= default;
		[SerializeField] private SpriteRenderer m_HealthBar		= default;
		[SerializeField] private TextMesh m_txtTopLeft			= default;
		[SerializeField] private TextMesh m_txtBottomRight		= default;

		[SerializeField] private Sprite[] m_SpritesByHealth		= default;

		public PlayerData player	{ get; private set; }
		public UnitData unit		{ get; private set; }


		public void Initialize(PlayerData player, UnitData unit)
		{
			this.player = player;
			this.unit = unit;

			// Set displayed sprite based on health level
			if (unit.health > 0.66f)
			{
				m_Renderer.sprite = m_SpritesByHealth[0];
				m_HealthBar.color = Color.green;
			}
			if (unit.health > 0.33f)
			{
				m_Renderer.sprite = m_SpritesByHealth[1];
				m_HealthBar.color = Color.yellow;
			}
			else
			{
				m_Renderer.sprite = m_SpritesByHealth[2];
				m_HealthBar.color = Color.red;
			}

			m_ColorOverlay.color = GetPlayerColor();
			m_HealthBar.transform.localScale = new Vector3(unit.health, 1, 1);

			OnShowTextOverlay();
		}

		protected override void OnShowTextOverlay()
		{
			base.OnShowTextOverlay();

			if (unit.id > 0)
				m_txtTopLeft.text = unit.id.ToString();
			else
				m_txtTopLeft.gameObject.SetActive(false);

			if (unit.barVariant != Variant.Random)
				m_txtBottomRight.text = "v" + ((int)(unit.barVariant)+1).ToString();
			else
				m_txtBottomRight.gameObject.SetActive(false);

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
	}
}
