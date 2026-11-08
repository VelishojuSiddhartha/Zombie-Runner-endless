using UnityEngine;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Imparts a subtle, stable cinematic breathing sway to the Main Menu camera.
    /// Uses low-frequency Perlin noise to simulate a steady, atmospheric camera feel
    /// without any jarring or disorienting movement.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuCameraPan : MonoBehaviour
    {
        [Header("Breathing Sway Settings")]
        [SerializeField, Range(0f, 0.15f)] private float swayAmount = 0.035f;
        [SerializeField, Range(0.1f, 1.5f)] private float swayFrequency = 0.35f;
        [SerializeField, Range(0f, 1f)] private float tiltAmount = 0.35f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float seedX;
        private float seedY;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            seedX = Random.Range(0f, 100f);
            seedY = Random.Range(100f, 200f);
        }

        private void Update()
        {
            float t = Time.time * swayFrequency;
            float offsetX = (Mathf.PerlinNoise(seedX, t) - 0.5f) * 2f * swayAmount;
            float offsetY = (Mathf.PerlinNoise(seedY, t) - 0.5f) * 2f * (swayAmount * 0.6f);

            transform.position = initialPosition + new Vector3(offsetX, offsetY, 0f);

            float tiltZ = (Mathf.PerlinNoise(seedX + 50f, t * 0.7f) - 0.5f) * 2f * tiltAmount;
            float tiltY = (Mathf.PerlinNoise(seedY + 50f, t * 0.7f) - 0.5f) * 2f * (tiltAmount * 0.8f);

            transform.rotation = initialRotation * Quaternion.Euler(-offsetY * 8f, tiltY, tiltZ);
        }
    }
}
