using UnityEngine;

public class cameramovement : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeed = 2.0f;

    private void LateUpdate()
    {
        Vector3 position = transform.position;

        // X方向だけを一定速度で移動させる
        position.x += scrollSpeed * Time.deltaTime;

        // Y・Zは変更しない
        transform.position = position;
    }
}