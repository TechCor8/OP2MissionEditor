using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Represents a dynamically generated button on the paint window (e.g. tile and unit buttons).
	/// </summary>
	public class PaintButton : MonoBehaviour
	{
		[SerializeField] private Image m_SpriteImage			= default;

		public delegate void OnClickCallback(object data);

		private OnClickCallback m_OnClickCB;
		private object m_Data;
		

		public void Initialize(Transform parent, Sprite spriteToDisplay, OnClickCallback onClickCB, object data)
		{
			m_OnClickCB = onClickCB;
			m_Data = data;

			transform.SetParent(parent);
			GetComponent<Toggle>().group = parent.GetComponent<ToggleGroup>();

			m_SpriteImage.sprite = spriteToDisplay;
		}

		public void OnToggle_Button(bool isOn)
		{
			if (!isOn)
				return;

			m_OnClickCB?.Invoke(m_Data);
		}
	}
}
