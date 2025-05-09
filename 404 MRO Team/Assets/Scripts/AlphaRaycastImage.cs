using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class AlphaRaycastImage : Image
{
    [SerializeField, Range(0f, 1f)]
    private float alphaThreshold = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        this.alphaHitTestMinimumThreshold = alphaThreshold;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.alphaHitTestMinimumThreshold = alphaThreshold;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        this.alphaHitTestMinimumThreshold = alphaThreshold;
    }
#endif
}
