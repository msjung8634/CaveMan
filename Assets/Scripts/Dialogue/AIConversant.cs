using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	public class AIConversant : MonoBehaviour, IRaycastable
	{
		[SerializeField]
		private Dialogue dialogue = null;

		public CursorType GetCursorType()
		{
			return CursorType.Dialogue;
		}

		public bool HandleRaycast(PlayerControl playerControl)
		{
			if (dialogue == null)
				return false;

			if (Input.GetMouseButtonDown(0))
			{
				playerControl.TryGetComponent(out PlayerConversant conversant);
				conversant.StartDialogue(dialogue);
				return true;
			}

			return false;
		}
	}
}
