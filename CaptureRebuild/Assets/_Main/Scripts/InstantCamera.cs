using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class InstantCamera : MonoBehaviour
{
    public Volume volume;
    public Vector3 startPointer;
    public Vector3 endPointer;
    public Vector3 startRotation;

    private CameraBlurVolumeComponent blurVolume;
    private Sequence inSequence;
    private Sequence outSequence;

    private void Start()
    {
        blurVolume = VolumeManager.instance.stack.GetComponent<CameraBlurVolumeComponent>();
        blurVolume.blurSize.value = 6f;
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
            .Join(transform.DOLocalMove(endPointer, 1f))
            .Join(transform.DOLocalRotate(Vector3.zero, 1f))
            .Join(transform.DOScale(Vector3.one, 1f))
            .Join(DOVirtual.Float(volume.weight, 1f, 1f, value => { volume.weight = value; }));
    }

    private void OutAnimation()
    {
        inSequence?.Kill();

        outSequence = DOTween.Sequence();
        outSequence
            .Join(transform.DOLocalMove(startPointer, 1f))
            .Join(transform.DOLocalRotate(startRotation, 1f))
            .Join(transform.DOScale(Vector3.one * 0.5f, 1f))
            .Join(DOVirtual.Float(volume.weight, 0f, 1f, value => { volume.weight = value; }));
    }

    private void OnDestroy()
    {
        inSequence?.Kill();
        outSequence?.Kill();
    }

    private void Test()
    {
        volume.weight = 0.6f;
    }
}
