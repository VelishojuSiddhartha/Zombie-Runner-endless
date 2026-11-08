using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Provides cinematic hover, focus, and click animations and audio cues for PC menu buttons.
    /// Supports both mouse hover and keyboard/gamepad navigation selection.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    [DisallowMultipleComponent]
    public sealed class MenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        [Header("Visual Feedback")]
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private Color normalColor = new Color(0.12f, 0.14f, 0.18f, 0.85f);
        [SerializeField] private Color hoverColor = new Color(0.65f, 0.12f, 0.14f, 0.95f);
        [SerializeField] private Color normalTextColor = new Color(0.92f, 0.94f, 0.96f, 1f);
        [SerializeField] private Color hoverTextColor = new Color(1.0f, 0.35f, 0.30f, 1f);
        [SerializeField] private TMPro.TMP_Text buttonLabel;
        [SerializeField] private GameObject selectionGlow;

        [Header("Primary Action & Flash Effect")]
        [SerializeField] private bool isPrimaryButton = false;
        [SerializeField] private bool enableBreathingFlash = true;

        [Header("Transform Punch")]
        [SerializeField] private float hoverScale = 1.035f;
        [SerializeField] private float scaleSpeed = 12f;

        private Vector3 originalScale;
        private Vector3 targetScale;
        private bool isHoveredOrSelected;

        public void SetPrimaryButton(bool primary)
        {
            isPrimaryButton = primary;
            if (primary)
            {
                normalColor = new Color(0.72f, 0.14f, 0.16f, 0.95f);
                hoverColor = new Color(0.96f, 0.22f, 0.25f, 1.0f);
                normalTextColor = Color.white;
                hoverTextColor = Color.white;
            }
            ApplyVisualState(isHoveredOrSelected);
        }

        private void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;

            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
            if (buttonLabel == null) buttonLabel = GetComponentInChildren<TMPro.TMP_Text>();

            if (isPrimaryButton || gameObject.name.Contains("Play"))
            {
                SetPrimaryButton(true);
            }
            else
            {
                ApplyVisualState(false);
            }
        }

        private void OnEnable()
        {
            transform.localScale = originalScale;
            targetScale = originalScale;

            if (isPrimaryButton || gameObject.name.Contains("Play"))
            {
                SetPrimaryButton(true);
            }
            else
            {
                ApplyVisualState(false);
            }
        }

        private void Update()
        {
            if (transform.localScale != targetScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
            }

            // Subtle pulsing breathing flash effect on the primary Play Survival button
            if (isPrimaryButton && enableBreathingFlash && !isHoveredOrSelected && targetGraphic != null)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * 3.5f) + 1f) * 0.5f;
                targetGraphic.color = Color.Lerp(normalColor, hoverColor, pulse * 0.35f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayClickSound();
        }

        private void SetHighlight(bool highlight)
        {
            if (isHoveredOrSelected == highlight) return;
            isHoveredOrSelected = highlight;
            targetScale = highlight ? originalScale * hoverScale : originalScale;

            ApplyVisualState(highlight);

            if (highlight)
            {
                PlayHoverSound();
            }
        }

        private void ApplyVisualState(bool highlight)
        {
            if (targetGraphic != null)
            {
                targetGraphic.color = highlight ? hoverColor : normalColor;
            }

            if (buttonLabel != null)
            {
                buttonLabel.color = highlight ? hoverTextColor : normalTextColor;
            }

            if (selectionGlow != null)
            {
                selectionGlow.SetActive(highlight);
            }
        }

        private void PlayHoverSound()
        {
            // Sound effect removed per user request
        }

        private void PlayClickSound()
        {
            FindAnyObjectByType<MenuAudioController>()?.PlayClick();
        }
    }
}
