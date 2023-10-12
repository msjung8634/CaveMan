using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Player;

namespace Dialogue
{
	[RequireComponent(typeof(PlayerStateMachine))]
	public class PlayerConversant : MonoBehaviour
	{
		[SerializeField]
		Dialogue testDialogue = null;
		[SerializeField]
		Dialogue currentDialogue = null;
		[SerializeField]
		DialogueNode currentNode = null;
		public bool IsActive { get; private set; } = false;
		public bool IsChoosing { get; set; } = false;

		// 대화가 변경되면 모든 callback 실행
		public event Action onConversationUpdated;

		private PlayerStateMachine playerStateMachine;

        private void Awake()
        {
			TryGetComponent(out playerStateMachine);
        }

        public void StartDialogue(Dialogue newDialogue)
		{
			playerStateMachine.IsTalking = true;
			playerStateMachine.SetControlState(FSM.ControlState.Uncontrollable);
			playerStateMachine.SetHitState(FSM.HitState.Unhittable);

			currentDialogue = newDialogue;
			currentNode = currentDialogue.GetRootNode();
			IsActive = true;
			TriggerEnterAction();
			onConversationUpdated();
		}

		public void Quit()
		{
			playerStateMachine.IsTalking = false;
			playerStateMachine.SetControlState(FSM.ControlState.Controllable);
			playerStateMachine.SetHitState(FSM.HitState.Hittable);

			currentDialogue = null;
			currentNode = null;
			IsChoosing = false;
			IsActive = false;
			TriggerExitAction();
			onConversationUpdated();
		}

		public string GetFirst()
		{
			if (currentNode == null)
				return "";

			return currentNode.Context;
		}

		public IEnumerable<DialogueNode> GetChoices()
		{
			return currentDialogue.GetPlayerChildren(currentNode);
		}

		public void SelectChoice(DialogueNode chosenNode)
		{
			currentNode = chosenNode;
			GetNext();
		}

		public void GetNext()
		{
			IsChoosing = false;
			int playerResponsesCount = currentDialogue.GetPlayerChildren(currentNode).Count() ;
			if (playerResponsesCount > 0)
			{
				IsChoosing = true;
				TriggerExitAction();
				onConversationUpdated();
				return;
			}

			DialogueNode[] children = currentDialogue.GetAIChildren(currentNode).ToArray();
			if (children.Length > 0)
			{
				int index = UnityEngine.Random.Range(0, children.Length);
				TriggerEnterAction();
				currentNode = children[index];
				TriggerExitAction();
				onConversationUpdated();
			}
		}

		public bool HasNext()
		{
			return currentNode.Children.Count > 0;
		}

		private void TriggerEnterAction()
		{
			if (currentNode == null || currentNode.OnEnterAction == "")
				return;

			Debug.Log(currentNode.OnEnterAction);
		}

		private void TriggerExitAction()
		{
			if (currentNode == null || currentNode.OnExitAction == "")
				return;

			Debug.Log(currentNode.OnExitAction);

			////////////////////////////////////
			if (currentNode.OnExitAction == "StartQuest")
			{
				Quit();
				QuestUI.SetActive(true);
			}
			////////////////////////////////////
		}

		[SerializeField]
		private GameObject QuestUI;
		private void Start()
		{
			 QuestUI.SetActive(false);
		}
	}
}
