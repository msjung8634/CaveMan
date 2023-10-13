using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dialogue;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class DialogueUI : MonoBehaviour
    {
        private PlayerConversant playerConversant;
		[SerializeField]
		private TextMeshProUGUI speakerNameText;
		[SerializeField]
        private TextMeshProUGUI aiText;
		[SerializeField]
		private Button quitButton;
		[SerializeField]
		private Button nextButton;
		[SerializeField]
		private GameObject aiResponse;
		[SerializeField]
		private Transform choiceRoot;
		[SerializeField]
		private GameObject choicePrefab;

		private void Awake()
		{
            GameObject.FindGameObjectWithTag("Player").TryGetComponent(out playerConversant);
		}

		private void Start()
		{
			playerConversant.onConversationUpdated += UpdateUI;
			quitButton.onClick.AddListener(() => playerConversant.Quit());
			nextButton.onClick.AddListener(() => playerConversant.GetNext());

			UpdateUI();
		}

		private void UpdateUI()
		{
			gameObject.SetActive(playerConversant.IsActive);
			if (!playerConversant.IsActive)
				return;

			speakerNameText.text = playerConversant.IsChoosing ? "¸²°ü¿µ" : "NPC";

			aiResponse.SetActive(!playerConversant.IsChoosing);
			choiceRoot.gameObject.SetActive(playerConversant.IsChoosing);

			if (playerConversant.IsChoosing)
			{
				BuildChoiceList();
			}
			else
			{
				aiText.text = playerConversant.GetFirst();
				nextButton.gameObject.SetActive(playerConversant.HasNext());
			}
		}

		private void BuildChoiceList()
		{
			foreach (Transform item in choiceRoot)
			{
				Destroy(item.gameObject);
			}
			foreach (DialogueNode choice in playerConversant.GetChoices())
			{
				GameObject choiceInstance = Instantiate(choicePrefab, choiceRoot);
				var textComponent = choiceInstance.GetComponentInChildren<TextMeshProUGUI>();
				textComponent.text = choice.Context;

				Button button = choiceInstance.GetComponentInChildren<Button>();
				button.onClick.AddListener(() =>
				{
					playerConversant.SelectChoice(choice);
				});
			}
		}
	}
}
