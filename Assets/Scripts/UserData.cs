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
		public static UserData current		{ get; private set; }

		private Map m_Map;
		private MissionRoot m_MissionData;

		/// <summary>
		/// If true, there are no unsaved changes.
		/// </summary>
		public bool isSaved					{ get; private set; }
		public void SetUnsaved()			{ isSaved = false;	}

		public Map map						{ get { return m_Map; } }


		public static void CreateNew()
		{
			current?.Dispose();

			current = new UserData();
			current.m_Map = new Map();
			current.m_MissionData = new MissionRoot();
		}

		public static bool LoadMission(string path)
		{
			current?.Dispose();

			current = new UserData();
			current.m_Map = new Map();
			current.m_MissionData = MissionReader.GetMissionData(path);

			return true;
		}

		public static bool ImportMap(string path)
		{
			current.m_Map?.Dispose();

			current.m_Map = Map.ReadMap(path);

			return true;
		}

		public static bool ImportMap(byte[] data)
		{
			current.m_Map?.Dispose();

			current.m_Map = Map.ReadMap(data);

			return true;
		}

		public void SaveMission(string path)
		{
			MissionReader.WriteMissionData(path, current.m_MissionData);
		}

		public void ExportMap(string path)
		{
			m_Map.Write(path);
		}

		public void Dispose()
		{
			m_Map?.Dispose();
		}
	}
}
