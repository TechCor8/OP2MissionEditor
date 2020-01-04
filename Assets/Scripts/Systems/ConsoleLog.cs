using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace OP2MissionEditor.Systems
{
	/// <summary>
	/// Maintains a rolling list of console messages.
	/// </summary>
	public class ConsoleLog
	{
		private static bool m_IsInitialized;
		private static List<string> m_Messages = new List<string>();

		// Accessors
		public static ReadOnlyCollection<string> messages		{ get { return m_Messages.AsReadOnly();		} }

		// Events
		public static event System.Action onLogUpdated;


		/// <summary>
		/// Initializes the console log.
		/// </summary>
		public static void Initialize()
		{
			if (m_IsInitialized)
				throw new System.Exception("ConsoleLog already initialized!");

			Application.logMessageReceived += OnLogMessageReceived;

			m_IsInitialized = true;
		}

		private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Warning:		condition = "<color=#D05000>" + condition + "</color>";						break;
				case LogType.Error:			condition = "<color=#B00000>" + condition + "</color>";						break;
				case LogType.Exception:		condition = "<color=#B00000>" + condition + "\n" + stackTrace + "</color>";	break;
			}

			m_Messages.Add(condition);

			if (m_Messages.Count > 30)
				m_Messages.RemoveAt(0);

			onLogUpdated?.Invoke();
		}
	}
}
