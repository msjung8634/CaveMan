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

			// Build된 exe에서 강제로 한번은 실행
			OnValidate();
		}
#endif
		// Scirptable Object 변경 시 발생하는 이벤트
		// Build된 exe에서는 동작하지 않음!
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
			// yield return 사용해 한 개씩 넘겨준다.
			// 완성해서 넘겨주는 것 보다 빠르므로, Rendering관련 부분에서 사용하기 좋다.
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
