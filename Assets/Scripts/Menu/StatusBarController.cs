using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Menu
{
	public class StatusBarController : MonoBehaviour
	{
		[SerializeField] private Text m_txtStatus = default;


		private void Awake()
		{
			Application.logMessageReceived += OnLogMessageReceived;
		}

		private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			m_txtStatus.text = condition;
		}
	}
}
