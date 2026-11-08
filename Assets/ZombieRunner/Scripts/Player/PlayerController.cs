using UnityEngine;
using UnityEngine.InputSystem;
using ZombieRunner.World;

namespace ZombieRunner.Player
{
    /// <summary>
    /// Three lane endless-runner movement with keyboard and touch-swipe input.
    /// The visual model is kept separate from the CharacterController so slide
    /// can change the silhouette without scaling the collision volume.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int GroundedParameter = Animator.StringToHash("Grounded");
        private static readonly int SlidingParameter = Animator.StringToHash("Sliding");
        private static readonly int JumpParameter = Animator.StringToHash("Jump");
        private static readonly int SlideParameter = Animator.StringToHash("Slide");

        [Header("Running")]
        [SerializeField, Min(0f)] private float forwardSpeed = 8f;
        [SerializeField, Min(0.1f)] private float laneWidth = 3f;
        [SerializeField, Min(0.1f)] private float laneChangeSpeed = 11f;
        [SerializeField, Min(1f)] private float maxLaneSpeed = 22f;

        [Header("Difficulty Progression")]
        [SerializeField, Min(0f)] private float maxForwardSpeed = 16f;
        [SerializeField, Min(0f)] private float speedIncreaseRate = 0.005f;

        [Header("Jump and slide")]
        [SerializeField, Min(0.1f)] private float jumpHeight = 2.1f;
        [SerializeField] private float gravity = -25f;
        [SerializeField, Min(0.1f)] private float slideDuration = 0.8f;
        [SerializeField, Min(0.5f)] private float slideColliderHeight = 1.05f;
        [SerializeField, Range(0.35f, 1f)] private float slideVisualHeight = 0.62f;

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Animator animator;

        private int currentLane;
        private float verticalSpeed;
        private float speedMultiplier = 1f;
        private float slideTimeRemaining;
        private float normalColliderHeight;
        private Vector3 normalColliderCenter;
        private Vector3 normalVisualScale;
        private Vector3 normalVisualPosition;
        private Vector2 touchStartPosition;
        private bool trackingTouch;
        private bool hasSpeedParameter;
        private bool hasGroundedParameter;
        private bool hasSlidingParameter;
        private bool hasJumpParameter;
        private bool hasSlideParameter;
        private bool isJumping;
        private int lastStateHash;

        private float startingZ;

        public int CurrentLane => currentLane;
        public bool IsSliding => slideTimeRemaining > 0f;
        public float ForwardSpeed => forwardSpeed;
        public float BaseForwardSpeed => Mathf.Min(maxForwardSpeed, forwardSpeed + Mathf.Max(0f, transform.position.z - startingZ) * speedIncreaseRate);
        public float CurrentForwardSpeed => BaseForwardSpeed * speedMultiplier;
        public Animator ModelAnimator => animator;

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        private void Awake()
        {
            startingZ = transform.position.z;
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            // Ensure guaranteed 3D visual model is available
            PlayerVisualModel pvm = GetComponent<PlayerVisualModel>();
            if (pvm == null)
                pvm = gameObject.AddComponent<PlayerVisualModel>();
            pvm.EnsureVisualRepresentation();

            // Resolve best available visual root with actual renderers
            Transform newChar = transform.Find("Visual - New Character");
            if (newChar != null && newChar.gameObject.activeSelf && HasValidRenderer(newChar))
            {
                visualRoot = newChar;
                animator = newChar.GetComponent<Animator>() ?? newChar.GetComponentInChildren<Animator>();
            }
            else
            {
                Transform xbot = transform.Find("Visual - Mixamo X Bot");
                if (xbot != null && HasValidRenderer(xbot))
                {
                    visualRoot = xbot;
                    animator = visualRoot.GetComponent<Animator>() ?? visualRoot.GetComponentInChildren<Animator>();
                }
                else
                {
                    Transform proc = transform.Find("Visual - Procedural Survivor");
                    if (proc != null)
                    {
                        visualRoot = proc;
                    }
                    else
                    {
                        Transform placeholder = transform.Find("Visual - Placeholder Capsule");
                        if (placeholder != null)
                        {
                            placeholder.gameObject.SetActive(true);
                            visualRoot = placeholder;
                        }
                        else
                        {
                            visualRoot = FindFirstVisualChild();
                        }
                    }
                }
            }

            if (animator != null)
            {
                if (!animator.enabled)
                    animator.enabled = true;
                if (animator.avatar == null)
                {
                    Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
                    foreach (Animator a in allAnimators)
                    {
                        if (a.avatar != null && a.avatar.isValid)
                        {
                            animator.avatar = a.avatar;
                            break;
                        }
                    }
                }
            }
            CacheAnimatorParameters();

            normalColliderHeight = characterController.height;
            normalColliderCenter = characterController.center;
            if (visualRoot != null)
            {
                normalVisualScale = visualRoot.localScale;
                normalVisualPosition = visualRoot.localPosition;
            }
        }

