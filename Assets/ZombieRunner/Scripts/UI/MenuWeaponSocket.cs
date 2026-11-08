using UnityEngine;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Attach the realistic Tommy Gun firearm model to the Menu Survivor's hands.
    /// Anchors the weapon between the RightHand (grip) and LeftHand (foregrip):
    /// - Beautiful 35-degree hero combat pose (clearly displays barrel, drum, receiver, and stock)
    /// - Pushed forward in front of chest so it never clips or gets swallowed into the body
    /// - Lowered below chin so the face and eyes remain 100% visible
    /// - Includes a toggle to cleanly remove/hide the weapon if desired
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class MenuWeaponSocket : MonoBehaviour
    {
        [Header("Weapon Visibility")]
        [Tooltip("Toggle weapon on or off. Uncheck to totally remove the weapon from the character.")]
        [SerializeField] private bool showWeapon = true;

        [Header("Weapon Assets")]
        [Tooltip("Direct reference to the weapon prefab or model.")]
        [SerializeField] private GameObject weaponPrefab;

        [Tooltip("Resources path fallback if prefab is not set.")]
        [SerializeField] private string resourcesPath = "MenuWeapons/TommyGun";

        [Header("Materials (Auto-configured for URP)")]
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material drumMaterial;
        [SerializeField] private Material bulletMaterial;

        [Header("Weapon Sizing")]
        [Tooltip("Desired weapon length in meters (standard Tommy Gun is ~0.82m).")]
        [SerializeField, Range(0.5f, 1.2f)] private float targetWeaponLength = 0.82f;

        [Tooltip("Additional manual scale multiplier.")]
        [SerializeField, Range(0.5f, 2.0f)] private float scaleMultiplier = 1.0f;

        [Header("Hero Stance Positioning")]
        [Tooltip("Hand blend: 0 = at right hand, 0.5 = centered between hands, 1 = at left hand.")]
        [SerializeField, Range(0f, 1f)] private float handCenterBias = 0.42f;

        [Tooltip("Forward offset from hands (meters). Keeps the weapon fully out in front of the chest.")]
        [SerializeField] private float forwardShift = 0.10f;

        [Tooltip("Vertical drop below hands (meters). Keeps face, mouth, and eyes 100% visible.")]
        [SerializeField] private float verticalDrop = 0.10f;

        [Tooltip("Lateral shift right/left (meters).")]
        [SerializeField] private float lateralShift = -0.02f;

        [Header("Hero Stance Rotation")]
        [Tooltip("Blend toward character forward: 0 = along hands vector, 1 = full forward.")]
        [SerializeField, Range(0f, 1f)] private float forwardAngleBlend = 0.35f;

        [Tooltip("Pitch angle (degrees). Subtle downward tilt.")]
        [SerializeField] private float pitchAngle = 6f;

        [Tooltip("Roll angle (degrees).")]
        [SerializeField] private float rollAngle = -6f;

        [Tooltip("Manual yaw fine tune (degrees).")]
        [SerializeField] private float yawFineTune = 0f;

        [Header("Tactical Weapon Flashlight ('Flash Effect')")]
        [Tooltip("Restores the tactical weapon flashlight beam pointing down the street.")]
        [SerializeField] private bool enableTacticalFlashlight = true;

        [Tooltip("Flashlight intensity (spotlight lumen power).")]
        [SerializeField, Range(1f, 8f)] private float flashlightIntensity = 3.6f;

        [Tooltip("Flashlight range down the street (meters).")]
        [SerializeField, Range(15f, 90f)] private float flashlightRange = 50f;

        [Tooltip("Flashlight spot angle (degrees).")]
        [SerializeField, Range(20f, 90f)] private float flashlightSpotAngle = 50f;

        [Tooltip("Flashlight color tone.")]
        [SerializeField] private Color flashlightColor = new Color(0.96f, 0.98f, 1.0f);

        [Tooltip("Forward offset along weapon barrel (meters).")]
        [SerializeField] private float flashlightForwardOffset = 0.46f;

        [Tooltip("Vertical offset from weapon center (meters).")]
        [SerializeField] private float flashlightVerticalOffset = 0.04f;

        [Header("Diagnostics")]
        [SerializeField] private bool logDiagnostics = false;

        private Animator animator;
        private Transform rightHandTransform;
        private Transform leftHandTransform;
        private GameObject weaponInstance;
        private GameObject flashlightMountObj;
        private Light flashlightMainLight;
        private Light flashlightFillLight;
        private Light flashlightBulbLight;
        private Vector3 computedScale = new Vector3(0.0007f, 0.0007f, 0.0007f);

        public GameObject WeaponInstance => weaponInstance;
        public Transform HandTransform => rightHandTransform;
        public bool ShowWeapon
        {
            get => showWeapon;
            set
            {
                showWeapon = value;
                if (weaponInstance != null) weaponInstance.SetActive(showWeapon);
            }
        }

        private void Awake()
        {
            AttachWeapon();
        }

        private void Start()
        {
            if (weaponInstance == null)
            {
                AttachWeapon();
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponTransform();
        }

        private void OnValidate()
        {
            if (weaponInstance != null)
            {
                weaponInstance.SetActive(showWeapon);
                UpdateWeaponTransform();
            }
        }

        [ContextMenu("Snap Weapon To Stance")]
        public void AttachWeapon()
        {
            ResolveReferences();

            if (rightHandTransform == null)
            {
                if (logDiagnostics)
                    Debug.LogWarning("[MenuWeaponSocket] RightHand bone not found on character!");
                return;
            }

            // Remove any existing weapon instance under character or hand
            Transform existing = transform.Find("MenuWeapon_TommyGun");
            if (existing == null && rightHandTransform != null)
            {
                existing = rightHandTransform.Find("MenuWeapon_TommyGun");
            }

            if (existing != null)
            {
                weaponInstance = existing.gameObject;
                weaponInstance.transform.SetParent(transform, true);
            }
            else
            {
                GameObject sourcePrefab = weaponPrefab;
                if (sourcePrefab == null && !string.IsNullOrEmpty(resourcesPath))
                {
                    sourcePrefab = Resources.Load<GameObject>(resourcesPath);
                }

                if (sourcePrefab == null)
                {
                    #if UNITY_EDITOR
                    sourcePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/ZombieRunner/Models/Weapons/TommyGun/source/Tommy Gun.fbx");
                    #endif
                }

                if (sourcePrefab == null)
                {
                    Debug.LogError("[MenuWeaponSocket] Failed to load Tommy Gun model from Prefab, Resources, or AssetDatabase!");
                    return;
                }

                // Parent under character root (scale 1,1,1) to avoid non-uniform bone scale skewing
                weaponInstance = Instantiate(sourcePrefab, transform);
                weaponInstance.name = "MenuWeapon_TommyGun";
            }

            weaponInstance.SetActive(showWeapon);

            // Setup layer and shadows
            int charLayer = gameObject.layer;
            SetLayerRecursively(weaponInstance, charLayer);

            // Calculate auto-normalization scale so length matches targetWeaponLength
            ComputeNormalizationScale();

            // Apply materials if available
            ApplyWeaponMaterials();

            // Position and orient weapon
            UpdateWeaponTransform();

            if (logDiagnostics)
            {
                Debug.Log($"[MenuWeaponSocket] Tommy Gun successfully attached. Scale: {weaponInstance.transform.localScale}");
            }
        }

        public void UpdateWeaponTransform()
        {
            if (weaponInstance == null) return;
            weaponInstance.SetActive(showWeapon);
            if (!showWeapon) return;

            ResolveReferences();
            if (rightHandTransform == null) return;

            Vector3 charForward = transform.forward;
            Vector3 charRight = transform.right;
            Vector3 charUp = Vector3.up;

            if (leftHandTransform != null)
            {
                Vector3 rPos = rightHandTransform.position;
                Vector3 lPos = leftHandTransform.position;

                // Position: anchor in the middle of the hands, pushed forward in front of chest
                Vector3 centerHands = Vector3.Lerp(rPos, lPos, handCenterBias);
                Vector3 finalPos = centerHands
                                   + charForward * forwardShift
                                   - charUp * verticalDrop
                                   + charRight * lateralShift;

                weaponInstance.transform.position = finalPos;

                // Rotation: blend between the hands vector and character forward
                // In FBX model space, model length is along X, so aim vector maps to gun orientation
                Vector3 handAim = (lPos - rPos).normalized;
                Vector3 blendedAim = Vector3.Slerp(handAim, charForward, forwardAngleBlend).normalized;

                // Look along blended aim with subtle pitch/roll
                Quaternion aimRot = Quaternion.LookRotation(blendedAim, charUp);
                aimRot = aimRot * Quaternion.Euler(pitchAngle, yawFineTune, rollAngle);

                weaponInstance.transform.rotation = aimRot;
                weaponInstance.transform.localScale = computedScale * scaleMultiplier;

                UpdateTacticalFlashlight(blendedAim);
            }
            else
            {
                // Fallback to single hand
                weaponInstance.transform.position = rightHandTransform.position + charForward * forwardShift - charUp * verticalDrop;
                weaponInstance.transform.rotation = Quaternion.LookRotation(charForward, charUp);
                weaponInstance.transform.localScale = computedScale * scaleMultiplier;

                UpdateTacticalFlashlight(charForward);
            }
        }

        private void OnDisable()
        {
            if (flashlightMountObj != null)
            {
                flashlightMountObj.SetActive(false);
            }
        }

        private void UpdateTacticalFlashlight(Vector3 aimDir)
        {
            if (weaponInstance == null || !showWeapon || !enableTacticalFlashlight)
            {
                if (flashlightMountObj != null) flashlightMountObj.SetActive(false);
                return;
            }

            if (flashlightMountObj == null)
            {
                Transform existing = transform.Find("MenuWeapon_TacticalFlashlight");
                if (existing != null)
                {
                    flashlightMountObj = existing.gameObject;
                }
                else
                {
                    flashlightMountObj = new GameObject("MenuWeapon_TacticalFlashlight");
                    flashlightMountObj.transform.SetParent(transform, true);
                }
            }

            flashlightMountObj.SetActive(true);

            Vector3 muzzlePos = weaponInstance.transform.position
                                + aimDir * flashlightForwardOffset
                                + Vector3.up * flashlightVerticalOffset;

            flashlightMountObj.transform.position = muzzlePos;
            flashlightMountObj.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

            // 1. Focused Primary Beam
            if (flashlightMainLight == null)
            {
                Transform beamT = flashlightMountObj.transform.Find("Tactical_Flashlight_Beam");
                if (beamT == null)
                {
                    GameObject go = new GameObject("Tactical_Flashlight_Beam");
                    go.transform.SetParent(flashlightMountObj.transform, false);
                    flashlightMainLight = go.AddComponent<Light>();
                }
                else
                {
                    flashlightMainLight = beamT.GetComponent<Light>();
                }
            }
            flashlightMainLight.type = LightType.Spot;
            flashlightMainLight.color = flashlightColor;
            float jitter = 1f + (Mathf.PerlinNoise(Time.time * 16f, 0.5f) - 0.5f) * 0.04f;
            flashlightMainLight.intensity = flashlightIntensity * jitter;
            flashlightMainLight.range = flashlightRange;
            flashlightMainLight.spotAngle = flashlightSpotAngle;
            flashlightMainLight.innerSpotAngle = flashlightSpotAngle * 0.52f;
            flashlightMainLight.shadows = LightShadows.Soft;

            // 2. Wide Peripheral Spill Light
            if (flashlightFillLight == null)
            {
                Transform fillT = flashlightMountObj.transform.Find("Tactical_Flashlight_Wide_Fill");
                if (fillT == null)
                {
                    GameObject go = new GameObject("Tactical_Flashlight_Wide_Fill");
                    go.transform.SetParent(flashlightMountObj.transform, false);
                    flashlightFillLight = go.AddComponent<Light>();
                }
                else
                {
                    flashlightFillLight = fillT.GetComponent<Light>();
                }
            }
            flashlightFillLight.type = LightType.Spot;
            flashlightFillLight.color = new Color(flashlightColor.r * 0.90f, flashlightColor.g * 0.93f, 1.0f);
            flashlightFillLight.intensity = flashlightIntensity * 0.35f;
            flashlightFillLight.range = flashlightRange * 0.72f;
            flashlightFillLight.spotAngle = flashlightSpotAngle * 1.55f;
            flashlightFillLight.innerSpotAngle = flashlightSpotAngle * 0.8f;
            flashlightFillLight.shadows = LightShadows.None;

            // 3. Muzzle / Bulb Glare Point
            if (flashlightBulbLight == null)
            {
                Transform bulbT = flashlightMountObj.transform.Find("Tactical_Flashlight_Bulb");
                if (bulbT == null)
                {
                    GameObject go = new GameObject("Tactical_Flashlight_Bulb");
                    go.transform.SetParent(flashlightMountObj.transform, false);
                    flashlightBulbLight = go.AddComponent<Light>();
                }
                else
                {
                    flashlightBulbLight = bulbT.GetComponent<Light>();
                }
            }
            flashlightBulbLight.type = LightType.Point;
            flashlightBulbLight.color = flashlightColor;
            flashlightBulbLight.intensity = 2.0f;
            flashlightBulbLight.range = 0.85f;
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null && animator.isHuman)
            {
                rightHandTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
                leftHandTransform = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            }

            if (rightHandTransform == null)
            {
                rightHandTransform = FindChildRecursive(transform, "mixamorig:RightHand");
            }
            if (rightHandTransform == null)
            {
                rightHandTransform = FindChildRecursive(transform, "RightHand");
            }

            if (leftHandTransform == null)
            {
                leftHandTransform = FindChildRecursive(transform, "mixamorig:LeftHand");
            }
            if (leftHandTransform == null)
            {
                leftHandTransform = FindChildRecursive(transform, "LeftHand");
            }
        }

        private void ComputeNormalizationScale()
        {
            if (weaponInstance == null) return;

            Renderer[] renderers = weaponInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            // Measure unscaled bounds size
            Vector3 prevScale = weaponInstance.transform.localScale;
            weaponInstance.transform.localScale = Vector3.one;

            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combined.Encapsulate(renderers[i].bounds);
            }

            float maxDimension = Mathf.Max(combined.size.x, combined.size.y, combined.size.z);
            if (maxDimension > 0.001f)
            {
                float factor = targetWeaponLength / maxDimension;
                computedScale = new Vector3(factor, factor, factor);
            }
            else
            {
                computedScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            }

            weaponInstance.transform.localScale = computedScale * scaleMultiplier;
        }

        private void ApplyWeaponMaterials()
        {
            if (weaponInstance == null) return;

            if (bodyMaterial == null)
            {
                bodyMaterial = Resources.Load<Material>("MenuWeapons/TommyGun_Body");
                #if UNITY_EDITOR
                if (bodyMaterial == null)
                    bodyMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/ZombieRunner/Models/Weapons/TommyGun/TommyGun_Body.mat");
                #endif
            }

            if (drumMaterial == null)
            {
                drumMaterial = Resources.Load<Material>("MenuWeapons/TommyGun_Drum");
                #if UNITY_EDITOR
                if (drumMaterial == null)
                    drumMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/ZombieRunner/Models/Weapons/TommyGun/TommyGun_Drum.mat");
                #endif
            }

            if (bulletMaterial == null)
            {
                bulletMaterial = Resources.Load<Material>("MenuWeapons/TommyGun_Bullet");
                #if UNITY_EDITOR
                if (bulletMaterial == null)
                    bulletMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/ZombieRunner/Models/Weapons/TommyGun/TommyGun_Bullet.mat");
                #endif
            }

            Renderer[] renderers = weaponInstance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                string rName = r.name.ToLower();
                if (rName.Contains("bullet") && bulletMaterial != null)
                {
                    r.sharedMaterial = bulletMaterial;
                }
                else if ((rName.Contains("mag") || rName.Contains("drum") || rName.Contains("cylinder")) && drumMaterial != null)
                {
                    r.sharedMaterial = drumMaterial;
                }
                else if (bodyMaterial != null)
                {
                    r.sharedMaterial = bodyMaterial;
                }
            }
        }

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null) return null;
            if (parent.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null) return found;
            }

            return null;
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
