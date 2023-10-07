using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks; // Editor Mode���� Asset�� Ŭ���ϴ� ��� Callback �߻�

namespace Dialogue
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;

        // Window/Dialogue Editor �߰�
        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
		{
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
		}

        // DialougeSO ���� Ŭ�� �� Dialogue Editor Window ����
        [OnOpenAsset()]
        public static bool OnOpenAsset(int instanceID, int line)
		{
			// ������ Asset�� Object������ �����´�. (Object�� �ٷ� �������ų� / InstanceID�� ���� Object�� �����´�)
			Dialogue dialogue = Selection.activeObject as Dialogue;
            //Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
			if (dialogue != null)
			{
                ShowEditorWindow();
                return true;
			}
            return false;
		}

        // DialogueSO Ŭ�� �� Callback �߻�
		private void OnSelectionChange()
		{
            Dialogue newDialogue = Selection.activeObject as Dialogue;
			if (newDialogue != null)
			{
                selectedDialogue = newDialogue;
                // Dialogue Editor Window ����
                Repaint();
			}
		}

        // ��� ȣ��
		private void OnGUI()
		{
			if (selectedDialogue == null)
			{
                EditorGUILayout.LabelField("No Dialogue Selected.");
			}
			else
			{
				foreach (DialogueNode node in selectedDialogue.GetAllNodes())
				{
					OnGUINode(node);
				}
			}
		}

		private void OnGUINode(DialogueNode node)
		{
			// Rect�� �׸���
			GUILayout.BeginArea(new Rect(10, 10, 200, 200));

			// Undo/Redo�� ���� ������� üũ ���� (Ctrl+Z / Ctrl+Y)
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Node :");
			string newID = EditorGUILayout.TextField(node.uniqueID);
			string newText = EditorGUILayout.TextField(node.context);

			// Undo/Redo�� ���� ������� üũ ����
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(selectedDialogue, "Update Dialogue Text");
				node.uniqueID = newID;
				node.context = newText;

				// Dialogue Editor Window���� ������ ������ ���� SO�� �����ϱ� ���� DirtyFlag ����
				EditorUtility.SetDirty(selectedDialogue);
			}

			GUILayout.EndArea();
		}
	}
}
