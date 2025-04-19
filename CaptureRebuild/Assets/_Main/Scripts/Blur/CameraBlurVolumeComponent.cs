using System;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/CameraBlur")]
public class CameraBlurVolumeComponent : VolumeComponent
{
    public BoolParameter enableCameraBlur = new BoolParameter(false);
    public ClampedFloatParameter blurSize = new ClampedFloatParameter(0f, 0f, 5f);
}
