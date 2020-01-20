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
			
			UserData.current.onChangedValuesCB += OnChanged_MissionData;

			m_ColorOverlay.color = GetPlayerColor();

			// Add to minimap
			PlayerData.ResourceData resData = player.difficulties[UserData.current.selectedDifficultyIndex];
			m_UnitMinimap.AddUnit(this, GetMapCoordinates(new Vector2Int(resData.centerView.x, resData.centerView.y)), 4);

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

			// Update player color
			m_ColorOverlay.color = GetPlayerColor();

			PlayerData.ResourceData resData = player.difficulties[UserData.current.selectedDifficultyIndex];
			m_UnitMinimap.MoveUnit(this, GetMapCoordinates(new Vector2Int(resData.centerView.x, resData.centerView.y)));
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

		protected override void OnDestroy()
		{
			base.OnDestroy();

			UserData.current.onChangedValuesCB -= OnChanged_MissionData;
		}
	}
}
