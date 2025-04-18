using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class InstantCamera : ItemBase
{
    public Volume volume;
    public Camera sceneCamera;
    public Camera viewCamera;
    public Photo photo;

    private CameraBlurVolumeComponent blurVolume;
    private Sequence inSequence;
    private Sequence outSequence;

    private void Start()
    {
        volume.profile.TryGet(out blurVolume);

        ShowAnimation();
    }

    private void Update()
    {
        if (isShow && Input.GetMouseButtonDown(1))
        {
            InAnimation();
        }

        if (isShow && Input.GetMouseButtonUp(1))
        {
            OutAnimation();
        }

        if (isReady && Input.GetMouseButtonDown(0))
        {
            CapturePhoto();
        }
    }

    public override void ShowAnimation(float duration = 1f)
    {
        photo.HideAnimation();
        transform.DOLocalMoveY(pointerValues[2].position.y, duration).OnComplete(() => isShow = true);
    }

    public override void HideAnimation(float duration = 1f)
    {
        isShow = false;
        transform.DOLocalMoveY(pointerValues[0].position.y, duration);
    }

    public override void InAnimation(float duration = 1f)
    {
        outSequence?.Kill();

        inSequence = DOTween.Sequence();
        inSequence
            .OnStart(() => blurVolume.enableCameraBlur.value = true)
            .Join(transform.DOLocalMove(pointerValues[1].position, duration))
            .Join(transform.DOLocalRotate(pointerValues[1].rotation, duration))
            .Join(transform.DOScale(pointerValues[1].scale, duration))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 55f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 1.5f, duration, value => { blurVolume.blurSize.value = value; }))
            .OnComplete(() => isReady = true);
    }

    public override void OutAnimation(float duration = 1f)
    {
        inSequence?.Kill();

        outSequence = DOTween.Sequence();
        outSequence
            .OnStart(() => isReady = false)
            .Join(transform.DOLocalMove(pointerValues[2].position, duration))
            .Join(transform.DOLocalRotate(pointerValues[2].rotation, duration))
            .Join(transform.DOScale(pointerValues[2].scale, duration))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 60f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 0f, duration, value => { blurVolume.blurSize.value = value; }))
            .OnComplete(() => blurVolume.enableCameraBlur.value = false);
    }

    private void CapturePhoto()
    {
        Texture2D photoTexture = CommonTools.GetCameraTexture(viewCamera);

        photo.SetPhotoTexture(photoTexture);

        OutAnimation();
    }

    private void OnDestroy()
    {
        inSequence?.Kill();
        outSequence?.Kill();
    }
}
