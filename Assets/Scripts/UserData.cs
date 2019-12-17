using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2UtilityDotNet;
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


		public static void CreateNew()
		{
			current?.Dispose();

			current.map = new Map();
			current.mission = new MissionRoot();
		}

		public static bool LoadMission(string path)
		{
			current?.Dispose();

			current.map = new Map();
			current.mission = MissionReader.GetMissionData(path);

			return true;
		}

		public static bool ImportMap(string path)
		{
			current.map?.Dispose();

			current.map = Map.ReadMap(path);
			if (current.map == null)
			{
				current.map = new Map();
				return false;
			}

			return true;
		}

		public static bool ImportMap(byte[] data)
		{
			current.map?.Dispose();

			current.map = Map.ReadMap(data);
			if (current.map == null)
			{
				current.map = new Map();
				return false;
			}

			return true;
		}

		public void SaveMission(string path)
		{
			MissionReader.WriteMissionData(path, current.mission);
		}

		public void ExportMap(string path)
		{
			map.Write(path);
		}

		public void Dispose()
		{
			map?.Dispose();
		}
	}
}
