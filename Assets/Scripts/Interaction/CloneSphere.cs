﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using Leap;
using Leap.Unity;
using DG.Tweening;

public class CloneSphere : InteractionEventReceiver
{
    protected InteractionEventHoverSender hoverSender;
    public Transform highlightSphere;
    [Range(0, 100F)]
    public float lerpSpeed = 10f;
    public float shrinkTime = 5f;
    public Vector3 highlightScale = new Vector3(0.1f, 0.1f, 0.1f);

    protected Vector3 originalScale;
    protected Vector3 originalPosition;

    protected InteractionHand handRight, handLeft;
    Transform leftPinchPosition, rightPinchPosition;

    protected virtual void Start()
    {
        hoverSender = (InteractionEventHoverSender)sender;
        handLeft = Game.Instance.handLeft;
        handRight = Game.Instance.handRight;

        originalScale = highlightSphere.localScale;
        originalPosition = highlightSphere.position;

        startOpacity = meshRenderer.sharedMaterial.GetFloat("_Alpha");
    }

    private bool countDownActive = false;
    private IEnumerator coroutine;
    private void Update()
    {
        if (hoverSender.hovered && hoverSender.isClosestHandHolding())
        {
            highlightSphere.position = Vector3.Lerp(highlightSphere.position, hoverSender.closestHandPinchPosition, lerpSpeed * Time.deltaTime);

            if (coroutine == null && highlightSphere.localScale == highlightScale)
            {
                coroutine = Shrink(shrinkTime);
                StartCoroutine(coroutine);
            }

            if (coroutine == null)
            {
                highlightSphere.localScale = Vector3.Lerp(highlightSphere.localScale, highlightScale, lerpSpeed * Time.deltaTime);
            }

        }
        else
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                OnCountdownAbort("update");
            }

            highlightSphere.position = Vector3.Lerp(highlightSphere.position, originalPosition, lerpSpeed * Time.deltaTime);

            if (coroutine == null)
            {
                highlightSphere.localScale = Vector3.Lerp(highlightSphere.localScale, originalScale, lerpSpeed * Time.deltaTime);
            }
        }
    }

    protected virtual void OnCountdownFinished() 
    {
        Debug.Log("Stop: make clone");
    }

    protected virtual void OnCountdownAbort(string location) 
    {
        Debug.Log("Stop: no clone " + location);
    }

    IEnumerator Shrink(float time)
    {
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
        {
            highlightSphere.localScale = Vector3.Lerp(highlightSphere.localScale, originalScale * 2f, t);

            if (t > 0.32f && coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                OnCountdownFinished();
            }
            else if (!hoverSender.hovered && coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                OnCountdownAbort("hover distance");
            }
            else
            {
                yield return null;
            }
        }
    }

    protected override void OnHoldingBegin()
    {
        SetAlpha(targetOpacity);
    }

    protected override void OnHoldingEnd()
    {
        SetAlpha(startOpacity);
    }

    //protected override void OnHoldingSustain()
    //{
    //}

    protected virtual void SetAlpha(float to, float t = 0.15f)
    {
        if (meshRenderer.material.HasProperty("_Alpha"))
        {
            float currentAlpha = meshRenderer.sharedMaterial.GetFloat("_Alpha");
            Tween tween = DOTween.To(() => { return currentAlpha; }, x => { currentAlpha = x; }, to, t);
            tween.OnUpdate(() =>
            {
                meshRenderer.sharedMaterial.SetFloat("_Alpha", currentAlpha);
            });
        }
    }

}