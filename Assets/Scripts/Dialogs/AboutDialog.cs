using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Displays log messages.
	/// </summary>
	public class AboutDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtVersion		= default;


		private void Awake()
		{
			string version = "Version " + Application.version.ToString();
			version += "\nUnity " + Application.unityVersion;

			m_txtVersion.text = version;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);
		}

		/// <summary>
		/// Creates and presents the About dialog to the user.
		/// </summary>
		public static AboutDialog Create()
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/AboutDialog"));
			AboutDialog dialog = goDialog.GetComponent<AboutDialog>();
			
			return dialog;
		}
	}
}
