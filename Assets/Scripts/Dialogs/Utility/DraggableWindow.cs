using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Makes a window draggable.
	/// </summary>
	public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private RectTransform m_DraggableWindow		= default;


		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
			// Drag the window
			m_DraggableWindow.anchoredPosition += eventData.delta;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			CanvasScaler canvas = m_DraggableWindow.GetComponentInParent<CanvasScaler>();

			Vector2 refResolution = canvas.referenceResolution / 2;
			refResolution.x = refResolution.y * (Screen.width / (float)Screen.height);
			Vector2 position = m_DraggableWindow.anchoredPosition;
			Vector2 size = m_DraggableWindow.rect.size / 2;

			// Keep the window inside the screen bounds
			if (position.x+size.x > refResolution.x)		position.x = refResolution.x - size.x;
			if (position.y+size.y > refResolution.y)		position.y = refResolution.y - size.y;
			if (position.x-size.x < -refResolution.x)		position.x = -refResolution.x + size.x;
			if (position.y-size.y < -refResolution.y)		position.y = -refResolution.y + size.y;

			m_DraggableWindow.anchoredPosition = position;
		}

		private void OnRectTransformDimensionsChange()
		{
			// If window has changed size, keep it in bounds
			OnEndDrag(null);
		}
	}
}
