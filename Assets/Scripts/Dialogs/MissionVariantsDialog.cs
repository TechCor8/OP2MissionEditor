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

		
		private void Initialize(UnitRenderer unitRenderer)
		{
			m_UnitRenderer = unitRenderer;

			UserData.current.onChangedValuesCB += OnChanged_UserData;

			// Initialize display
			m_DropdownMissionVariant.value	= UserData.current.selectedVariantIndex >= 0 ? UserData.current.selectedVariantIndex+1 : 0;
			m_InputVariantName.SetTextWithoutNotify(UserData.current.selectedVariant.name);
			m_InputVariantName.interactable = UserData.current.selectedVariantIndex >= 0;

			m_DropdownDifficulty.value		= UserData.current.selectedDifficultyIndex >= 0 ? UserData.current.selectedDifficultyIndex+1 : 0;

			RefreshVariantDropdown();
			RefreshDifficultyDropdown();
		}

		private void OnChanged_UserData(UserData src)
		{
			RefreshVariantDropdown();
			RefreshDifficultyDropdown();
		}

		private void RefreshVariantDropdown()
		{
			int selectedIndex = m_DropdownMissionVariant.value;

			// Populate mission variant options
			List<string> variantOptions = new List<string>();

			foreach (MissionVariant variant in UserData.current.mission.missionVariants)
				variantOptions.Add(variant.name);

			variantOptions.Insert(0, "All Variants");

			m_DropdownMissionVariant.ClearOptions();
			m_DropdownMissionVariant.AddOptions(variantOptions);

			// Reset selected index
			if (selectedIndex >= variantOptions.Count)
				selectedIndex = variantOptions.Count-1;

			m_DropdownMissionVariant.SetValueWithoutNotify(selectedIndex);
			OnSelect_Variant();
			
			m_BtnAddVariant.interactable = UserData.current.mission.missionVariants.Count < 8;
			m_BtnRemoveVariant.interactable = UserData.current.mission.missionVariants.Count > 0 && m_DropdownMissionVariant.value > 0;
		}

		private void RefreshDifficultyDropdown()
		{
			int selectedIndex = m_DropdownDifficulty.value;

			// Populate difficulty options
			List<string> difficultyOptions = new List<string>();
			difficultyOptions.Add("All Difficulties");

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

			m_DropdownDifficulty.SetValueWithoutNotify(selectedIndex);
			OnSelect_Difficulty();
			
			m_BtnAddDifficulty.interactable = difficultyOptions.Count < 4;
			m_BtnRemoveDifficulty.interactable = difficultyOptions.Count > 1;
		}

		public void OnClick_AddVariant()
		{
			// Clone selected index unless it is the "all variants" index.
			if (UserData.current.selectedVariantIndex >= 0)
				UserData.current.AddMissionVariant(UserData.current.selectedVariantIndex);
			else
				UserData.current.AddMissionVariant();

			RefreshVariantDropdown();

			// Select the newly added variant
			m_DropdownMissionVariant.value = UserData.current.mission.missionVariants.Count;
		}

		public void OnClick_RemoveVariant()
		{
			if (UserData.current.selectedVariantIndex < 0)
				return;

			UserData.current.RemoveMissionVariant(UserData.current.selectedVariantIndex);

			RefreshVariantDropdown();
		}

		public void OnChanged_VariantName()
		{
			if (UserData.current.selectedVariantIndex < 0) return;

			UserData.current.selectedVariant.name = m_InputVariantName.text;
			UserData.current.SetUnsaved();

			RefreshVariantDropdown();
		}

		public void OnSelect_Variant()
		{
			UserData.current.SetSelectedVariant(m_DropdownMissionVariant.value-1);

			m_InputVariantName.SetTextWithoutNotify(UserData.current.selectedVariant.name);
			
			m_InputVariantName.interactable = UserData.current.selectedVariantIndex >= 0;
			m_BtnRemoveVariant.interactable = UserData.current.mission.missionVariants.Count > 0 && m_DropdownMissionVariant.value > 0;

			m_UnitRenderer.Refresh();
		}

		public void OnClick_AddDifficulty()
		{
			// Clone selected index unless it is the "all difficulties" index.
			if (UserData.current.selectedDifficultyIndex >= 0)
				UserData.current.AddDifficulty(UserData.current.selectedDifficultyIndex);
			else
				UserData.current.AddDifficulty();

			RefreshDifficultyDropdown();

			// Select the newly added difficulty
			m_DropdownDifficulty.value = UserData.current.selectedVariant.players[0].difficulties.Count;
		}

		public void OnClick_RemoveDifficulty()
		{
			if (UserData.current.selectedDifficultyIndex < 0)
				return;

			UserData.current.RemoveDifficulty(UserData.current.selectedDifficultyIndex);

			RefreshDifficultyDropdown();
		}

		public void OnSelect_Difficulty()
		{
			UserData.current.SetSelectedDifficulty(m_DropdownDifficulty.value-1);

			m_BtnRemoveDifficulty.interactable = m_DropdownDifficulty.options.Count > 1 && m_DropdownDifficulty.value > 0;

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
