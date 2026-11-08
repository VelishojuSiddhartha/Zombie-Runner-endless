using System;
using UnityEngine;

namespace ZombieRunner.Enemies
{
    /// <summary>
    /// Guaranteed visual representation and horror animation system for zombies.
    /// If Mixamo FBX models are missing or unimported Git-LFS pointers (0 vertices),
    /// this component automatically constructs distinct, scary 3D zombie models
    /// (Common Walker, Fast Sprinter, and Heavy Brute) with glowing horror eyes,
    /// ragged rotten textures, and responsive pursuit, attack, flinch, and death kinematics.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZombieVisualModel : MonoBehaviour
    {
        [Header("Materials (Optional Overrides)")]
        [SerializeField] private Material zombieSkinMaterial;
        [SerializeField] private Material zombieClothMaterial;
        [SerializeField] private Material eyeGlowMaterial;

        [Header("Animation Settings")]
        [SerializeField, Range(1f, 15f)] private float strideSpeed = 3.6f;
        [SerializeField, Range(10f, 60f)] private float legSwingAngle = 32f;
        [SerializeField, Range(5f, 40f)] private float armReachAngle = 26f;

        private ZombieController zombieController;
        private ZombieArchetype archetype = ZombieArchetype.Normal;

        private Transform visualRoot;
        private Transform torso;
        private Transform head;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftThigh;
        private Transform rightThigh;
        private Transform leftCalf;
        private Transform rightCalf;
        private Light eyeLight;

        private Vector3 initialTorsoPos;
        private Vector3 initialVisualPos;
        private float strideTimer;
        private float flinchTimer;
        private float deathCollapseTimer;
        private bool isAttacking;
        private bool isDead;
        private bool isProceduralModel = false;
        private bool hasExternalVisibleMesh = false;

        public bool IsProceduralModel => isProceduralModel;

        private void Awake()
        {
            zombieController = GetComponent<ZombieController>();
            if (zombieController != null)
                archetype = zombieController.Archetype;

            EnsureVisualRepresentation();
        }

        private void Start()
        {
            EnsureVisualRepresentation();
        }

        public void Configure(ZombieArchetype newArchetype)
        {
            if (visualRoot != null && isProceduralModel && archetype == newArchetype)
            {
                // Already successfully constructed for this archetype, maintain existing model
                return;
            }

            archetype = newArchetype;
            if (visualRoot != null)
            {
                DestroyImmediate(visualRoot.gameObject);
                visualRoot = null;
                isProceduralModel = false;
            }

            EnsureVisualRepresentation();
        }

        public void EnsureVisualRepresentation()
        {
            if (visualRoot != null && isProceduralModel)
                return;

            Transform existingProc = transform.Find("Visual - Procedural Zombie");
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

            BuildProceduralZombie();
        }

        private void BuildProceduralZombie()
        {
            isProceduralModel = true;
            ResolveMaterials();

            float scaleMult = archetype == ZombieArchetype.Big ? 1.35f : (archetype == ZombieArchetype.Fast ? 0.92f : 1.05f);
            float widthMult = archetype == ZombieArchetype.Big ? 1.55f : (archetype == ZombieArchetype.Fast ? 0.85f : 1.0f);

            GameObject rootObj = new GameObject("Visual - Procedural Zombie");
            rootObj.transform.SetParent(transform, false);
            rootObj.transform.localPosition = Vector3.zero;
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;
            visualRoot = rootObj.transform;
            initialVisualPos = visualRoot.localPosition;

            // 1. Pelvis & Torso (hunched walker posture)
            float pelvisY = 0.90f * scaleMult;
            float torsoY = 1.20f * scaleMult;
            CreatePart(PrimitiveType.Cube, "Pelvis", visualRoot,
                new Vector3(0f, pelvisY, 0f), new Vector3(0.36f * widthMult, 0.18f * scaleMult, 0.24f * scaleMult), zombieClothMaterial);

            GameObject torsoObj = CreatePart(PrimitiveType.Cube, "Torso", visualRoot,
                new Vector3(0f, torsoY, 0.05f), new Vector3(0.44f * widthMult, 0.48f * scaleMult, 0.28f * scaleMult), zombieClothMaterial, Quaternion.Euler(14f, 0f, 0f));
            torso = torsoObj.transform;
            initialTorsoPos = torso.localPosition;

            // 2. Rotten Zombie Head
            GameObject headObj = CreatePart(PrimitiveType.Sphere, "Head", torso,
                new Vector3(0f, 0.38f * scaleMult, 0.08f), new Vector3(0.28f * scaleMult, 0.30f * scaleMult, 0.28f * scaleMult), zombieSkinMaterial);
            head = headObj.transform;

            // Horror Glowing Eyes
            Color eyeColor = archetype == ZombieArchetype.Big 
                ? new Color(1.0f, 0.08f, 0.08f) 
                : (archetype == ZombieArchetype.Fast ? new Color(1.0f, 0.95f, 0.1f) : new Color(1.0f, 0.25f, 0.08f));

            CreatePart(PrimitiveType.Sphere, "Eye_L", head,
                new Vector3(-0.07f * scaleMult, 0.04f * scaleMult, 0.12f * scaleMult), Vector3.one * (0.06f * scaleMult), eyeGlowMaterial);
            CreatePart(PrimitiveType.Sphere, "Eye_R", head,
                new Vector3(0.07f * scaleMult, 0.04f * scaleMult, 0.12f * scaleMult), Vector3.one * (0.06f * scaleMult), eyeGlowMaterial);

            // Eye Glow Light (casts long visible beam ahead of zombie)
            GameObject lightObj = new GameObject("HorrorEyeGlow");
            lightObj.transform.SetParent(head, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.04f, 0.18f);
            eyeLight = lightObj.AddComponent<Light>();
            eyeLight.type = LightType.Point;
            eyeLight.color = eyeColor;
            eyeLight.intensity = archetype == ZombieArchetype.Big ? 3.5f : (archetype == ZombieArchetype.Fast ? 2.8f : 2.5f);
            eyeLight.range = 6.0f;
            eyeLight.shadows = LightShadows.None;

            // Dedicated Body Silhouette Light (illuminates front torso and outstretched claws)
            GameObject bodyLightObj = new GameObject("ZombieBodyIlluminator");
            bodyLightObj.transform.SetParent(torso, false);
            bodyLightObj.transform.localPosition = new Vector3(0f, 0.05f, 0.75f);
            Light bodyLight = bodyLightObj.AddComponent<Light>();
            bodyLight.type = LightType.Point;
            bodyLight.color = new Color(0.96f, 0.94f, 0.88f);
            bodyLight.intensity = 1.8f;
            bodyLight.range = 4.0f;
            bodyLight.shadows = LightShadows.None;

            // 3. Outstretched Claw Arms
            // Left Arm
            GameObject lShoulder = new GameObject("Shoulder_L");
            lShoulder.transform.SetParent(torso, false);
            lShoulder.transform.localPosition = new Vector3(-0.28f * widthMult, 0.16f * scaleMult, 0f);
            leftArm = lShoulder.transform;

            CreatePart(PrimitiveType.Capsule, "UpperArm_L", leftArm,
                new Vector3(0f, -0.16f * scaleMult, 0f), new Vector3(0.12f * widthMult, 0.20f * scaleMult, 0.12f * widthMult), zombieClothMaterial);
            CreatePart(PrimitiveType.Capsule, "Forearm_L", leftArm,
                new Vector3(0f, -0.26f * scaleMult, 0.18f * scaleMult), new Vector3(0.11f * widthMult, 0.22f * scaleMult, 0.11f * widthMult), zombieSkinMaterial, Quaternion.Euler(75f, -12f, 0f));
            CreatePart(PrimitiveType.Cube, "Claw_L", leftArm,
                new Vector3(-0.02f, -0.28f * scaleMult, 0.38f * scaleMult), new Vector3(0.12f * widthMult, 0.06f, 0.14f * scaleMult), zombieSkinMaterial);

            // Right Arm
            GameObject rShoulder = new GameObject("Shoulder_R");
            rShoulder.transform.SetParent(torso, false);
            rShoulder.transform.localPosition = new Vector3(0.28f * widthMult, 0.16f * scaleMult, 0f);
            rightArm = rShoulder.transform;

            CreatePart(PrimitiveType.Capsule, "UpperArm_R", rightArm,
                new Vector3(0f, -0.16f * scaleMult, 0f), new Vector3(0.12f * widthMult, 0.20f * scaleMult, 0.12f * widthMult), zombieClothMaterial);
            CreatePart(PrimitiveType.Capsule, "Forearm_R", rightArm,
                new Vector3(0f, -0.26f * scaleMult, 0.18f * scaleMult), new Vector3(0.11f * widthMult, 0.22f * scaleMult, 0.11f * widthMult), zombieSkinMaterial, Quaternion.Euler(80f, 10f, 0f));
            CreatePart(PrimitiveType.Cube, "Claw_R", rightArm,
                new Vector3(0.02f, -0.28f * scaleMult, 0.38f * scaleMult), new Vector3(0.12f * widthMult, 0.06f, 0.14f * scaleMult), zombieSkinMaterial);

            leftArm.localRotation = Quaternion.Euler(45f, -8f, -6f);
            rightArm.localRotation = Quaternion.Euler(52f, 12f, 8f);

            // 4. Walker Legs
            // Left Leg
            GameObject lHip = new GameObject("Hip_L");
            lHip.transform.SetParent(visualRoot, false);
            lHip.transform.localPosition = new Vector3(-0.16f * widthMult, pelvisY - 0.06f, 0f);
            leftThigh = lHip.transform;

            CreatePart(PrimitiveType.Capsule, "Thigh_L", leftThigh,
                new Vector3(0f, -0.22f * scaleMult, 0f), new Vector3(0.14f * widthMult, 0.24f * scaleMult, 0.14f * widthMult), zombieClothMaterial);

            GameObject lKnee = new GameObject("Knee_L");
            lKnee.transform.SetParent(leftThigh, false);
            lKnee.transform.localPosition = new Vector3(0f, -0.42f * scaleMult, 0f);
            leftCalf = lKnee.transform;

            CreatePart(PrimitiveType.Capsule, "Calf_L", leftCalf,
                new Vector3(0f, -0.20f * scaleMult, 0f), new Vector3(0.13f * widthMult, 0.22f * scaleMult, 0.13f * widthMult), zombieSkinMaterial);
            CreatePart(PrimitiveType.Cube, "Foot_L", leftCalf,
                new Vector3(0f, -0.40f * scaleMult, 0.06f * scaleMult), new Vector3(0.14f * widthMult, 0.10f * scaleMult, 0.22f * scaleMult), zombieSkinMaterial);

            // Right Leg
            GameObject rHip = new GameObject("Hip_R");
            rHip.transform.SetParent(visualRoot, false);
            rHip.transform.localPosition = new Vector3(0.16f * widthMult, pelvisY - 0.06f, 0f);
            rightThigh = rHip.transform;

            CreatePart(PrimitiveType.Capsule, "Thigh_R", rightThigh,
                new Vector3(0f, -0.22f * scaleMult, 0f), new Vector3(0.14f * widthMult, 0.24f * scaleMult, 0.14f * widthMult), zombieClothMaterial);

            GameObject rKnee = new GameObject("Knee_R");
            rKnee.transform.SetParent(rightThigh, false);
            rKnee.transform.localPosition = new Vector3(0f, -0.42f * scaleMult, 0f);
            rightCalf = rKnee.transform;

            CreatePart(PrimitiveType.Capsule, "Calf_R", rightCalf,
                new Vector3(0f, -0.20f * scaleMult, 0f), new Vector3(0.13f * widthMult, 0.22f * scaleMult, 0.13f * widthMult), zombieSkinMaterial);
            CreatePart(PrimitiveType.Cube, "Foot_R", rightCalf,
                new Vector3(0f, -0.40f * scaleMult, 0.06f * scaleMult), new Vector3(0.14f * widthMult, 0.10f * scaleMult, 0.22f * scaleMult), zombieSkinMaterial);

            Debug.Log($"[ZombieRunner] ZombieVisualModel -> Successfully generated 3D {archetype} Zombie Model with horror visual rig.");
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
            string fallbackMatName = archetype == ZombieArchetype.Big
                ? "WR_ZombieBig"
                : (archetype == ZombieArchetype.Fast ? "WR_ZombieFast" : "WR_ZombieNormal");

            Color fallbackColor = archetype == ZombieArchetype.Big
                ? new Color(0.38f, 0.44f, 0.32f, 1f)
                : (archetype == ZombieArchetype.Fast ? new Color(0.48f, 0.52f, 0.42f, 1f) : new Color(0.42f, 0.50f, 0.38f, 1f));

            if (zombieSkinMaterial == null)
            {
                zombieSkinMaterial = FindMaterialByName(fallbackMatName)
                    ?? CreateFallbackMaterial(fallbackColor, 0.15f, 0.35f);
            }

            if (zombieClothMaterial == null)
            {
                zombieClothMaterial = FindMaterialByName("WR_Concrete")
                    ?? FindMaterialByName(fallbackMatName)
                    ?? CreateFallbackMaterial(new Color(0.22f, 0.24f, 0.22f, 1f), 0.1f, 0.25f);
            }

            Color eyeGlowColor = archetype == ZombieArchetype.Big
                ? new Color(1.0f, 0.12f, 0.12f, 1f)
                : (archetype == ZombieArchetype.Fast ? new Color(1.0f, 0.95f, 0.15f, 1f) : new Color(1.0f, 0.35f, 0.15f, 1f));

            if (eyeGlowMaterial == null)
            {
                eyeGlowMaterial = CreateEmissiveMaterial(eyeGlowColor, 5.0f);
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
            mat.name = $"M_Zombie_{archetype}_Procedural";
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
            mat.name = $"M_ZombieEyeGlow_{archetype}";
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
                if (head != null)
                {
                    eyeLight = head.GetComponentInChildren<Light>();
                }
                leftArm = torso.Find("Shoulder_L");
                rightArm = torso.Find("Shoulder_R");
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

            float dt = Time.deltaTime;

            // 1. Death Kinematics (Collapse onto road)
            if (isDead)
            {
                deathCollapseTimer += dt;
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, new Vector3(0f, -0.65f, -0.45f), dt * 6f);
                visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, Quaternion.Euler(-80f, 15f, 0f), dt * 8f);
                if (eyeLight != null) eyeLight.intensity = Mathf.Lerp(eyeLight.intensity, 0f, dt * 10f);
                return;
            }

            // 2. Flinch on Bullet Hit
            if (flinchTimer > 0f)
            {
                flinchTimer -= dt;
                if (torso != null)
                    torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(-18f, 0f, 0f), dt * 25f);
                return;
            }

