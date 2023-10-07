using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
	public class Dialogue : ScriptableObject
	{
		[SerializeField]
		private List<DialogueNode> nodes = null;

#if UNITY_EDITOR
		private void Awake()
		{
			if (nodes == null)
			{
				nodes = new List<DialogueNode>();
				nodes.Add(new DialogueNode());
			}
		}
#endif

		public IEnumerable<DialogueNode> GetAllNodes()
		{
			return nodes;
		}
	}
}