        private void Start()
        {
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (!animator.enabled) animator.enabled = true;
                animator.Play("Run", 0, 0f);
            }
            UpdateAnimator();

            if (animator != null)
            {
                string avatarName = animator.avatar != null ? animator.avatar.name : "MISSING";
                string controllerName = animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "MISSING";
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"[ZombieRunner] Runtime Animator Verified -> GameObject: '{animator.gameObject.name}', Active: {animator.gameObject.activeInHierarchy}, Enabled: {animator.enabled}, Avatar: '{avatarName}' (isHuman: {animator.isHuman}, isValid: {(animator.avatar != null && animator.avatar.isValid)}), Controller: '{controllerName}', State: {state.shortNameHash}, Speed: {CurrentForwardSpeed:F1}");
            }
            else
            {
                Debug.LogError("[ZombieRunner] PlayerController could not find Animator on player character!");
            }
        }

        private void Update()
        {
            ReadKeyboard();
            ReadTouchSwipe();
            UpdateSlide();
            MoveRunner();
            UpdateAnimator();
        }

        /// <summary>Move one lane left. Safe for future on-screen controls.</summary>
        public void MoveLeft()
        {
            currentLane = Mathf.Max(-1, currentLane - 1);
        }

        /// <summary>Move one lane right. Safe for future on-screen controls.</summary>
        public void MoveRight()
        {
            currentLane = Mathf.Min(1, currentLane + 1);
        }

        /// <summary>Jump when grounded. Safe for future on-screen controls.</summary>
        public void Jump()
        {
            if (characterController == null || !characterController.isGrounded || IsSliding || verticalSpeed > 0.1f)
                return;

            isJumping = true;
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
            {
                if (hasGroundedParameter)
                    animator.SetBool(GroundedParameter, false);
                if (hasJumpParameter)
                {
                    animator.ResetTrigger(JumpParameter);
                    animator.SetTrigger(JumpParameter);
                }
            }
            Debug.Log($"[ZombieRunner] Jump Triggered -> verticalSpeed: {verticalSpeed:F2}, isGrounded: {characterController.isGrounded}, isJumping: true");
        }

        /// <summary>Start the timed slide. Safe for future on-screen controls.</summary>
        public void Slide()
        {
            if (characterController == null || IsSliding || !characterController.isGrounded || isJumping || verticalSpeed > 0.1f)
                return;

            slideTimeRemaining = slideDuration;
            if (animator != null && hasSlideParameter)
                animator.SetTrigger(SlideParameter);
            float heightReduction = Mathf.Max(0f, normalColliderHeight - slideColliderHeight);
            characterController.height = slideColliderHeight;
            characterController.center = normalColliderCenter + Vector3.down * (heightReduction * 0.5f);

            // Only squash visual root if there is no humanoid animator (e.g. placeholder capsule)
            // Rigged humanoid models use the slide/crouch animation directly so feet remain planted on the ground
            if (visualRoot != null && animator == null)
            {
                visualRoot.localScale = new Vector3(normalVisualScale.x, normalVisualScale.y * slideVisualHeight, normalVisualScale.z);
                float yOffset = normalVisualPosition.y <= -0.5f ? 0f : (heightReduction * 0.5f);
                visualRoot.localPosition = normalVisualPosition + Vector3.down * yOffset;
            }

            Debug.Log($"[ZombieRunner] Slide Triggered -> duration: {slideDuration:F1}s, colliderHeight: {slideColliderHeight:F2}, isGrounded: {characterController.isGrounded}, isJumping: {isJumping}");
        }

        private static bool HasValidRenderer(Transform t)
        {
            if (t == null) return false;
            SkinnedMeshRenderer[] smrs = t.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer s in smrs)
            {
                if (s != null && s.sharedMesh != null && s.sharedMesh.vertexCount > 0)
                    return true;
            }
            MeshFilter[] mfs = t.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in mfs)
            {
                if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                {
                    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null && mr.enabled)
                        return true;
                }
            }
            return false;
        }

        private Transform FindFirstVisualChild()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (HasValidRenderer(child))
                    return child;
            }
            return null;
        }

        private void ReadKeyboard()
        {
            bool left = false;
            bool right = false;
            bool jump = false;
            bool slide = false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                left |= keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame;
                right |= keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame;
                jump |= keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;
                slide |= keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame;
            }

            // Fallback for legacy input or standalone desktop focus
            try
            {
                left |= Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                right |= Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
                jump |= Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space);
                slide |= Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            }
            catch { }

            if (left) MoveLeft();
            if (right) MoveRight();
            if (jump) Jump();
            if (slide) Slide();
        }

        private void ReadTouchSwipe()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var touch = touchscreen.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    touchStartPosition = touch.position.ReadValue();
                    trackingTouch = true;
                }

                if (trackingTouch && touch.press.wasReleasedThisFrame)
                {
                    Vector2 delta = touch.position.ReadValue() - touchStartPosition;
                    trackingTouch = false;
                    ApplySwipe(delta);
                }
            }

            // Fallback mouse swipe for desktop testing and mouse controls
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    touchStartPosition = mouse.position.ReadValue();
                    trackingTouch = true;
                }
                if (trackingTouch && mouse.leftButton.wasReleasedThisFrame)
                {
                    Vector2 delta = mouse.position.ReadValue() - touchStartPosition;
                    trackingTouch = false;
                    ApplySwipe(delta);
                }
            }
        }

        private void ApplySwipe(Vector2 delta)
        {
            const float minimumSwipePixels = 55f;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) && Mathf.Abs(delta.x) >= minimumSwipePixels)
            {
                if (delta.x < 0f) MoveLeft();
                else MoveRight();
            }
            else if (Mathf.Abs(delta.y) >= minimumSwipePixels)
            {
                if (delta.y > 0f) Jump();
                else Slide();
            }
        }

        private void UpdateSlide()
        {
            if (slideTimeRemaining <= 0f)
                return;

            slideTimeRemaining -= Time.deltaTime;
            if (slideTimeRemaining > 0f)
                return;

            slideTimeRemaining = 0f;
            characterController.height = normalColliderHeight;
            characterController.center = normalColliderCenter;
            if (visualRoot != null)
            {
                visualRoot.localScale = normalVisualScale;
                visualRoot.localPosition = normalVisualPosition;
            }
            Debug.Log($"[ZombieRunner] Slide Completed -> returning to Run (colliderHeight: {normalColliderHeight:F2})");
        }

        private void OnDisable()
        {
            isJumping = false;
            if (animator != null && hasSpeedParameter)
                animator.SetFloat(SpeedParameter, 0f);

            if (slideTimeRemaining > 0f)
            {
                slideTimeRemaining = 0f;
                if (characterController != null)
                {
                    characterController.height = normalColliderHeight;
                    characterController.center = normalColliderCenter;
                }
                if (visualRoot != null)
                {
                    visualRoot.localScale = normalVisualScale;
                    visualRoot.localPosition = normalVisualPosition;
                }
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            ObstacleController obstacle = hit.collider.GetComponentInParent<ObstacleController>();
            if (obstacle != null)
                obstacle.OnPlayerCollision(GetComponent<PlayerHealth>());
        }

        private void MoveRunner()
        {
            if (characterController == null || !characterController.enabled)
                return;

            float deltaTime = Time.deltaTime;
            if (characterController.isGrounded && verticalSpeed < 0f)
            {
                verticalSpeed = -2f;
                if (isJumping)
                {
                    isJumping = false;
                    Debug.Log($"[ZombieRunner] Jump Landed -> verticalSpeed: {verticalSpeed:F2}, isGrounded: true, isJumping: false");
                }
            }
            verticalSpeed += gravity * deltaTime;

            float targetX = currentLane * laneWidth;
            float horizontalSpeed = Mathf.Clamp((targetX - transform.position.x) * laneChangeSpeed, -maxLaneSpeed, maxLaneSpeed);
            float forwardVelocity = BaseForwardSpeed * speedMultiplier;
            Vector3 velocity = new Vector3(horizontalSpeed, verticalSpeed, forwardVelocity);
            characterController.Move(velocity * deltaTime);

            if (isJumping && verticalSpeed <= 0f && characterController.isGrounded)
            {
                isJumping = false;
                Debug.Log($"[ZombieRunner] Jump Landed -> verticalSpeed: {verticalSpeed:F2}, isGrounded: true, isJumping: false");
            }
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null)
                return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedParameter && parameter.type == AnimatorControllerParameterType.Float) hasSpeedParameter = true;
                else if (parameter.nameHash == GroundedParameter && parameter.type == AnimatorControllerParameterType.Bool) hasGroundedParameter = true;
                else if (parameter.nameHash == SlidingParameter && parameter.type == AnimatorControllerParameterType.Bool) hasSlidingParameter = true;
                else if (parameter.nameHash == JumpParameter && parameter.type == AnimatorControllerParameterType.Trigger) hasJumpParameter = true;
                else if (parameter.nameHash == SlideParameter && parameter.type == AnimatorControllerParameterType.Trigger) hasSlideParameter = true;
            }
        }

        private void UpdateAnimator()
        {
            if (animator == null)
                return;

            bool groundedForAnim = !isJumping;

            if (hasSpeedParameter) animator.SetFloat(SpeedParameter, CurrentForwardSpeed);
            if (hasGroundedParameter) animator.SetBool(GroundedParameter, groundedForAnim);
            if (hasSlidingParameter) animator.SetBool(SlidingParameter, IsSliding);

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.shortNameHash != lastStateHash)
            {
                lastStateHash = currentState.shortNameHash;
                string stateName = "Unknown";
                if (currentState.IsName("Run")) stateName = "Run";
                else if (currentState.IsName("Jump")) stateName = "Jump";
                else if (currentState.IsName("Idle")) stateName = "Idle";
                else if (currentState.IsName("Slide")) stateName = "Slide";
                else if (currentState.IsName("Shoot")) stateName = "Shoot";
                Debug.Log($"[ZombieRunner] Animator State -> '{stateName}' (normalizedTime: {currentState.normalizedTime:F2}, isJumping: {isJumping}, GroundedParam: {groundedForAnim})");
            }
        }

        private void OnValidate()
        {
            forwardSpeed = Mathf.Max(0f, forwardSpeed);
            maxForwardSpeed = Mathf.Max(forwardSpeed, maxForwardSpeed);
            speedIncreaseRate = Mathf.Max(0f, speedIncreaseRate);
            laneWidth = Mathf.Max(0.1f, laneWidth);
            laneChangeSpeed = Mathf.Max(0.1f, laneChangeSpeed);
            maxLaneSpeed = Mathf.Max(1f, maxLaneSpeed);
            gravity = Mathf.Min(-0.1f, gravity);
            slideColliderHeight = Mathf.Max(0.5f, slideColliderHeight);
        }
    }
}
