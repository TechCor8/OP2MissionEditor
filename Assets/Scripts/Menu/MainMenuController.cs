using DotNetMissionSDK.Json;
using OP2MissionEditor.Dialogs;
using OP2MissionEditor.Dialogs.Generic;
using OP2MissionEditor.Systems;
using OP2MissionEditor.Systems.Map;
using OP2UtilityDotNet;
using OP2UtilityDotNet.Archive;
using OP2UtilityDotNet.Bitmap;
using OP2UtilityDotNet.Sprite;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OP2MissionEditor.Menu
{
	/// <summary>
	/// Handles click events for the menu bar at the top of the screen.
	/// </summary>
	public class MainMenuController : MonoBehaviour
	{
		[System.Serializable]
		private class MenuOption
		{
			public string id									= default;
			public Text txtShortcut								= default;

			public KeyCode shortcutModifier1					= default;
			public KeyCode shortcutModifier2					= default;
			public KeyCode shortcutKey							= default;

			public UnityEvent buttonMethod						= default;
		}

		[SerializeField] private CanvasGroup m_CanvasGroup		= default;
		[SerializeField] private MapRenderer m_MapRenderer		= default;
		[SerializeField] private UnitRenderer m_UnitRenderer	= default;

		[SerializeField] private MenuOption[] m_MenuOptions		= default;

		private MissionVariantsDialog m_MissionVariantsDialog;
		private MinimapDialog m_MinimapDialog;
		private PaintDialog m_PaintDialog;

		private string m_SavePath;

		public bool interactable { get { return m_CanvasGroup.interactable; } set { m_CanvasGroup.interactable = value; } }


		private void Awake()
		{
			// Show these windows at startup
			OnClick_ShowMinimap();
			OnClick_ShowPaintWindow();

			InitializeMenuShortcuts();
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

				Debug.Log("New mission loaded.");
			});
		}

		private void CreateNew_NoRefresh()
		{
			m_SavePath = null;
			UserData.current.CreateNew();
		}

		public void OnClick_Open()
		{
			interactable = false;

			// User needs to choose what mission to open
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowLoadDialog(OnOpenPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Open Mission", "Open");
		}

		private void OnOpenPath(string path)
		{
			m_SavePath = path;

			if (!UserData.current.LoadMission(path))
			{
				// If load fails, clear out data by creating new mission instead
				OnClick_New();
				return;
			}

			m_MapRenderer.Refresh(() =>
			{
				interactable = true;
				Debug.Log("Opened mission \"" + Path.GetFileName(path) + "\".");
				CheckSDKVersion();
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
			FileBrowser.ShowSaveDialog(OnImportMapPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Import Map", "Import");
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
					for (int i=0; i < vol.GetCount(); ++i)
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

					interactable = true;

					if (UserData.current.ImportMap(path))
					{
						interactable = false;

						m_MapRenderer.Refresh(() =>
						{
							interactable = true;
							Debug.Log("Import Complete.");
						});
					}
					else
					{
						// Import failed
						interactable = false;

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
			interactable = true;

			Stream mapStream = volFile.OpenStream(mapName);
			
			if (!UserData.current.ImportMap(mapStream))
			{
				Debug.LogError("Failed to read map: " + mapName);
				volFile.Dispose();
				return;
			}

			volFile.Dispose();

			interactable = false;

			m_MapRenderer.Refresh(() =>
			{
				interactable = true;
				Debug.Log("Import map complete.");
			});
		}

		public void OnClick_ImportMission()
		{
			interactable = false;

			// User needs to choose what mission to import
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowLoadDialog(OnImportMissionPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Import Mission", "Import");
		}

		private void OnImportMissionPath(string missionPath)
		{
			interactable = true;

			if (!UserData.current.ImportMission(missionPath))
			{
				Debug.LogError("Failed to read mission: " + missionPath);
				return;
			}

			Debug.Log("Import mission complete.");
			CheckSDKVersion();
		}

		private void CheckSDKVersion()
		{
			int versionCompare = CompareVersion(UserData.current.mission.sdkVersion, MissionRoot.SDKVersion);
			if (versionCompare > 0)
			{
				string body = "This mission was created with a newer version of the editor and may no longer work correctly if you save over it.";
				body += "\n\nDo you want to set this mission's version to the current editor version?";
				ConfirmDialog.Create(OnConfirmChangeSDKVersion, "Newer Mission", body, "Downgrade");
			}
			else if (versionCompare < 0)
			{
				string body = "This mission was created with an older version of the editor and may no longer work correctly if you save over it.";
				body += "\n\nDo you want to set this mission's version to the current editor version?";
				ConfirmDialog.Create(OnConfirmChangeSDKVersion, "Outdated Mission", body, "Upgrade");
			}
		}

		private void OnConfirmChangeSDKVersion(bool didConfirm)
		{
			if (!didConfirm)
				return;

			UserData.current.mission.sdkVersion = MissionRoot.SDKVersion;
		}

		private static int CompareVersion(string missionVersion, string editorVersion)
		{
			List<string> missionDots = new List<string>(missionVersion.Split('.'));
			List<string> editorDots = new List<string>(editorVersion.Split('.'));

			// Make sure number of version components is the same
			while (missionDots.Count < editorDots.Count)
				missionDots.Add("0");
			while (editorDots.Count < missionDots.Count)
				editorDots.Add("0");

			for (int i=0; i < missionDots.Count; ++i)
			{
				int missionDot;
				int editorDot;
				if (!int.TryParse(missionDots[i], out missionDot))	missionDot = 0;
				if (!int.TryParse(editorDots[i], out editorDot))	editorDot = 0;

				if (missionDot > editorDot)
					return 1;
				else if (missionDot < editorDot)
					return -1;
			}

			// Versions are the same
			return 0;
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
			FileBrowser.ShowSaveDialog(OnSavePath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Save Mission", "Save");
		}

		private void OnSavePath(string path)
		{
			interactable = true;

			string missionName = Path.GetFileName(path);

			// Check if the file name has the needed type prefix. If not, add it.
			string prefix = UserData.current.GetMissionTypePrefix();
			if (!missionName.StartsWith(prefix))
			{
				missionName = prefix + missionName;
				path = Path.Combine(Path.GetDirectoryName(path), missionName);
			}

			string warning = "";
			if (Path.GetFileNameWithoutExtension(missionName).Length > 7)
				warning = " WARNING: File name length is greater than 7 and is not supported by Outpost 2!";

			m_SavePath = path;

			UserData.current.SaveMission(path);

			Debug.Log("Mission saved to \"" + m_SavePath + "\"." + warning);
		}

		public void OnClick_ExportMap()
		{
			interactable = false;

			// User needs to choose where to save the map
			FileBrowser.SetFilters(false, ".map");
			FileBrowser.ShowSaveDialog(OnExportMapPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Export Map", "Export");
		}

		private void OnExportMapPath(string path)
		{
			interactable = true;

			UserData.current.ExportMap(path);

			Debug.Log("Map exported to \"" + path + "\".");
		}

		public void OnClick_ExportMission()
		{
			interactable = false;

			// User needs to choose where to save the mission
			FileBrowser.SetFilters(false, ".opm");
			FileBrowser.ShowSaveDialog(OnExportMissionPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Export Mission", "Export");
		}

		private void OnExportMissionPath(string missionPath)
		{
			interactable = true;

			UserData.current.ExportMission(missionPath);

			Debug.Log("Mission exported to \"" + missionPath + "\".");
		}

		public void OnClick_ExportPlugin()
		{
			interactable = false;

			// User needs to choose where to save the plugin
			FileBrowser.SetFilters(false, ".dll");
			FileBrowser.ShowSaveDialog(OnExportPluginPath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Export Plugin", "Export");
		}

		private void OnExportPluginPath(string path)
		{
			interactable = true;

			// OP2 does not support plugins with more than 7 characters.
			if (Path.GetFileNameWithoutExtension(path).Length > 7)
			{
				Debug.Log("Plugin filename cannot be longer than 7 characters.");
				return;
			}

			PluginExporter.ExportPlugin(path, UserData.current.mission.sdkVersion, UserData.current.mission.levelDetails);

			Debug.Log("Plugin exported to \"" + path + "\".");
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

			PlayerPropertiesDialog.Create(() =>
			{
				interactable = true;
			});
		}

		public void OnClick_ClearTextureCache()
		{
			TextureManager.ClearCache();
			Debug.Log("Texture cache cleared successfully.");
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// View Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_ShowMinimap()
		{
			if (m_MinimapDialog != null)
			{
				// Close Minimap
				Destroy(m_MinimapDialog.gameObject);
				return;
			}

			// Open Minimap
			MinimapDialog minimapDlg = MinimapDialog.Create(m_MapRenderer, m_UnitRenderer);

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

			m_MinimapDialog = minimapDlg;
		}

		public void OnClick_ShowPaintWindow()
		{
			if (m_PaintDialog != null)
			{
				// Close Paint Window
				Destroy(m_PaintDialog.gameObject);
				return;
			}

			// Open Paint Window
			PaintDialog paintDialog = PaintDialog.Create();

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

			m_PaintDialog = paintDialog;
		}

		public void OnClick_ShowMissionVariants()
		{
			if (m_MissionVariantsDialog != null)
			{
				// Close mission variants
				Destroy(m_MissionVariantsDialog.gameObject);
				return;
			}

			// Open mission variants
			MissionVariantsDialog variantsDialog = MissionVariantsDialog.Create(m_UnitRenderer);

			m_MissionVariantsDialog = variantsDialog;
		}

		public void OnClick_ShowGrid()
		{
			UserPrefs.isGridOverlayVisible = !UserPrefs.isGridOverlayVisible;
		}

		public void OnClick_UnitOverlay()
		{
			UserPrefs.isUnitOverlayVisible = !UserPrefs.isUnitOverlayVisible;
		}

		public void OnClick_UnitInfo()
		{
			UserPrefs.isUnitInfoVisible = !UserPrefs.isUnitInfoVisible;
		}

		public void OnClick_Console()
		{
			ConsoleDialog.Create();
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Archive Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_ExtractFile()
		{
			interactable = false;

			// User needs to choose what archive to extract from
			FileBrowser.SetFilters(false, ".clm", ".vol");
			FileBrowser.ShowLoadDialog(OnExtractFileArchivePath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Extract From Archive", "Select");
		}

		private void OnExtractFileArchivePath(string path)
		{
			// Get the appropriate archive type
			string extension = Path.GetExtension(path);
			ArchiveFile archive;

			switch (extension.ToLower())
			{
				case ".vol":	archive = new VolFile(path);	break;
				case ".clm":	archive = new ClmFile(path);	break;
				default:
					interactable = true;
					Debug.Log("Invalid archive selected.");
					return;
			}

			// Get list of files from the archive
			List<string> fileNames = new List<string>();

			for (int i=0; i < archive.GetCount(); ++i)
				fileNames.Add(archive.GetName(i));

			// Open list of file names for selection
			ListSelectDialog.Create(fileNames, "Extract File", "Select", (string fileName) => OnExtractFileSelected(archive, fileName), () => OnExtractFileCanceled(archive));
		}

		private void OnExtractFileSelected(ArchiveFile archive, string fileName)
		{
			// User needs to choose where to save the extracted file
			FileBrowser.SetFilters(false, Path.GetExtension(fileName));
			FileBrowser.ShowSaveDialog((string path) => OnExtractFileSavePathSelected(archive, fileName, path),
				() => OnExtractFileCanceled(archive), false, UserPrefs.gameDirectory, "Destination File Path", "Extract");
		}

		private void OnExtractFileSavePathSelected(ArchiveFile archive, string fileName, string destPath)
		{
			interactable = true;
			try
			{
				if (Path.GetExtension(fileName).ToLowerInvariant() == ".bmp")
				{
					// Special processing to convert tileset to a standard bmp format
					BitmapFile bitmapFile = TilesetLoader.ReadTileset(archive.ExtractFileToMemory(fileName));
					bitmapFile.Serialize(destPath);
				}
				else
				{
					archive.ExtractFile(fileName, destPath);
				}
			}
			finally
			{
				archive.Dispose();
			}

			Debug.Log(Path.GetFileName(destPath) + " extracted successfully.");
		}

		private void OnExtractFileCanceled(ArchiveFile archive)
		{
			interactable = true;
			archive.Dispose();
		}

		public void OnClick_ExtractAllFiles()
		{
			interactable = false;

			// User needs to choose what archive to extract from
			FileBrowser.SetFilters(false, ".clm", ".vol");
			FileBrowser.ShowLoadDialog(OnExtractAllFilesArchivePath, OnCancelFileDialog, false, UserPrefs.gameDirectory, "Extract All From Archive", "Select");
		}

		private void OnExtractAllFilesArchivePath(string archivePath)
		{
			StartCoroutine(ExtractAllFiles_WaitForSaveDirectory(archivePath));
		}

		private IEnumerator ExtractAllFiles_WaitForSaveDirectory(string archivePath)
		{
			// Wait a frame to allow previous FileBrowser to clear.
			yield return 1;

			// User needs to choose where to save the extracted files
			FileBrowser.ShowSaveDialog((string destDirectory) => OnExtractAllFilesSavePathSelected(archivePath, destDirectory),
				OnCancelFileDialog, true, UserPrefs.gameDirectory, "Destination Directory", "Extract");
		}

		private void OnExtractAllFilesSavePathSelected(string archivePath, string destDirectory)
		{
			interactable = true;

			// Get the appropriate archive type
			string extension = Path.GetExtension(archivePath);
			ArchiveFile archive;

			switch (extension.ToLower())
			{
				case ".vol":	archive = new VolFile(archivePath);	break;
				case ".clm":	archive = new ClmFile(archivePath);	break;
				default:
					Debug.Log("Invalid archive selected.");
					return;
			}

			archive.ExtractAllFiles(destDirectory);
			archive.Dispose();

			Debug.Log("Files extracted successfully.");
		}

		public void OnClick_CreateArchive()
		{
			interactable = false;

			// User needs to choose directory to archive
			FileBrowser.ShowLoadDialog(OnCreateArchiveSourceDirectorySelected, OnCancelFileDialog, true, UserPrefs.gameDirectory, "Directory to Archive", "Select");
		}

		private void OnCreateArchiveSourceDirectorySelected(string srcDirectory)
		{
			StartCoroutine(CreateArchive_WaitForSaveArchive(srcDirectory));
		}

		private IEnumerator CreateArchive_WaitForSaveArchive(string srcDirectory)
		{
			// Wait a frame to allow previous FileBrowser to clear.
			yield return 1;

			// User needs to choose where to save the archive
			FileBrowser.SetFilters(false, ".vol", ".clm");
			FileBrowser.ShowSaveDialog((string path) => CreateArchive(srcDirectory, path), OnCancelFileDialog, false, UserPrefs.gameDirectory, "Save Archive", "Save");
		}

		private void CreateArchive(string srcDirectory, string archivePath)
		{
			interactable = true;

			string[] files = Directory.GetFiles(srcDirectory, "*.*", SearchOption.TopDirectoryOnly);

			// Cull file names that are too long
			List<string> culledFiles = new List<string>(files.Length);
			foreach (string name in files)
			{
				string noExt = Path.GetFileNameWithoutExtension(name);

				if (noExt.Length <= 8)
					culledFiles.Add(name);
			}


			string extension = Path.GetExtension(archivePath);

			switch (extension)
			{
				case ".vol":	VolFile.CreateArchive(archivePath, culledFiles);	break;
				case ".clm":	ClmFile.CreateArchive(archivePath, culledFiles);	break;
				default:
					Debug.Log("Invalid archive type selected.");
					return;
			}

			if (files.Length == culledFiles.Count)
				Debug.Log(Path.GetFileName(archivePath) + " created successfully.");
			else
			{
				int filesRemoved = files.Length - culledFiles.Count;
				Debug.Log(filesRemoved.ToString() + " files exceeded 8 character limit and were not archived.");
			}
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Run Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_RunOutpost2()
		{
			string path = Path.Combine(UserPrefs.gameDirectory, "Outpost2.exe");

			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(path);
			startInfo.WorkingDirectory = UserPrefs.gameDirectory;
			System.Diagnostics.Process.Start(startInfo);
		}

		public void OnClick_CopySDKToGame()
		{
			// Copy SDK from streaming assets to the game directory
			string interopPath = Path.Combine(Application.streamingAssetsPath, "DotNetInterop.dll");
			string sdkPath = Path.Combine(Application.streamingAssetsPath, "DotNetMissionSDK_v" + MissionRoot.SDKVersion.Replace('.', '_') + ".dll");

			string interopOutputPath = Path.Combine(UserPrefs.gameDirectory, Path.GetFileName(interopPath));
			string sdkOutputPath = Path.Combine(UserPrefs.gameDirectory, Path.GetFileName(sdkPath));

			File.Copy(interopPath, interopOutputPath, true);
			File.Copy(sdkPath, sdkOutputPath, true);

			Debug.Log("SDK files copied to " + UserPrefs.gameDirectory + ".");
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Help Menu
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		public void OnClick_OutpostUniverseHome()
		{
			Application.OpenURL("https://outpost2.net/");
		}

		public void OnClick_OutpostUniverseForum()
		{
			Application.OpenURL("https://forum.outpost2.net/");
		}

		public void OnClick_GitHubRepository()
		{
			Application.OpenURL("https://github.com/TechCor8/OP2MissionEditor");
		}

		public void OnClick_AboutMissionEditor()
		{
			AboutDialog.Create();
		}

		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Menu Shortcuts
		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		private void InitializeMenuShortcuts()
		{
			// Set shortcut text for options that have shortcuts
			foreach (MenuOption option in m_MenuOptions)
			{
				if (option.shortcutKey == KeyCode.None)
					continue;

				option.txtShortcut.text = GetModifierName(option.shortcutModifier1) + GetModifierName(option.shortcutModifier2) + option.shortcutKey.ToString();
			}
		}

		private string GetModifierName(KeyCode modifier)
		{
			if (modifier == KeyCode.None)
				return "";

			switch (modifier)
			{
				case KeyCode.LeftShift:
				case KeyCode.RightShift:
					return "Shift+";

				case KeyCode.LeftControl:
				case KeyCode.RightControl:
					return "Ctrl+";

				case KeyCode.LeftAlt:
				case KeyCode.RightAlt:
					return "Alt+";
			}

			return modifier.ToString();
		}

		private bool IsModifierDown(KeyCode modifier)
		{
			if (modifier == KeyCode.None)
				return true;

			switch (modifier)
			{
				case KeyCode.LeftShift:
				case KeyCode.RightShift:
					return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

				case KeyCode.LeftControl:
				case KeyCode.RightControl:
					return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

				case KeyCode.LeftAlt:
				case KeyCode.RightAlt:
					return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			}

			return Input.GetKey(modifier);
		}

		private void Update()
		{
			// Invoke pressed shortcuts
			foreach (MenuOption option in m_MenuOptions)
			{
				if (!IsModifierDown(option.shortcutModifier1)) continue;
				if (!IsModifierDown(option.shortcutModifier2)) continue;
				if (!Input.GetKeyDown(option.shortcutKey)) continue;

				option.buttonMethod.Invoke();
			}
		}
	}
}
