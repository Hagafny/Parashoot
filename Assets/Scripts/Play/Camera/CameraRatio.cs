using UnityEngine;
using System.Collections;

public class CameraRatio : MonoBehaviour {

    public float aspect = 1.33333f;

    void Start()
    {
        Camera mainCaemra = Camera.main;
        float orthographicSize = mainCaemra.orthographicSize;

        mainCaemra.projectionMatrix = Matrix4x4.Ortho(
                -orthographicSize * aspect, orthographicSize * aspect,
                -orthographicSize, orthographicSize,
                mainCaemra.nearClipPlane, mainCaemra.farClipPlane);
    }
}
