using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks; // Editor Mode���� Asset�� Ŭ���ϴ� ��� Callback �߻�
using System;

namespace Dialogue
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;
		[NonSerialized]
		GUIStyle nonPlayerNodeStyle;
		[NonSerialized]
		GUIStyle playerNodeStyle;
		[NonSerialized]
		DialogueNode draggingNode = null;
		[NonSerialized]
		Vector2 draggingOffset;
		// Editor Window������ ���������� ��� Field�� Serialize�Ѵ�.
		// ��, Ư�� ���� ���� Inspector�� �� ���� �־�� ��ó�� �����Ѵ�.
		// ��带 �߰��ϰ� ���� creatingNode != null�� �ǹǷ�, ��ݾ��� ��尡 �ϳ� �� �����.
		// �̸� �����ϱ� ���ؼ��� NonSerialized�� �ݵ�� ����ؾ� �Ѵ�.
		[NonSerialized]
		DialogueNode creatingNode = null;
		[NonSerialized]
		DialogueNode deletingNode = null;
		[NonSerialized]
		DialogueNode linkingParentNode = null;

		Vector2 scrollPosition;
		[NonSerialized]
		bool isDraggingCanvas = false;
		[NonSerialized]
		Vector2 draggingCanvasOffset;

		const float canvasSize = 4000;
		// backgrond image size (50x50)
		const float backgroundTextureSize = 50;

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

		private void OnEnable()
		{
			// DialogueSO Ŭ������ Callback �߻� �� ó���� Event �߰�
			Selection.selectionChanged += OnSelectionChange;

			// Unity�⺻ ���� background�� GUIStyle ����
			nonPlayerNodeStyle = new GUIStyle();
			nonPlayerNodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
			nonPlayerNodeStyle.normal.textColor = Color.white;
			nonPlayerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
			nonPlayerNodeStyle.border = new RectOffset(12, 12, 12, 12);

			playerNodeStyle = new GUIStyle();
			playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
			playerNodeStyle.normal.textColor = Color.white;
			playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
			playerNodeStyle.border = new RectOffset(12, 12, 12, 12);
		}

		
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
			if (Event.current.type == EventType.ValidateCommand
				&& Event.current.commandName == "UndoRedoPerformed")
			{
				Repaint();
			}

			if (selectedDialogue == null)
			{
                EditorGUILayout.LabelField("No Dialogue Selected.");
			}
			else
			{
				ProcessMouseDragEvents();

				// Auto Layout�� �͸� �ν��� scroll bar�� �����Ѵ�.
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

				Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
				// Editor/Resources�� �ִ� file�� load
			 	Texture2D backgroundTexture = Resources.Load("background") as Texture2D;
				Rect textureCoordinates = new Rect(0, 0, canvasSize/backgroundTextureSize, canvasSize / backgroundTextureSize);
				// Grid(Tile)�� ���·� �׷��ش�
				// DrawTexture�� �Ѱ��� �� �׷��ش�
				GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textureCoordinates);

				// ��带 ������ �ʵ��� ���ἱ�� �ڿ� �׸���.
				foreach (DialogueNode node in selectedDialogue.GetAllNodes())
				{
					DrawConnections(node);
				}
				// ���� �������� �ʵ��� �տ� �׸���.
				foreach (DialogueNode node in selectedDialogue.GetAllNodes())
				{
					DrawNode(node);
				}

				EditorGUILayout.EndScrollView();

				if (creatingNode != null)
				{
					selectedDialogue.CreateNode(creatingNode);
					creatingNode = null;
				}
				if (deletingNode != null)
				{
					selectedDialogue.DeleteNode(deletingNode);
					deletingNode = null;
				}
			}
		}

		private void DrawConnections(DialogueNode node)
		{
			Vector3 startPos = new Vector2(node.Rect.xMax, node.Rect.center.y);
			foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
			{
				Vector3 endPos = new Vector2(childNode.Rect.xMin, childNode.Rect.center.y);
				Vector3 controlPointOffest = endPos - startPos;
				controlPointOffest.y = 0;
				controlPointOffest.x *= 0.8f;
				// �������� 4���� Bezier Curve
				Handles.DrawBezier(startPos, endPos, startPos + controlPointOffest, endPos - controlPointOffest, Color.gray, null, 4f);
			}
		}

		private void ProcessMouseDragEvents()
		{
			// Left MouseDown
			if (Event.current.type == EventType.MouseDown && draggingNode == null
				&& Event.current.button == 0)
			{
				// scrollPosition�� �߰��� Node�� ��ġ�� ����Ѵ�.
				// Node�� ScrollView �ȿ��� �����Ӱ� �̵���Ų��.
				draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
				if (draggingNode != null)
				{
					draggingOffset = draggingNode.Rect.position - Event.current.mousePosition;
					Selection.activeObject = draggingNode;
				}
				else
				{
					// Record dragOffset and dragging
					isDraggingCanvas = true;
					draggingCanvasOffset = scrollPosition + Event.current.mousePosition;
					Selection.activeObject = selectedDialogue;
				}
			}
			// Left MouseDrag with Node
			else if (Event.current.type == EventType.MouseDrag && draggingNode != null
					 && Event.current.button == 0)
			{
				draggingNode.SetPosition(Event.current.mousePosition + draggingOffset);
				Repaint();
			}
			// Left MouseDrag without Node
			else if (Event.current.type == EventType.MouseDrag && isDraggingCanvas
					 && Event.current.button == 0)
			{
				// Update ScrollPosition
				scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
				Repaint();
			}
			// Left MouseUp with Node
			else if (Event.current.type == EventType.MouseUp && draggingNode != null
					 && Event.current.button == 0)
			{
				draggingNode = null;
			}
			// Left MouseUp without Node
			else if (Event.current.type == EventType.MouseUp && isDraggingCanvas
					 && Event.current.button == 0)
			{
				isDraggingCanvas = false;
			}
		}

		private void DrawNode(DialogueNode node)
		{

			GUIStyle style = nonPlayerNodeStyle;
			if (node.IsPlayerSpeaking)
			{
				style = playerNodeStyle;
			}

			GUILayout.BeginArea(node.Rect, style);

			node.Context = EditorGUILayout.TextField(node.Context);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("-"))
			{
				deletingNode = node;
			}
			DrawLinkButtons(node);
			if (GUILayout.Button("+"))
			{
				creatingNode = node;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}

		private void DrawLinkButtons(DialogueNode node)
		{
			// ���� �� �ƴϸ�
			if (linkingParentNode == null)
			{
				if (GUILayout.Button("link"))
				{
					linkingParentNode = node;
				}
			}
			// ���� �� �̰�, �θ� ����̸�
			else if (node == linkingParentNode)
			{
				if (GUILayout.Button("done"))
				{
					linkingParentNode = null;
				}
			}
			// ���� �� �̰�, �θ� ��� �ƴϸ�
			else
			{
				// �̹� ����� ���¸�
				if (linkingParentNode.Children.Contains(node.name))
				{
					if (GUILayout.Button("unlink"))
					{
						linkingParentNode.RemoveChild(node.name);
					}
				}
				// ������� ���� ���¸�
				else
				{
					if (GUILayout.Button("child"))
					{
						linkingParentNode.AddChild(node.name);
					}
				}
			}
		}

		private DialogueNode GetNodeAtPoint(Vector2 point)
		{
			DialogueNode foundNode = null;
			foreach (DialogueNode node in selectedDialogue.GetAllNodes())
			{
				if (node.Rect.Contains(point))
				{
					foundNode = node;
					return node;
				}
			}

			return foundNode;
		}
	}
}
