using OP2MissionEditor.Dialogs;
using OP2MissionEditor.Systems;
using OP2MissionEditor.Systems.Map;
using UnityEngine;

namespace OP2MissionEditor.Scenes
{
	public class MainController : MonoBehaviour
	{
		[SerializeField] private MapRenderer m_MapRenderer		= default;

		private ProgressDialog m_ProgressDialog					= default;


		private void Awake()
		{
			ConsoleLog.Initialize();
			TextureManager.Initialize();

			// Register events
			m_MapRenderer.onMapRefreshProgressCB += OnMapRefreshProgress;
			m_MapRenderer.onMapRefreshedCB += OnMapRefreshed;

			UserData.current.CreateNew();

			// If game directory hasn't been set, Open "Locate Outpost2" dialog to force user to select one
			if (string.IsNullOrEmpty(UserPrefs.gameDirectory))
				PreferencesDialog.Create();

			Debug.Log("Editor initialized.");
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

			TextureManager.Release();
		}
	}
}
