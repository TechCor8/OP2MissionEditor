using OP2MissionEditor.Systems.Map;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor minimap to the user.
	/// </summary>
	public class MinimapDialog : MonoBehaviour
	{
		[SerializeField] private RectTransform m_Frame				= default;
		[SerializeField] private RawImage m_MinimapImage			= default;
		[SerializeField] private RawImage m_MinimapUnitImage		= default;
		[SerializeField] private RectTransform m_MinimapBounds		= default;
		
		public delegate void OnCloseCallback();

		private MapRenderer m_MapRenderer;
		private UnitRenderer m_UnitRenderer;
		private OnCloseCallback m_OnCloseCB;

		private bool m_IsMouseOverMinimap;
		private bool m_IsPainting;

		private Vector2Int m_MapSize;


		private void Initialize(MapRenderer mapRenderer, UnitRenderer unitRenderer, OnCloseCallback onCloseCB)
		{
			m_MapRenderer = mapRenderer;
			m_UnitRenderer = unitRenderer;
			m_OnCloseCB = onCloseCB;

			mapRenderer.onMapRefreshedCB += OnMapRefreshed;
			m_UnitRenderer.onRefreshedCB += OnUnitRendererRefreshed;

			OnMapRefreshed(mapRenderer);
		}

		private void OnUnitRendererRefreshed()
		{
			OnMapRefreshed(null);
		}

		private void OnMapRefreshed(MapRenderer mapRenderer)
		{
			// Update minimap texture
			m_MinimapImage.texture = m_MapRenderer.minimapTexture;
			m_MinimapUnitImage.texture = m_UnitRenderer.unitMinimap.minimapTexture;
			m_MapSize = new Vector2Int((int)UserData.current.map.WidthInTiles(), (int)UserData.current.map.HeightInTiles());

			// Don't change aspect if map has not yet been initialized
			if (m_MapSize.y == 0)
				return;

			// Adjust window to match map aspect ratio
			float mapAspect = (float)m_MapSize.x / m_MapSize.y;
			m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_Frame.rect.height * mapAspect);
		}

		public void OnPointerEnter(BaseEventData evData)
		{
			m_IsMouseOverMinimap = true;
		}

		public void OnPointerExit(BaseEventData evData)
		{
			m_IsMouseOverMinimap = false;
		}

		private void Update()
		{
			DetectPaintState();

			// Don't do anything if there is no map loaded
			if (m_MinimapImage.texture == null)
				return;

			// Update camera position if the user trying to move the minimap
			if (m_IsMouseOverMinimap && !m_IsPainting && Input.GetMouseButton(0))
			{
				// Get the minimap rect in world space
				Vector3[] minimapCorners = new Vector3[4];
				m_MinimapImage.rectTransform.GetWorldCorners(minimapCorners);
				Rect minimapRect = new Rect(minimapCorners[0], minimapCorners[2]-minimapCorners[0]);

				Vector2 localMousePosition = (Vector2)Input.mousePosition - minimapRect.min;
				Vector2 percPosition = new Vector2(localMousePosition.x / minimapRect.width, localMousePosition.y / minimapRect.height);

				// Move camera to position clicked on minimap
				Vector3 cameraPosition	= Camera.main.transform.position;
				Vector2 mapSize			= new Vector2(m_MapSize.x*32, m_MapSize.y*32);

				cameraPosition = new Vector3(percPosition.x * mapSize.x, percPosition.y * mapSize.y, cameraPosition.z);
				Camera.main.transform.position = cameraPosition;
			}
		}

		private void LateUpdate()
		{
			// Don't do anything if there is no map loaded
			if (m_MinimapImage.texture == null)
				return;

			// Get information about camera, map and minimap
			Vector3 cameraPosition	= Camera.main.transform.position;
			Vector2 cameraSize		= new Vector2(Camera.main.orthographicSize*2 * Camera.main.aspect, Camera.main.orthographicSize*2);
			Vector2 mapSize			= new Vector2(m_MapSize.x*32, m_MapSize.y*32);
			Vector2 minimapSize		= m_MinimapImage.rectTransform.rect.size;
			Vector2 minimapScale	= new Vector2(minimapSize.x / mapSize.x, minimapSize.y / mapSize.y);

			// Update bounds position
			Vector2 minimapCamPosition = new Vector2(cameraPosition.x * minimapScale.x, cameraPosition.y * minimapScale.y);
			Vector2 minimapCamSize = new Vector2(cameraSize.x * minimapScale.x, cameraSize.y * minimapScale.y);

			m_MinimapBounds.anchoredPosition = minimapCamPosition;
			m_MinimapBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minimapCamSize.x);
			m_MinimapBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minimapCamSize.y);
		}

		private void DetectPaintState()
		{
			// If mouse button is up, paint mode is off
			if (Input.GetMouseButtonUp(0))
				m_IsPainting = false;

			// If mouse is over UI, we can't start painting
			if (EventSystem.current.IsPointerOverGameObject())
				return;

			// If we mouse down, and we are not over the UI, paint mode is on
			if (Input.GetMouseButtonDown(0))
				m_IsPainting = true;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}

		private void OnDestroy()
		{
			m_UnitRenderer.onRefreshedCB -= OnUnitRendererRefreshed;
			m_MapRenderer.onMapRefreshedCB -= OnMapRefreshed;
		}


		/// <summary>
		/// Creates and presents the Preferences dialog to the user.
		/// </summary>
		/// <param name="mapRenderer">The map to render as a minimap.</param>
		/// <param name="unitRenderer">The units to render to the minimap.</param>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static MinimapDialog Create(MapRenderer mapRenderer, UnitRenderer unitRenderer, OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/MinimapDialog"));
			MinimapDialog dialog = goDialog.GetComponent<MinimapDialog>();
			dialog.Initialize(mapRenderer, unitRenderer, onCloseCB);

			return dialog;
		}
	}
}
