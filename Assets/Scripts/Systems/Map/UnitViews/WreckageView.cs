using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed wreckage on the map.
	/// </summary>
	public class WreckageView : MonoBehaviour
	{
		public GameData.Wreckage wreck { get; private set; }


		public void Initialize(GameData.Wreckage wreck)
		{
			this.wreck = wreck;
		}
	}
}
