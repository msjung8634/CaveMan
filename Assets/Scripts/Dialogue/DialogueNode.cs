using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string uniqueID;
        [TextArea(6, 40)]
        public string context;
        public List<string> children = new List<string>();
        public Rect rect;

		public DialogueNode()
		{
            rect = new Rect(0, 0, 200, 100);
		}
    }
}
