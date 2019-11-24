using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.UserInterface
{
	public class ListBoxItem : MonoBehaviour
	{
		[SerializeField] private Image m_Background				= default;
		[SerializeField] private Image m_Icon					= default;

		[SerializeField] private Text m_txtLabel				= default;
		[SerializeField] private Text m_txtSubLabel				= default;

		public object userData;		// ListBoxItem owner can pass around whatever data they need


		// Use this for initialization
		private void Initialize(string label, string subLabel, Sprite icon)
		{
			if (m_txtLabel != null && label != null)		m_txtLabel.text = label;
			if (m_txtSubLabel != null && subLabel != null)	m_txtSubLabel.text = subLabel;
			if (m_Icon != null && icon != null)				m_Icon.sprite = icon;

			if (m_txtSubLabel != null)
				m_txtSubLabel.gameObject.SetActive(subLabel != null);
		}

		public virtual void SetColor(Color textColor, Color backgroundColor)
		{
			if (m_txtLabel != null)		m_txtLabel.color = textColor;
			if (m_txtSubLabel != null)	m_txtSubLabel.color = textColor;
			if (m_Background != null)	m_Background.color = backgroundColor;
		}

		public static ListBoxItem Create(GameObject prefab)								{ return Create(prefab, null, null, null);		}
		public static ListBoxItem Create(GameObject prefab, string label)				{ return Create(prefab, label, null, null);		}
		public static ListBoxItem Create(GameObject prefab, string label, Sprite icon)	{ return Create(prefab, label, null, icon);		}

		public static ListBoxItem Create(GameObject prefab, string label, string subLabel, Sprite icon)
		{
			GameObject goItem = Instantiate(prefab);
			ListBoxItem item = goItem.GetComponent<ListBoxItem>();
			item.Initialize(label, subLabel, icon);

			return item;
		}
	}
}
