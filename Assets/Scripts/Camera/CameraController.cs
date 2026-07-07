using UnityEngine;

/// <summary>
/// Attach to the Main Camera.
/// Smoothly follows the player.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;   // drag the Player here
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);  // -10 keeps the camera back on Z

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
