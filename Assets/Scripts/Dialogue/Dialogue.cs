using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
	public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		Vector2 newNodeOffset = new Vector2(250, 0);

		[SerializeField]
		private List<DialogueNode> nodes = new List<DialogueNode>();

		Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();


		private void Awake()
		{
			OnValidate();
		}

		// Scirptable Object 변경 시 발생하는 이벤트
		// Build된 exe에서는 동작하지 않음!
		private void OnValidate()
		{
			if (nodes.Count == 0)
			{
				CreateNode(null);
			}
			else
			{
				nodeLookup.Clear();
				foreach (DialogueNode node in GetAllNodes())
				{
					nodeLookup.Add(node.name, node);
				}
			}
		}

		public IEnumerable<DialogueNode> GetAllNodes()
		{
			return nodes;
		}

		public IEnumerable<DialogueNode> GetPlayerChildren(DialogueNode currentNode)
		{
			foreach (DialogueNode childNode in GetAllChildren(currentNode))
			{
				if (childNode.IsPlayerSpeaking)
				{
					yield return childNode;
				}
			}
		}

		public IEnumerable<DialogueNode> GetAIChildren(DialogueNode currentNode)
		{
			foreach (DialogueNode childNode in GetAllChildren(currentNode))
			{
				if (!childNode.IsPlayerSpeaking)
				{
					yield return childNode;
				}
			}
		}

		public DialogueNode GetRootNode()
		{
			return nodes[0];
		}

		public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
		{
			// yield return 사용해 한 개씩 넘겨준다.
			// 완성해서 넘겨주는 것 보다 빠르므로, Rendering관련 부분에서 사용하기 좋다.
			foreach (string childID in parentNode.Children)
			{
				if (nodeLookup.ContainsKey(childID))
				{
					yield return nodeLookup[childID];
				}
			}
		}

		public void CreateNode(DialogueNode parentNode)
		{

			DialogueNode newNode = MakeNode(parentNode);
#if UNITY_EDITOR
			// 저장하면 Undo를 할 수 있도록 함
			Undo.RegisterCreatedObjectUndo(newNode, "Created Dialouge Node");
			Undo.RecordObject(this, "Added Dialouge Node");
#endif
			AddNode(newNode);
		}

		public void DeleteNode(DialogueNode nodeToDelete)
		{
#if UNITY_EDITOR
			Undo.RecordObject(this, "Deleted Dialouge Node");
#endif
			nodes.Remove(nodeToDelete);
			OnValidate();
			CleanDanglingChildren(nodeToDelete);
#if UNITY_EDITOR
			Undo.DestroyObjectImmediate(nodeToDelete);
#endif
		}

		private DialogueNode MakeNode(DialogueNode parentNode)
		{
			DialogueNode newNode = CreateInstance<DialogueNode>();
			newNode.name = Guid.NewGuid().ToString();
			newNode.Context = "Context Area";

			if (parentNode != null)
			{
				parentNode.Children.Add(newNode.name);
				newNode.IsPlayerSpeaking = !parentNode.IsPlayerSpeaking;
				newNode.SetPosition(parentNode.Rect.position + newNodeOffset);
			}

			return newNode;
		}

		private void AddNode(DialogueNode newNode)
		{
			nodes.Add(newNode);
			OnValidate();
		}

		private void CleanDanglingChildren(DialogueNode nodeToDelete)
		{
			foreach (DialogueNode node in GetAllNodes())
			{
				node.Children.Remove(nodeToDelete.name);
			}
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			if (AssetDatabase.GetAssetPath(this) != "")
			{
				foreach (DialogueNode node in GetAllNodes())
				{
					// DialogueNode가 AssetDB에 저장되지 않은 경우
					if (AssetDatabase.GetAssetPath(node) == "")
					{
						AssetDatabase.AddObjectToAsset(node, this);
					}
				}
			}
#endif
		}

		public void OnAfterDeserialize()
		{
			
		}
	}
}
