using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents a list of string items to the user for selection.
	/// </summary>
	public class ProgressDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtTitle					= default;
		[SerializeField] private Image m_ProgressBar				= default;
		

		private void Initialize(string title)
		{
			m_txtTitle.text = title;
		}

		public void SetTitle(string title)
		{
			m_txtTitle.text = title;
		}

		public void SetProgress(float progress)
		{
			m_ProgressBar.fillAmount = progress;
		}

		public void Close()
		{
			Destroy(gameObject);
		}


		/// <summary>
		/// Creates and presents the progress dialog to the user.
		/// </summary>
		/// <param name="title">The title of the dialog box.</param>
		public static ProgressDialog Create(string title)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/ProgressDialog"));
			ProgressDialog dialog = goDialog.GetComponent<ProgressDialog>();
			dialog.Initialize(title);

			return dialog;
		}
	}
}
