using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.UserInterface
{
	public class ListBox : MonoBehaviour
	{
		[SerializeField] private Transform m_Content			= default;
		private ToggleGroup m_ToggleGroup;

		private List<ListBoxItem> m_Items = new List<ListBoxItem>();

		public delegate void OnSelectedItemCallback(ListBoxItem item);

		public event OnSelectedItemCallback onSelectedItemCB;


		public int count { get { return m_Items.Count;		}	}

		public int selectedIndex
		{
			get { return FindSelectedToggle();	}
			set { SetSelectedIndex(value);		}
		}

		public ListBoxItem selectedItem
		{
			get
			{
				int index = FindSelectedToggle();
				if (index >= 0)
					return m_Items[index];

				return null;
			}
		}


		// Use this for initialization
		void Awake()
		{
			m_ToggleGroup = GetComponent<ToggleGroup>();

			// Clear templates
			foreach (Transform t in m_Content)
				Destroy(t.gameObject);
		}

		private void OnEnable()
		{
			// Reset scroll position to top when enabled
			m_Content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		}

		public void AddItem(ListBoxItem item)
		{
			m_Items.Add(item);

			item.transform.SetParent(m_Content);
			item.transform.localScale = Vector3.one;

			Toggle toggle = item.GetComponent<Toggle>();
			if (toggle != null)
			{
				toggle.group = m_ToggleGroup;
				toggle.onValueChanged.AddListener(OnSelect_Item);
			}
		}

		public ListBoxItem[] GetItems()
		{
			return m_Items.ToArray();
		}

		public void Clear()
		{
			for (int i=0; i < m_Items.Count; ++i)
				Destroy(m_Items[i].gameObject);

			m_Items.Clear();
		}

		private void OnSelect_Item(bool isOn)
		{
			if (!isOn)
				return;

			int selectedIndex = FindSelectedToggle();

			// Inform listeners that item has been selected
			if (onSelectedItemCB != null)
				onSelectedItemCB(m_Items[selectedIndex]);
		}

		private int FindSelectedToggle()
		{
			for (int i=0; i < m_Items.Count; ++i)
			{
				Toggle toggle = m_Items[i].GetComponent<Toggle>();
				if (toggle.isOn)
					return i;
			}

			return -1;
		}

		private void SetSelectedIndex(int index)
		{
			if (index == -1)
				m_ToggleGroup.SetAllTogglesOff();

			for (int i=0; i < m_Items.Count; ++i)
			{
				Toggle toggle = m_Items[i].GetComponent<Toggle>();

				if (toggle.isOn && i != index)
					toggle.isOn = false;
				else if (!toggle.isOn && i == index)
					toggle.isOn = true;
			}
		}
	}
}
