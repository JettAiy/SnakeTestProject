using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LeanAnimation : MonoBehaviour
{
    [SerializeField] private float animationSacleTime;
    [SerializeField] private Vector3 animationScale;

    [SerializeField] private bool scaleAnimationOnStart = true;
    [SerializeField] private bool rotationAnimationOnStart = true;

    private void Start()
    {
        if (scaleAnimationOnStart)
            ScaleAnimation(false);

        if (rotationAnimationOnStart)
            RotationAnimation(false);
    }

    public void ScaleAnimation(bool clear = false)
    {
        if (clear) LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, animationScale, animationSacleTime).setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
    }

    public void RotationAnimation(bool clear = false)
    {
        if (clear) LeanTween.cancel(gameObject);

        LeanTween.rotateZ(gameObject, -15, animationSacleTime).setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
    }

    public void ScaleAnimationOneTime()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, animationScale, animationSacleTime).setOnComplete(() => { LeanTween.scale(gameObject, new Vector3(0.5f, 0.5f, 1f), animationSacleTime/2);});
    }

    public void DissapearAnimation(System.Action OnDissapearAction)
    {
        LeanTween.cancel(gameObject);

        Vector3 currentScale = transform.localScale;

        LeanTween.scale(gameObject, Vector3.zero, animationSacleTime).setOnComplete(()=> { transform.localScale = currentScale; });
        LeanTween.rotateAround(gameObject, new Vector3(0, 0, 1), 360, animationSacleTime).setOnComplete(OnDissapearAction);
    }
}
