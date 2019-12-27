using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed unit on the map.
	/// </summary>
	public abstract class UnitView : MonoBehaviour
	{
		protected virtual void OnShowTextOverlay() { }
		protected virtual void OnHideTextOverlay() { }
	}
}
