using System;
using UnityEngine;
using ZombieRunner.Audio;

namespace ZombieRunner.Player
{
    /// <summary>Player health foundation used by enemy attacks and later HUD/power-ups.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour
    {
        private static readonly int DeadParameter = Animator.StringToHash("Dead");
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField, Min(0f)] private float currentHealth = 100f;
        private bool shielded;
        private Animator animator;
        private bool hasDeadParameter;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float MaximumHealth => maximumHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0f;
        public bool IsShielded => shielded;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maximumHealth);
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null && controller.ModelAnimator != null)
                animator = controller.ModelAnimator;
            else
                animator = GetComponentInChildren<Animator>();

            if (animator != null)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    if (parameter.nameHash == DeadParameter && parameter.type == AnimatorControllerParameterType.Bool)
                    {
                        hasDeadParameter = true;
                        break;
                    }
                }
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || shielded || amount <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            AudioManager.Instance?.Play(GameAudioCue.PlayerHit);
            HealthChanged?.Invoke(currentHealth, maximumHealth);
            if (!IsDead)
                return;

            PlayerController movement = GetComponent<PlayerController>();
            if (movement != null)
                movement.enabled = false;
            if (animator != null && hasDeadParameter)
                animator.SetBool(DeadParameter, true);
            Died?.Invoke();
        }

        public void RestoreHealth(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            float previous = currentHealth;
            currentHealth = Mathf.Min(maximumHealth, currentHealth + amount);
            if (!Mathf.Approximately(previous, currentHealth))
                HealthChanged?.Invoke(currentHealth, maximumHealth);
        }

        public void RestoreToFull()
        {
            RestoreHealth(maximumHealth);
        }

        /// <summary>Restores a defeated runner and re-enables its movement for a revive flow.</summary>
        public void Revive(float healthAmount)
        {
            if (!IsDead || healthAmount <= 0f)
                return;

            currentHealth = Mathf.Clamp(healthAmount, 1f, maximumHealth);
            PlayerController movement = GetComponent<PlayerController>();
            if (movement != null)
                movement.enabled = true;
            if (animator != null && hasDeadParameter)
                animator.SetBool(DeadParameter, false);
            HealthChanged?.Invoke(currentHealth, maximumHealth);
        }

        public void SetShielded(bool value)
        {
            shielded = value;
        }

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maximumHealth);
        }
    }
}
