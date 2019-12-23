using OP2MissionEditor.Dialogs;
using OP2MissionEditor.Systems.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Scenes
{
	public class MainController : MonoBehaviour
	{
		[SerializeField] private MapRenderer m_MapRenderer		= default;

		private ProgressDialog m_ProgressDialog					= default;


		private void Awake()
		{
			// Register events
			m_MapRenderer.onMapRefreshProgressCB += OnMapRefreshProgress;
			m_MapRenderer.onMapRefreshedCB += OnMapRefreshed;

			UserData.CreateNew();

			// Set default tileset vol to art.vol
			if (string.IsNullOrEmpty(UserPrefs.tilesetVolFileName))
				UserPrefs.tilesetVolFileName = "art.vol";

			// If game directory hasn't been set, Open "Locate Outpost2" dialog to force user to select one
			if (string.IsNullOrEmpty(UserPrefs.gameDirectory))
				PreferencesDialog.Create();
		}

		private void OnMapRefreshProgress(MapRenderer mapRenderer, string state, float progress)
		{
			if (m_ProgressDialog == null)
				m_ProgressDialog = ProgressDialog.Create(state);

			m_ProgressDialog.SetTitle(state);
			m_ProgressDialog.SetProgress(progress);
		}

		private void OnMapRefreshed(MapRenderer mapRenderer)
		{
			if (m_ProgressDialog != null)
				m_ProgressDialog.Close();

			m_ProgressDialog = null;
		}

		private void OnDestroy()
		{
			// Unregister events
			m_MapRenderer.onMapRefreshProgressCB -= OnMapRefreshProgress;
			m_MapRenderer.onMapRefreshedCB -= OnMapRefreshed;

			UserData.current.Dispose();
		}
	}
}
