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


		public static void CreateNew()
		{
			current?.Dispose();

			current.map = new Map();
			current.mission = new MissionRoot();

			current.onChangedValuesCB?.Invoke(current);
		}

		public static bool LoadMission(string path)
		{
			// Load mission
			MissionRoot missionRoot = GetMissionData(path);
			if (missionRoot == null)
				return false;

			// Load map
			using (ResourceManager resourceManager = new ResourceManager(UserPrefs.gameDirectory))
			{
				byte[] mapData = resourceManager.GetResource(current.mission.levelDetails.mapName, true);
				if (mapData == null)
					return false;

				Map map = Map.ReadMap(mapData);
				if (map == null)
					return false;

				// Replace current mission with the loaded mission
				current?.Dispose();
				current.mission = missionRoot;
				current.map = map;
			}

			// Inform listeners
			current.onChangedValuesCB?.Invoke(current);

			return true;
		}

		private static MissionRoot GetMissionData(string path)
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

		public static bool ImportMap(string path)
		{
			// Read map
			Map map = Map.ReadMap(path);
			if (map == null)
				return false;

			// Replace current map with the imported map
			current.map?.Dispose();
			current.map = map;

			// Import successful. Inform listeners.
			current.onChangedValuesCB?.Invoke(current);

			return true;
		}

		public static bool ImportMap(byte[] data)
		{
			// Read map
			Map map = Map.ReadMap(data);
			if (map == null)
				return false;

			// Replace current map with the imported map
			current.map?.Dispose();
			current.map = map;

			// Import successful. Inform listeners.
			current.onChangedValuesCB?.Invoke(current);

			return true;
		}

		public static bool ImportMission(string path)
		{
			MissionRoot root = GetMissionData(path);
			if (root == null)
				return false;

			// Replace current mission with the imported mission
			current.mission = root;

			// Import successful. Inform listeners.
			current.onChangedValuesCB?.Invoke(current);

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