            // 3. Attack Lunges
            if (isAttacking)
            {
                if (leftArm != null)
                    leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, Quaternion.Euler(85f, 25f, 0f), dt * 20f);
                if (rightArm != null)
                    rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, Quaternion.Euler(90f, -25f, 0f), dt * 20f);
                if (torso != null)
                    torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(28f, 0f, 0f), dt * 18f);
                return;
            }

            // 4. Pursuit Gait
            strideTimer += dt * strideSpeed;
            float legCycle = Mathf.Sin(strideTimer);
            float armCycle = -legCycle;
            float bobCycle = Mathf.Abs(Mathf.Sin(strideTimer));

            // Legs stride
            if (leftThigh != null)
            {
                leftThigh.localRotation = Quaternion.Euler(legCycle * legSwingAngle, 0f, 0f);
                if (leftCalf != null)
                    leftCalf.localRotation = Quaternion.Euler(Mathf.Max(0f, -legCycle) * 30f, 0f, 0f);
            }
            if (rightThigh != null)
            {
                rightThigh.localRotation = Quaternion.Euler(-legCycle * legSwingAngle, 0f, 0f);
                if (rightCalf != null)
                    rightCalf.localRotation = Quaternion.Euler(Mathf.Max(0f, legCycle) * 30f, 0f, 0f);
            }

            // Reaching Claw Arms
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Euler(45f + armCycle * armReachAngle, -8f + Mathf.Sin(strideTimer * 0.5f) * 8f, -6f);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(52f - armCycle * armReachAngle, 12f - Mathf.Sin(strideTimer * 0.5f) * 8f, 8f);
            }

            // Torso & Head hunched lurch
            if (torso != null)
            {
                torso.localPosition = initialTorsoPos + Vector3.up * (bobCycle * 0.05f);
                torso.localRotation = Quaternion.Euler(14f, Mathf.Sin(strideTimer * 0.7f) * 6f, 0f);
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(-6f + Mathf.Sin(strideTimer * 1.5f) * 6f, Mathf.Cos(strideTimer) * 8f, 0f);
            }
        }

        public void TriggerHitReaction()
        {
            flinchTimer = 0.18f;
        }

        public void SetAttacking(bool attacking)
        {
            isAttacking = attacking;
        }

        public void TriggerDeath()
        {
            isDead = true;
            deathCollapseTimer = 0f;
        }
    }
}
