using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ZombieRunner.Audio;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.Utilities;

namespace ZombieRunner.Weapons
{
    /// <summary>
    /// Advanced auto-aiming weapon controller with ammunition, reload, fire rate,
    /// smooth visual weapon tracking, and reliable mobile-friendly targeting.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShootingController : MonoBehaviour
    {
        private static readonly int FireParameter = Animator.StringToHash("Fire");

        [Header("Weapon Settings")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private GameObject muzzleFlashVisual;
        [SerializeField] private Light muzzleFlashLight;
        [SerializeField, Min(0f)] private float damagePerShot = 1f;
        [SerializeField, Min(0.05f)] private float shotsPerSecond = 4f;
        [SerializeField, Min(1f)] private float maximumRange = 90f;
        [SerializeField, Min(0.05f)] private float bulletRadius = 0.4f;
        [SerializeField, Min(0)] private int startingAmmo = 30;
        [SerializeField, Min(1)] private int maximumAmmo = 60;
        [SerializeField] private LayerMask targetLayers = Physics.DefaultRaycastLayers;
        [SerializeField, Min(0.01f)] private float muzzleFlashDuration = 0.05f;

        [Header("Weapon Kick & Recoil")]
        [SerializeField, Min(0f)] private float kickbackDistance = 0.07f;
        [SerializeField, Min(0f)] private float kickbackAngle = 3.5f;
        [SerializeField, Min(1f)] private float kickbackRecoverySpeed = 16f;

        [Header("Auto-Aim")]
        [SerializeField] private bool autoAimEnabled = true;
        [SerializeField, Min(5f)] private float aimRange = 45f;
        [SerializeField] private bool preferCurrentLane = true;
        [SerializeField] private bool allowAdjacentLaneTargeting = true;
        [SerializeField, Min(1f)] private float aimSmoothing = 14f;
        [SerializeField, Min(1f)] private float targetSwitchThreshold = 1.3f;
        [SerializeField, Min(0f)] private float aimHeightOffset = 1.1f;
        [SerializeField] private bool debugAutoAim = true;

        [Header("Diagnostics")]
        [SerializeField] private bool enableDebugLogs = true;

        private int currentAmmo;
        private float nextShotTime;
        private Coroutine muzzleFlashRoutine;
        private Animator animator;
        private bool hasFireParameter;

        private PlayerController playerController;
        private ZombieSpawner zombieSpawner;
        private Transform weaponHolder;
        private Quaternion weaponDefaultLocalRotation;

        private ZombieController currentTarget;
        private Vector3 currentAimDirection;
        private Vector3 weaponKickOffset;
        private Quaternion weaponKickRot = Quaternion.identity;

        public event Action<int, int> AmmoChanged;

        public int CurrentAmmo => currentAmmo;
        public int MaximumAmmo => maximumAmmo;
        public bool HasAmmo => currentAmmo > 0;
        public ZombieController CurrentTarget => currentTarget;

        private struct TargetCandidate
        {
            public ZombieController zombie;
            public float score;
            public float distance;
            public int lane;
            public Vector3 aimPoint;
        }

        private struct TargetHit
        {
            public Collider collider;
            public float distance;
        }

        private void Awake()
        {
            currentAmmo = Mathf.Clamp(startingAmmo, 0, maximumAmmo);
            ResolveReferences();
            if (muzzleFlashVisual != null)
                muzzleFlashVisual.SetActive(false);
            currentAimDirection = transform.root.forward;
            AmmoChanged?.Invoke(currentAmmo, maximumAmmo);
        }

        private void Start()
        {
            ResolveReferences();
            currentAimDirection = transform.root.forward;
        }

        private void ResolveReferences()
        {
            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>() ?? GetComponent<PlayerController>() ?? FindAnyObjectByType<PlayerController>();

            if (zombieSpawner == null)
                zombieSpawner = FindAnyObjectByType<ZombieSpawner>();

            if (weaponHolder == null)
            {
                weaponHolder = transform.Find("Weapon Placeholder");
                if (weaponHolder == null)
                    weaponHolder = transform.root.Find("Weapon Placeholder");
                if (weaponHolder != null)
                    weaponDefaultLocalRotation = weaponHolder.localRotation;
            }

            if (muzzle == null)
            {
                if (weaponHolder != null)
                    muzzle = weaponHolder.Find("Muzzle");
                if (muzzle == null)
                    muzzle = transform.Find("Muzzle");
            }

            if (animator == null)
            {
                if (playerController != null)
                    animator = playerController.ModelAnimator;
                if (animator == null)
                    animator = GetComponentInChildren<Animator>() ?? transform.root.GetComponentInChildren<Animator>();
            }

            if (animator != null && !hasFireParameter)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    if (parameter.nameHash == FireParameter && parameter.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasFireParameter = true;
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            ResolveReferences();
            UpdateAutoAim();

            bool shoot = false;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.fKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame))
                shoot = true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                shoot = true;

            try
            {
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0))
                    shoot = true;
            }
            catch { }

