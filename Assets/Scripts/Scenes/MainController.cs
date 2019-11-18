using OP2MissionEditor.Dialogs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OP2MissionEditor.Scenes
{
	public class MainController : MonoBehaviour
	{
		private void Awake()
		{
			UserData.CreateNew();

			// If game directory hasn't been set, Open "Locate Outpost2" dialog to force user to select one
			if (string.IsNullOrEmpty(UserPrefs.GameDirectory))
				PreferencesDialog.Create();
		}

		private void OnDestroy()
		{
			UserData.current.Dispose();
		}
	}
}
