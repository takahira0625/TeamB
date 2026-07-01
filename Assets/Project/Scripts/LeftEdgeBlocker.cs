using UnityEngine;

public class LeftEdgeBlocker : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    // 調整値をまとめたCameraParamsアセット(左端オフセット・余白)
    [SerializeField]
    private CameraParams param;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null || param == null)
        {
            return;
        }

        Vector3 playerPosition = transform.position;

        float depth = Vector3.Dot(
            playerPosition - targetCamera.transform.position,
            targetCamera.transform.forward
        );

        if (depth <= 0f)
        {
            return;
        }

        Vector3 leftEdgeWorldPosition = targetCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, depth)
        );

        // プレイヤーの「中心座標」がこれより左に行かないようにする
        float minCenterX = leftEdgeWorldPosition.x + param.leftOffsetFromCenter + param.edgePadding;

        if (playerPosition.x < minCenterX)
        {
            playerPosition.x = minCenterX;
            transform.position = playerPosition;
        }
    }
}