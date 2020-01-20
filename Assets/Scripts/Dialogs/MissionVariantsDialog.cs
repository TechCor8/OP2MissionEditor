using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionEditor.Systems.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionEditor.Dialogs
{
	/// <summary>
	/// Presents editor mission properties to the user.
	/// </summary>
	public class MissionVariantsDialog : MonoBehaviour
	{
		[SerializeField] private Dropdown	m_DropdownMissionVariant	= default;
		[SerializeField] private InputField	m_InputVariantName			= default;
		[SerializeField] private Button		m_BtnAddVariant				= default;
		[SerializeField] private Button		m_BtnRemoveVariant			= default;

		[SerializeField] private Dropdown	m_DropdownDifficulty		= default;
		[SerializeField] private Button		m_BtnAddDifficulty			= default;
		[SerializeField] private Button		m_BtnRemoveDifficulty		= default;

		private UnitRenderer m_UnitRenderer;

		private bool m_ShouldSave;
		

		private void Initialize(UnitRenderer unitRenderer)
		{
			m_UnitRenderer = unitRenderer;

			UserData.current.onChangedValuesCB += OnChanged_UserData;

			// Initialize display
			m_DropdownMissionVariant.value	= UserData.current.selectedVariantIndex;
			m_InputVariantName.text			= UserData.current.selectedVariant.name;
			m_InputVariantName.interactable = UserData.current.selectedVariantIndex > 0;

			m_DropdownDifficulty.value		= UserData.current.selectedDifficultyIndex;

			RefreshVariantDropdown();
			RefreshDifficultyDropdown();

			m_ShouldSave = true;
		}

		private void OnChanged_UserData(UserData src)
		{
			m_ShouldSave = false;

			RefreshVariantDropdown();
			RefreshDifficultyDropdown();

			m_ShouldSave = true;
		}

		private void RefreshVariantDropdown()
		{
			int selectedIndex = UserData.current.selectedVariantIndex;

			// Populate mission variant options
			List<string> variantOptions = new List<string>();

			foreach (MissionVariant variant in UserData.current.mission.missionVariants)
				variantOptions.Add(variant.name);

			variantOptions[0] = "All Variants";

			m_DropdownMissionVariant.ClearOptions();
			m_DropdownMissionVariant.AddOptions(variantOptions);

			// Reset selected index
			if (selectedIndex >= variantOptions.Count)
				selectedIndex = variantOptions.Count-1;

			m_DropdownMissionVariant.value = selectedIndex;
			
			m_BtnAddVariant.interactable = UserData.current.mission.missionVariants.Count < 8;
			m_BtnRemoveVariant.interactable = UserData.current.mission.missionVariants.Count >= 2 && m_DropdownMissionVariant.value > 0;
		}

		private void RefreshDifficultyDropdown()
		{
			int selectedIndex = UserData.current.selectedDifficultyIndex;

			// Populate difficulty options
			List<string> difficultyOptions = new List<string>();

			for (int i=0; i < UserData.current.selectedVariant.players[0].difficulties.Count; ++i)
			{
				switch (i)
				{
					case 0:		difficultyOptions.Add("Easy");		break;
					case 1:		difficultyOptions.Add("Normal");	break;
					case 2:		difficultyOptions.Add("Hard");		break;
				}
			}

			m_DropdownDifficulty.ClearOptions();
			m_DropdownDifficulty.AddOptions(difficultyOptions);

			// Reset selected index
			if (selectedIndex >= difficultyOptions.Count)
				selectedIndex = difficultyOptions.Count-1;

			m_DropdownDifficulty.value = selectedIndex;
			
			m_BtnAddDifficulty.interactable = difficultyOptions.Count < 3;
			m_BtnRemoveDifficulty.interactable = difficultyOptions.Count > 1;
		}

		public void OnClick_AddVariant()
		{
			// Clone selected index unless it is the "all variants" index.
			if (UserData.current.selectedVariantIndex > 0)
				UserData.current.AddMissionVariant(UserData.current.selectedVariantIndex);
			else
				UserData.current.AddMissionVariant();

			RefreshVariantDropdown();

			// Select the newly added variant
			m_DropdownMissionVariant.value = UserData.current.mission.missionVariants.Count-1;
		}

		public void OnClick_RemoveVariant()
		{
			if (UserData.current.selectedVariantIndex == 0)
				return;

			UserData.current.RemoveMissionVariant(UserData.current.selectedVariantIndex);

			RefreshVariantDropdown();

			// Select the last variant
			//m_DropdownMissionVariant.value = UserData.current.mission.missionVariants.Count-1;
		}

		public void OnChanged_VariantName()
		{
			if (!m_ShouldSave) return;
			if (UserData.current.selectedVariantIndex == 0) return;

			UserData.current.selectedVariant.name = m_InputVariantName.text;
			UserData.current.SetUnsaved();

			RefreshVariantDropdown();
		}

		public void OnSelect_Variant()
		{
			UserData.current.SetSelectedVariant(m_DropdownMissionVariant.value);

			m_ShouldSave = false;
			m_InputVariantName.text = UserData.current.selectedVariant.name;
			m_ShouldSave = true;

			m_InputVariantName.interactable = UserData.current.selectedVariantIndex > 0;
			m_BtnRemoveVariant.interactable = UserData.current.mission.missionVariants.Count >= 2 && m_DropdownMissionVariant.value > 0;

			m_UnitRenderer.Refresh();
		}

		public void OnClick_AddDifficulty()
		{
			UserData.current.AddDifficulty(UserData.current.selectedDifficultyIndex);

			RefreshDifficultyDropdown();

			// Select the newly added difficulty
			m_DropdownDifficulty.value = UserData.current.selectedVariant.players[0].difficulties.Count-1;
		}

		public void OnClick_RemoveDifficulty()
		{
			UserData.current.RemoveDifficulty(UserData.current.selectedVariantIndex);

			RefreshDifficultyDropdown();

			// Select the last variant
			//m_DropdownMissionVariant.value = UserData.current.mission.missionVariants.Count-1;
		}

		public void OnSelect_Difficulty()
		{
			UserData.current.SetSelectedDifficulty(m_DropdownDifficulty.value);

			m_UnitRenderer.Refresh();
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			UserData.current.onChangedValuesCB -= OnChanged_UserData;
		}


		/// <summary>
		/// Creates and presents the Mission Variants dialog to the user.
		/// </summary>
		public static MissionVariantsDialog Create(UnitRenderer unitRenderer)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/MissionVariantsDialog"));
			MissionVariantsDialog dialog = goDialog.GetComponent<MissionVariantsDialog>();
			dialog.Initialize(unitRenderer);

			return dialog;
		}
	}
}
