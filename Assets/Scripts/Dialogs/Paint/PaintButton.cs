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
		

		/// <summary>
		/// Initializes a new button and adds it to a scroll view (or other container).
		/// </summary>
		public void Initialize(Transform parent, Sprite spriteToDisplay, OnClickCallback onClickCB, object data)
		{
			m_OnClickCB = onClickCB;
			m_Data = data;

			transform.SetParent(parent);
			GetComponent<Toggle>().group = parent.GetComponent<ToggleGroup>();

			m_SpriteImage.sprite = spriteToDisplay;
		}

		/// <summary>
		/// Initializes a button that is already in the scroll view.
		/// </summary>
		public void Initialize(OnClickCallback onClickCB, object data)
		{
			m_OnClickCB = onClickCB;
			m_Data = data;

			GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
		}

		public void OnToggle_Button(bool isOn)
		{
			if (!isOn)
				return;

			m_OnClickCB?.Invoke(m_Data);
		}
	}
}
