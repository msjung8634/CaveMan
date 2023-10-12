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
#if UNITY_EDITOR
                // context 저장하면 Undo를 할 수 있도록 함
                Undo.RecordObject(this, "Change Dialogue Speaker");
#endif
                _isPlayerSpeaking = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                    // context 저장하면 Undo를 할 수 있도록 함
                    Undo.RecordObject(this, "Update Dialogue Text");
#endif
                    _context = value;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }


        public void SetPosition(Vector2 newPosition)
		{
#if UNITY_EDITOR
            Undo.RecordObject(this, "Move Dialogue Node");
#endif
            Rect rect = Rect;
            rect.position = newPosition;
            Rect = rect;
        }

        public void AddChild(string childID)
		{
#if UNITY_EDITOR
            Undo.RecordObject(this, "Add Dialogue Link");
#endif
            Children.Add(childID);
        }

        public void RemoveChild(string childID)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Remove Dialogue Link");
#endif
            Children.Remove(childID);
        }
    }
}
