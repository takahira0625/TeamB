using UnityEngine;

public class LeftEdgeBlocker : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private float edgePadding = 0.3f;

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

        float minX = leftEdgeWorldPosition.x + edgePadding;

        if (playerPosition.x < minX)
        {
            playerPosition.x = minX;
            transform.position = playerPosition;
        }
    }
}
