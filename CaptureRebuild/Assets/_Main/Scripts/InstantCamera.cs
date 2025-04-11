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
            TakePhoto();
        }
    }

    public override void ShowAnimation()
    {
        photo.HideAnimation();
        transform.DOLocalMoveY(pointerValues[2].position.y, 1f).OnComplete(() => isShow = true);
    }

    public override void HideAnimation()
    {
        isShow = false;
        transform.DOLocalMoveY(pointerValues[0].position.y, 1f);
    }

    public override void InAnimation()
    {
        outSequence?.Kill();

        inSequence = DOTween.Sequence();
        inSequence
            .OnStart(() => blurVolume.enableCameraBlur.value = true)
            .Join(transform.DOLocalMove(pointerValues[1].position, 1f))
            .Join(transform.DOLocalRotate(pointerValues[1].rotation, 1f))
            .Join(transform.DOScale(pointerValues[1].scale, 1f))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 55f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 10f, 1f, value => { blurVolume.blurSize.value = value; }))
            .OnComplete(() => isReady = true);
    }

    public override void OutAnimation()
    {
        inSequence?.Kill();

        outSequence = DOTween.Sequence();
        outSequence
            .OnStart(() => isReady = false)
            .Join(transform.DOLocalMove(pointerValues[2].position, 1f))
            .Join(transform.DOLocalRotate(pointerValues[2].rotation, 1f))
            .Join(transform.DOScale(pointerValues[2].scale, 1f))
            .Join(DOVirtual.Float(sceneCamera.fieldOfView, 60f, 0.7f, value => { sceneCamera.fieldOfView = value; }).SetDelay(0.3f))
            .Join(DOVirtual.Float(blurVolume.blurSize.value, 0f, 1f, value => { blurVolume.blurSize.value = value; }))
            .OnComplete(() => blurVolume.enableCameraBlur.value = false);
    }

    private void TakePhoto()
    {
        RenderTexture targetTexture = viewCamera.targetTexture;
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = viewCamera.targetTexture;
        int width = viewCamera.targetTexture.width;
        int height = viewCamera.targetTexture.height;

        Texture2D photoTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        photoTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        photoTexture.Apply();
        RenderTexture.active = activeRT;

        photo.SetPhotoTexture(photoTexture);
        OutAnimation();
    }

    private void OnDestroy()
    {
        inSequence?.Kill();
        outSequence?.Kill();
    }
}
