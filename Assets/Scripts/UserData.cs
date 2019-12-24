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
			current?.Dispose();

			current.map = new Map();
			current.mission = MissionReader.GetMissionData(path);

			current.onChangedValuesCB?.Invoke(current);

			return true;
		}

		public static bool ImportMap(string path)
		{
			current.map?.Dispose();

			current.map = Map.ReadMap(path);
			if (current.map == null)
			{
				// Import failed. Create new map.
				current.map = new Map();
				current.onChangedValuesCB?.Invoke(current);
				return false;
			}

			// Import successful. Inform listeners.
			current.onChangedValuesCB?.Invoke(current);

			return true;
		}

		public static bool ImportMap(byte[] data)
		{
			current.map?.Dispose();

			current.map = Map.ReadMap(data);
			if (current.map == null)
			{
				// Import failed. Create new map.
				current.map = new Map();
				current.onChangedValuesCB?.Invoke(current);
				return false;
			}

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
