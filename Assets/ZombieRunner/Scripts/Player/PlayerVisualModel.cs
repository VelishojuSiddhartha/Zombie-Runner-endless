using System;
using UnityEngine;
using ZombieRunner.Weapons;

namespace ZombieRunner.Player
{
    /// <summary>
    /// Guaranteed visual representation and procedural locomotion animator for the player.
    /// If Mixamo FBX models are missing or unimported Git-LFS pointers (0 vertices),
    /// this component automatically constructs a high-detail 3D Survivor model with tactical armor,
    /// animated limbs, weapon, and responsive running, jumping, sliding, and banking kinematics.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerVisualModel : MonoBehaviour
    {
        [Header("Materials (Optional Overrides)")]
        [SerializeField] private Material armorMaterial;
        [SerializeField] private Material jointsMaterial;
        [SerializeField] private Material weaponMaterial;
        [SerializeField] private Material visorGlowMaterial;

        [Header("Animation Tuning")]
        [SerializeField, Range(1f, 25f)] private float strideSpeedFactor = 1.35f;
        [SerializeField, Range(10f, 60f)] private float maxLegSwingAngle = 36f;
        [SerializeField, Range(10f, 60f)] private float maxArmSwingAngle = 28f;
        [SerializeField, Range(0.01f, 0.15f)] private float runningBobHeight = 0.055f;
        [SerializeField, Range(5f, 25f)] private float laneBankingAngle = 14f;

        private PlayerController playerController;
        private CharacterController characterController;
        private ShootingController shootingController;

        private Transform visualRoot;
        private Transform torso;
        private Transform head;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForearm;
        private Transform rightForearm;
        private Transform leftThigh;
        private Transform rightThigh;
        private Transform leftCalf;
        private Transform rightCalf;
        private Transform weaponRoot;

        private Vector3 initialTorsoPos;
        private Vector3 initialVisualPos;
        private Quaternion initialLeftArmRot;
        private Quaternion initialRightArmRot;

        private float strideTimer;
        private float currentBankAngle;
        private float recoilAmount;
        private bool isProceduralModel = false;
        private bool hasExternalVisibleMesh = false;

        public bool IsProceduralModel => isProceduralModel;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            characterController = GetComponent<CharacterController>();
            shootingController = GetComponent<ShootingController>();

            EnsureVisualRepresentation();
        }

        private void Start()
        {
            EnsureVisualRepresentation();
        }

        public void EnsureVisualRepresentation()
        {
            if (visualRoot != null && isProceduralModel)
                return;

            // Check if our procedural model is already built
            Transform existingProc = transform.Find("Visual - Procedural Survivor");
            if (existingProc != null)
            {
                if (existingProc.childCount > 0)
                {
                    visualRoot = existingProc;
                    isProceduralModel = true;
                    CacheLimbReferences();
                    return;
                }
                else
                {
                    DestroyImmediate(existingProc.gameObject);
                }
            }

            // Check if there is already a functional, visible 3D mesh attached
            if (HasFunctionalMesh(gameObject))
            {
                hasExternalVisibleMesh = true;
                return;
            }

            BuildProceduralSurvivor();
        }

        private static bool HasFunctionalMesh(GameObject root)
        {
            if (root == null) return false;

            // Check skinned mesh renderers with real vertices
            SkinnedMeshRenderer[] smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in smrs)
            {
                if (smr != null && smr.enabled && smr.sharedMesh != null && smr.sharedMesh.vertexCount > 0)
                {
                    if (smr.gameObject.activeInHierarchy || smr.transform.parent != null)
                        return true;
                }
            }

