using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CloudVolume))]
public class CloudVolumeFollowCamera : MonoBehaviour
{
    public Transform targetCamera;
    public float altitude = 120.0f;

    private void LateUpdate()
    {
        Transform cameraTransform = targetCamera;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
            return;

        transform.position = new Vector3(
            cameraTransform.position.x,
            altitude,
            cameraTransform.position.z
        );
    }
}