using UnityEngine;
using ZombieRunner.Player;

namespace ZombieRunner.PowerUps
{
    /// <summary>World pickup for an instant refill or timed power-up.</summary>
    [DisallowMultipleComponent]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType;
        [SerializeField, Min(0f)] private float durationOverride;
        private PowerUpManager manager;
        private bool collected;

        public PowerUpType Type => powerUpType;

        public void Configure(PowerUpType type, float duration, PowerUpManager owner)
        {
            powerUpType = type;
            durationOverride = Mathf.Max(0f, duration);
            manager = owner;
            ApplyColor();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected)
                return;

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            if (manager == null)
                manager = FindAnyObjectByType<PowerUpManager>();
            if (manager == null)
                return;

            collected = true;
            manager.Activate(powerUpType, durationOverride);
            manager.NotifyCollected(this);
            Destroy(gameObject);
        }

        private void ApplyColor()
        {
            Color color;
            switch (powerUpType)
            {
                case PowerUpType.HealthRestore: color = new Color(0.24f, 0.85f, 0.34f); break;
                case PowerUpType.AmmoRefill: color = new Color(1f, 0.55f, 0.12f); break;
                case PowerUpType.CoinMagnet: color = new Color(0.1f, 0.8f, 0.9f); break;
                case PowerUpType.Shield: color = new Color(0.22f, 0.48f, 1f); break;
                case PowerUpType.SpeedBoost: color = new Color(1f, 0.25f, 0.08f); break;
                default: color = new Color(1f, 0.8f, 0.12f); break;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer item in renderers)
            {
                item.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                item.SetPropertyBlock(block);
            }
        }

        private void OnValidate()
        {
            durationOverride = Mathf.Max(0f, durationOverride);
        }
    }
}
