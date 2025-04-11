using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class PointerValue
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

public class InstantCamera : MonoBehaviour
{
    public Camera sceneCamera;
    public Volume volume;
    public List<PointerValue> pointerValues;

    private CameraBlurVolumeComponent blurVolume;
    private Sequence inSequence;
    private Sequence outSequence;

    private void Start()
    {
        volume.profile.TryGet(out blurVolume);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            InAnimation();
        }

        if (Input.GetMouseButtonUp(1))
        {
            OutAnimation();
        }
    }

    private void InAnimation()
    {
        outSequence?.Kill();

        inSequence = DOTween.Sequence();
        inSequence
            .OnStart(() => blurVolume.enableCameraBlur.value = true)
            .Join(transform.DOLocalMove(pointerValues[1].position, 1f))
            .Join(transform.DOLocalRotate(pointerValues[1].rotation, 1f))
            .Join(transform.DOScale(pointerValues[1].scale, 1f))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 55f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 10f, 1f, value => { blurVolume.blurSize.value = value; }));
    }

    private void OutAnimation()
    {
        inSequence?.Kill();

        outSequence = DOTween.Sequence();
        outSequence
            .Join(transform.DOLocalMove(pointerValues[0].position, 1f))
            .Join(transform.DOLocalRotate(pointerValues[0].rotation, 1f))
            .Join(transform.DOScale(pointerValues[0].scale, 1f))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 60f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 0f, 1f, value => { blurVolume.blurSize.value = value; }))
            .OnComplete(() => blurVolume.enableCameraBlur.value = false);
    }

    private void OnDestroy()
    {
        inSequence?.Kill();
        outSequence?.Kill();
    }
}
