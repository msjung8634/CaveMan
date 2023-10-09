using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dialogue
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField]
        private bool _isPlayerSpeaking = false;
        public bool IsPlayerSpeaking
        {
            get
            {
                return _isPlayerSpeaking;
            }
            set
            {
                // context 저장하면 Undo를 할 수 있도록 함
                Undo.RecordObject(this, "Change Dialogue Speaker");
                _isPlayerSpeaking = value;
                EditorUtility.SetDirty(this);
            }
        }
		[TextArea(6, 40)]
        [SerializeField]
        private string _context;
        public string Context
        {
            get
            {
                return _context;
            }
            set
            {
				if (_context != value)
				{
                    // context 저장하면 Undo를 할 수 있도록 함
                    Undo.RecordObject(this, "Update Dialogue Text");
                    _context = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        [SerializeField]
        private List<string> _children = new List<string>();
        public List<string> Children
        {
			get
			{
                return _children;
			}
            private set
            {
                _children = value;
                EditorUtility.SetDirty(this);
            }
        }
        [SerializeField]
        private Rect _rect = new Rect(0, 0, 300, 120);
        public Rect Rect
        {
            get
            {
                return _rect;
            }
            set
            {
                _rect = value;
                EditorUtility.SetDirty(this);
            }
        }

        [SerializeField]
        private string _onEnterAction = "";
        public string OnEnterAction
		{
			get
            {
                return _onEnterAction;
            }
            set
            {
                _onEnterAction = value;
                EditorUtility.SetDirty(this);
            }
        }

        [SerializeField]
        private string _onExitAction = "";
        public string OnExitAction
        {
            get
            {
                return _onExitAction;
            }
            set
            {
                _onExitAction = value;
                EditorUtility.SetDirty(this);
            }
        }

#if UNITY_EDITOR
        public void SetPosition(Vector2 newPosition)
		{
            Undo.RecordObject(this, "Move Dialogue Node");
            Rect rect = Rect;
            rect.position = newPosition;
            Rect = rect;
		}

        public void AddChild(string childID)
		{
            Undo.RecordObject(this, "Add Dialogue Link");
            Children.Add(childID);
		}

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            Children.Remove(childID);
        }
#endif
	}
}
