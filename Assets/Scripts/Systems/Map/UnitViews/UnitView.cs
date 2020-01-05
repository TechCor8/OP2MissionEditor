using UnityEngine;

namespace OP2MissionEditor.Systems.Map
{
	/// <summary>
	/// Represents a displayed unit on the map.
	/// </summary>
	public abstract class UnitView : MonoBehaviour
	{
		private bool m_CanShowTextOverlay = true;
		private bool m_IsUnitOverlayVisible;


		private void Awake()
		{
			UserPrefs.onChangedPrefsCB += OnChangedPrefs;
		}

		private void OnChangedPrefs()
		{
			// Don't do anything if the overlay pref did not change.
			if (m_IsUnitOverlayVisible == UserPrefs.isUnitOverlayVisible)
				return;

			if (!m_CanShowTextOverlay)
				return;

			RefreshOverlay();
		}

		protected void RefreshOverlay()
		{
			m_IsUnitOverlayVisible = UserPrefs.isUnitOverlayVisible;

			if (UserPrefs.isUnitOverlayVisible)
				OnShowTextOverlay();
			else
				OnHideTextOverlay();
		}

		public void SetCanShowTextOverlay(bool canShow)
		{
			m_CanShowTextOverlay = canShow;

			if (m_CanShowTextOverlay)
				RefreshOverlay();
			else
				OnHideTextOverlay();
		}

		protected virtual void OnShowTextOverlay() { }
		protected virtual void OnHideTextOverlay() { }


		private void OnDestroy()
		{
			UserPrefs.onChangedPrefsCB -= OnChangedPrefs;
		}
	}
}
