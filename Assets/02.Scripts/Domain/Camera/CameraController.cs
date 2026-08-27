using UnityEngine;

namespace Domain.Camera
{
    /// <summary>
    /// Attach to the Main Camera.
    /// Smoothly follows the player.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;   // drag the Player here
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);  // -10 keeps the camera back on Z

        private float _shakeElapsed = 0f;
        private float _shakeMagnitude = 0f;

        public void Shake(float duration, float magnitude)
        {
            _shakeElapsed = duration;
            _shakeMagnitude = magnitude;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 targetPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            if (_shakeElapsed > 0f)
            {
                _shakeElapsed -= Time.unscaledDeltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeMagnitude;
                shakeOffset.z = 0; // Keep Z position flat
                targetPos += shakeOffset;
            }

            transform.position = targetPos;
        }
    }
}
