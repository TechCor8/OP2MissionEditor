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
		[SerializeField] private Dropdown		m_DropdownPlayer			= default;
		[SerializeField] private Dropdown		m_DropdownEditMode			= default;
		[SerializeField] private GameObject[]	m_EditModePanes				= default;
		
		public delegate void OnChangedDataCallback();
		public delegate void OnCloseCallback();

		private OnCloseCallback m_OnCloseCB;
		

		private void Initialize(OnCloseCallback onCloseCB)
		{
			UserData.current.onChangedValuesCB += OnChangedUserData;

			m_OnCloseCB = onCloseCB;

			// Set default edit mode to Terrain
			OnClick_EditMode(0);

			Refresh();
		}

		private void OnChangedUserData(UserData src)
		{
			Refresh();
		}

		private void Refresh()
		{
			// Add player options
			m_DropdownPlayer.ClearOptions();
			List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
			for (int i=0; i < UserData.current.mission.players.Length; ++i)
				options.Add(new Dropdown.OptionData("Player " + (i+1)));

			m_DropdownPlayer.AddOptions(options);

			//m_DropdownPlayer.value			= (-(int)userData.mission.levelDetails.missionType)-1;
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

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChangedUserData;
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
