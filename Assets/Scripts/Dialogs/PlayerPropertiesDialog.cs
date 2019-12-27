using DotNetMissionSDK;
using DotNetMissionSDK.AI;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Systems.TechTree;
using OP2MissionEditor.UserInterface;
using OP2UtilityDotNet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor player properties to the user.
	/// </summary>
	public class PlayerPropertiesDialog : MonoBehaviour
	{
		[SerializeField] private GameObject	m_PlayerListPrefab			= default;
		[SerializeField] private ListBox	m_PlayerListBox				= default;
		[SerializeField] private Button		m_BtnAddPlayer				= default;
		[SerializeField] private Button		m_BtnRemovePlayer			= default;

		[SerializeField] private Dropdown	m_DropdownColonyType		= default;
		[SerializeField] private Dropdown	m_DropdownColor				= default;
		[SerializeField] private Dropdown	m_DropdownControlType		= default;
		[SerializeField] private InputField	m_InputTechLevel			= default;
		[SerializeField] private Dropdown	m_DropdownMoraleLevel		= default;
		[SerializeField] private Toggle		m_ToggleFreeMorale			= default;

		[SerializeField] private GameObject	m_AllyListPrefab			= default;
		[SerializeField] private ListBox	m_AllyListBox				= default;
		[SerializeField] private Button		m_BtnAddAlly				= default;
		[SerializeField] private Button		m_BtnRemoveAlly				= default;
		[SerializeField] private Dropdown	m_DropdownAlly				= default;

		[SerializeField] private GameObject	m_CompletedTechListPrefab	= default;
		[SerializeField] private ListBox	m_CompletedTechListBox		= default;
		[SerializeField] private Button		m_BtnAddCompletedTech		= default;
		[SerializeField] private Button		m_BtnRemoveCompletedTech	= default;
		[SerializeField] private Dropdown	m_DropdownCompletedTech		= default;

		[SerializeField] private InputField	m_InputSolarSats			= default;
		[SerializeField] private InputField	m_InputKids					= default;
		[SerializeField] private InputField	m_InputWorkers				= default;
		[SerializeField] private InputField	m_InputScientists			= default;
		[SerializeField] private InputField	m_InputCommonMetal			= default;
		[SerializeField] private InputField	m_InputRareMetal			= default;
		[SerializeField] private InputField	m_InputFood					= default;
		
		public delegate void OnCloseCallback();

		private UserData m_UserData;
		private OnCloseCallback m_OnCloseCB;

		private bool m_CanSave;

		// Available Technologies
		private Dictionary<int, string> m_TechNames = new Dictionary<int, string>();
		private List<int> m_TechIds = new List<int>();

		private List<PlayerData> m_Players;
		
		// Selected player parameters
		private PlayerData m_SelectedPlayer;
		private List<int> m_Allies;
		private List<int> m_CompletedTech;


		private void Initialize(UserData userData, OnCloseCallback onCloseCB)
		{
			m_UserData = userData;
			m_OnCloseCB = onCloseCB;

			// Initialize display
			m_Players = new List<PlayerData>(userData.mission.players);

			m_PlayerListBox.onSelectedItemCB += OnSelect_Player;
			m_AllyListBox.onSelectedItemCB += OnSelect_Ally;
			m_CompletedTechListBox.onSelectedItemCB += OnSelect_CompletedTech;

			PopulatePlayerList();

			ReadTechFile();
			
			m_CanSave = true;
		}

		private void ReadTechFile()
		{
			// Read technology from tech sheet
			m_TechNames.Clear();
			m_TechIds.Clear();

			TechData[] technologies = TechFileReader.GetTechnologies(UserPrefs.gameDirectory, m_UserData.mission.levelDetails.techTreeName);
			foreach (TechData tech in technologies)
			{
				m_TechNames.Add(tech.techID, tech.techName);
				m_TechIds.Add(tech.techID);
			}
		}

		private void PopulatePlayerList()
		{
			m_PlayerListBox.Clear();

			for (int i=0; i < m_Players.Count; ++i)
			{
				ListBoxItem item = ListBoxItem.Create(m_PlayerListPrefab, "Player " + (i+1));
				item.userData = m_Players[i];
				m_PlayerListBox.AddItem(item);
			}

			// Can't have more than 6 players in the game
			if (m_Players.Count >= 6)
				m_BtnAddPlayer.interactable = false;
		}

		public void OnClick_AddPlayer()
		{
			m_Players.Add(new PlayerData(m_Players.Count));
			Save();

			PopulatePlayerList();
		}

		public void OnClick_RemovePlayer()
		{
			m_Players.RemoveAt(m_PlayerListBox.selectedIndex);
			Save();

			m_BtnRemovePlayer.interactable = false;

			PopulatePlayerList();
		}

		private void OnSelect_Player(ListBoxItem item)
		{
			PlayerData player = (PlayerData)item.userData;

			m_CanSave = false;

			m_SelectedPlayer = player;

			if (m_Players.Count > 1)
				m_BtnRemovePlayer.interactable = true;

			m_DropdownColonyType.value				= player.isEden ? 0 : 1;
			m_DropdownColor.value					= (int)player.color;
			if (player.isHuman)
				m_DropdownControlType.value			= player.botType == BotType.None ? 0 : 2;
			else
				m_DropdownControlType.value			= 1; // OP2 AI
			m_InputTechLevel.text					= player.techLevel.ToString();
			m_DropdownMoraleLevel.value				= (int)player.moraleLevel;
			m_ToggleFreeMorale.isOn					= player.freeMorale;

			m_Allies								= new List<int>(player.allies);
			m_CompletedTech							= new List<int>(player.completedResearch);

			PopulateAllyList();
			PopulateCompletedTechList();

			m_InputSolarSats.text					= player.solarSatellites.ToString();
			m_InputKids.text						= player.kids.ToString();
			m_InputWorkers.text						= player.workers.ToString();
			m_InputScientists.text					= player.scientists.ToString();
			m_InputCommonMetal.text					= player.commonOre.ToString();
			m_InputRareMetal.text					= player.rareOre.ToString();
			m_InputFood.text						= player.food.ToString();

			m_CanSave = true;
		}

		private void PopulateAllyList()
		{
			m_AllyListBox.Clear();

			// Populate ally list
			for (int i=0; i < m_Allies.Count; ++i)
			{
				ListBoxItem item = ListBoxItem.Create(m_AllyListPrefab, "Player " + (m_Allies[i]+1));
				item.userData = i;
				m_AllyListBox.AddItem(item);
			}

			// Fill ally dropdown with unallied players
			List<string> unalliedPlayers = new List<string>();
			foreach (PlayerData player in m_Players)
			{
				if (m_Allies.Contains(player.id)) continue;
				if (m_SelectedPlayer.id == player.id) continue;

				unalliedPlayers.Add("Player " + (player.id+1).ToString());
			}

			m_DropdownAlly.ClearOptions();
			m_DropdownAlly.AddOptions(unalliedPlayers);
			m_DropdownAlly.interactable = m_DropdownAlly.options.Count > 0;

			m_BtnAddAlly.interactable = m_DropdownAlly.options.Count > 0;
		}

		private void PopulateCompletedTechList()
		{
			m_CompletedTechListBox.Clear();

			// Populate completed tech list
			for (int i=0; i < m_CompletedTech.Count; ++i)
			{
				ListBoxItem item = ListBoxItem.Create(m_CompletedTechListPrefab, m_CompletedTech[i] + " - " + m_TechNames[m_CompletedTech[i]]);
				item.userData = i;
				m_CompletedTechListBox.AddItem(item);
			}

			// Fill completed tech dropdown with incomplete tech
			List<string> incompleteTech = new List<string>();
			for (int i=0; i < m_TechIds.Count; ++i)
			{
				if (m_CompletedTech.Contains(m_TechIds[i]))
					continue;

				incompleteTech.Add(m_TechIds[i] + " - " + m_TechNames[m_TechIds[i]]);
			}

			m_DropdownCompletedTech.ClearOptions();
			m_DropdownCompletedTech.AddOptions(incompleteTech);
			m_DropdownCompletedTech.interactable = m_DropdownCompletedTech.options.Count > 0;

			m_BtnAddCompletedTech.interactable = m_DropdownCompletedTech.options.Count > 0;
		}

		public void OnClick_AddAlly()
		{
			// Parse selected dropdown option to be added to ally list
			string selectedOption = m_DropdownAlly.options[m_DropdownAlly.value].text;
			selectedOption = selectedOption.Substring("Player ".Length);
			int allyId = int.Parse(selectedOption)-1;

			m_Allies.Add(allyId);
			Save();

			PopulateAllyList();
		}

		public void OnClick_AddCompletedTech()
		{
			// Parse selected dropdown option to be added to completed tech list
			string selectedOption = m_DropdownCompletedTech.options[m_DropdownCompletedTech.value].text;
			selectedOption = selectedOption.Substring(0, selectedOption.IndexOf('-') - 1);
			int techId = int.Parse(selectedOption);

			m_CompletedTech.Add(techId);
			Save();

			PopulateCompletedTechList();
		}

		public void OnClick_RemoveAlly()
		{
			m_Allies.RemoveAt(m_AllyListBox.selectedIndex);
			Save();

			PopulateAllyList();

			m_BtnRemoveAlly.interactable = false;
		}

		public void OnClick_RemoveCompletedTech()
		{
			m_CompletedTech.RemoveAt(m_CompletedTechListBox.selectedIndex);
			Save();

			PopulateCompletedTechList();

			m_BtnRemoveCompletedTech.interactable = false;
		}

		private void OnSelect_Ally(ListBoxItem item)
		{
			m_BtnRemoveAlly.interactable = true;
		}

		private void OnSelect_CompletedTech(ListBoxItem item)
		{
			m_BtnRemoveCompletedTech.interactable = true;
		}

		public void Save()
		{
			if (!m_CanSave)
				return;

			m_UserData.mission.players				= m_Players.ToArray();

			m_UserData.mission.levelDetails.numPlayers = m_Players.Count;

			if (m_SelectedPlayer == null)
				return;

			m_SelectedPlayer.isEden					= m_DropdownColonyType.value == 0;
			m_SelectedPlayer.color					= (PlayerColor)m_DropdownColor.value;
			m_SelectedPlayer.isHuman				= m_DropdownControlType.value != 1; // OP2 AI
			m_SelectedPlayer.botType				= m_DropdownControlType.value == 0 ? BotType.None : BotType.Balanced;
			m_SelectedPlayer.techLevel				= GetValueFromInputField(m_InputTechLevel, "Tech Level", m_SelectedPlayer.techLevel);
			m_SelectedPlayer.moraleLevel			= (MoraleLevel)m_DropdownMoraleLevel.value;
			m_SelectedPlayer.freeMorale				= m_ToggleFreeMorale.isOn;

			m_SelectedPlayer.allies					= m_Allies.ToArray();
			m_SelectedPlayer.completedResearch		= m_CompletedTech.ToArray();

			m_SelectedPlayer.solarSatellites		= GetValueFromInputField(m_InputSolarSats, "Solar Satellites", m_SelectedPlayer.solarSatellites);
			m_SelectedPlayer.kids					= GetValueFromInputField(m_InputKids, "Kids", m_SelectedPlayer.kids);
			m_SelectedPlayer.workers				= GetValueFromInputField(m_InputWorkers, "Workers", m_SelectedPlayer.workers);
			m_SelectedPlayer.scientists				= GetValueFromInputField(m_InputScientists, "Scientists", m_SelectedPlayer.scientists);
			m_SelectedPlayer.commonOre				= GetValueFromInputField(m_InputCommonMetal, "Common Metal", m_SelectedPlayer.commonOre);
			m_SelectedPlayer.rareOre				= GetValueFromInputField(m_InputRareMetal, "Rare Metal", m_SelectedPlayer.rareOre);
			m_SelectedPlayer.food					= GetValueFromInputField(m_InputFood, "Food", m_SelectedPlayer.food);
		}

		private int GetValueFromInputField(InputField input, string fieldName, int originalValue)
		{
			int val;
			if (int.TryParse(input.text, out val))
				return val;
			else
			{
				Debug.Log("Bad value assigned to " + fieldName + ".");
				input.text = originalValue.ToString();
			}

			return originalValue;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCloseCB?.Invoke();
		}

		private void OnDestroy()
		{
			m_PlayerListBox.onSelectedItemCB -= OnSelect_Player;
			m_AllyListBox.onSelectedItemCB -= OnSelect_Ally;
			m_CompletedTechListBox.onSelectedItemCB -= OnSelect_CompletedTech;
		}


		/// <summary>
		/// Creates and presents the Player Properties dialog to the user.
		/// </summary>
		/// <param name="userData">The user data to display and modify.</param>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static PlayerPropertiesDialog Create(UserData userData, OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/PlayerPropertiesDialog"));
			PlayerPropertiesDialog dialog = goDialog.GetComponent<PlayerPropertiesDialog>();
			dialog.Initialize(userData, onCloseCB);

			return dialog;
		}
	}
}
