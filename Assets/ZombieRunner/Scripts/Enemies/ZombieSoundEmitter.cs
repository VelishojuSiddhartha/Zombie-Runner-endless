using System.Collections;
using UnityEngine;

namespace ZombieRunner.Enemies
{
    /// <summary>
    /// 3D spatialized horror vocalization emitter for zombies.
    /// Handles archetype-specific ambient groans, lunging snarls, hurt gasps, and death screams.
    /// </summary>
    [RequireComponent(typeof(ZombieController))]
    [DisallowMultipleComponent]
    public sealed class ZombieSoundEmitter : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;

        [Header("Clips - Normal Zombie")]
        [SerializeField] private AudioClip[] normalGroans;
        [SerializeField] private AudioClip normalAttack;
        [SerializeField] private AudioClip normalHurt;
        [SerializeField] private AudioClip normalDeath;

        [Header("Clips - Fast Zombie")]
        [SerializeField] private AudioClip[] fastHisses;
        [SerializeField] private AudioClip fastAttack;
        [SerializeField] private AudioClip fastHurt;
        [SerializeField] private AudioClip fastDeath;

        [Header("Clips - Heavy Zombie")]
        [SerializeField] private AudioClip[] heavyRoars;
        [SerializeField] private AudioClip heavyAttack;
        [SerializeField] private AudioClip heavyHurt;
        [SerializeField] private AudioClip heavyDeath;

        private ZombieController controller;
        private Coroutine vocalLoopRoutine;

        private void Awake()
        {
            controller = GetComponent<ZombieController>();
            EnsureAudioSource();
        }

        private void OnEnable()
        {
            if (vocalLoopRoutine != null)
                StopCoroutine(vocalLoopRoutine);
            vocalLoopRoutine = StartCoroutine(AmbientVocalRoutine());
        }

        private void OnDisable()
        {
            if (vocalLoopRoutine != null)
            {
                StopCoroutine(vocalLoopRoutine);
                vocalLoopRoutine = null;
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.spatialBlend = 1.0f; // Full 3D
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 2.5f;
            audioSource.maxDistance = 38f;
            audioSource.dopplerLevel = 0.5f;
            audioSource.playOnAwake = false;

            EnsureMixerGroup();
        }

        private void EnsureMixerGroup()
        {
            if (audioSource != null && audioSource.outputAudioMixerGroup == null && ZombieRunner.Audio.AudioManager.Instance != null && ZombieRunner.Audio.AudioManager.Instance.SFXGroup != null)
            {
                audioSource.outputAudioMixerGroup = ZombieRunner.Audio.AudioManager.Instance.SFXGroup;
            }
        }

        private IEnumerator AmbientVocalRoutine()
        {
            // Initial random delay so zombies don't all groan at the exact same moment
            yield return new WaitForSeconds(Random.Range(0.8f, 2.5f));

            while (controller != null && !controller.IsDead)
            {
                PlayRandomIdle();
                float interval = controller.Archetype switch
                {
                    ZombieArchetype.Fast => Random.Range(1.8f, 3.2f),
                    ZombieArchetype.Big => Random.Range(3.5f, 6.0f),
                    _ => Random.Range(2.5f, 4.5f)
                };
                yield return new WaitForSeconds(interval);
            }
        }

        public void PlayRandomIdle()
        {
            if (audioSource == null || (controller != null && controller.IsDead)) return;

            AudioClip clip = null;
            switch (controller.Archetype)
            {
                case ZombieArchetype.Fast:
                    if (fastHisses != null && fastHisses.Length > 0)
                        clip = fastHisses[Random.Range(0, fastHisses.Length)];
                    break;
                case ZombieArchetype.Big:
                    if (heavyRoars != null && heavyRoars.Length > 0)
                        clip = heavyRoars[Random.Range(0, heavyRoars.Length)];
                    break;
                default:
                    if (normalGroans != null && normalGroans.Length > 0)
                        clip = normalGroans[Random.Range(0, normalGroans.Length)];
                    break;
            }

            if (clip != null)
            {
                EnsureMixerGroup();
                float sfxVol = ZombieRunner.Audio.AudioManager.Instance != null ? ZombieRunner.Audio.AudioManager.Instance.EffectsVolume : 1f;
                audioSource.pitch = Random.Range(0.92f, 1.08f);
                audioSource.PlayOneShot(clip, 0.75f * sfxVol);
            }
        }

        public void PlayAttack()
        {
            if (audioSource == null) return;

            AudioClip clip = controller.Archetype switch
            {
                ZombieArchetype.Fast => fastAttack,
                ZombieArchetype.Big => heavyAttack,
                _ => normalAttack
            };

            if (clip != null)
            {
                EnsureMixerGroup();
                float sfxVol = ZombieRunner.Audio.AudioManager.Instance != null ? ZombieRunner.Audio.AudioManager.Instance.EffectsVolume : 1f;
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(clip, 0.95f * sfxVol);
            }
        }

        public void PlayHurt()
        {
            if (audioSource == null) return;

            AudioClip clip = controller.Archetype switch
            {
                ZombieArchetype.Fast => fastHurt,
                ZombieArchetype.Big => heavyHurt,
                _ => normalHurt
            };

            if (clip != null)
            {
                EnsureMixerGroup();
                float sfxVol = ZombieRunner.Audio.AudioManager.Instance != null ? ZombieRunner.Audio.AudioManager.Instance.EffectsVolume : 1f;
                audioSource.pitch = Random.Range(0.92f, 1.08f);
                audioSource.PlayOneShot(clip, 0.9f * sfxVol);
            }
        }

        public void PlayDeath()
        {
            if (audioSource == null) return;

            if (vocalLoopRoutine != null)
            {
                StopCoroutine(vocalLoopRoutine);
                vocalLoopRoutine = null;
            }

            AudioClip clip = controller.Archetype switch
            {
                ZombieArchetype.Fast => fastDeath,
                ZombieArchetype.Big => heavyDeath,
                _ => normalDeath
            };

            if (clip != null)
            {
                EnsureMixerGroup();
                float sfxVol = ZombieRunner.Audio.AudioManager.Instance != null ? ZombieRunner.Audio.AudioManager.Instance.EffectsVolume : 1f;
                audioSource.pitch = Random.Range(0.94f, 1.04f);
                audioSource.PlayOneShot(clip, 1.0f * sfxVol);
            }
        }

        public void AssignClips(
            AudioClip[] nGroans, AudioClip nAttack, AudioClip nHurt, AudioClip nDeath,
            AudioClip[] fHisses, AudioClip fAttack, AudioClip fHurt, AudioClip fDeath,
            AudioClip[] hRoars, AudioClip hAttack, AudioClip hHurt, AudioClip hDeath)
        {
            normalGroans = nGroans;
            normalAttack = nAttack;
            normalHurt = nHurt;
            normalDeath = nDeath;

            fastHisses = fHisses;
            fastAttack = fAttack;
            fastHurt = fHurt;
            fastDeath = fDeath;

            heavyRoars = hRoars;
            heavyAttack = hAttack;
            heavyHurt = hHurt;
            heavyDeath = hDeath;
        }
    }
}
