using System;
using UnityEngine;
using ZombieRunner.Audio;
using ZombieRunner.Player;
using ZombieRunner.Utilities;

namespace ZombieRunner.Enemies
{
    public enum ZombieArchetype
    {
        Normal,
        Fast,
        Big
    }

    /// <summary>Configurable enemy movement, health, attacking, and death behavior.</summary>
    [DisallowMultipleComponent]
    public sealed class ZombieController : MonoBehaviour, IDamageable
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int AttackingParameter = Animator.StringToHash("Attacking");
        private static readonly int DeadParameter = Animator.StringToHash("Dead");
        private static readonly int AttackParameter = Animator.StringToHash("Attack");
        private static readonly int HitParameter = Animator.StringToHash("Hit");
        private static readonly int DieParameter = Animator.StringToHash("Die");

        [SerializeField] private ZombieArchetype archetype;
        [SerializeField] private ZombieConfig config;
        [SerializeField] private Transform player;
        [SerializeField] private Animator animator;
        [SerializeField, Min(1f)] private float maximumHealth = 3f;
        [SerializeField, Min(0f)] private float currentHealth = 3f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.2f;
        [SerializeField, Min(0f)] private float destroyDelay = 2.8f;
        [SerializeField] private bool enableDebugLogs = true;

        private ZombieSoundEmitter soundEmitter;
        private float nextAttackTime;
        private float hitStunTimer;
        private bool dead;
        private bool hasSpeedParameter;
        private bool hasAttackingParameter;
        private bool hasDeadParameter;
        private bool hasAttackParameter;
        private bool hasHitParameter;
        private bool hasDieParameter;

        public event Action<ZombieController> Died;

        public ZombieConfig Config => config;
        public ZombieArchetype Archetype => archetype;
        public float CurrentHealth => currentHealth;
        public float MaximumHealth => maximumHealth;
        public bool IsDead => dead;

