using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Allows only one instance of a window.
	/// </summary>
	public class SingleInstanceWindow : MonoBehaviour
	{
		[SerializeField] private MonoBehaviour m_WindowType		= default;

		private static List<MonoBehaviour> m_ActiveWindows = new List<MonoBehaviour>();


		private void Awake()
		{
			string typeName = m_WindowType.GetType().Name;

			// Don't allow multiple instances by checking the window's type
			if (m_ActiveWindows.FindIndex((window) => window.GetType().Name == typeName) >= 0)
			{
				Destroy(gameObject);
				return;
			}

			m_ActiveWindows.Add(m_WindowType);
		}

		private void OnDestroy()
		{
			m_ActiveWindows.Remove(m_WindowType);
		}
	}
}
