﻿using UnityEngine;

namespace OP2MissionEditor
{
	public class UserPrefs
	{
		/// <summary>
		/// Called when user prefs change.
		/// </summary>
		public static event System.Action onChangedPrefsCB;

		/// <summary>
		/// The Outpost 2 game directory that contains the .vol files for rendering.
		/// </summary>
		public static string gameDirectory
		{
			get { return PlayerPrefs.GetString("OP2Directory");														}
			set { PlayerPrefs.SetString("OP2Directory", value); PlayerPrefs.Save(); onChangedPrefsCB?.Invoke();		}
		}

		/// <summary>
		/// The color of the grid overlay.
		/// </summary>
		public static Color32 gridOverlayColor
		{
			get
			{
				return new Color32(	(byte)PlayerPrefs.GetInt("GridOverlay_Red", 0),
									(byte)PlayerPrefs.GetInt("GridOverlay_Green", 174),
									(byte)PlayerPrefs.GetInt("GridOverlay_Blue", 0),
									(byte)PlayerPrefs.GetInt("GridOverlay_Alpha", 182));
			}
			set
			{
				PlayerPrefs.SetInt("GridOverlay_Red", value.r);
				PlayerPrefs.SetInt("GridOverlay_Green", value.g);
				PlayerPrefs.SetInt("GridOverlay_Blue", value.b);
				PlayerPrefs.SetInt("GridOverlay_Alpha", value.a);
				PlayerPrefs.Save();

				onChangedPrefsCB?.Invoke();
			}
		}

		/// <summary>
		/// Is the grid overlay visible?
		/// </summary>
		public static bool isGridOverlayVisible
		{
			get { return PlayerPrefs.GetInt("GridOverlay_Visible", 1) != 0;													}
			set { PlayerPrefs.SetInt("GridOverlay_Visible", value ? 1 : 0); PlayerPrefs.Save(); onChangedPrefsCB?.Invoke();	}
		}

		/// <summary>
		/// Is the unit overlay visible?
		/// </summary>
		public static bool isUnitOverlayVisible
		{
			get { return PlayerPrefs.GetInt("UnitOverlay_Visible", 0) != 0;													}
			set { PlayerPrefs.SetInt("UnitOverlay_Visible", value ? 1 : 0); PlayerPrefs.Save(); onChangedPrefsCB?.Invoke();	}
		}

		/// <summary>
		/// Is unit info text visible?
		/// </summary>
		public static bool isUnitInfoVisible
		{
			get { return PlayerPrefs.GetInt("UnitInfo_Visible", 1) != 0;													}
			set { PlayerPrefs.SetInt("UnitInfo_Visible", value ? 1 : 0); PlayerPrefs.Save(); onChangedPrefsCB?.Invoke();	}
		}
	}
}
