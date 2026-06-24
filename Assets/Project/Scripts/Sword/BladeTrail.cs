using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BladeTrail : MonoBehaviour
{
    [SerializeField]
    private SwordSwing swordSwing;

    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private float trailTime = 0.12f;

    [SerializeField]
    private float startWidth = 0.25f;

    [SerializeField]
    private float endWidth = 0.0f;

    private bool wasAttacking;

    private void Awake()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        if (swordSwing == null)
        {
            swordSwing = GetComponentInParent<SwordSwing>();
        }

        ApplyTrailSettings();
        SetTrailVisible(false, true);
    }

    private void OnValidate()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        ApplyTrailSettings();
    }

    private void LateUpdate()
    {
        if (swordSwing == null || trailRenderer == null)
        {
            return;
        }

        SetTrailVisible(swordSwing.IsAttacking, false);
    }

    private void ApplyTrailSettings()
    {
        if (trailRenderer == null)
        {
            return;
        }

        trailRenderer.time = trailTime;
        trailRenderer.startWidth = startWidth;
        trailRenderer.endWidth = endWidth;
    }

    private void SetTrailVisible(bool isAttacking, bool forceClear)
    {
        if (trailRenderer == null)
        {
            return;
        }

        if (isAttacking)
        {
            if (!wasAttacking || forceClear)
            {
                trailRenderer.Clear();
            }

            trailRenderer.emitting = true;
        }
        else
        {
            trailRenderer.emitting = false;

            if (wasAttacking || forceClear)
            {
                trailRenderer.Clear();
            }
        }

        wasAttacking = isAttacking;
    }
}
