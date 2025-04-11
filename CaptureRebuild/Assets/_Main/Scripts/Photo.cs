using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Photo : ItemBase
{
    public MeshRenderer meshRenderer;
    public InstantCamera instantCamera;

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
    }

    public override void ShowAnimation()
    {
        instantCamera.HideAnimation();
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
            .Join(transform.DOLocalMove(pointerValues[1].position, 1f))
            .Join(transform.DOLocalRotate(pointerValues[1].rotation, 1f))
            .Join(transform.DOScale(pointerValues[1].scale, 1f))
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
            .Join(transform.DOScale(pointerValues[2].scale, 1f));
    }

    public void SetPhotoTexture(Texture2D texture)
    {
        meshRenderer.materials[0].SetTexture("_BaseMap", texture);
        Invoke("ShowAnimation", 1f);
    }
}
