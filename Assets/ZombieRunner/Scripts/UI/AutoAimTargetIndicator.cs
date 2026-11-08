using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieRunner.Core;
using ZombieRunner.Enemies;
using ZombieRunner.Weapons;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Screen-space auto-aim target indicator that visually highlights and tracks
    /// the active zombie locked by the ShootingController.
    /// Polished sci-fi red bracket reticle with lock-on animation and zero raycast blocking.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class AutoAimTargetIndicator : MonoBehaviour
    {
        [Header("Targeting References")]
        [SerializeField] private ShootingController shootingController;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Canvas targetCanvas;

        [Header("Reticle Visuals")]
        [SerializeField] private RectTransform reticleRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color primaryColor = new Color(1f, 0.12f, 0.22f, 0.95f);
        [SerializeField] private Color accentColor = new Color(1f, 0.35f, 0.42f, 0.75f);
        [SerializeField] private float reticleSize = 72f;
        [SerializeField] private bool showTargetBadge = true;

        [Header("Animation & Tracking")]
        [SerializeField, Range(5f, 40f)] private float trackingSmoothing = 25f;
        [SerializeField, Range(2f, 20f)] private float fadeSpeed = 12f;
        [SerializeField, Range(1f, 10f)] private float pulseSpeed = 6f;
        [SerializeField, Range(0f, 0.2f)] private float pulseAmplitude = 0.05f;

        [Header("Aim Height Offsets by Zombie Type")]
        [SerializeField] private float normalAimHeight = 1.35f;
        [SerializeField] private float fastAimHeight = 1.20f;
        [SerializeField] private float bigAimHeight = 1.65f;

        private RectTransform canvasRect;
        private ZombieController lastTarget;
        private Vector2 currentScreenLocalPos;
        private float popInTimer;
        private bool hasValidTarget;
        private TMP_Text badgeText;
        private RectTransform[] cornerBrackets;
        private RectTransform centerPip;

        public ZombieController ActiveTarget => hasValidTarget && shootingController != null ? shootingController.CurrentTarget : null;

        private void Awake()
        {
            ResolveReferences();
            EnsureVisualHierarchy();
        }

        private void OnEnable()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            lastTarget = null;
            hasValidTarget = false;
        }

        private void Start()
        {
            ResolveReferences();
            EnsureVisualHierarchy();
        }

        private void LateUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                hasValidTarget = false;
                lastTarget = null;
                HideImmediately();
                return;
            }

            if (shootingController == null)
            {
                shootingController = FindAnyObjectByType<ShootingController>();
                if (shootingController == null)
                {
                    HideImmediately();
                    return;
                }
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    HideImmediately();
                    return;
                }
            }

            ZombieController currentTarget = shootingController.CurrentTarget;
            bool targetAlive = currentTarget != null && !currentTarget.IsDead && currentTarget.CurrentHealth > 0f;

            if (!targetAlive)
            {
                hasValidTarget = false;
                lastTarget = null;
                FadeOut();
                return;
            }

            // New target acquired: trigger pop-in animation
            if (currentTarget != lastTarget)
            {
                lastTarget = currentTarget;
                popInTimer = 0.25f;
                SnapToTarget(currentTarget);
            }

            hasValidTarget = true;
            UpdateTracking(currentTarget);
            UpdateVisualAnimation(currentTarget);
        }

        private void ResolveReferences()
        {
            if (shootingController == null)
                shootingController = FindAnyObjectByType<ShootingController>();

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInParent<Canvas>();
                if (targetCanvas == null)
                    targetCanvas = FindAnyObjectByType<Canvas>();
            }

            if (targetCanvas != null)
                canvasRect = targetCanvas.GetComponent<RectTransform>();
        }

        private void SnapToTarget(ZombieController target)
        {
            Vector3 aimPos = GetTargetAimPosition(target);
            Vector3 screenPoint = targetCamera.WorldToScreenPoint(aimPos);

            if (screenPoint.z > 0f && canvasRect != null)
            {
                Camera uiCam = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? targetCamera : null;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCam, out Vector2 localPoint);
                currentScreenLocalPos = localPoint;
                if (reticleRoot != null)
                {
                    reticleRoot.anchoredPosition = currentScreenLocalPos;
                }
            }
        }

        private void UpdateTracking(ZombieController target)
        {
            Vector3 aimPos = GetTargetAimPosition(target);
            Vector3 screenPoint = targetCamera.WorldToScreenPoint(aimPos);

            // Hide if behind camera
            if (screenPoint.z <= 0.1f)
            {
                FadeOut();
                return;
            }

            if (canvasRect == null) return;

            Camera uiCam = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? targetCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCam, out Vector2 localPoint))
            {
                float smooth = Time.deltaTime * trackingSmoothing;
                currentScreenLocalPos = Vector2.Lerp(currentScreenLocalPos, localPoint, Mathf.Clamp01(smooth));
                if (reticleRoot != null)
                {
                    reticleRoot.anchoredPosition = currentScreenLocalPos;
                }
            }
        }

        private void UpdateVisualAnimation(ZombieController target)
        {
            if (canvasGroup == null || reticleRoot == null) return;

            // Smooth fade-in
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);

            // Scale animation: pop-in on lock-on + subtle breathing pulse
            float baseScale = 1f;
            if (popInTimer > 0f)
            {
                popInTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(popInTimer / 0.25f);
                // Elastic bounce from 1.35 down to 1.0
                baseScale = Mathf.Lerp(1.35f, 1f, Mathf.SmoothStep(0f, 1f, t));
            }

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            reticleRoot.localScale = Vector3.one * (baseScale * pulse);

            // Update badge text if present
            if (badgeText != null && showTargetBadge)
            {
                float dist = Vector3.Distance(targetCamera.transform.position, target.transform.position);
                string arch = target.Archetype.ToString().ToUpperInvariant();
                badgeText.text = $"<color=red>TARGET LOCK</color>\n{arch} {dist:F0}m";
            }
        }

        private void FadeOut()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                if (reticleRoot != null && canvasGroup.alpha < 0.01f)
                {
                    reticleRoot.localScale = Vector3.one * 0.8f;
                }
            }
        }

        private void HideImmediately()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            hasValidTarget = false;
            lastTarget = null;
        }

        private Vector3 GetTargetAimPosition(ZombieController target)
        {
            float height = normalAimHeight;
            switch (target.Archetype)
            {
                case ZombieArchetype.Fast:
                    height = fastAimHeight;
                    break;
                case ZombieArchetype.Big:
                    height = bigAimHeight;
                    break;
            }
            return target.transform.position + Vector3.up * height;
        }

        /// <summary>
        /// Dynamically builds a crisp, non-blocking tactical red reticle if not already created in the Inspector.
        /// </summary>
        private void EnsureVisualHierarchy()
        {
            if (reticleRoot != null && canvasGroup != null)
            {
                EnsureNonBlocking(reticleRoot.gameObject);
                return;
            }

            if (targetCanvas == null)
            {
                ResolveReferences();
                if (targetCanvas == null) return;
            }

            // Create root container
            GameObject rootGO = new GameObject("AutoAim_ReticleRoot", typeof(RectTransform), typeof(CanvasGroup));
            rootGO.transform.SetParent(targetCanvas.transform, false);

            reticleRoot = rootGO.GetComponent<RectTransform>();
            reticleRoot.sizeDelta = new Vector2(reticleSize, reticleSize);
            reticleRoot.anchorMin = new Vector2(0.5f, 0.5f);
            reticleRoot.anchorMax = new Vector2(0.5f, 0.5f);
            reticleRoot.pivot = new Vector2(0.5f, 0.5f);

            canvasGroup = rootGO.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Generate Crisp Red Reticle Elements
            cornerBrackets = new RectTransform[4];
            float half = reticleSize * 0.5f;
            float barLen = 16f;
            float barThick = 3f;

            // Top-Left (-half, +half)
            cornerBrackets[0] = CreateBracket(rootGO.transform, "Corner_TL", -half, half, barLen, barThick, true, true);
            // Top-Right (+half, +half)
            cornerBrackets[1] = CreateBracket(rootGO.transform, "Corner_TR", half, half, barLen, barThick, false, true);
            // Bottom-Left (-half, -half)
            cornerBrackets[2] = CreateBracket(rootGO.transform, "Corner_BL", -half, -half, barLen, barThick, true, false);
            // Bottom-Right (+half, -half)
            cornerBrackets[3] = CreateBracket(rootGO.transform, "Corner_BR", half, -half, barLen, barThick, false, false);

            // Center Aim Pip (Diamond / Dot)
            GameObject pipGO = new GameObject("CenterPip", typeof(RectTransform), typeof(Image));
            pipGO.transform.SetParent(rootGO.transform, false);
            centerPip = pipGO.GetComponent<RectTransform>();
            centerPip.sizeDelta = new Vector2(7f, 7f);
            centerPip.anchoredPosition = Vector2.zero;
            centerPip.localRotation = Quaternion.Euler(0, 0, 45f); // diamond shape

            Image pipImg = pipGO.GetComponent<Image>();
            pipImg.color = primaryColor;
            pipImg.raycastTarget = false;

            // Target Info Badge
            if (showTargetBadge)
            {
                GameObject badgeGO = new GameObject("TargetBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
                badgeGO.transform.SetParent(rootGO.transform, false);
                RectTransform badgeRect = badgeGO.GetComponent<RectTransform>();
                badgeRect.sizeDelta = new Vector2(140f, 32f);
                badgeRect.anchoredPosition = new Vector2(0f, -half - 18f);

                badgeText = badgeGO.GetComponent<TextMeshProUGUI>();
                badgeText.fontSize = 11f;
                badgeText.alignment = TextAlignmentOptions.Center;
                badgeText.color = accentColor;
                badgeText.raycastTarget = false;
                badgeText.text = "TARGET LOCK";
            }

            EnsureNonBlocking(rootGO);
        }

        private RectTransform CreateBracket(Transform parent, string name, float x, float y, float len, float thick, bool left, bool top)
        {
            GameObject cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            RectTransform cornerRect = cornerGO.GetComponent<RectTransform>();
            cornerRect.anchoredPosition = new Vector2(x, y);

            // Horizontal Bar
            GameObject hBar = new GameObject("HBar", typeof(RectTransform), typeof(Image));
            hBar.transform.SetParent(cornerGO.transform, false);
            RectTransform hRect = hBar.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(len, thick);
            hRect.pivot = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
            hRect.anchoredPosition = Vector2.zero;
            Image hImg = hBar.GetComponent<Image>();
            hImg.color = primaryColor;
            hImg.raycastTarget = false;

            // Vertical Bar
            GameObject vBar = new GameObject("VBar", typeof(RectTransform), typeof(Image));
            vBar.transform.SetParent(cornerGO.transform, false);
            RectTransform vRect = vBar.GetComponent<RectTransform>();
            vRect.sizeDelta = new Vector2(thick, len);
            vRect.pivot = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
            vRect.anchoredPosition = Vector2.zero;
            Image vImg = vBar.GetComponent<Image>();
            vImg.color = primaryColor;
            vImg.raycastTarget = false;

            return cornerRect;
        }

        private void EnsureNonBlocking(GameObject go)
        {
            Graphic[] graphics = go.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }

            CanvasGroup[] groups = go.GetComponentsInChildren<CanvasGroup>(true);
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].blocksRaycasts = false;
                groups[i].interactable = false;
            }
        }
    }
}
