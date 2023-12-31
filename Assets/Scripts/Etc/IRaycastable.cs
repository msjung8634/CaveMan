using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public enum CursorType
{
    Default,
    Dialogue,
    Shop,
    Hookable,
}

public interface IRaycastable
{
    CursorType GetCursorType();
    bool HandleRaycast(PlayerControl playerControl);
}
