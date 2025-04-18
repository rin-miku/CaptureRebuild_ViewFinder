using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Photo : ItemBase
{
    public MeshRenderer meshRenderer;
    public InstantCamera instantCamera;
    public CaptureRebuild captureRebuild;
    public Transform photoMod;

    private Sequence inSequence;
    private Sequence outSequence;

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
            RebuildPhoto();
        }
    }

    public override void ShowAnimation(float duration = 1f)
    {
        instantCamera.HideAnimation();
        photoMod.DOLocalMoveY(pointerValues[2].position.y, duration).OnComplete(() => isShow = true);
    }

    public override void HideAnimation(float duration = 1f)
    {
        isShow = false;
        photoMod.DOLocalMoveY(pointerValues[0].position.y, duration);
    }

    public override void InAnimation(float duration = 1f)
    {
        outSequence?.Kill();

        inSequence = DOTween.Sequence();
        inSequence
            .Join(photoMod.DOLocalMove(pointerValues[1].position, duration))
            .Join(photoMod.DOLocalRotate(pointerValues[1].rotation, duration))
            .Join(photoMod.DOScale(pointerValues[1].scale, duration))
            .OnComplete(() => isReady = true);
    }

    public override void OutAnimation(float duration = 1f)
    {
        inSequence?.Kill();

        outSequence = DOTween.Sequence();
        outSequence
            .OnStart(() => isReady = false)
            .Join(photoMod.DOLocalMove(pointerValues[2].position, duration))
            .Join(photoMod.DOLocalRotate(pointerValues[2].rotation, duration))
            .Join(photoMod.DOScale(pointerValues[2].scale, duration));
    }

    public void SetPhotoTexture(Texture2D texture)
    {
        captureRebuild.Capture();
        meshRenderer.materials[0].SetTexture("_BaseMap", texture);
        StartCoroutine(ShowAnimationDelay());
    }

    private IEnumerator ShowAnimationDelay()
    {
        yield return new WaitForSeconds(1f);
        ShowAnimation();
    }

    private void RebuildPhoto()
    {
        captureRebuild.Rebuild();

        OutAnimation(0f);
        HideAnimation(0f); 
    }
}
