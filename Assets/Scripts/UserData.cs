using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Systems;
using OP2UtilityDotNet;
using System.IO;
using UnityEngine;

namespace OP2MissionEditor
{
	/// <summary>
	/// Contains mission/map state.
	/// </summary>
	public class UserData
	{
		public static UserData current		{ get; private set; } = new UserData();

		/// <summary>
		/// If true, there are no unsaved changes.
		/// </summary>
		public bool isSaved					{ get; private set; }
		public void SetUnsaved()			{ isSaved = false;	}

		public Map map						{ get; private set; }
		public MissionRoot mission			{ get; private set; }

		public delegate void OnUserDataCallback(UserData src);

		public event OnUserDataCallback onChangedValuesCB;


		public void CreateNew()
		{
			Dispose();

			map = new Map();
			mission = new MissionRoot();

			onChangedValuesCB?.Invoke(this);
		}

		public bool LoadMission(string path)
		{
			// Load mission
			MissionRoot missionRoot = GetMissionData(path);
			if (missionRoot == null)
				return false;

			// Load map
			using (ResourceManager resourceManager = new ResourceManager(UserPrefs.gameDirectory))
			{
				byte[] mapData = resourceManager.GetResource(missionRoot.levelDetails.mapName, true);
				if (mapData == null)
					return false;

				Map tempMap = Map.ReadMap(mapData);
				if (tempMap == null)
					return false;

				// Replace current mission with the loaded mission
				Dispose();
				mission = missionRoot;
				map = tempMap;
			}

			// Inform listeners
			onChangedValuesCB?.Invoke(this);

			return true;
		}

		private MissionRoot GetMissionData(string path)
		{
			try
			{
				return MissionReader.GetMissionData(path);
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
				return null;
			}
		}

		public bool ImportMap(string path)
		{
			// Read map
			Map tempMap = Map.ReadMap(path);
			if (tempMap == null)
				return false;

			// Replace current map with the imported map
			map?.Dispose();
			map = tempMap;

			// Import successful. Inform listeners.
			onChangedValuesCB?.Invoke(this);

			return true;
		}

		public bool ImportMap(byte[] data)
		{
			// Read map
			Map tempMap = Map.ReadMap(data);
			if (tempMap == null)
				return false;

			// Replace current map with the imported map
			map?.Dispose();
			map = tempMap;

			// Import successful. Inform listeners.
			onChangedValuesCB?.Invoke(this);

			return true;
		}

		public bool ImportMission(string path)
		{
			MissionRoot root = GetMissionData(path);
			if (root == null)
				return false;

			// Replace current mission with the imported mission
			mission = root;

			// Import successful. Inform listeners.
			onChangedValuesCB?.Invoke(this);

			return true;
		}

		/// <summary>
		/// Marks the user data as unsaved and dispatches an event to refresh dependent UI.
		/// </summary>
		public void Dirty()
		{
			isSaved = false;

			onChangedValuesCB?.Invoke(this);
		}

		public void SaveMission(string path)
		{
			string dirPath = Path.GetDirectoryName(path);
			string missionName = Path.GetFileNameWithoutExtension(path);

			// Save mission file
			MissionReader.WriteMissionData(path, mission);

			// Save map file
			map.Write(Path.Combine(dirPath, mission.levelDetails.mapName));

			// Save plugin file
			PluginExporter.ExportPlugin(Path.Combine(dirPath, missionName + ".dll"), mission.levelDetails);
		}

		public void ExportMap(string path)
		{
			map.Write(path);
		}

		public void ExportMission(string path)
		{
			MissionReader.WriteMissionData(path, mission);
		}

		public string GetMissionTypePrefix()
		{
			switch (mission.levelDetails.missionType)
			{
				case MissionType.MultiLastOneStanding:	return "ml" + mission.levelDetails.numPlayers;
				case MissionType.MultiMidas:			return "mm" + mission.levelDetails.numPlayers;
				case MissionType.MultiResourceRace:		return "mr" + mission.levelDetails.numPlayers;
				case MissionType.MultiSpaceRace:		return "mf" + mission.levelDetails.numPlayers;
				case MissionType.MultiLandRush:			return "mu" + mission.levelDetails.numPlayers;
				case MissionType.Tutorial:				return "t";
				case MissionType.AutoDemo:				return "a";
				case MissionType.Colony:				return "c";
			}

			return "";
		}

		public void Dispose()
		{
			map?.Dispose();
			map = null;
		}
	}
}
