using System.Collections;
using UnityEngine;

namespace ZombieRunner.World
{
    /// <summary>
    /// Imparts periodic atmospheric lightning flashes across the main menu abandoned city scene.
    /// Surges directional moonlight and ambient sky illumination with realistic multi-burst lightning flashes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AtmosphericLightningFlash : MonoBehaviour
    {
        [Header("Target Lights")]
        [SerializeField] private Light directionalMoonlight;

        [Header("Flash Timing")]
        [SerializeField, Range(5f, 30f)] private float minInterval = 8f;
        [SerializeField, Range(10f, 60f)] private float maxInterval = 18f;

        [Header("Flash Intensity")]
        [SerializeField, Range(1.5f, 5f)] private float flashIntensityMultiplier = 2.8f;
        [SerializeField] private Color flashColor = new Color(0.88f, 0.94f, 1.0f);

        private float baseIntensity = 1.0f;
        private Color baseColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            ResolveMoonlight();
        }

        private void Start()
        {
            ResolveMoonlight();
            StartCoroutine(LightningCycleRoutine());
        }

        private void OnEnable()
        {
            ResolveMoonlight();
        }

        private void OnDisable()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
            RestoreBaseLight();
        }

#if UNITY_EDITOR
        [ContextMenu("Trigger Lightning Flash Now")]
        public void TriggerFlashNow()
        {
            ResolveMoonlight();
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(TriggerFlashSequence());
        }
#endif

        private void ResolveMoonlight()
        {
            if (directionalMoonlight == null)
            {
                GameObject moonObj = GameObject.Find("Directional Light (Moonlight)");
                if (moonObj != null)
                {
                    directionalMoonlight = moonObj.GetComponent<Light>();
                }
            }

            if (directionalMoonlight != null)
            {
                baseIntensity = directionalMoonlight.intensity;
                baseColor = directionalMoonlight.color;
            }
        }

        private IEnumerator LightningCycleRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(waitTime);

                if (directionalMoonlight != null && gameObject.activeInHierarchy)
                {
                    yield return TriggerFlashSequence();
                }
            }
        }

        public IEnumerator TriggerFlashSequence()
        {
            if (directionalMoonlight == null) yield break;

            // Flash 1 (Initial fork strike)
            directionalMoonlight.intensity = baseIntensity * (flashIntensityMultiplier * 0.7f);
            directionalMoonlight.color = flashColor;
            yield return new WaitForSeconds(0.04f);

            // Brief intermediate dip
            directionalMoonlight.intensity = baseIntensity * 1.2f;
            yield return new WaitForSeconds(0.035f);

            // Flash 2 (Main discharge peak)
            directionalMoonlight.intensity = baseIntensity * flashIntensityMultiplier;
            directionalMoonlight.color = flashColor;
            yield return new WaitForSeconds(0.08f);

            // Smooth exponential decay back to ambient moonlight
            float elapsed = 0f;
            float decayDuration = 0.32f;
            float startIntensity = directionalMoonlight.intensity;

            while (elapsed < decayDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / decayDuration;
                float curve = 1f - Mathf.Pow(t, 2f); // Ease-out curve
                directionalMoonlight.intensity = Mathf.Lerp(baseIntensity, startIntensity, curve);
                directionalMoonlight.color = Color.Lerp(baseColor, flashColor, curve);
                yield return null;
            }

            RestoreBaseLight();
        }

        private void RestoreBaseLight()
        {
            if (directionalMoonlight != null)
            {
                directionalMoonlight.intensity = baseIntensity;
                directionalMoonlight.color = baseColor;
            }
        }
    }
}
