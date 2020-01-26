using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OP2MissionEditor.UserInterface
{
	/// <summary>
	/// Activates Toggle button on mouse over if the ToggleGroup has active toggles.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class MouseOverToggle : MonoBehaviour, IPointerEnterHandler
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
			Toggle toggle = GetComponent<Toggle>();

			if (toggle.group != null)
			{
				if (toggle.group.AnyTogglesOn())
					toggle.isOn = true;
			}
		}
	}
}
