using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed beacon on the map.
	/// </summary>
	public class BeaconView : MonoBehaviour
	{
		public GameData.Beacon beacon { get; private set; }


		public void Initialize(GameData.Beacon beacon)
		{
			this.beacon = beacon;
		}
	}
}
