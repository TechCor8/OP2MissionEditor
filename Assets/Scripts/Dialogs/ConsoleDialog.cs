using OP2MissionEditor.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Displays log messages.
	/// </summary>
	public class ConsoleDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtConsole		= default;


		private void Awake()
		{
			// Listen for log updates
			ConsoleLog.onLogUpdated += OnLogUpdated;

			// Initialize displayed messages
			OnLogUpdated();
		}

		private void OnLogUpdated()
		{
			m_txtConsole.text = string.Join("\n", ConsoleLog.messages);
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			ConsoleLog.onLogUpdated -= OnLogUpdated;
		}

		/// <summary>
		/// Creates and presents the Console dialog to the user.
		/// </summary>
		public static ConsoleDialog Create()
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/ConsoleDialog"));
			ConsoleDialog dialog = goDialog.GetComponent<ConsoleDialog>();
			
			return dialog;
		}
	}
}
