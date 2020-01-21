using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor mission properties to the user.
	/// </summary>
	public class MissionPropertiesDialog : MonoBehaviour
	{
		[SerializeField] private InputField	m_InputDescription			= default;
		[SerializeField] private InputField	m_InputMapName				= default;
		[SerializeField] private InputField	m_InputTechTreeName			= default;
		[SerializeField] private Dropdown	m_DropdownMissionType		= default;
		[SerializeField] private InputField	m_InputMaxTechLevel			= default;
		[SerializeField] private Toggle		m_ToggleUnitOnlyMission		= default;

		[SerializeField] private Toggle		m_ToggleDaylightEverywhere	= default;
		[SerializeField] private Toggle		m_ToggleDaylightMoves		= default;
		[SerializeField] private InputField	m_InputInitialLightLevel	= default;

		[SerializeField] private GameObject	m_MusicPlayListPrefab		= default;
		[SerializeField] private Transform	m_MusicPlayListContainer	= default;
		[SerializeField] private Dropdown	m_DropdownMusicTracks		= default;
		[SerializeField] private Dropdown	m_DropdownRepeatStartIndex	= default;
		[SerializeField] private Button		m_BtnAddMusicTrack			= default;
		[SerializeField] private Button		m_BtnMoveMusicTrackUp		= default;
		[SerializeField] private Button		m_BtnMoveMusicTrackDown		= default;
		[SerializeField] private Button		m_BtnRemoveMusicTrack		= default;
		
		public delegate void OnCloseCallback();

		private UserData m_UserData;
		private OnCloseCallback m_OnCloseCB;

		private bool m_ShouldSave;
		private List<int> m_SongIDs;


		private void Initialize(UserData userData, OnCloseCallback onCloseCB)
		{
			m_UserData = userData;
			m_OnCloseCB = onCloseCB;

			UserData.current.onSelectVariantCB += OnChangedUserData;

			// Initialize display
			m_InputDescription.text			= userData.mission.levelDetails.description;
			m_InputMapName.text				= userData.mission.levelDetails.mapName;
			m_InputTechTreeName.text		= userData.mission.levelDetails.techTreeName;
			m_DropdownMissionType.value		= (-(int)userData.mission.levelDetails.missionType)-1;
			m_InputMaxTechLevel.text		= userData.mission.levelDetails.maxTechLevel.ToString();
			m_ToggleUnitOnlyMission.isOn	= userData.mission.levelDetails.unitOnlyMission;

			OnChangedUserData(userData);

			m_ShouldSave = true;
		}

		private void OnChangedUserData(UserData userData)
		{
			m_ShouldSave = false;

			m_ToggleDaylightEverywhere.isOn = userData.selectedTethysGame.daylightEverywhere;
			m_ToggleDaylightMoves.isOn		= userData.selectedTethysGame.daylightMoves;
			m_InputInitialLightLevel.text	= userData.selectedTethysGame.initialLightLevel.ToString();

			m_SongIDs						= new List<int>(userData.selectedTethysGame.musicPlayList.songIDs);

			RefreshMusicTrackState();

			m_ShouldSave = true;
		}

		private void RefreshMusicTrackState()
		{
			// Destroy music tracks
			foreach (Transform t in m_MusicPlayListContainer)
				Destroy(t.gameObject);

			// Populate music tracks
			List<string> repeatOptions = new List<string>();

			for (int i=0; i < m_UserData.selectedTethysGame.musicPlayList.songIDs.Length; ++i)
			{
				int songID = m_UserData.selectedTethysGame.musicPlayList.songIDs[i];
				string name = i.ToString() + ": " + (SongID)songID;

				GameObject item = Instantiate(m_MusicPlayListPrefab);
				item.GetComponentInChildren<Text>().text = name;
				item.transform.SetParent(m_MusicPlayListContainer);
				item.transform.localScale = Vector3.one;

				repeatOptions.Add(name);
			}

			// Set music list repeat start index options
			m_DropdownRepeatStartIndex.ClearOptions();
			m_DropdownRepeatStartIndex.AddOptions(repeatOptions);
			m_DropdownRepeatStartIndex.interactable = m_DropdownRepeatStartIndex.options.Count > 0;
		}

		public void OnClick_AddMusicTrack()
		{
			m_SongIDs.Add(m_DropdownMusicTracks.value);
			Save();

			RefreshMusicTrackState();

			// Select the newly added track
			Toggle selectedTrack = m_MusicPlayListContainer.GetChild(m_MusicPlayListContainer.childCount-1).GetComponent<Toggle>();
			selectedTrack.isOn = true;

			StartCoroutine(WaitToRefreshSelection());
		}

		public void OnClick_MoveMusicTrackUp()
		{
			// Get the selected track
			Toggle selectedTrack = GetSelectedMusicTrack();
			int index = selectedTrack.transform.GetSiblingIndex();

			// Move the track up
			int songID = m_SongIDs[index];
			m_SongIDs.RemoveAt(index);
			m_SongIDs.Insert(--index, songID);

			Save();

			RefreshMusicTrackState();

			// Reselect the selected track
			StartCoroutine(WaitToSelectTrack(index));
		}

		public void OnClick_MoveMusicTrackDown()
		{
			// Get the selected track
			Toggle selectedTrack = GetSelectedMusicTrack();
			int index = selectedTrack.transform.GetSiblingIndex();

			// Move the track down
			int songID = m_SongIDs[index];
			m_SongIDs.RemoveAt(index);
			m_SongIDs.Insert(++index, songID);

			Save();

			RefreshMusicTrackState();

			// Reselect the selected track
			StartCoroutine(WaitToSelectTrack(index));
		}

		public void OnClick_RemoveMusicTrack()
		{
			Toggle selectedTrack = GetSelectedMusicTrack();
			
			m_SongIDs.RemoveAt(selectedTrack.transform.GetSiblingIndex());
			Save();

			RefreshMusicTrackState();

			StartCoroutine(WaitToRefreshSelection());
		}

		public void OnSelect_MusicTrack()
		{
			Toggle selectedTrack = GetSelectedMusicTrack();
			if (selectedTrack != null)
			{
				// Track selected
				m_BtnMoveMusicTrackUp.interactable = selectedTrack.transform.GetSiblingIndex() > 0;
				m_BtnMoveMusicTrackDown.interactable = selectedTrack.transform.GetSiblingIndex() < selectedTrack.transform.parent.childCount-1;
				m_BtnRemoveMusicTrack.interactable = true;
			}
			else
			{
				// No track selected
				m_BtnMoveMusicTrackUp.interactable = false;
				m_BtnMoveMusicTrackDown.interactable = false;
				m_BtnRemoveMusicTrack.interactable = false;
			}
		}

		private IEnumerator WaitToSelectTrack(int index)
		{
			yield return new WaitForEndOfFrame();

			Toggle selectedTrack = m_MusicPlayListContainer.GetChild(index).GetComponent<Toggle>();
			selectedTrack.isOn = true;

			OnSelect_MusicTrack();
		}

		private IEnumerator WaitToRefreshSelection()
		{
			yield return new WaitForEndOfFrame();

			OnSelect_MusicTrack();
		}

		private Toggle GetSelectedMusicTrack()
		{
			foreach (Toggle toggle in m_MusicPlayListContainer.GetComponentsInChildren<Toggle>())
			{
				if (toggle.isOn)
					return toggle;
			}

			return null;
		}

		public void Save()
		{
			if (!m_ShouldSave)
				return;

			m_UserData.mission.levelDetails.description			= m_InputDescription.text;
			m_UserData.mission.levelDetails.mapName				= m_InputMapName.text;
			m_UserData.mission.levelDetails.techTreeName		= m_InputTechTreeName.text;
			m_UserData.mission.levelDetails.missionType			= (MissionType)(-(m_DropdownMissionType.value+1));
			int val;
			if (int.TryParse(m_InputMaxTechLevel.text, out val))
				m_UserData.mission.levelDetails.maxTechLevel = val;
			else
			{
				Debug.Log("Bad value assigned to Max Tech Level.");
				m_InputMaxTechLevel.text = m_UserData.mission.levelDetails.maxTechLevel.ToString();
			}

			m_UserData.mission.levelDetails.unitOnlyMission		= m_ToggleUnitOnlyMission.isOn;

			m_UserData.selectedTethysGame.daylightEverywhere	= m_ToggleDaylightEverywhere.isOn;
			m_UserData.selectedTethysGame.daylightMoves			= m_ToggleDaylightMoves.isOn;
			if (int.TryParse(m_InputInitialLightLevel.text, out val))
				m_UserData.selectedTethysGame.initialLightLevel = val;
			else
			{
				Debug.Log("Bad value assigned to Initial Light Level.");
				m_InputInitialLightLevel.text = m_UserData.selectedTethysGame.initialLightLevel.ToString();
			}

			m_UserData.selectedTethysGame.musicPlayList.songIDs = m_SongIDs.ToArray();
			m_UserData.selectedTethysGame.musicPlayList.repeatStartIndex = m_DropdownRepeatStartIndex.value;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}

		private void OnDestroy()
		{
			UserData.current.onSelectVariantCB -= OnChangedUserData;
		}


		/// <summary>
		/// Creates and presents the Mission Properties dialog to the user.
		/// </summary>
		/// <param name="userData">The user data to display and modify.</param>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static MissionPropertiesDialog Create(UserData userData, OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/MissionPropertiesDialog"));
			MissionPropertiesDialog dialog = goDialog.GetComponent<MissionPropertiesDialog>();
			dialog.Initialize(userData, onCloseCB);

			return dialog;
		}
	}
}
