using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.UserInterface
{
	/// <summary>
	/// Displays a slider's value in a text field.
	/// </summary>
	public class SliderValueText : MonoBehaviour
	{
		public void OnValueChanged(float value)
		{
			GetComponent<Text>().text = value.ToString("N0");
		}
	}
}