            if (shoot)
                TryShoot();
        }

        private void UpdateAutoAim()
        {
            Transform playerRoot = transform.root;
            Vector3 fireOrigin = GetShotOrigin(playerRoot);

            if (!autoAimEnabled)
            {
                currentTarget = null;
                currentAimDirection = Vector3.Slerp(currentAimDirection, playerRoot.forward, Time.deltaTime * aimSmoothing);
                ResetWeaponVisual(playerRoot);
                return;
            }

            ZombieController bestCandidate = FindBestTarget(playerRoot, fireOrigin);
            if (bestCandidate != currentTarget)
            {
                if (debugAutoAim && bestCandidate != null)
                {
                    float dist = Vector3.Distance(fireOrigin, bestCandidate.transform.position);
                    int zLane = Mathf.RoundToInt(bestCandidate.transform.position.x / 3f);
                    Debug.Log($"[ZombieRunner AutoAim] Locked Target: '{bestCandidate.name}' ({bestCandidate.Archetype}) at Lane {zLane}, Dist: {dist:F1}m");
                }
                currentTarget = bestCandidate;
            }

            Vector3 desiredAimDirection = playerRoot.forward;
            if (currentTarget != null && !currentTarget.IsDead)
            {
                Vector3 targetPoint = currentTarget.transform.position + Vector3.up * aimHeightOffset;
                desiredAimDirection = (targetPoint - fireOrigin).normalized;
            }

            currentAimDirection = Vector3.Slerp(currentAimDirection, desiredAimDirection, Time.deltaTime * aimSmoothing);

            // Smoothly recover from weapon kickback recoil
            weaponKickOffset = Vector3.Lerp(weaponKickOffset, Vector3.zero, Time.deltaTime * kickbackRecoverySpeed);
            weaponKickRot = Quaternion.Slerp(weaponKickRot, Quaternion.identity, Time.deltaTime * kickbackRecoverySpeed);

            // Smooth visual weapon aim tracking + kickback recoil
            if (weaponHolder != null)
            {
                Quaternion targetWorldRotation = Quaternion.LookRotation(currentAimDirection, Vector3.up);
                weaponHolder.rotation = Quaternion.Slerp(weaponHolder.rotation, targetWorldRotation * weaponKickRot, Time.deltaTime * aimSmoothing);
            }
        }

        private void ResetWeaponVisual(Transform playerRoot)
        {
            if (weaponHolder != null)
            {
                weaponHolder.localRotation = Quaternion.Slerp(weaponHolder.localRotation, weaponDefaultLocalRotation, Time.deltaTime * aimSmoothing);
            }
        }

        private ZombieController FindBestTarget(Transform playerRoot, Vector3 fireOrigin)
        {
            IEnumerable<ZombieController> candidates = null;
            if (zombieSpawner != null && zombieSpawner.ActiveZombies != null && zombieSpawner.ActiveZombies.Count > 0)
                candidates = zombieSpawner.ActiveZombies;
            else
                candidates = FindObjectsByType<ZombieController>(FindObjectsInactive.Exclude);

            if (candidates == null)
                return null;

            const float laneWidth = 3f;
            int playerLane = playerController != null ? playerController.CurrentLane : Mathf.RoundToInt(playerRoot.position.x / laneWidth);

            TargetCandidate best = default;
            best.score = float.MinValue;

            foreach (ZombieController zombie in candidates)
            {
                if (zombie == null || !zombie.isActiveAndEnabled || zombie.IsDead || zombie.CurrentHealth <= 0f)
                    continue;

                Vector3 zPos = zombie.transform.position;
                float deltaZ = zPos.z - playerRoot.position.z;

                // Only consider zombies in front of the player (with small leeway for close-quarter / point-blank combat)
                if (deltaZ < -0.6f)
                    continue;

                Vector3 aimTargetPoint = zPos + Vector3.up * aimHeightOffset;
                float dist = Vector3.Distance(fireOrigin, aimTargetPoint);
                if (dist > aimRange)
                    continue;

                int zombieLane = Mathf.RoundToInt(zPos.x / laneWidth);
                int laneDiff = Mathf.Abs(zombieLane - playerLane);

                if (!allowAdjacentLaneTargeting && laneDiff > 0)
                    continue;
                if (laneDiff > 1)
                    continue; // Do not target across more than 1 lane

                // Line-of-sight check against obstacles
                Vector3 toZombie = aimTargetPoint - fireOrigin;
                if (Physics.Raycast(fireOrigin, toZombie.normalized, out RaycastHit obsHit, toZombie.magnitude, targetLayers, QueryTriggerInteraction.Ignore))
                {
                    if (obsHit.collider != null &&
                        obsHit.collider.transform.root != zombie.transform.root &&
                        obsHit.collider.transform.root != playerRoot &&
                        obsHit.collider.GetComponentInParent<ZombieController>() == null)
                    {
                        // Ray is occluded by an obstacle
                        continue;
                    }
                }

                // Scoring formula:
                // Base: 1000 - dist * 10
                // Current lane bonus: +600
                // Point blank bonus (dist < 4m): +300
                // Target hysteresis retention bonus: +150
                float score = 1000f - dist * 10f;
                if (laneDiff == 0 && preferCurrentLane)
                    score += 600f;
                else if (laneDiff == 1)
                    score += 100f;

                if (dist < 4f)
                    score += 300f;

                if (zombie == currentTarget)
                    score += (targetSwitchThreshold - 1f) * 250f;

                if (score > best.score)
                {
                    best.zombie = zombie;
                    best.score = score;
                    best.distance = dist;
                    best.lane = zombieLane;
                    best.aimPoint = aimTargetPoint;
                }
            }

            return best.zombie;
        }

        private Vector3 GetShotOrigin(Transform playerRoot)
        {
            if (muzzle != null)
                return muzzle.position;

            float y = playerRoot.position.y + 0.25f;
            float z = playerRoot.position.z + 0.6f;
            return new Vector3(playerRoot.position.x, y, z);
        }

        /// <summary>Fire once if cooldown and ammo allow. Suitable for keyboard or HUD Fire button.</summary>
        public bool TryShoot()
        {
            if (currentAmmo <= 0 || Time.time < nextShotTime)
                return false;

            currentAmmo--;
            nextShotTime = Time.time + 1f / shotsPerSecond;
            AmmoChanged?.Invoke(currentAmmo, maximumAmmo);
            ResolveReferences();

            if (animator != null && hasFireParameter)
            {
                animator.ResetTrigger(FireParameter);
                animator.SetTrigger(FireParameter);
            }

            AudioManager.Instance?.Play(GameAudioCue.Shot);
            ShowMuzzleFlash();

            // Weapon kickback recoil & camera shake feedback
            weaponKickOffset = -Vector3.forward * kickbackDistance;
            weaponKickRot = Quaternion.Euler(-kickbackAngle, UnityEngine.Random.Range(-0.8f, 0.8f), 0f);
            RunnerCameraFollow.Instance?.AddShake(0.04f, 0.08f);

            PlayerVisualModel pvm = GetComponent<PlayerVisualModel>() ?? transform.root.GetComponentInChildren<PlayerVisualModel>();
            if (pvm != null)
                pvm.TriggerWeaponRecoil();

            Transform playerRoot = transform.root;
            Vector3 origin = GetShotOrigin(playerRoot);
            Vector3 shootDirection = playerRoot.forward;

            // 1. If an auto-aim target is selected and alive, shoot directly at it
            if (autoAimEnabled && currentTarget != null && !currentTarget.IsDead && currentTarget.CurrentHealth > 0f)
            {
                Vector3 targetAimPoint = currentTarget.transform.position + Vector3.up * aimHeightOffset;
                shootDirection = (targetAimPoint - origin).normalized;

                // Spawn visible high-velocity bullet tracer
                HitImpactManager.Instance?.SpawnTracer(
                    origin,
                    targetAimPoint,
                    hasHit: true,
                    isZombie: true,
                    isMetal: false,
                    normal: (origin - targetAimPoint).normalized
                );

                float healthBefore = currentTarget.CurrentHealth;
                currentTarget.TakeDamage(damagePerShot);
                float healthAfter = currentTarget.CurrentHealth;

                if (enableDebugLogs)
                {
                    int zLane = Mathf.RoundToInt(currentTarget.transform.position.x / 3f);
                    float dist = Vector3.Distance(origin, targetAimPoint);
                    Debug.Log($"[ZombieRunner Shoot] Auto-Aim HIT -> Target: '{currentTarget.name}' ({currentTarget.Archetype}) at Lane {zLane}, Dist: {dist:F1}m, ShotDir: {shootDirection} | Health: {healthBefore:F0} -> {healthAfter:F0} {(healthAfter <= 0 ? "[DEAD]" : "[HIT REACTION]")}");
                }

                return true;
            }

            // 2. Fallback: Ray/Sphere sweep along current aim direction (or straight forward)
            if (currentAimDirection.sqrMagnitude > 0.01f)
                shootDirection = currentAimDirection;

            Ray ray = new Ray(origin, shootDirection);
            if (enableDebugLogs)
                Debug.Log($"[ZombieRunner Shoot] Fired Shot -> Origin: {origin}, Dir: {shootDirection}, Range: {maximumRange:F0}m");

            List<TargetHit> validHits = new List<TargetHit>();
            HashSet<Collider> seenColliders = new HashSet<Collider>();

            void AddHit(Collider col, float dist)
            {
                if (col == null || col.transform.root == playerRoot)
                    return;
                if (seenColliders.Add(col))
                    validHits.Add(new TargetHit { collider = col, distance = dist });
            }

            // Point-blank detection
            Collider[] closeColliders = Physics.OverlapSphere(origin, bulletRadius + 0.6f, targetLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < closeColliders.Length; i++)
            {
                Collider col = closeColliders[i];
                if (col != null)
                    AddHit(col, Vector3.Distance(origin, col.ClosestPoint(origin)));
            }

            // Wide sphere sweep
            RaycastHit[] sphereHits = Physics.SphereCastAll(ray, bulletRadius, maximumRange, targetLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sphereHits.Length; i++)
                AddHit(sphereHits[i].collider, sphereHits[i].distance);

            // Direct thin raycast
            RaycastHit[] rayHits = Physics.RaycastAll(ray, maximumRange, targetLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < rayHits.Length; i++)
                AddHit(rayHits[i].collider, rayHits[i].distance);

            validHits.Sort((a, b) => a.distance.CompareTo(b.distance));

            bool damagedTarget = false;
            Vector3 finalDestination = origin + shootDirection * maximumRange;
            Vector3 finalNormal = -shootDirection;
            bool isZombieHit = false;
            bool isMetalHit = false;

            foreach (TargetHit hit in validHits)
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>() ??
                                         hit.collider.GetComponent<IDamageable>() ??
                                         hit.collider.GetComponentInChildren<IDamageable>();

                if (damageable != null)
                {
                    ZombieController hitZombie = hit.collider.GetComponentInParent<ZombieController>();
                    float healthBefore = hitZombie != null ? hitZombie.CurrentHealth : -1f;

                    damageable.TakeDamage(damagePerShot);
                    damagedTarget = true;
                    finalDestination = origin + shootDirection * hit.distance;
                    isZombieHit = hitZombie != null;
                    isMetalHit = !isZombieHit && (hit.collider.CompareTag("Obstacle") || hit.collider.GetComponentInParent<ZombieRunner.World.ObstacleController>() != null);

                    float healthAfter = hitZombie != null ? hitZombie.CurrentHealth : -1f;

                    if (enableDebugLogs)
                    {
                        string zName = hitZombie != null ? hitZombie.name : hit.collider.gameObject.name;
                        string zType = hitZombie != null ? hitZombie.Archetype.ToString() : "Unknown";
                        Debug.Log($"[ZombieRunner Shoot] Sweep HIT -> Target: '{zName}' ({zType}), Collider: '{hit.collider.name}', Dist: {hit.distance:F1}m | Health: {healthBefore:F0} -> {healthAfter:F0}");
                    }
                    break;
                }
            }

            // Spawn visible high-velocity bullet tracer
            HitImpactManager.Instance?.SpawnTracer(
                origin,
                finalDestination,
                hasHit: damagedTarget,
                isZombie: isZombieHit,
                isMetal: isMetalHit,
                normal: finalNormal
            );

            if (!damagedTarget && enableDebugLogs)
            {
                Debug.Log($"[ZombieRunner Shoot] Missed -> Bullet tracer traveled {maximumRange:F0}m along {shootDirection}.");
            }

            return true;
        }

        public void AddAmmo(int amount)
        {
            if (amount <= 0)
                return;
            int previous = currentAmmo;
            currentAmmo = Mathf.Min(maximumAmmo, currentAmmo + amount);
            if (previous != currentAmmo)
                AmmoChanged?.Invoke(currentAmmo, maximumAmmo);
        }

        public void RefillAmmo()
        {
            AddAmmo(maximumAmmo);
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlashLight == null && muzzle != null)
            {
                muzzleFlashLight = muzzle.GetComponentInChildren<Light>();
                if (muzzleFlashLight == null)
                {
                    GameObject lightGo = new GameObject("MuzzleFlash_MicroLight");
                    lightGo.transform.SetParent(muzzle, false);
                    muzzleFlashLight = lightGo.AddComponent<Light>();
                    muzzleFlashLight.type = LightType.Point;
                    muzzleFlashLight.color = new Color(1.0f, 0.88f, 0.65f);
                    muzzleFlashLight.range = 3.8f;
                    muzzleFlashLight.intensity = 3.5f;
                    muzzleFlashLight.shadows = LightShadows.None;
                    muzzleFlashLight.enabled = false;
                }
            }

            if (muzzleFlashRoutine != null)
                StopCoroutine(muzzleFlashRoutine);
            muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());
        }

        private IEnumerator MuzzleFlashRoutine()
        {
            if (muzzleFlashVisual != null) muzzleFlashVisual.SetActive(true);
            if (muzzleFlashLight != null) muzzleFlashLight.enabled = true;
            yield return new WaitForSeconds(muzzleFlashDuration);
            if (muzzleFlashVisual != null) muzzleFlashVisual.SetActive(false);
            if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
            muzzleFlashRoutine = null;
        }

        private void OnValidate()
        {
            damagePerShot = Mathf.Max(0f, damagePerShot);
            shotsPerSecond = Mathf.Max(0.05f, shotsPerSecond);
            maximumRange = Mathf.Max(1f, maximumRange);
            bulletRadius = Mathf.Max(0.05f, bulletRadius);
            maximumAmmo = Mathf.Max(1, maximumAmmo);
            startingAmmo = Mathf.Clamp(startingAmmo, 0, maximumAmmo);
            muzzleFlashDuration = Mathf.Max(0.01f, muzzleFlashDuration);
            aimRange = Mathf.Max(5f, aimRange);
            aimSmoothing = Mathf.Max(1f, aimSmoothing);
            targetSwitchThreshold = Mathf.Max(1f, targetSwitchThreshold);
            aimHeightOffset = Mathf.Max(0f, aimHeightOffset);
        }
    }
}
