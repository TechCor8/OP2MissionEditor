using OP2MissionEditor.Dialogs;
using OP2MissionEditor.Systems.Map;
using OP2UtilityDotNet;
using SimpleFileBrowser;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OP2MissionEditor.Menu
{
	/// <summary>
	/// Handles click events for the menu bar at the top of the screen.
	/// </summary>
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private MapRenderer m_MapRenderer		= default;

		private string m_SavePath;


		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// File Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_New()
		{
			m_SavePath = null;
			UserData.CreateNew();

			m_MapRenderer.Refresh();
		}

		public void OnClick_Open()
		{
			// User needs to choose what mission to open
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowLoadDialog(OnOpenPath, null, false, UserPrefs.GameDirectory, "Open Mission", "Open");
		}

		private void OnOpenPath(string path)
		{
			m_SavePath = path;

			if (!UserData.LoadMission(path))
			{
				// If load fails, clear out data by creating new mission instead
				OnClick_New();
				return;
			}

			m_MapRenderer.Refresh();

			Debug.Log("Opened mission \"" + Path.GetFileName(path) + "\".");
		}

		public void OnClick_ImportMap()
		{
			// User needs to choose what map to import
			FileBrowser.SetFilters(false, ".map", ".vol");
			FileBrowser.ShowSaveDialog(OnImportMapPath, null, false, UserPrefs.GameDirectory, "Import Map", "Import");

			//OP2UtilityDotNet.VolFile volFile = new OP2UtilityDotNet.VolFile("C:/Users/kevin/Desktop/Outpost2-1.3.0.7-OPU/art.vol");
			//Debug.Log("Size = " + volFile.GetArchiveFileSize());
		}

		private void OnImportMapPath(string path)
		{
			string extension = Path.GetExtension(path);

			switch (extension.ToLower())
			{
				case ".vol":
					List<string> fileNames = new List<string>();

					// Get list of map names from the archive
					VolFile vol = new VolFile(path);
					for (ulong i=0; i < vol.GetCount(); ++i)
					{
						string name = vol.GetName(i);
						if (Path.GetExtension(name).ToLower() == ".map")
							fileNames.Add(name);
					}

					// Open list of map names for selection
					ListSelectDialog.Create(fileNames, "Select Map", "Import", (string mapName) => OnImportMapSelected(vol, mapName), vol.Dispose);

					break;

				default:
					OnClick_New();

					if (UserData.ImportMap(path))
					{
						m_MapRenderer.Refresh();
						Debug.Log("Import Complete.");
					}
					break;
			}
		}

		private void OnImportMapSelected(VolFile volFile, string mapName)
		{
			OnClick_New();

			byte[] mapData = volFile.ReadFileByName(mapName);
			volFile.Dispose();

			if (UserData.ImportMap(mapData))
			{
				m_MapRenderer.Refresh();
				Debug.Log("Import Complete.");
			}
		}

		public void OnClick_Save()
		{
			// If there is no save path, force user to choose where to save
			if (string.IsNullOrEmpty(m_SavePath))
				OnClick_SaveAs();
			else
			{
				UserData.current.SaveMission(m_SavePath);
				Debug.Log("Mission saved to \"" + m_SavePath + "\".");
			}
		}

		public void OnClick_SaveAs()
		{
			// User needs to choose where to save the mission
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowSaveDialog(OnSavePath, null, false, UserPrefs.GameDirectory, "Save Mission", "Save");
		}

		private void OnSavePath(string path)
		{
			m_SavePath = path;

			UserData.current.SaveMission(path);

			Debug.Log("Mission saved to \"" + m_SavePath + "\".");
		}

		public void OnClick_ExportMap()
		{
			// User needs to choose where to save the map
			FileBrowser.SetFilters(false, ".map");
			FileBrowser.ShowSaveDialog(OnExportMapPath, null, false, UserPrefs.GameDirectory, "Export Map", "Export");
		}

		private void OnExportMapPath(string path)
		{
			UserData.current.ExportMap(path);

			Debug.Log("Map exported to \"" + path + "\".");
		}

		public void OnClick_Preferences()
		{
			PreferencesDialog.Create();
		}

		public void OnClick_Exit()
		{
			Application.Quit();
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Edit Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_MapProperties()
		{
		}

		public void OnClick_MissionProperties()
		{
		}

		public void OnClick_PlayerProperties()
		{
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// View Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_ShowGridLines()
		{
		}
	}
}
