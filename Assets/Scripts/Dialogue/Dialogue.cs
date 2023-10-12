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

		// Scirptable Object ���� �� �߻��ϴ� �̺�Ʈ
		// Build�� exe������ �������� ����!
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
			// yield return ����� �� ���� �Ѱ��ش�.
			// �ϼ��ؼ� �Ѱ��ִ� �� ���� �����Ƿ�, Rendering���� �κп��� ����ϱ� ����.
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
			// �����ϸ� Undo�� �� �� �ֵ��� ��
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
					// DialogueNode�� AssetDB�� ������� ���� ���
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