            // Check static mesh filters with active mesh renderers (exclude weapons and placeholders)
            MeshFilter[] mfs = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in mfs)
            {
                if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                {
                    string objName = mf.gameObject.name;
                    if (objName.Contains("Muzzle") || objName.Contains("Weapon") || objName.Contains("Placeholder") || objName.Contains("Procedural"))
                        continue;

                    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null && mr.enabled && (mr.gameObject.activeInHierarchy || mr.gameObject.activeSelf))
                    {
                        if (mf.sharedMesh.vertexCount > 24)
                            return true;
                    }
                }
            }

            return false;
        }

        private void BuildProceduralSurvivor()
        {
            isProceduralModel = true;

            // Load best matching materials from project assets
            ResolveMaterials();

            // Root at feet level (-1.0f on 2.0m CharacterController)
            GameObject rootObj = new GameObject("Visual - Procedural Survivor");
            rootObj.transform.SetParent(transform, false);
            rootObj.transform.localPosition = new Vector3(0f, -1.0f, 0f);
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;
            visualRoot = rootObj.transform;
            initialVisualPos = visualRoot.localPosition;

            // 1. Pelvis / Hips
            GameObject pelvisObj = CreatePart(PrimitiveType.Cube, "Pelvis", visualRoot,
                new Vector3(0f, 0.95f, 0f), new Vector3(0.38f, 0.16f, 0.24f), jointsMaterial);

            // 2. Torso (Tactical Assault Vest)
            GameObject torsoObj = CreatePart(PrimitiveType.Cube, "Torso", visualRoot,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.44f, 0.44f, 0.28f), armorMaterial);
            torso = torsoObj.transform;
            initialTorsoPos = torso.localPosition;

            // Tactical Armor Chest Plate
            CreatePart(PrimitiveType.Cube, "ArmorPlate", torso,
                new Vector3(0f, 0.04f, 0.15f), new Vector3(0.36f, 0.28f, 0.08f), jointsMaterial);

            // Tactical Ammo Pouches
            CreatePart(PrimitiveType.Cube, "Pouch_L", torso,
                new Vector3(-0.14f, -0.15f, 0.15f), new Vector3(0.10f, 0.10f, 0.06f), jointsMaterial);
            CreatePart(PrimitiveType.Cube, "Pouch_R", torso,
                new Vector3(0.14f, -0.15f, 0.15f), new Vector3(0.10f, 0.10f, 0.06f), jointsMaterial);

            // 3. Head & Tactical Visor
            GameObject headObj = CreatePart(PrimitiveType.Sphere, "Head", torso,
                new Vector3(0f, 0.38f, 0.02f), new Vector3(0.26f, 0.28f, 0.26f), armorMaterial);
            head = headObj.transform;

            // Glowing Tactical Eye Visor
            CreatePart(PrimitiveType.Cube, "Visor", head,
                new Vector3(0f, 0.03f, 0.12f), new Vector3(0.20f, 0.06f, 0.06f), visorGlowMaterial);

            // Tactical Helmet Brim
            CreatePart(PrimitiveType.Cube, "HelmetBrim", head,
                new Vector3(0f, 0.12f, 0.04f), new Vector3(0.28f, 0.07f, 0.30f), jointsMaterial);

            // 4. Arms
            // Left Arm (holding front gun handguard)
            GameObject lShoulder = new GameObject("Shoulder_L");
            lShoulder.transform.SetParent(torso, false);
            lShoulder.transform.localPosition = new Vector3(-0.28f, 0.14f, 0f);
            leftArm = lShoulder.transform;

            CreatePart(PrimitiveType.Capsule, "UpperArm_L", leftArm,
                new Vector3(0f, -0.14f, 0f), new Vector3(0.11f, 0.16f, 0.11f), armorMaterial);

            GameObject lElbow = new GameObject("Forearm_L");
            lElbow.transform.SetParent(leftArm, false);
            lElbow.transform.localPosition = new Vector3(0f, -0.28f, 0f);
            leftForearm = lElbow.transform;
            CreatePart(PrimitiveType.Capsule, "ForearmMesh_L", leftForearm,
                new Vector3(0.08f, -0.10f, 0.14f), new Vector3(0.10f, 0.15f, 0.10f), jointsMaterial);

            // Right Arm (holding grip & trigger)
            GameObject rShoulder = new GameObject("Shoulder_R");
            rShoulder.transform.SetParent(torso, false);
            rShoulder.transform.localPosition = new Vector3(0.28f, 0.14f, 0f);
            rightArm = rShoulder.transform;

            CreatePart(PrimitiveType.Capsule, "UpperArm_R", rightArm,
                new Vector3(0f, -0.14f, 0f), new Vector3(0.11f, 0.16f, 0.11f), armorMaterial);

            GameObject rElbow = new GameObject("Forearm_R");
            rElbow.transform.SetParent(rightArm, false);
            rElbow.transform.localPosition = new Vector3(0f, -0.28f, 0f);
            rightForearm = rElbow.transform;
            CreatePart(PrimitiveType.Capsule, "ForearmMesh_R", rightForearm,
                new Vector3(-0.04f, -0.08f, 0.16f), new Vector3(0.10f, 0.15f, 0.10f), jointsMaterial);

            // Orient arms to hold gun forward
            leftArm.localRotation = Quaternion.Euler(38f, 24f, -12f);
            rightArm.localRotation = Quaternion.Euler(32f, -18f, 10f);
            initialLeftArmRot = leftArm.localRotation;
            initialRightArmRot = rightArm.localRotation;

            // 5. Weapon (Submachine Gun / Combat Rifle)
            GameObject weaponObj = new GameObject("Weapon_Rifle");
            weaponObj.transform.SetParent(torso, false);
            weaponObj.transform.localPosition = new Vector3(0.16f, -0.05f, 0.42f);
            weaponObj.transform.localRotation = Quaternion.Euler(-4f, -2f, 0f);
            weaponRoot = weaponObj.transform;

            // Receiver & Barrel
            CreatePart(PrimitiveType.Cube, "Receiver", weaponRoot,
                Vector3.zero, new Vector3(0.08f, 0.12f, 0.46f), weaponMaterial);
            CreatePart(PrimitiveType.Cylinder, "Barrel", weaponRoot,
                new Vector3(0f, 0.02f, 0.32f), new Vector3(0.04f, 0.14f, 0.04f), weaponMaterial, Quaternion.Euler(90f, 0f, 0f));
            CreatePart(PrimitiveType.Cube, "Magazine", weaponRoot,
                new Vector3(0f, -0.14f, 0.06f), new Vector3(0.05f, 0.18f, 0.10f), jointsMaterial, Quaternion.Euler(14f, 0f, 0f));
            CreatePart(PrimitiveType.Cube, "Stock", weaponRoot,
                new Vector3(0f, -0.02f, -0.28f), new Vector3(0.06f, 0.12f, 0.20f), armorMaterial);

            // Connect weapon to ShootingController muzzle if present
            Transform muzzleTrans = transform.Find("Muzzle") ?? weaponRoot.Find("Muzzle");
            if (muzzleTrans == null)
            {
                GameObject muzzleObj = new GameObject("Muzzle");
                muzzleObj.transform.SetParent(weaponRoot, false);
                muzzleObj.transform.localPosition = new Vector3(0f, 0.02f, 0.48f);
                muzzleTrans = muzzleObj.transform;
            }

            // 6. Legs
            // Left Leg
            GameObject lHip = new GameObject("Hip_L");
            lHip.transform.SetParent(visualRoot, false);
            lHip.transform.localPosition = new Vector3(-0.16f, 0.88f, 0f);
            leftThigh = lHip.transform;

            CreatePart(PrimitiveType.Capsule, "Thigh_L", leftThigh,
                new Vector3(0f, -0.20f, 0f), new Vector3(0.14f, 0.22f, 0.14f), armorMaterial);

            GameObject lKnee = new GameObject("Knee_L");
            lKnee.transform.SetParent(leftThigh, false);
            lKnee.transform.localPosition = new Vector3(0f, -0.40f, 0f);
            leftCalf = lKnee.transform;

            CreatePart(PrimitiveType.Capsule, "Calf_L", leftCalf,
                new Vector3(0f, -0.18f, 0f), new Vector3(0.12f, 0.20f, 0.12f), jointsMaterial);
            CreatePart(PrimitiveType.Cube, "Boot_L", leftCalf,
                new Vector3(0f, -0.38f, 0.06f), new Vector3(0.14f, 0.10f, 0.24f), jointsMaterial);

            // Right Leg
            GameObject rHip = new GameObject("Hip_R");
            rHip.transform.SetParent(visualRoot, false);
            rHip.transform.localPosition = new Vector3(0.16f, 0.88f, 0f);
            rightThigh = rHip.transform;

            CreatePart(PrimitiveType.Capsule, "Thigh_R", rightThigh,
                new Vector3(0f, -0.20f, 0f), new Vector3(0.14f, 0.22f, 0.14f), armorMaterial);

            GameObject rKnee = new GameObject("Knee_R");
            rKnee.transform.SetParent(rightThigh, false);
            rKnee.transform.localPosition = new Vector3(0f, -0.40f, 0f);
            rightCalf = rKnee.transform;

            CreatePart(PrimitiveType.Capsule, "Calf_R", rightCalf,
                new Vector3(0f, -0.18f, 0f), new Vector3(0.12f, 0.20f, 0.12f), jointsMaterial);
            CreatePart(PrimitiveType.Cube, "Boot_R", rightCalf,
                new Vector3(0f, -0.38f, 0.06f), new Vector3(0.14f, 0.10f, 0.24f), jointsMaterial);

            Debug.Log("[ZombieRunner] PlayerVisualModel -> Successfully generated high-detail 3D Survivor Model with procedural rig.");
        }

        private GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Quaternion? localRot = null)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            obj.transform.localRotation = localRot ?? Quaternion.identity;

            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                if (mat != null) mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                mr.receiveShadows = true;
            }

            return obj;
        }

        private void ResolveMaterials()
        {
            if (armorMaterial == null)
            {
                armorMaterial = FindMaterialByName("WR_PlayerXBot_Surface") 
                    ?? FindMaterialByName("WR_PlayerPlaceholder")
                    ?? CreateFallbackMaterial(new Color(0.24f, 0.28f, 0.35f, 1f), 0.35f, 0.65f);
            }

            if (jointsMaterial == null)
            {
                jointsMaterial = FindMaterialByName("WR_PlayerXBot_Joints") 
                    ?? CreateFallbackMaterial(new Color(0.12f, 0.13f, 0.15f, 1f), 0.6f, 0.75f);
            }

            if (weaponMaterial == null)
            {
                weaponMaterial = FindMaterialByName("WR_WeaponMetal") 
                    ?? CreateFallbackMaterial(new Color(0.15f, 0.15f, 0.18f, 1f), 0.85f, 0.8f);
            }

            if (visorGlowMaterial == null)
            {
                visorGlowMaterial = CreateEmissiveMaterial(new Color(0.1f, 0.9f, 1.0f, 1f), 4.5f);
            }
        }

        private Material FindMaterialByName(string matName)
        {
            Material mat = Resources.Load<Material>("Materials/" + matName);
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color bc = mat.GetColor("_BaseColor");
                    if (bc.a < 0.1f)
                    {
                        bc.a = 1f;
                        mat.SetColor("_BaseColor", bc);
                    }
                }
                return mat;
            }

            Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material m in mats)
            {
                if (m != null && m.name.Equals(matName, StringComparison.OrdinalIgnoreCase))
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color bc = m.GetColor("_BaseColor");
                        if (bc.a < 0.1f)
                        {
                            bc.a = 1f;
                            m.SetColor("_BaseColor", bc);
                        }
                    }
                    return m;
                }
            }
            return null;
        }

        private Material CreateFallbackMaterial(Color color, float metallic, float smoothness)
        {
            color.a = 1f;
            Shader uShader = Shader.Find("Universal Render Pipeline/Lit") 
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");

            Material mat = new Material(uShader);
            mat.name = "M_Survivor_Procedural";
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private Material CreateEmissiveMaterial(Color glowColor, float intensity)
        {
            glowColor.a = 1f;
            Shader uShader = Shader.Find("Universal Render Pipeline/Lit") 
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");

            Material mat = new Material(uShader);
            mat.name = "M_Survivor_VisorGlow";
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", glowColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", glowColor);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColor * intensity);
            }
            return mat;
        }

        private void CacheLimbReferences()
        {
            if (visualRoot == null) return;
            torso = visualRoot.Find("Torso");
            if (torso != null)
            {
                initialTorsoPos = torso.localPosition;
                head = torso.Find("Head");
                leftArm = torso.Find("Shoulder_L");
                rightArm = torso.Find("Shoulder_R");
                if (leftArm != null)
                {
                    leftForearm = leftArm.Find("Forearm_L");
                    initialLeftArmRot = leftArm.localRotation;
                }
                if (rightArm != null)
                {
                    rightForearm = rightArm.Find("Forearm_R");
                    initialRightArmRot = rightArm.localRotation;
                }
                weaponRoot = torso.Find("Weapon_Rifle");
            }
            leftThigh = visualRoot.Find("Hip_L");
            if (leftThigh != null) leftCalf = leftThigh.Find("Knee_L");
            rightThigh = visualRoot.Find("Hip_R");
            if (rightThigh != null) rightCalf = rightThigh.Find("Knee_R");
        }

        private void Update()
        {
            if (hasExternalVisibleMesh || !isProceduralModel || visualRoot == null)
                return;

            AnimateProceduralLimbs();
        }

        private void AnimateProceduralLimbs()
        {
            float dt = Time.deltaTime;
            bool isGrounded = characterController != null && characterController.isGrounded;
            bool isSliding = playerController != null && playerController.IsSliding;
            float forwardSpeed = playerController != null ? playerController.CurrentForwardSpeed : 0f;
            bool isMoving = forwardSpeed > 0.5f;

            // 1. Sliding Kinematics (Drop low, extend legs, tuck under barriers)
            if (isSliding)
            {
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, initialVisualPos + Vector3.down * 0.45f, dt * 16f);
                if (torso != null)
                {
                    torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(-32f, 0f, 0f), dt * 14f);
                }
                if (leftThigh != null)
                {
                    leftThigh.localRotation = Quaternion.Slerp(leftThigh.localRotation, Quaternion.Euler(-65f, 15f, 0f), dt * 14f);
                }
                if (rightThigh != null)
                {
                    rightThigh.localRotation = Quaternion.Slerp(rightThigh.localRotation, Quaternion.Euler(45f, -10f, 0f), dt * 14f);
                }
                if (weaponRoot != null)
                {
                    weaponRoot.localRotation = Quaternion.Slerp(weaponRoot.localRotation, Quaternion.Euler(-25f, 0f, 0f), dt * 14f);
                }
                return;
            }

            // Return from slide
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, initialVisualPos, dt * 10f);

            // 2. Jumping Kinematics (Tuck legs up, brace arms)
            if (!isGrounded && isMoving)
            {
                if (leftThigh != null)
                    leftThigh.localRotation = Quaternion.Slerp(leftThigh.localRotation, Quaternion.Euler(-42f, 5f, 0f), dt * 12f);
                if (rightThigh != null)
                    rightThigh.localRotation = Quaternion.Slerp(rightThigh.localRotation, Quaternion.Euler(-42f, -5f, 0f), dt * 12f);
                if (leftCalf != null)
                    leftCalf.localRotation = Quaternion.Slerp(leftCalf.localRotation, Quaternion.Euler(55f, 0f, 0f), dt * 12f);
                if (rightCalf != null)
                    rightCalf.localRotation = Quaternion.Slerp(rightCalf.localRotation, Quaternion.Euler(55f, 0f, 0f), dt * 12f);
                if (torso != null)
                    torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(8f, 0f, 0f), dt * 8f);
                return;
            }

            // 3. Running Kinematics (Natural stride, arm swing, athletic vertical bob)
            if (isMoving)
            {
                strideTimer += dt * forwardSpeed * strideSpeedFactor;
                float legCycle = Mathf.Sin(strideTimer);
                float armCycle = -legCycle;
                float bobCycle = Mathf.Abs(Mathf.Sin(strideTimer));

                // Leg swing
                if (leftThigh != null)
                {
                    float angle = legCycle * maxLegSwingAngle;
                    leftThigh.localRotation = Quaternion.Euler(angle, 0f, 0f);
                    if (leftCalf != null)
                        leftCalf.localRotation = Quaternion.Euler(Mathf.Max(0f, -legCycle) * 35f, 0f, 0f);
                }
                if (rightThigh != null)
                {
                    float angle = -legCycle * maxLegSwingAngle;
                    rightThigh.localRotation = Quaternion.Euler(angle, 0f, 0f);
                    if (rightCalf != null)
                        rightCalf.localRotation = Quaternion.Euler(Mathf.Max(0f, legCycle) * 35f, 0f, 0f);
                }

                // Arm swing in counter-motion to legs
                if (leftArm != null)
                {
                    leftArm.localRotation = initialLeftArmRot * Quaternion.Euler(armCycle * maxArmSwingAngle, 0f, 0f);
                }
                if (rightArm != null)
                {
                    rightArm.localRotation = initialRightArmRot * Quaternion.Euler(-armCycle * (maxArmSwingAngle * 0.7f), 0f, 0f);
                }

                // Torso & head running bounce and slight forward lean
                if (torso != null)
                {
                    torso.localPosition = initialTorsoPos + Vector3.up * (bobCycle * runningBobHeight);
                    torso.localRotation = Quaternion.Euler(6f, Mathf.Sin(strideTimer * 0.5f) * 3f, 0f);
                }
            }
            else
            {
                // Idle breathing for Main Menu & stopped state
                strideTimer += dt * 1.8f;
                float breathe = Mathf.Sin(strideTimer);
                if (torso != null)
                {
                    torso.localPosition = initialTorsoPos + Vector3.up * (breathe * 0.015f);
                    torso.localRotation = Quaternion.Euler(breathe * 1.2f, 0f, 0f);
                }
                if (leftThigh != null) leftThigh.localRotation = Quaternion.Slerp(leftThigh.localRotation, Quaternion.identity, dt * 6f);
                if (rightThigh != null) rightThigh.localRotation = Quaternion.Slerp(rightThigh.localRotation, Quaternion.identity, dt * 6f);
                if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, initialLeftArmRot, dt * 6f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, initialRightArmRot, dt * 6f);
            }

            // 4. Lane Banking (tilt body smoothly during lane changes)
            if (playerController != null)
            {
                float targetX = playerController.CurrentLane * 3f;
                float diffX = targetX - transform.position.x;
                float targetBank = Mathf.Clamp(-diffX * 6f, -laneBankingAngle, laneBankingAngle);
                currentBankAngle = Mathf.Lerp(currentBankAngle, targetBank, dt * 12f);
                visualRoot.localRotation = Quaternion.Euler(0f, 0f, currentBankAngle);
            }

            // 5. Weapon Recoil Recovery
            if (recoilAmount > 0.001f)
            {
                recoilAmount = Mathf.Lerp(recoilAmount, 0f, dt * 18f);
                if (weaponRoot != null)
                    weaponRoot.localPosition = new Vector3(0.16f, -0.05f, 0.42f - recoilAmount);
            }
        }

        /// <summary>Invoked by ShootingController on fire for visual kickback recoil.</summary>
        public void TriggerWeaponRecoil()
        {
            recoilAmount = 0.08f;
        }
    }
}
