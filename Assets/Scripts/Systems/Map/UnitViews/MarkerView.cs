using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed marker on the map.
	/// </summary>
	public class MarkerView : MonoBehaviour
	{
		public GameData.Marker marker { get; private set; }


		public void Initialize(GameData.Marker marker)
		{
			this.marker = marker;
		}
	}
}
