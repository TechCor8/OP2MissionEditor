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
		public bool isSaved						{ get; private set; }
		public void SetUnsaved()				{ isSaved = false;	}

		public Map map							{ get; private set; }
		public MissionRoot mission				{ get; private set; }

		// Mission Variants
		public int selectedVariantIndex			{ get; private set; }
		public MissionVariant selectedVariant	{ get { return mission.missionVariants[selectedVariantIndex]; } }

		// Selected Difficulty (all players)
		public int selectedDifficultyIndex		{ get; private set; }

		// Events
		public delegate void OnUserDataCallback(UserData src);

		public event OnUserDataCallback onChangedValuesCB;
		public event OnUserDataCallback onSelectVariantCB;


		/// <summary>
		/// Sets the selected mission variant index.
		/// </summary>
		public void SetSelectedVariant(int index)
		{
			if (index < 0 || index >= mission.missionVariants.Count)
				throw new System.IndexOutOfRangeException("index: " + index);

			selectedVariantIndex = index;

			onSelectVariantCB?.Invoke(this);
		}

		public void AddMissionVariant(int cloneIndex)
		{
			MissionVariant srcVariant = mission.missionVariants[cloneIndex];

			MissionVariant destVariant = new MissionVariant(srcVariant);
			destVariant.name = "Variant " + mission.missionVariants.Count;
			mission.missionVariants.Add(destVariant);

			SetUnsaved();
		}

		public void AddMissionVariant()
		{
			MissionVariant srcVariant = new MissionVariant(mission.missionVariants[0]);
			srcVariant.layouts.Clear();
			srcVariant.name = "Variant " + mission.missionVariants.Count;
			srcVariant.tethysGame.beacons.Clear();
			srcVariant.tethysGame.markers.Clear();
			srcVariant.tethysGame.wallTubes.Clear();
			srcVariant.tethysGame.wreckage.Clear();
			foreach (PlayerData player in srcVariant.players)
			{
				foreach (PlayerData.ResourceData resData in player.difficulties)
				{
					resData.commonOre = 0;
					resData.completedResearch = new int[0];
					resData.food = 0;
					resData.kids = 0;
					resData.rareOre = 0;
					resData.scientists = 0;
					resData.solarSatellites = 0;
					resData.workers = 0;
					resData.units.Clear();
				}
			}

			mission.missionVariants.Add(srcVariant);

			SetUnsaved();
		}

		public void RemoveMissionVariant(int index)
		{
			mission.missionVariants.RemoveAt(index);

			// Keep selected index in range
			index = selectedVariantIndex;
			if (index >= mission.missionVariants.Count)
				--index;

			SetSelectedVariant(index);
		}

		/// <summary>
		/// Gets the combined master variant (applies to all variants) and the selected variant.
		/// </summary>
		public MissionVariant GetCombinedVariant()
		{
			if (selectedVariantIndex == 0)
				return selectedVariant;

			return MissionVariant.Concat(mission.missionVariants[0], selectedVariant);
		}

		/// <summary>
		/// Sets the selected difficulty index (all players).
		/// </summary>
		public void SetSelectedDifficulty(int index)
		{
			if (index < 0 || index >= selectedVariant.players[0].difficulties.Count)
				throw new System.IndexOutOfRangeException("index: " + index);

			selectedDifficultyIndex = index;

			onSelectVariantCB?.Invoke(this);
		}

		/// <summary>
		/// Clones a difficulty index and adds it to all players in all mission variants.
		/// </summary>
		public void AddDifficulty(int cloneIndex)
		{
			if (cloneIndex < 0 || cloneIndex >= selectedVariant.players[0].difficulties.Count)
				throw new System.IndexOutOfRangeException("index: " + cloneIndex);

			foreach (MissionVariant variant in mission.missionVariants)
			{
				for (int i=0; i < variant.players.Count; ++i)
				{
					PlayerData.ResourceData resData = variant.players[i].difficulties[cloneIndex];
					variant.players[i].difficulties.Add(new PlayerData.ResourceData(resData));
				}
			}

			SetUnsaved();
		}

		/// <summary>
		/// Removes a difficulty from all players and all mission variants.
		/// </summary>
		public void RemoveDifficulty(int index)
		{
			if (selectedVariant.players[0].difficulties.Count <= 1)
				throw new System.Exception("Difficulty count is at minimum size!");

			if (index < 0 || index >= selectedVariant.players[0].difficulties.Count)
				throw new System.IndexOutOfRangeException("index: " + index);

			foreach (MissionVariant variant in mission.missionVariants)
			{
				for (int i=0; i < variant.players.Count; ++i)
					variant.players[i].difficulties.RemoveAt(index);
			}

			SetUnsaved();

			// Keep selected index in range
			index = selectedDifficultyIndex;
			if (index >= selectedVariant.players[0].difficulties.Count)
				--index;

			SetSelectedDifficulty(index);
		}

		public void AddPlayer(PlayerData player)
		{
			// Add to master variant
			mission.missionVariants[0].players.Add(player);

			// Clear concat data
			player = new PlayerData(player);

			foreach (PlayerData.ResourceData resData in player.difficulties)
			{
				resData.commonOre = 0;
				resData.completedResearch = new int[0];
				resData.food = 0;
				resData.kids = 0;
				resData.rareOre = 0;
				resData.scientists = 0;
				resData.solarSatellites = 0;
				resData.workers = 0;
				resData.units.Clear();
			}

			// Add player to remaining variants
			for (int i=1; i < mission.missionVariants.Count; ++i)
				mission.missionVariants[i].players.Add(new PlayerData(player));

			SetUnsaved();
		}

		public void RemovePlayer(int index)
		{
			foreach (MissionVariant variant in mission.missionVariants)
				variant.players.RemoveAt(index);

			SetUnsaved();
		}

		/// <summary>
		/// Gets the player's resource data for the currently selected variant and difficulty.
		/// </summary>
		public PlayerData.ResourceData GetPlayerResourceData(int playerIndex)
		{
			return selectedVariant.players[playerIndex].difficulties[selectedDifficultyIndex];
		}


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

			try
			{
				selectedVariantIndex = 0;
				selectedDifficultyIndex = 0;
			}
			catch (System.IndexOutOfRangeException ex)
			{
				Debug.LogException(ex);
				return false;
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

			try
			{
				selectedVariantIndex = 0;
				selectedDifficultyIndex = 0;
			}
			catch (System.IndexOutOfRangeException ex)
			{
				Debug.LogException(ex);
				mission = new MissionRoot();
				return false;
			}

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
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				// Write mission file
				System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(MissionRoot));
				using (MemoryStream stream = new MemoryStream())
				{
					serializer.WriteObject(stream, mission);
					stream.Position = 0;
					using (StreamReader reader = new StreamReader(stream))
						writer.Write(Utility.JsonFormatter.Format(reader.ReadToEnd()));
				}
			}
			//MissionReader.WriteMissionData(path, mission);

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
