using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZombieRunner.Audio;
using ZombieRunner.Core;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Manages the polished, cinematic survival-horror Game Over experience:
    /// - Semi-translucent dark city backdrop (city scene remains subtly visible).
    /// - Centered horror-themed results card with crisp typographic hierarchy.
    /// - Primary final score display with glowing "NEW RECORD!" indicator when beating best.
    /// - Structured stats breakdown (Best Distance/Score, Distance Survived, Zombies Eliminated, Banked Coins).
    /// - Consistent, perfectly aligned vertical action buttons (Revive, Try Again, Main Menu).
    /// - Dynamic spacing so hiding Revive leaves zero awkward gaps.
    /// - Seamless keyboard, gamepad, and mouse navigation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameOverUIController : MonoBehaviour
    {
        [Header("Canvas & Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardTransform;
        [SerializeField, Range(0.1f, 1f)] private float fadeInDuration = 0.35f;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;

        [Header("Summary Display")]
        [SerializeField] private TMP_Text summaryText;

        [Header("Hero Score Display (Optional Discrete Components)")]
        [SerializeField] private TMP_Text scoreValueText;
        [SerializeField] private GameObject newBestBadge;
        [SerializeField] private TMP_Text newBestBadgeText;

        [Header("Stats Grid (Optional Discrete Components)")]
        [SerializeField] private TMP_Text bestScoreValueText;
        [SerializeField] private TMP_Text distanceValueText;
        [SerializeField] private TMP_Text zombiesKilledValueText;
        [SerializeField] private TMP_Text coinsCollectedValueText;

        [Header("Action Buttons")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private RectTransform reviveButtonRect;
        [SerializeField] private RectTransform retryButtonRect;
        [SerializeField] private RectTransform homeButtonRect;
        [SerializeField] private TMP_Text reviveButtonLabel;
        [SerializeField] private TMP_Text retryButtonLabel;
        [SerializeField] private TMP_Text homeButtonLabel;
        [SerializeField] private RectTransform buttonContainer;

        [Header("Colors (Survival Horror Palette)")]
        [SerializeField] private Color titleColor = new Color(0.90f, 0.22f, 0.22f, 1.0f);        // Muted blood red
        [SerializeField] private Color primaryScoreColor = new Color(0.98f, 0.98f, 0.98f, 1.0f); // Crisp bright off-white
        [SerializeField] private Color statValueColor = new Color(0.88f, 0.92f, 0.96f, 1.0f);    // Crisp light slate
        [SerializeField] private Color statLabelColor = new Color(0.55f, 0.62f, 0.70f, 1.0f);    // Muted slate gray
        [SerializeField] private Color newBestColor = new Color(0.98f, 0.72f, 0.22f, 1.0f);      // Warm amber gold

        private Coroutine fadeRoutine;

        public Button ReviveButton => reviveButton;
        public Button RetryButton => retryButton;
        public Button HomeButton => homeButton;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            EnsureReferences();
        }

        private void OnEnable()
        {
            // Ensure cursor is always unlocked for PC player
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Populates stats from actual gameplay data and smoothly presents the screen.
        /// </summary>
        public void Show(int currentScore, int bestScore, int distanceMeters, int zombieKills, int coinsCollected, bool canRevive)
        {
            EnsureReferences();

            bool isNewRecord = currentScore > bestScore;
            int displayBest = Mathf.Max(currentScore, bestScore);

            // Populate unified rich-text summary if available
            if (summaryText != null)
            {
                string recordBadge = isNewRecord ? "   <size=15><color=#F59E0B><b>★ NEW RECORD!</b></color></size>" : string.Empty;
                string bestColorHex = isNewRecord ? "#F59E0B" : "#E2E8F0";

                summaryText.text =
                    $"<size=14><color=#8B98A5>FINAL SCORE</color></size>{recordBadge}\n" +
                    $"<size=44><color=#FFFFFF><b>{currentScore:N0}</b></color></size>\n" +
                    $"<size=12><color=#3A4454>───────────────────────────────────</color></size>\n" +
                    $"<size=16><line-height=160%><color=#8B98A5>BEST SCORE:</color>  <color={bestColorHex}><b>{displayBest:N0}</b></color>\n" +
                    $"<color=#8B98A5>DISTANCE:</color>  <color=#E2E8F0><b>{distanceMeters:N0} m</b></color>\n" +
                    $"<color=#8B98A5>ZOMBIES ELIMINATED:</color>  <color=#E2E8F0><b>{zombieKills:N0}</b></color>\n" +
                    $"<color=#8B98A5>COINS COLLECTED:</color>  <color=#E2E8F0><b>{coinsCollected:N0}</b></color></size>";
            }

            // Populate discrete TMP elements if present
            if (scoreValueText != null)
            {
                scoreValueText.text = currentScore.ToString("N0");
                scoreValueText.color = primaryScoreColor;
            }

            if (newBestBadge != null)
            {
                newBestBadge.SetActive(isNewRecord);
                if (newBestBadgeText != null)
                {
                    newBestBadgeText.text = "NEW RECORD!";
                    newBestBadgeText.color = newBestColor;
                }
            }

            if (bestScoreValueText != null)
            {
                bestScoreValueText.text = displayBest.ToString("N0");
                bestScoreValueText.color = isNewRecord ? newBestColor : statValueColor;
            }

            if (distanceValueText != null)
            {
                distanceValueText.text = distanceMeters.ToString("N0") + " m";
                distanceValueText.color = statValueColor;
            }

            if (zombiesKilledValueText != null)
            {
                zombiesKilledValueText.text = zombieKills.ToString("N0");
                zombiesKilledValueText.color = statValueColor;
            }

            if (coinsCollectedValueText != null)
            {
                coinsCollectedValueText.text = coinsCollected.ToString("N0");
                coinsCollectedValueText.color = statValueColor;
            }

            // Configure Buttons & Spacing
            if (reviveButton != null)
            {
                reviveButton.gameObject.SetActive(canRevive);
                if (canRevive && reviveButtonLabel != null)
                {
                    reviveButtonLabel.text = "REVIVE (1 FREE)";
                }
            }

            // Ensure consistent vertical layout without awkward gaps
            if (canRevive)
            {
                if (reviveButtonRect != null) reviveButtonRect.anchoredPosition = new Vector2(0f, -115f);
                if (retryButtonRect != null) retryButtonRect.anchoredPosition = new Vector2(0f, -180f);
                if (homeButtonRect != null) homeButtonRect.anchoredPosition = new Vector2(0f, -245f);
            }
            else
            {
                // When Revive is unavailable, shift Retry and Home up into a balanced 2-button group
                if (retryButtonRect != null) retryButtonRect.anchoredPosition = new Vector2(0f, -140f);
                if (homeButtonRect != null) homeButtonRect.anchoredPosition = new Vector2(0f, -210f);
            }

            if (buttonContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContainer);
            }

            // Activate panel
            gameObject.SetActive(true);

            // Start smooth fade-in
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeInRoutine());

            // Select default button for keyboard/gamepad navigation
            StartCoroutine(SelectDefaultButton(canRevive));
        }

        private IEnumerator SelectDefaultButton(bool canRevive)
        {
            yield return null; // Wait 1 frame for layout and active state to settle
            if (EventSystem.current != null)
            {
                GameObject target = (canRevive && reviveButton != null && reviveButton.gameObject.activeSelf)
                    ? reviveButton.gameObject
                    : (retryButton != null ? retryButton.gameObject : null);

                if (target != null)
                {
                    EventSystem.current.SetSelectedGameObject(target);
                }
            }
        }

        private IEnumerator FadeInRoutine()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            Vector3 startScale = new Vector3(0.96f, 0.96f, 1f);
            Vector3 endScale = Vector3.one;

            if (cardTransform != null) cardTransform.localScale = startScale;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                // Smooth cubic ease-out curve
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);

                canvasGroup.alpha = smoothT;
                if (cardTransform != null)
                {
                    cardTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, smoothT);
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            if (cardTransform != null) cardTransform.localScale = endScale;
        }

        public void Hide()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void EnsureReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (cardTransform == null)
            {
                Transform c = transform.Find("Game Over Card");
                if (c != null) cardTransform = c as RectTransform;
            }

            if (titleText == null)
            {
                Transform t = transform.Find("Game Over Title") ?? transform.Find("Game Over Card/Header/Title");
                if (t != null) titleText = t.GetComponent<TMP_Text>();
            }

            if (summaryText == null)
            {
                Transform s = transform.Find("Game Over Summary");
                if (s != null) summaryText = s.GetComponent<TMP_Text>();
            }

            if (buttonContainer == null)
            {
                Transform bc = transform.Find("Game Over Card/Button Container") ?? transform.Find("Button Container");
                if (bc != null) buttonContainer = bc as RectTransform;
            }

            if (reviveButton == null)
            {
                Transform b = transform.Find("Revive Button") ?? transform.Find("Game Over Card/Button Container/Revive Button");
                if (b != null) reviveButton = b.GetComponent<Button>();
            }

            if (retryButton == null)
            {
                Transform b = transform.Find("Retry Button") ?? transform.Find("Game Over Card/Button Container/Retry Button");
                if (b != null) retryButton = b.GetComponent<Button>();
            }

            if (homeButton == null)
            {
                Transform b = transform.Find("Game Over Home Button") ?? transform.Find("Game Over Card/Button Container/Game Over Home Button");
                if (b != null) homeButton = b.GetComponent<Button>();
            }

            if (reviveButtonRect == null && reviveButton != null) reviveButtonRect = reviveButton.GetComponent<RectTransform>();
            if (retryButtonRect == null && retryButton != null) retryButtonRect = retryButton.GetComponent<RectTransform>();
            if (homeButtonRect == null && homeButton != null) homeButtonRect = homeButton.GetComponent<RectTransform>();

            if (reviveButtonLabel == null && reviveButton != null)
                reviveButtonLabel = reviveButton.GetComponentInChildren<TMP_Text>();
            if (retryButtonLabel == null && retryButton != null)
                retryButtonLabel = retryButton.GetComponentInChildren<TMP_Text>();
            if (homeButtonLabel == null && homeButton != null)
                homeButtonLabel = homeButton.GetComponentInChildren<TMP_Text>();
        }
    }
}
