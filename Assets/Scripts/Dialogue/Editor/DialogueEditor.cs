using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks; // Editor Mode에서 Asset을 클릭하는 경우 Callback 발생
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
		// Editor Window에서는 내부적으로 모든 Field를 Serialize한다.
		// 즉, 특정 값이 들어가면 Inspector에 그 값을 넣어둔 것처럼 동작한다.
		// 노드를 추가하고 나면 creatingNode != null이 되므로, 뜬금없이 노드가 하나 더 생긴다.
		// 이를 방지하기 위해서는 NonSerialized를 반드시 명시해야 한다.
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

		private void OnEnable()
		{
			// DialogueSO 클릭으로 Callback 발생 시 처리할 Event 추가
			Selection.selectionChanged += OnSelectionChange;

			// Unity기본 제공 background로 GUIStyle 설정
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
                // Dialogue Editor Window 갱신
                Repaint();
			}
		}

        // 계속 호출
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

				// Auto Layout인 것만 인식해 scroll bar를 생성한다.
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

				Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
				// Editor/Resources에 있는 file을 load
			 	Texture2D backgroundTexture = Resources.Load("background") as Texture2D;
				Rect textureCoordinates = new Rect(0, 0, canvasSize/backgroundTextureSize, canvasSize / backgroundTextureSize);
				// Grid(Tile)의 형태로 그려준다
				// DrawTexture는 한개만 딱 그려준다
				GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textureCoordinates);

				// 노드를 가리지 않도록 연결선을 뒤에 그린다.
				foreach (DialogueNode node in selectedDialogue.GetAllNodes())
				{
					DrawConnections(node);
				}
				// 노드는 가려지지 않도록 앞에 그린다.
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
				// 조절점이 4개인 Bezier Curve
				Handles.DrawBezier(startPos, endPos, startPos + controlPointOffest, endPos - controlPointOffest, Color.gray, null, 4f);
			}
		}

		private void ProcessMouseDragEvents()
		{
			// Left MouseDown
			if (Event.current.type == EventType.MouseDown && draggingNode == null
				&& Event.current.button == 0)
			{
				// scrollPosition을 추가해 Node의 위치를 계산한다.
				// Node를 ScrollView 안에서 자유롭게 이동시킨다.
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
			// 연결 중 아니면
			if (linkingParentNode == null)
			{
				if (GUILayout.Button("link"))
				{
					linkingParentNode = node;
				}
			}
			// 연결 중 이고, 부모 노드이면
			else if (node == linkingParentNode)
			{
				if (GUILayout.Button("done"))
				{
					linkingParentNode = null;
				}
			}
			// 연결 중 이고, 부모 노드 아니면
			else
			{
				// 이미 연결된 상태면
				if (linkingParentNode.Children.Contains(node.name))
				{
					if (GUILayout.Button("unlink"))
					{
						linkingParentNode.RemoveChild(node.name);
					}
				}
				// 연결되지 않은 상태면
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
