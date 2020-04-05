using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Generic
{
	public class InfoDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtTitle						= default;
		[SerializeField] private Text m_txtBody							= default;

		public delegate void OnDialogCallback();

		protected OnDialogCallback m_OnDialogClosedCB;


		public TextAnchor bodyAlignment
		{
			get { return m_txtBody.alignment;	}
			set { m_txtBody.alignment = value;	}
		}


		// Use this for initialization
		public void Initialize(string title, string body, OnDialogCallback cb=null)
		{
			m_txtTitle.text = title;
			m_txtBody.text = body;
			
			m_OnDialogClosedCB = cb;
		}

		public void OnClick_Close()
		{
			m_OnDialogClosedCB?.Invoke();

			Destroy(gameObject);
		}

		// Update is called once per frame
		private void Update()
		{
			// Trigger close on enter key
			if (Input.GetKeyDown(KeyCode.Return))
				OnClick_Close();
		}

		public static InfoDialog Create(string title, string body, OnDialogCallback cb=null)
		{
			GameObject goDlg = Instantiate(Resources.Load<GameObject>("Dialogs/Generic/CanvasInfoDialog"));
			InfoDialog dlg = goDlg.GetComponent<InfoDialog>();

			dlg.Initialize(title, body, cb);

			return dlg;
		}
	}
}
