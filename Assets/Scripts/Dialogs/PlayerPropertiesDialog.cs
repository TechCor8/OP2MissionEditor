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

		private OnCloseCallback m_OnCloseCB;

		private bool m_CanSave;

		// Available Technologies
		private Dictionary<int, string> m_TechNames = new Dictionary<int, string>();
		private List<int> m_TechIds = new List<int>();

		// Selected player parameters
		private PlayerData m_SelectedPlayer;
		private List<int> m_Allies;
		private List<int> m_CompletedTech;


		private void Initialize(OnCloseCallback onCloseCB)
		{
			m_OnCloseCB = onCloseCB;

			UserData.current.onSelectVariantCB += OnChanged_SelectedVariant;

			// Initialize display
			m_PlayerListBox.onSelectedItemCB += OnSelect_Player;
			m_AllyListBox.onSelectedItemCB += OnSelect_Ally;
			m_CompletedTechListBox.onSelectedItemCB += OnSelect_CompletedTech;

			PopulatePlayerList();

			ReadTechFile();
			
			m_CanSave = true;
		}

		private void OnChanged_SelectedVariant(UserData src)
		{
			m_CanSave = false;

			PopulatePlayerList();

			m_CanSave = true;
		}

		private void ReadTechFile()
		{
			// Read technology from tech sheet
			m_TechNames.Clear();
			m_TechIds.Clear();

			TechData[] technologies = TechFileReader.GetTechnologies(UserPrefs.gameDirectory, UserData.current.mission.levelDetails.techTreeName);
			foreach (TechData tech in technologies)
			{
				m_TechNames.Add(tech.techID, tech.techName);
				m_TechIds.Add(tech.techID);
			}
		}

		private void PopulatePlayerList()
		{
			int selectedIndex = m_PlayerListBox.selectedIndex;

			m_PlayerListBox.Clear();

			for (int i=0; i < UserData.current.selectedVariant.players.Count; ++i)
			{
				ListBoxItem item = ListBoxItem.Create(m_PlayerListPrefab, "Player " + (i+1));
				item.userData = UserData.current.selectedVariant.players[i];
				m_PlayerListBox.AddItem(item);
			}

			m_PlayerListBox.selectedIndex = selectedIndex;

			// Can't have more than 6 players in the game
			if (UserData.current.selectedVariant.players.Count >= 6)
				m_BtnAddPlayer.interactable = false;
		}

		public void OnClick_AddPlayer()
		{
			PlayerData player = new PlayerData(UserData.current.selectedVariant.players.Count);

			// Make sure difficulty count is sync'd to other players
			player.difficulties.Clear();
			foreach (PlayerData.ResourceData resData in UserData.current.selectedVariant.players[0].difficulties)
				player.difficulties.Add(new PlayerData.ResourceData());

			UserData.current.AddPlayer(player);
			Save();

			PopulatePlayerList();
		}

		public void OnClick_RemovePlayer()
		{
			UserData.current.RemovePlayer(m_PlayerListBox.selectedIndex);
			Save();

			m_BtnRemovePlayer.interactable = false;

			PopulatePlayerList();
		}

		private void OnSelect_Player(ListBoxItem item)
		{
			PlayerData player = (PlayerData)item.userData;
			PlayerData.ResourceData playerResData = player.difficulties[UserData.current.selectedDifficultyIndex];

			m_CanSave = false;

			m_SelectedPlayer = player;

			if (UserData.current.selectedVariant.players.Count > 1)
				m_BtnRemovePlayer.interactable = true;

			m_DropdownColonyType.value				= player.isEden ? 0 : 1;
			m_DropdownColor.value					= (int)player.color;
			if (player.isHuman)
				m_DropdownControlType.value			= player.botType == BotType.None ? 0 : 2;
			else
				m_DropdownControlType.value			= 1; // OP2 AI
			m_InputTechLevel.text					= playerResData.techLevel.ToString();
			m_DropdownMoraleLevel.value				= (int)playerResData.moraleLevel;
			m_ToggleFreeMorale.isOn					= playerResData.freeMorale;

			m_Allies								= new List<int>(player.allies);
			m_CompletedTech							= new List<int>(playerResData.completedResearch);

			PopulateAllyList();
			PopulateCompletedTechList();

			m_InputSolarSats.text					= playerResData.solarSatellites.ToString();
			m_InputKids.text						= playerResData.kids.ToString();
			m_InputWorkers.text						= playerResData.workers.ToString();
			m_InputScientists.text					= playerResData.scientists.ToString();
			m_InputCommonMetal.text					= playerResData.commonOre.ToString();
			m_InputRareMetal.text					= playerResData.rareOre.ToString();
			m_InputFood.text						= playerResData.food.ToString();

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
			foreach (PlayerData player in UserData.current.selectedVariant.players)
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

			UserData.current.mission.levelDetails.numPlayers = UserData.current.selectedVariant.players.Count;

			if (m_SelectedPlayer == null)
			{
				UserData.current.Dirty();
				return;
			}

			PlayerData.ResourceData playerResData = m_SelectedPlayer.difficulties[UserData.current.selectedDifficultyIndex];

			m_SelectedPlayer.isEden					= m_DropdownColonyType.value == 0;
			m_SelectedPlayer.color					= (PlayerColor)m_DropdownColor.value;
			m_SelectedPlayer.isHuman				= m_DropdownControlType.value != 1; // OP2 AI
			m_SelectedPlayer.botType				= m_DropdownControlType.value == 0 ? BotType.None : BotType.Balanced;
			playerResData.techLevel					= GetValueFromInputField(m_InputTechLevel, "Tech Level", playerResData.techLevel);
			playerResData.moraleLevel				= (MoraleLevel)m_DropdownMoraleLevel.value;
			playerResData.freeMorale				= m_ToggleFreeMorale.isOn;

			m_SelectedPlayer.allies					= m_Allies.ToArray();
			playerResData.completedResearch			= m_CompletedTech.ToArray();

			playerResData.solarSatellites			= GetValueFromInputField(m_InputSolarSats, "Solar Satellites", playerResData.solarSatellites);
			playerResData.kids						= GetValueFromInputField(m_InputKids, "Kids", playerResData.kids);
			playerResData.workers					= GetValueFromInputField(m_InputWorkers, "Workers", playerResData.workers);
			playerResData.scientists				= GetValueFromInputField(m_InputScientists, "Scientists", playerResData.scientists);
			playerResData.commonOre					= GetValueFromInputField(m_InputCommonMetal, "Common Metal", playerResData.commonOre);
			playerResData.rareOre					= GetValueFromInputField(m_InputRareMetal, "Rare Metal", playerResData.rareOre);
			playerResData.food						= GetValueFromInputField(m_InputFood, "Food", playerResData.food);

			UserData.current.Dirty();
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

			UserData.current.onSelectVariantCB -= OnChanged_SelectedVariant;
		}


		/// <summary>
		/// Creates and presents the Player Properties dialog to the user.
		/// </summary>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static PlayerPropertiesDialog Create(OnCloseCallback onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/PlayerPropertiesDialog"));
			PlayerPropertiesDialog dialog = goDialog.GetComponent<PlayerPropertiesDialog>();
			dialog.Initialize(onCloseCB);

			return dialog;
		}
	}
}
