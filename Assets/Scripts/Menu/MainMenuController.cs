using OP2MissionEditor.Dialogs;
using OP2MissionEditor.Systems;
using OP2MissionEditor.Systems.Map;
using OP2UtilityDotNet;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Menu
{
	/// <summary>
	/// Handles click events for the menu bar at the top of the screen.
	/// </summary>
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private CanvasGroup m_CanvasGroup		= default;
		[SerializeField] private MapRenderer m_MapRenderer		= default;

		private string m_SavePath;

		public bool interactable { get { return m_CanvasGroup.interactable; } set { m_CanvasGroup.interactable = value; } }


		private void Awake()
		{
			// Show these windows at startup
			OnClick_ShowMinimap();
			OnClick_ShowPaintWindow();
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// File Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_New()
		{
			CreateNew_NoRefresh();

			interactable = false;

			m_MapRenderer.Refresh(() =>
			{
				interactable = true;
			});
		}

		private void CreateNew_NoRefresh()
		{
			m_SavePath = null;
			UserData.CreateNew();
		}

		public void OnClick_Open()
		{
			interactable = false;

			// User needs to choose what mission to open
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowLoadDialog(OnOpenPath, OnCancelFileDialog, false, UserPrefs.GameDirectory, "Open Mission", "Open");
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

			m_MapRenderer.Refresh(() =>
			{
				interactable = true;
				Debug.Log("Opened mission \"" + Path.GetFileName(path) + "\".");
			});
		}

		private void OnCancelFileDialog()
		{
			interactable = true;
		}

		public void OnClick_ImportMap()
		{
			interactable = false;

			// User needs to choose what map to import
			FileBrowser.SetFilters(false, ".map", ".vol");
			FileBrowser.ShowSaveDialog(OnImportMapPath, OnCancelFileDialog, false, UserPrefs.GameDirectory, "Import Map", "Import");
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
					ListSelectDialog.Create(fileNames, "Select Map", "Import", (string mapName) => OnImportMapSelected(vol, mapName), () =>
					{
						interactable = true;
						vol.Dispose();
					});

					break;

				default:
					CreateNew_NoRefresh();

					if (UserData.ImportMap(path))
					{
						m_MapRenderer.Refresh(() =>
						{
							interactable = true;
							Debug.Log("Import Complete.");
						});
					}
					else
					{
						// Import failed
						m_MapRenderer.Refresh(() =>
						{
							interactable = true;
							Debug.LogError("Failed to read map: " + path);
						});
					}
					break;
			}
		}

		private void OnImportMapSelected(VolFile volFile, string mapName)
		{
			byte[] mapData = volFile.ReadFileByName(mapName);
			volFile.Dispose();

			if (!UserData.ImportMap(mapData))
			{
				Debug.LogError("Failed to read map: " + mapName);

				// If import fails, clear out data by creating new mission instead
				OnClick_New();
				return;
			}

			m_MapRenderer.Refresh(() =>
			{
				interactable = true;
				Debug.Log("Import Complete.");
			});
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
			interactable = false;

			// User needs to choose where to save the mission
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowSaveDialog(OnSavePath, OnCancelFileDialog, false, UserPrefs.GameDirectory, "Save Mission", "Save");
		}

		private void OnSavePath(string path)
		{
			interactable = true;

			m_SavePath = path;

			UserData.current.SaveMission(path);

			Debug.Log("Mission saved to \"" + m_SavePath + "\".");
		}

		public void OnClick_ExportMap()
		{
			interactable = false;

			// User needs to choose where to save the map
			FileBrowser.SetFilters(false, ".map");
			FileBrowser.ShowSaveDialog(OnExportMapPath, OnCancelFileDialog, false, UserPrefs.GameDirectory, "Export Map", "Export");
		}

		private void OnExportMapPath(string path)
		{
			interactable = true;

			UserData.current.ExportMap(path);

			Debug.Log("Map exported to \"" + path + "\".");
		}

		public void OnClick_ExportPlugin()
		{
			interactable = false;

			// User needs to choose where to save the plugin
			FileBrowser.SetFilters(false, ".dll");
			FileBrowser.ShowSaveDialog(OnExportPluginPath, OnCancelFileDialog, false, UserPrefs.GameDirectory, "Export Plugin", "Export");
		}

		private void OnExportPluginPath(string path)
		{
			interactable = true;

			PluginExporter.ExportPlugin(path, UserData.current.mission.levelDetails);

			Debug.Log("Plugin exported to \"" + path + "\".");
		}

		private int GetLengthOfSkipString(string str, int startIndex, char skipChar)
		{
			for (int i=startIndex; i < str.Length; ++i)
			{
				if (str[i] != skipChar)
					return i-startIndex;
			}

			// Hit end of string before skip char ended
			return str.Length - startIndex;
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
			interactable = false;

			MissionPropertiesDialog.Create(UserData.current, () =>
			{
				interactable = true;
			});
		}

		public void OnClick_PlayerProperties()
		{
			interactable = false;

			PlayerPropertiesDialog.Create(UserData.current, () =>
			{
				interactable = true;
			});
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// View Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_ShowMinimap()
		{
			MinimapDialog minimapDlg = MinimapDialog.Create(m_MapRenderer);

			// Set minimap to top-right corner
			RectTransform rTransform = minimapDlg.transform.GetChild(0).GetComponent<RectTransform>();
			CanvasScaler canvas = rTransform.GetComponentInParent<CanvasScaler>();

			Vector2 refResolution = canvas.referenceResolution / 2;
			refResolution.x = refResolution.y * (Screen.width / (float)Screen.height);
			Vector2 position = rTransform.anchoredPosition;
			Vector2 size = rTransform.rect.size / 2;

			position.x = refResolution.x - size.x;
			position.y = 362 - size.y;

			rTransform.anchoredPosition = position;
		}

		public void OnClick_ShowPaintWindow()
		{
			PaintDialog paintDialog = PaintDialog.Create(UserData.current);

			// Set paint window to mid-right corner
			RectTransform rTransform = paintDialog.transform.GetChild(0).GetComponent<RectTransform>();
			CanvasScaler canvas = rTransform.GetComponentInParent<CanvasScaler>();

			Vector2 refResolution = canvas.referenceResolution / 2;
			refResolution.x = refResolution.y * (Screen.width / (float)Screen.height);
			Vector2 position = rTransform.anchoredPosition;
			Vector2 size = rTransform.rect.size / 2;

			position.x = refResolution.x - size.x;
			position.y = 362 - 256 - size.y;

			rTransform.anchoredPosition = position;

			//RectTransform rTransform = paintDialog.transform.GetChild(0).GetComponent<RectTransform>();
			//rTransform.anchorMin = rTransform.anchorMax = rTransform.pivot = new Vector2(1.0f, 1.0f);
			//rTransform.anchoredPosition = new Vector2(0, -277);
			//rTransform.anchorMin = rTransform.anchorMax = rTransform.pivot = new Vector2(0.5f, 0.5f);
		}
	}
}
