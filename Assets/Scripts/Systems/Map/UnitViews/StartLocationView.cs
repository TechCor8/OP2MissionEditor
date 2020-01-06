using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed start location on the map.
	/// </summary>
	public class StartLocationView : UnitView
	{
		[SerializeField] private SpriteRenderer m_ColorOverlay		= default;
		
		public PlayerData player	{ get; private set; }
		

		public void Initialize(PlayerData player)
		{
			this.player = player;

			m_ColorOverlay.color = GetPlayerColor();

			// Add to minimap
			m_UnitMinimap.AddUnit(this, GetMapCoordinates(new Vector2Int(player.centerView.x, player.centerView.y)), 4);

			RefreshOverlay();
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
	}
}
