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
		}

		private void OnDisable()
		{
			m_MapRenderer.HideCellTypeMap();
		}

		protected override void OnPaintTile(Vector3Int tileXY)
		{
			// Paint tile with cell type
			UserData.current.map.SetCellType((ulong)tileXY.x, (ulong)tileXY.y, (CellType)m_DropdownCellType.value);
			UserData.current.SetUnsaved();

			m_MapRenderer.RefreshTile(tileXY);
		}
	}
}
