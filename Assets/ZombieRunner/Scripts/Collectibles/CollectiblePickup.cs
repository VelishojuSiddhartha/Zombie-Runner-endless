using UnityEngine;
using ZombieRunner.Audio;
using ZombieRunner.Player;
using ZombieRunner.Weapons;

namespace ZombieRunner.Collectibles
{
    public enum CollectibleType
    {
        Coin,
        Ammo,
        Health
    }

    /// <summary>Trigger pickup that immediately updates the player or run counter.</summary>
    [DisallowMultipleComponent]
    public sealed class CollectiblePickup : MonoBehaviour
    {
        [SerializeField] private CollectibleType collectibleType;
        [SerializeField, Min(1)] private int amount = 1;
        private CollectibleManager manager;
        private bool collected;

        public CollectibleType Type => collectibleType;
        public int Amount => amount;

        public void Configure(CollectibleType type, int pickupAmount, CollectibleManager owner)
        {
            collectibleType = type;
            amount = Mathf.Max(1, pickupAmount);
            manager = owner;
        }

        public void Collect(PlayerController player)
        {
            if (collected || player == null)
                return;

            bool applied = false;
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                    if (manager == null)
                        manager = FindAnyObjectByType<CollectibleManager>();
                    if (manager != null)
                    {
                        manager.CollectCoins(amount);
                        applied = true;
                    }
                    break;
                case CollectibleType.Ammo:
                    ShootingController shooting = player.GetComponent<ShootingController>();
                    if (shooting != null)
                    {
                        shooting.AddAmmo(amount);
                        applied = true;
                    }
                    break;
                case CollectibleType.Health:
                    PlayerHealth health = player.GetComponent<PlayerHealth>();
                    if (health != null && !health.IsDead)
                    {
                        float before = health.CurrentHealth;
                        health.RestoreHealth(amount);
                        applied = health.CurrentHealth > before;
                    }
                    break;
            }

            if (!applied)
                return;

            collected = true;
            AudioManager.Instance?.Play(GameAudioCue.Pickup);
            manager?.NotifyCollected(this);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                Collect(player);
        }

        private void OnValidate()
        {
            amount = Mathf.Max(1, amount);
        }
    }
}
