using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor paint window to the user.
	/// </summary>
	public class PaintDialog : MonoBehaviour
	{
		[SerializeField] private Dropdown		m_DropdownEditMode			= default;
		[SerializeField] private GameObject[]	m_EditModePanes				= default;
		
		public delegate void OnCloseCallback();

		private OnCloseCallback m_OnCloseCB;
		

		private void Initialize(OnCloseCallback onCloseCB)
		{
			m_OnCloseCB = onCloseCB;

			// Set default edit mode to Terrain
			OnClick_EditMode(0);
		}

		public void OnClick_EditMode(int editMode)
		{
			// Activate correct pane for the selected mode
			for (int i=0; i < m_EditModePanes.Length; ++i)
			{
				if (editMode != i)
					m_EditModePanes[i].SetActive(false);
				else
					m_EditModePanes[editMode].SetActive(true);
			}
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}

		/// <summary>
		/// Creates and presents the paint window to the user.
		/// </summary>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static PaintDialog Create(OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/PaintDialog"));
			PaintDialog dialog = goDialog.GetComponent<PaintDialog>();
			dialog.Initialize(onCloseCB);

			return dialog;
		}
	}
}
