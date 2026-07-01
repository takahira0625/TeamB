using UnityEngine;

public class cameramovement : MonoBehaviour
{
    [Tooltip("調整値をまとめたCameraParamsアセット")]
    [SerializeField]
    private CameraParams param;

    private void LateUpdate()
    {
        if (param == null)
        {
            return;
        }

        Vector3 position = transform.position;

        // X方向だけを一定速度で移動させる
        position.x += param.scrollSpeed * Time.deltaTime;

        // Y・Zは変更しない
        transform.position = position;
    }
}