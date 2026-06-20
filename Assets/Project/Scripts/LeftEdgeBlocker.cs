using UnityEngine;

public class LeftEdgeBlocker : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    // プレイヤーの中心から、見た目の左端までの距離
    [SerializeField]
    private float leftOffsetFromCenter = 0.1f;

    // 画面左端から少し余白を空けたい場合だけ使う
    [SerializeField]
    private float edgePadding = 0.0f;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
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
        float minCenterX = leftEdgeWorldPosition.x + leftOffsetFromCenter + edgePadding;

        if (playerPosition.x < minCenterX)
        {
            playerPosition.x = minCenterX;
            transform.position = playerPosition;
        }
    }
}