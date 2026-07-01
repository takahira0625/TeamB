using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BladeTrail : MonoBehaviour
{
    [SerializeField]
    private SwordSwing swordSwing;

    [SerializeField]
    private TrailRenderer trailRenderer;

    // 軌跡の時間・太さをまとめたSwordParamsアセット(剣スクリプトと同じものを割り当てる)
    [SerializeField]
    private SwordParams param;

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
        if (trailRenderer == null || param == null)
        {
            return;
        }

        trailRenderer.time = param.trailTime;
        trailRenderer.startWidth = param.trailStartWidth;
        trailRenderer.endWidth = param.trailEndWidth;
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
