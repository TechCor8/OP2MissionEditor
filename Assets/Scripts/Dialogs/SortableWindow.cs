using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Sorts a window to top when window is focused.
	/// </summary>
	public class SortableWindow : MonoBehaviour, IPointerDownHandler
	{
		private static List<SortableWindow> m_SortableWindows = new List<SortableWindow>();


		private void Awake()
		{
			m_SortableWindows.Add(this);

			Refresh();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			// Sort to top
			m_SortableWindows.Remove(this);
			m_SortableWindows.Add(this);

			Refresh();
		}

		private void Refresh()
		{
			// Sort windows
			int sortOrder = 0;
			foreach (SortableWindow window in m_SortableWindows)
				window.GetComponent<Canvas>().sortingOrder = sortOrder++;
		}

		private void OnDestroy()
		{
			m_SortableWindows.Remove(this);
		}
	}
}
