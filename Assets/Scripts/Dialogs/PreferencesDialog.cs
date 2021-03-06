﻿using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor preferences to the user.
	/// </summary>
	public class PreferencesDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtGameDirectory			= default;
		
		[SerializeField] private Slider m_SliderGridRed				= default;
		[SerializeField] private Slider m_SliderGridGreen			= default;
		[SerializeField] private Slider m_SliderGridBlue			= default;
		[SerializeField] private Slider m_SliderGridAlpha			= default;

		[SerializeField] private Dropdown m_DropdownResolution		= default;
		[SerializeField] private Toggle m_ToggleIsWindowed			= default;

		private bool m_IsInitialized;

		public delegate void OnCloseCallback();

		private OnCloseCallback m_OnCloseCB;


		private void Initialize(OnCloseCallback onCloseCB)
		{
			m_OnCloseCB = onCloseCB;

			m_txtGameDirectory.text = UserPrefs.gameDirectory;

			// If user does not have a game directory, force them to select one
			if (string.IsNullOrEmpty(UserPrefs.gameDirectory))
				OnClick_LocateGameDirectory();

			// Initialize grid overlay sliders
			Color32 gridColor = UserPrefs.gridOverlayColor;
			m_SliderGridRed.value = m_SliderGridGreen.value = m_SliderGridBlue.value = m_SliderGridAlpha.value = 1; // Force refresh
			m_SliderGridRed.value = gridColor.r;
			m_SliderGridGreen.value = gridColor.g;
			m_SliderGridBlue.value = gridColor.b;
			m_SliderGridAlpha.value = gridColor.a;

			// Initialize resolution options
			m_DropdownResolution.ClearOptions();
			List<string> resOptions = new List<string>();
			for (int i=0; i < Screen.resolutions.Length; ++i)
				resOptions.Add(Screen.resolutions[i].width.ToString() + "x" + Screen.resolutions[i].height + " @ " + Screen.resolutions[i].refreshRate + "hz");

			m_DropdownResolution.AddOptions(resOptions);

			// Set current resolution
			for (int i=0; i < Screen.resolutions.Length; ++i)
			{
				if (Screen.resolutions[i].Equals(Screen.currentResolution))
				{
					m_DropdownResolution.value = i;
					break;
				}
			}

			m_ToggleIsWindowed.isOn = !Screen.fullScreen;

			m_IsInitialized = true;
		}

		public void OnClick_LocateGameDirectory()
		{
			FileBrowser.ShowLoadDialog(OnSelect_GameDirectory, OnCancel_GameDirectory, true, UserPrefs.gameDirectory, "Locate Outpost 2 Directory", "Select");
		}

		private void OnSelect_GameDirectory(string path)
		{
			UserPrefs.gameDirectory = path;
			m_txtGameDirectory.text = path;
		}

		private void OnCancel_GameDirectory()
		{
			StartCoroutine(WaitToDisplayLocateGameDialog());
		}

		private IEnumerator WaitToDisplayLocateGameDialog()
		{
			yield return null;

			// If user does not have a game directory and they try to cancel, force them to select one
			if (string.IsNullOrEmpty(UserPrefs.gameDirectory))
				OnClick_LocateGameDirectory();
		}

		public void OnValueChanged_GridColor()
		{
			UserPrefs.gridOverlayColor = new Color32(	(byte)m_SliderGridRed.value,
														(byte)m_SliderGridGreen.value,
														(byte)m_SliderGridBlue.value,
														(byte)m_SliderGridAlpha.value);
		}

		public void OnChanged_Resolution()
		{
			if (!m_IsInitialized)
				return;

			Resolution resolution = Screen.resolutions[m_DropdownResolution.value];
			
			Screen.SetResolution(resolution.width, resolution.height, !m_ToggleIsWindowed.isOn, resolution.refreshRate);
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}


		/// <summary>
		/// Creates and presents the Preferences dialog to the user.
		/// </summary>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static PreferencesDialog Create(OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/PreferencesDialog"));
			PreferencesDialog dialog = goDialog.GetComponent<PreferencesDialog>();
			dialog.Initialize(onCloseCB);

			return dialog;
		}
	}
}
