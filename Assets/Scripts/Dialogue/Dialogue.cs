using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
	public class Dialogue : ScriptableObject
	{
		[SerializeField]
		private List<DialogueNode> nodes = new List<DialogueNode>();

		Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

#if UNITY_EDITOR
		private void Awake()
		{
			DialogueNode rootNode = new DialogueNode();
			rootNode.uniqueID = Guid.NewGuid().ToString();
			nodes.Add(rootNode);

			// Build�� exe���� ������ �ѹ��� ����
			OnValidate();
		}
#endif
		// Scirptable Object ���� �� �߻��ϴ� �̺�Ʈ
		// Build�� exe������ �������� ����!
		private void OnValidate()
		{
			nodeLookup.Clear();
			foreach (DialogueNode node in GetAllNodes())
			{
				nodeLookup[node.uniqueID] = node;
			}
		}

		public IEnumerable<DialogueNode> GetAllNodes()
		{
			return nodes;
		}

		public DialogueNode GetRootNode()
		{
			return nodes[0];
		}

		public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
		{
			// yield return ����� �� ���� �Ѱ��ش�.
			// �ϼ��ؼ� �Ѱ��ִ� �� ���� �����Ƿ�, Rendering���� �κп��� ����ϱ� ����.
			foreach (string childID in parentNode.children)
			{
				if (nodeLookup.ContainsKey(childID))
				{
					yield return nodeLookup[childID];
				}
			}
		}

		public void CreateNode(DialogueNode parentNode)
		{
			DialogueNode newNode = new DialogueNode();
			newNode.uniqueID = Guid.NewGuid().ToString();
			newNode.context = "Context Area";
			parentNode.children.Add(newNode.uniqueID);
			nodes.Add(newNode);
			OnValidate();
		}

		public void DeleteNode(DialogueNode nodeToDelete)
		{
			nodes.Remove(nodeToDelete);
			OnValidate();
			CleanDanglingChildren(nodeToDelete);
		}

		private void CleanDanglingChildren(DialogueNode nodeToDelete)
		{
			foreach (DialogueNode node in GetAllNodes())
			{
				node.children.Remove(nodeToDelete.uniqueID);
			}
		}
	}
}
