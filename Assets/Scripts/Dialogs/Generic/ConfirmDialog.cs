using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs.Generic
{
	public class ConfirmDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtTitle						= default;
		[SerializeField] private Text m_txtBody							= default;

		[SerializeField] private Text m_txtSubmit						= default;
		[SerializeField] private Text m_txtCancel						= default;

		public delegate void OnDialogCallback(bool didConfirm);

		protected OnDialogCallback m_OnDialogClosedCB;


		public TextAnchor bodyAlignment
		{
			get { return m_txtBody.alignment;	}
			set { m_txtBody.alignment = value;	}
		}


		// Use this for initialization
		public void Initialize(OnDialogCallback cb, string title, string body, string submit="OK", string cancel="Cancel")
		{
			m_txtTitle.text = title;
			m_txtBody.text = body;

			m_txtSubmit.text = submit;
			m_txtCancel.text = cancel;
			
			m_OnDialogClosedCB = cb ?? throw new System.ArgumentNullException("cb");
		}

		public void OnClick_Submit()
		{
			m_OnDialogClosedCB?.Invoke(true);

			Destroy(gameObject);
		}

		public void OnClick_Cancel()
		{
			m_OnDialogClosedCB?.Invoke(false);

			Destroy(gameObject);
		}

		public static ConfirmDialog Create(OnDialogCallback cb, string title, string body, string submit="OK", string cancel="Cancel")
		{
			GameObject goDlg = Instantiate(Resources.Load<GameObject>("Dialogs/Generic/CanvasConfirmDialog"));
			ConfirmDialog dlg = goDlg.GetComponent<ConfirmDialog>();

			dlg.Initialize(cb, title, body, submit, cancel);

			return dlg;
		}
	}
}
