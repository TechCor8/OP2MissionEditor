using DotNetMissionSDK;
using System.Collections;
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
		[SerializeField] private Dropdown	m_DropdownPlayer			= default;
		[SerializeField] private Dropdown	m_DropdownEditMode			= default;
		
		public delegate void OnChangedDataCallback();
		public delegate void OnCloseCallback();

		private UserData m_UserData;
		private OnCloseCallback m_OnCloseCB;
		

		private void Initialize(UserData userData, OnCloseCallback onCloseCB)
		{
			m_UserData = userData;
			m_OnCloseCB = onCloseCB;

			// Add player options
			m_DropdownPlayer.ClearOptions();
			List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
			for (int i=0; i < m_UserData.mission.players.Length; ++i)
				options.Add(new Dropdown.OptionData("Player " + (i+1)));

			m_DropdownPlayer.AddOptions(options);
				
			//m_DropdownPlayer.value			= (-(int)userData.mission.levelDetails.missionType)-1;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}

		// TODO: Poll for change in parameters (e.g. player count)

		/// <summary>
		/// Creates and presents the paint window to the user.
		/// </summary>
		/// <param name="userData">The user data to use for painting.</param>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static PaintDialog Create(UserData userData, OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/PaintDialog"));
			PaintDialog dialog = goDialog.GetComponent<PaintDialog>();
			dialog.Initialize(userData, onCloseCB);

			return dialog;
		}
	}
}
