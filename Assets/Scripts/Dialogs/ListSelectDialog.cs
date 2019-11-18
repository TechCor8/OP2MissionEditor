using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents a list of string items to the user for selection.
	/// </summary>
	public class ListSelectDialog : MonoBehaviour
	{
		[SerializeField] private Transform m_ItemContainer			= default;
		[SerializeField] private GameObject m_ItemPrefab			= default;

		[SerializeField] private Text m_txtTitle					= default;
		[SerializeField] private Text m_txtSelectButtonName			= default;

		[SerializeField] private Button m_btnSelect					= default;

		public delegate void OnSelectCallback(string itemName);
		public delegate void OnCancelCallback();

		private OnSelectCallback m_OnSelectCB;
		private OnCancelCallback m_OnCancelCB;

		private string m_SelectedItemName;


		private void Initialize(IEnumerable<string> itemNames, string title, string selectButtonName, OnSelectCallback onSelectCB, OnCancelCallback onCancelCB)
		{
			m_txtTitle.text = title;
			m_txtSelectButtonName.text = selectButtonName;

			m_OnSelectCB = onSelectCB;
			m_OnCancelCB = onCancelCB;

			m_btnSelect.interactable = false;

			// Create list of items in scroll view from itemNames
			foreach (string name in itemNames)
			{
				GameObject item = Instantiate(m_ItemPrefab);
				item.GetComponentInChildren<Text>().text = name;
				item.transform.SetParent(m_ItemContainer);
				item.transform.localScale = Vector3.one;
			}
		}

		public void OnClick_Select()
		{
			Destroy(gameObject);

			m_OnSelectCB?.Invoke(m_SelectedItemName);
		}

		public void OnClick_Cancel()
		{
			Destroy(gameObject);

			m_OnCancelCB?.Invoke();
		}

		public void OnSelect_Item()
		{
			foreach (Toggle toggle in m_ItemContainer.GetComponentsInChildren<Toggle>())
			{
				if (toggle.isOn)
					m_SelectedItemName = toggle.GetComponentInChildren<Text>().text;
			}

			m_btnSelect.interactable = true;
		}


		/// <summary>
		/// Creates and presents the selection dialog to the user.
		/// </summary>
		/// <param name="itemNames">The list of string items to present for selection.</param>
		/// <param name="title">The title of the dialog box.</param>
		/// <param name="selectButtonName">The text of the select button.</param>
		/// <param name="onSelectCB">The callback that receives the selected item.</param>
		/// <param name="onCancelCB">The callback for when the user cancels selection.</param>
		public static ListSelectDialog Create(IEnumerable<string> itemNames, string title, string selectButtonName, OnSelectCallback onSelectCB, OnCancelCallback onCancelCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/ListSelectDialog"));
			ListSelectDialog dialog = goDialog.GetComponent<ListSelectDialog>();
			dialog.Initialize(itemNames, title, selectButtonName, onSelectCB, onCancelCB);

			return dialog;
		}
	}
}
