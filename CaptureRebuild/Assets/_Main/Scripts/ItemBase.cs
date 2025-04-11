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

    public abstract void ShowAnimation();
    public abstract void HideAnimation();
    public abstract void InAnimation();
    public abstract void OutAnimation();
}
