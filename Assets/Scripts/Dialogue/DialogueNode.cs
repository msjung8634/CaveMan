using System.Collections;
using System.Collections.Generic;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string uniqueID;
        public string context;
        public string[] children;
    }
}