        private void Awake()
        {
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null)
                    player = runner.transform;
            }
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            soundEmitter = GetComponent<ZombieSoundEmitter>();
            CacheAnimatorParameters();
            ApplyArchetypeSettings();
            currentHealth = maximumHealth;

            // Ensure guaranteed 3D visual representation
            ZombieVisualModel zvm = GetComponent<ZombieVisualModel>();
            if (zvm == null)
                zvm = gameObject.AddComponent<ZombieVisualModel>();
            zvm.Configure(archetype);
        }

        private void Update()
        {
            if (dead || player == null)
                return;

            if (hitStunTimer > 0f)
            {
                hitStunTimer -= Time.deltaTime;
                UpdateAnimator(0f, false);
                return;
            }

            Vector3 toPlayer = player.position - transform.position;
            float distance = toPlayer.magnitude;
            if (distance <= attackRange)
            {
                UpdateAnimator(0f, true);
                TryAttack();
                return;
            }

            Vector3 direction = toPlayer / Mathf.Max(distance, 0.001f);
            direction.y = 0f;
            transform.position += direction * moveSpeed * Time.deltaTime;
            UpdateAnimator(moveSpeed, false);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        public void Configure(ZombieArchetype type, Transform target)
        {
            archetype = type;
            if (target != null)
                player = target;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            soundEmitter = GetComponent<ZombieSoundEmitter>();
            CacheAnimatorParameters();
            ApplyArchetypeSettings();
            currentHealth = maximumHealth;
            dead = false;
            hitStunTimer = 0f;

            ZombieVisualModel zvm = GetComponent<ZombieVisualModel>();
            if (zvm == null)
                zvm = gameObject.AddComponent<ZombieVisualModel>();
            zvm.Configure(archetype);
        }

        public void TakeDamage(float amount)
        {
            if (dead || amount <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (enableDebugLogs)
                Debug.Log($"[ZombieRunner] {gameObject.name} ({archetype}) took {amount:F1} damage -> Health: {currentHealth:F1}/{maximumHealth:F1}");

            ZombieVisualModel zvm = GetComponent<ZombieVisualModel>();
            if (zvm != null)
                zvm.TriggerHitReaction();

            if (currentHealth <= 0f)
            {
                Die();
            }
            else
            {
                soundEmitter?.PlayHurt();
                hitStunTimer = 0.35f;
                if (animator != null && hasHitParameter)
                {
                    animator.ResetTrigger(HitParameter);
                    animator.SetTrigger(HitParameter);
                    if (enableDebugLogs)
                        Debug.Log($"[ZombieRunner] {gameObject.name} ({archetype}) triggered Hit reaction animation. Pausing movement for 0.35s.");
                }
                UpdateAnimator(0f, false);
            }
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + attackCooldown;
            soundEmitter?.PlayAttack();
            if (animator != null && hasAttackParameter)
                animator.SetTrigger(AttackParameter);

            ZombieVisualModel zvm = GetComponent<ZombieVisualModel>();
            if (zvm != null)
                zvm.SetAttacking(true);

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(attackDamage);
            else
                player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        private void Die()
        {
            if (dead)
                return;

            dead = true;
            hitStunTimer = 0f;
            soundEmitter?.PlayDeath();
            if (enableDebugLogs)
                Debug.Log($"[ZombieRunner] {gameObject.name} ({archetype}) died! Playing death animation (duration: {destroyDelay:F1}s).");

            ZombieVisualModel zvm = GetComponent<ZombieVisualModel>();
            if (zvm != null)
                zvm.TriggerDeath();

            if (animator != null)
            {
                if (hasSpeedParameter)
                    animator.SetFloat(SpeedParameter, 0f);
                if (hasAttackingParameter)
                    animator.SetBool(AttackingParameter, false);
                if (hasDieParameter)
                {
                    animator.ResetTrigger(DieParameter);
                    animator.SetTrigger(DieParameter);
                }
                if (hasDeadParameter)
                    animator.SetBool(DeadParameter, true);
            }
            AudioManager.Instance?.Play(GameAudioCue.ZombieDeath);
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider item in colliders)
                item.enabled = false;
            Died?.Invoke(this);
            Destroy(gameObject, destroyDelay);
        }

        private void CacheAnimatorParameters()
        {
            hasSpeedParameter = false;
            hasAttackingParameter = false;
            hasDeadParameter = false;
            hasAttackParameter = false;
            hasHitParameter = false;
            hasDieParameter = false;
            if (animator == null)
                return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedParameter && parameter.type == AnimatorControllerParameterType.Float) hasSpeedParameter = true;
                else if (parameter.nameHash == AttackingParameter && parameter.type == AnimatorControllerParameterType.Bool) hasAttackingParameter = true;
                else if (parameter.nameHash == DeadParameter && parameter.type == AnimatorControllerParameterType.Bool) hasDeadParameter = true;
                else if (parameter.nameHash == AttackParameter && parameter.type == AnimatorControllerParameterType.Trigger) hasAttackParameter = true;
                else if (parameter.nameHash == HitParameter && parameter.type == AnimatorControllerParameterType.Trigger) hasHitParameter = true;
                else if (parameter.nameHash == DieParameter && parameter.type == AnimatorControllerParameterType.Trigger) hasDieParameter = true;
            }
        }

        private void UpdateAnimator(float speed, bool attacking)
        {
            if (animator == null)
                return;
            if (hasSpeedParameter) animator.SetFloat(SpeedParameter, speed);
            if (hasAttackingParameter) animator.SetBool(AttackingParameter, attacking);
        }

        private void ApplyArchetypeSettings()
        {
            if (config != null)
            {
                archetype = config.Archetype;
                maximumHealth = config.MaximumHealth;
                moveSpeed = config.MoveSpeed;
                attackDamage = config.AttackDamage;
                attackRange = config.AttackRange;
                attackCooldown = config.AttackCooldown;
                return;
            }

            switch (archetype)
            {
                case ZombieArchetype.Fast:
                    maximumHealth = 2f;
                    moveSpeed = 4.1f;
                    attackDamage = 8f;
                    attackCooldown = 1.05f;
                    break;
                case ZombieArchetype.Big:
                    maximumHealth = 8f;
                    moveSpeed = 1.45f;
                    attackDamage = 22f;
                    attackCooldown = 1.7f;
                    break;
                default:
                    maximumHealth = 3f;
                    moveSpeed = 2.5f;
                    attackDamage = 10f;
                    attackCooldown = 1.2f;
                    break;
            }
        }

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maximumHealth);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            attackDamage = Mathf.Max(0f, attackDamage);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackCooldown = Mathf.Max(0.1f, attackCooldown);
        }
    }
}
