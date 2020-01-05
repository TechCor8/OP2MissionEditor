using OP2UtilityDotNet;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Paint
{
	/// <summary>
	/// Paint pane for editing map cell types.
	/// </summary>
	public class CellTypePane : PaintPane
	{
		[SerializeField] private Dropdown	m_DropdownCellType			= default;


		private void OnEnable()
		{
			m_MapRenderer.ShowCellTypeMap();

			// Refresh overlay
			OnChanged_CellType();
		}

		private void OnDisable()
		{
			m_MapRenderer.HideCellTypeMap();
		}

		public void OnChanged_CellType()
		{
			// Set overlay sprite
			m_OverlayRenderer.SetOverlay(m_MapRenderer.GetCellTypeSprite((CellType)m_DropdownCellType.value));
		}

		protected override void OnPaintTile(Vector2Int tileXY)
		{
			// Paint tile with cell type
			UserData.current.map.SetCellType((ulong)tileXY.x, (ulong)tileXY.y, (CellType)m_DropdownCellType.value);
			UserData.current.SetUnsaved();

			m_MapRenderer.RefreshTile(tileXY);
		}
	}
}
