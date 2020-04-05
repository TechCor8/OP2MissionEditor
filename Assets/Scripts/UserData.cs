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
		public int selectedVariantIndex			{ get; private set; } = -1;
		public MissionVariant selectedVariant	{ get { return selectedVariantIndex < 0 ? mission.masterVariant : mission.missionVariants[selectedVariantIndex];					} }
		
		// Selected Difficulty (all players)
		public int selectedDifficultyIndex		{ get; private set; } = -1;
		public GameData selectedTethysGame		{ get { return selectedDifficultyIndex < 0 ? selectedVariant.tethysGame : selectedVariant.tethysDifficulties[selectedDifficultyIndex];} }

		// Events
		public delegate void OnUserDataCallback(UserData src);

		public event OnUserDataCallback onChangedValuesCB;
		public event OnUserDataCallback onSelectVariantCB;


		/// <summary>
		/// Sets the selected mission variant index.
		/// </summary>
		public void SetSelectedVariant(int index)
		{
			if (index < -1 || index >= mission.missionVariants.Count)
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

		/// <summary>
		/// Add new mission variant based on master variant.
		/// </summary>
		public void AddMissionVariant()
		{
			MissionVariant srcVariant = new MissionVariant(mission.masterVariant);
			srcVariant.layouts.Clear();
			srcVariant.name = "Variant " + mission.missionVariants.Count;
			srcVariant.tethysGame.beacons.Clear();
			srcVariant.tethysGame.markers.Clear();
			srcVariant.tethysGame.wreckage.Clear();

			foreach (GameData tethysGame in srcVariant.tethysDifficulties)
			{
				tethysGame.beacons.Clear();
				tethysGame.markers.Clear();
				tethysGame.wreckage.Clear();
			}

			foreach (PlayerData player in srcVariant.players)
			{
				ClearPlayerResourceData(player.resources);

				foreach (PlayerData.ResourceData resData in player.difficulties)
					ClearPlayerResourceData(resData);
			}

			mission.missionVariants.Add(srcVariant);

			SetUnsaved();
		}

		public void RemoveMissionVariant(int index)
		{
			mission.missionVariants.RemoveAt(index);

			// Keep selected index in range
			int selectedIndex = selectedVariantIndex;
			if (selectedIndex >= mission.missionVariants.Count)
				--selectedIndex;

			SetSelectedVariant(selectedIndex);
		}

		/// <summary>
		/// Gets the combined master variant (applies to all variants) and the selected variant.
		/// </summary>
		public MissionVariant GetCombinedVariant()
		{
			if (selectedVariantIndex < 0)
				return new MissionVariant(mission.masterVariant);

			return MissionVariant.Concat(mission.masterVariant, selectedVariant);
		}

		/// <summary>
		/// Gets the combined master variant and difficulties
		/// </summary>
		/// <returns></returns>
		public MissionVariant GetCombinedMission()
		{
			MissionVariant variant = GetCombinedVariant();

			variant.tethysGame = GetCombinedTethysGame(variant);

			foreach (PlayerData player in variant.players)
				player.resources = GetCombinedResourceData(player);

			return variant;
		}

		public GameData GetCombinedTethysGame()
		{
			return GetCombinedTethysGame(GetCombinedVariant());
		}
		public GameData GetCombinedTethysGame(MissionVariant variant)
		{
			// If no difficulty selected, return base data
			if (selectedDifficultyIndex < 0)
				return variant.tethysGame;

			// Return combined base and difficulty data
			variant.tethysGame.Concat(variant.tethysDifficulties[selectedDifficultyIndex]);
			return variant.tethysGame;
		}

		/// <summary>
		/// Sets the selected difficulty index (all players).
		/// </summary>
		public void SetSelectedDifficulty(int index)
		{
			if (index < -1 || index >= selectedVariant.players[0].difficulties.Count)
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

			// Add to master
			GameData tethysGame = mission.masterVariant.tethysDifficulties[cloneIndex];
			mission.masterVariant.tethysDifficulties.Add(new GameData(tethysGame));
			
			for (int i=0; i < mission.masterVariant.players.Count; ++i)
			{
				PlayerData.ResourceData resData = mission.masterVariant.players[i].difficulties[cloneIndex];
				mission.masterVariant.players[i].difficulties.Add(new PlayerData.ResourceData(resData));
			}

			// Add to variants
			foreach (MissionVariant variant in mission.missionVariants)
			{
				tethysGame = variant.tethysDifficulties[cloneIndex];
				variant.tethysDifficulties.Add(new GameData(tethysGame));

				for (int i=0; i < variant.players.Count; ++i)
				{
					PlayerData.ResourceData resData = variant.players[i].difficulties[cloneIndex];
					variant.players[i].difficulties.Add(new PlayerData.ResourceData(resData));
				}
			}

			SetUnsaved();
		}

		/// <summary>
		/// Add new difficulty based on master resource data.
		/// </summary>
		public void AddDifficulty()
		{
			// Add to master
			GameData tethysGame = new GameData(mission.masterVariant.tethysGame);
			tethysGame.beacons.Clear();
			tethysGame.markers.Clear();
			tethysGame.wreckage.Clear();
			mission.masterVariant.tethysDifficulties.Add(tethysGame);

			foreach (PlayerData player in mission.masterVariant.players)
			{
				PlayerData.ResourceData resData = new PlayerData.ResourceData(player.resources);
				ClearPlayerResourceData(resData);

				player.difficulties.Add(resData);
			}

			// Add to variants
			foreach (MissionVariant variant in mission.missionVariants)
			{
				variant.tethysDifficulties.Add(new GameData(mission.masterVariant.tethysGame));

				foreach (PlayerData player in variant.players)
				{
					PlayerData.ResourceData resData = new PlayerData.ResourceData(player.resources);
					ClearPlayerResourceData(resData);

					player.difficulties.Add(resData);
				}
			}

			SetUnsaved();
		}

		/// <summary>
		/// Removes a difficulty from all players and all mission variants.
		/// </summary>
		public void RemoveDifficulty(int index)
		{
			if (selectedVariant.players[0].difficulties.Count < 1)
				throw new System.Exception("Difficulty count is at minimum size!");

			if (index < 0 || index >= selectedVariant.players[0].difficulties.Count)
				throw new System.IndexOutOfRangeException("index: " + index);

			// Remove from master
			for (int i=0; i < mission.masterVariant.players.Count; ++i)
				mission.masterVariant.players[i].difficulties.RemoveAt(index);

			// Remove from variants
			foreach (MissionVariant variant in mission.missionVariants)
			{
				for (int i=0; i < variant.players.Count; ++i)
					variant.players[i].difficulties.RemoveAt(index);
			}

			SetUnsaved();

			// Keep selected index in range
			int selectedIndex = selectedDifficultyIndex;
			if (selectedIndex >= selectedVariant.players[0].difficulties.Count)
				--selectedIndex;

			SetSelectedDifficulty(selectedIndex);
		}

		public void AddPlayer()
		{
			PlayerData player = new PlayerData(mission.masterVariant.players.Count);

			// Make sure difficulty count is sync'd to other players
			player.difficulties.Clear();
			foreach (PlayerData.ResourceData resData in mission.masterVariant.players[0].difficulties)
				player.difficulties.Add(new PlayerData.ResourceData());

			// Clear difficulty resources
			foreach (PlayerData.ResourceData resData in player.difficulties)
				ClearPlayerResourceData(resData);

			// Add to master variant
			mission.masterVariant.players.Add(player);

			// Clear concat data
			player = new PlayerData(player);

			ClearPlayerResourceData(player.resources);

			// Add player to remaining variants
			for (int i=0; i < mission.missionVariants.Count; ++i)
				mission.missionVariants[i].players.Add(new PlayerData(player));

			SetUnsaved();
		}

		private void ClearPlayerResourceData(PlayerData.ResourceData resData)
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
			resData.wallTubes.Clear();
		}

		public void RemovePlayer(int index)
		{
			mission.masterVariant.players.RemoveAt(index);

			foreach (MissionVariant variant in mission.missionVariants)
				variant.players.RemoveAt(index);

			SetUnsaved();
		}

		/// <summary>
		/// Gets the player's resource data for the currently selected variant and difficulty.
		/// </summary>
		public PlayerData.ResourceData GetPlayerResourceData(int playerIndex)
		{
			return selectedDifficultyIndex < 0 ? selectedVariant.players[playerIndex].resources : selectedVariant.players[playerIndex].difficulties[selectedDifficultyIndex];
		}

		/// <summary>
		/// Gets the player's resource data for the currently selected difficulty.
		/// </summary>
		public PlayerData.ResourceData GetPlayerResourceData(PlayerData player)
		{
			return selectedDifficultyIndex < 0 ? player.resources : player.difficulties[selectedDifficultyIndex];
		}

		public PlayerData.ResourceData GetCombinedResourceData(PlayerData player)
		{
			// If no difficulty selected, return base data
			if (selectedDifficultyIndex < 0)
				return new PlayerData.ResourceData(player.resources);

			// Return combined base and difficulty data
			return PlayerData.ResourceData.Concat(player.resources, player.difficulties[selectedDifficultyIndex]);
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
				selectedVariantIndex = -1;
				selectedDifficultyIndex = -1;
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
				selectedVariantIndex = -1;
				selectedDifficultyIndex = -1;
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
			PluginExporter.ExportPlugin(Path.Combine(dirPath, missionName + ".dll"), mission.sdkVersion, mission.levelDetails);
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
