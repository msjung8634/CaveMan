using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks; // Editor Mode에서 Asset을 클릭하는 경우 Callback 발생

namespace Dialogue
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;

        // Window/Dialogue Editor 추가
        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
		{
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
		}

        // DialougeSO 더블 클릭 시 Dialogue Editor Window 켜짐
        [OnOpenAsset()]
        public static bool OnOpenAsset(int instanceID, int line)
		{
			// 선택한 Asset의 Object정보를 가져온다. (Object를 바로 가져오거나 / InstanceID를 통해 Object를 가져온다)
			Dialogue dialogue = Selection.activeObject as Dialogue;
            //Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
			if (dialogue != null)
			{
                ShowEditorWindow();
                return true;
			}
            return false;
		}

        // DialogueSO 클릭 시 Callback 발생
		private void OnSelectionChange()
		{
            Dialogue newDialogue = Selection.activeObject as Dialogue;
			if (newDialogue != null)
			{
                selectedDialogue = newDialogue;
                // Dialogue Editor Window 갱신
                Repaint();
			}
		}

        // 계속 호출
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
			// Rect를 그린다
			GUILayout.BeginArea(new Rect(10, 10, 200, 200));

			// Undo/Redo를 위해 변경사항 체크 시작 (Ctrl+Z / Ctrl+Y)
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Node :");
			string newID = EditorGUILayout.TextField(node.uniqueID);
			string newText = EditorGUILayout.TextField(node.context);

			// Undo/Redo를 위해 변경사항 체크 종료
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(selectedDialogue, "Update Dialogue Text");
				node.uniqueID = newID;
				node.context = newText;

				// Dialogue Editor Window에서 변경한 사항을 실제 SO에 적용하기 위해 DirtyFlag 설정
				EditorUtility.SetDirty(selectedDialogue);
			}

			GUILayout.EndArea();
		}
	}
}
