using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PointerValue
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

public abstract class ItemBase : MonoBehaviour
{
    public bool isShow;
    public bool isReady;
    public List<PointerValue> pointerValues;

    public abstract void ShowAnimation(float duration = 1f);
    public abstract void HideAnimation(float duration = 1f);
    public abstract void InAnimation(float duration = 1f);
    public abstract void OutAnimation(float duration = 1f);
}
