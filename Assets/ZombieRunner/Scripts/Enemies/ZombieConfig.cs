using System;
using UnityEngine;

namespace ZombieRunner.Enemies
{
    /// <summary>
    /// Configurable stats and data asset for a zombie enemy archetype.
    /// Allows designers to tweak combat, movement, and reward values modularly in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "ZombieConfig", menuName = "ZombieRunner/Enemies/Zombie Config", order = 1)]
    public class ZombieConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name for this zombie archetype.")]
        [SerializeField] private string zombieName = "Common Zombie";

        [Tooltip("The archetype classification used by AI and spawner logic.")]
        [SerializeField] private ZombieArchetype archetype = ZombieArchetype.Normal;

        [Header("Combat Stats")]
        [Tooltip("Maximum hit points before dying.")]
        [SerializeField, Min(1f)] private float maximumHealth = 3f;

        [Tooltip("Forward chasing speed in units/sec.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;

        [Tooltip("Direct damage dealt to player on hit.")]
        [SerializeField, Min(0f)] private float attackDamage = 10f;

        [Tooltip("Distance threshold to trigger attack on runner.")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.5f;

        [Tooltip("Seconds required between successive attacks.")]
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.2f;

        [Header("Rewards & Progression")]
        [Tooltip("Score points awarded to player when killed.")]
        [SerializeField, Min(0)] private int scoreReward = 25;

        [Tooltip("Probability (0.0 to 1.0) of dropping a coin or powerup pickup.")]
        [SerializeField, Range(0f, 1f)] private float coinDropChance = 0.2f;

        public string ZombieName => zombieName;
        public ZombieArchetype Archetype => archetype;
        public float MaximumHealth => maximumHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public int ScoreReward => scoreReward;
        public float CoinDropChance => coinDropChance;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            attackDamage = Mathf.Max(0f, attackDamage);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackCooldown = Mathf.Max(0.1f, attackCooldown);
            scoreReward = Mathf.Max(0, scoreReward);
            coinDropChance = Mathf.Clamp01(coinDropChance);
        }
    }
}
