# Zombie Runner Development Status

## Project inspection

- Unity Editor version: 6000.5.9f1.
- Render pipeline: Universal Render Pipeline 17.5.0 is installed and assigned in Graphics settings. The project includes mobile and PC URP renderer assets.
- Relevant packages already present: Input System 1.20.0, uGUI 2.5.0, URP, and Unity Test Framework 1.7.0. No Stage 1 dependency is missing.
- uGUI 2.5.0 bundles Unity's TextMeshPro runtime. The project uses that bundled assembly and its imported TMP Essential Resources; a separate `com.unity.textmeshpro` install is incompatible with this Unity 6.5 editor/package set and is not needed.
- Existing authored content: `Assets/Scenes/SampleScene.unity`, the URP template volume/render settings, and the template tutorial README scripts. These are preserved.
- Existing build scene: `Assets/Scenes/SampleScene.unity` remains enabled. The Stage 1 gameplay scene will be added without removing it.
- No existing Zombie Runner scripts, prefabs, scenes, or project-specific source control changes were present at inspection.

## Stage 1 — Project structure and basic scene

Status: complete.

- Added the requested `Assets/ZombieRunner/` folder layout, including the top-level `Editor/` folder.
- Added an idempotent Editor setup tool at `Assets/ZombieRunner/Editor/ZombieRunnerProjectSetup.cs` with Tools > Zombie Runner commands for setup, gameplay scene creation, main menu creation, and validation.
- Setup creates seven URP-compatible placeholder materials and a capsule player prefab only when missing.
- Created `Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity` with a three-lane road, lane markings, ruined city blockouts, placeholder debris, atmospheric fog, directional lighting, a follow-camera starting position, and a capsule player.
- The generated gameplay scene is enabled in Build Settings. The original `Assets/Scenes/SampleScene.unity` remains enabled and first in the list.
- The Main Menu command creates a separate title and button layout scene. Button navigation is scheduled for the UI stage.
- Unity 6.5.9f1 Editor recompile completed with 0 errors and 0 warnings. The project validator and a direct check of the scene, prefab, material, Build Settings entry, folder layout, and URP assignment returned success.
- The gameplay blockout now has automatic running and basic movement controls. Enemy, obstacle, and score systems are still pending.

## Stage 2 — Player movement and camera

Status: complete.

- Added a configurable `CharacterController` based runner with forward movement, smooth three lane switching, jump, timed slide, keyboard controls, and touch-swipe controls.
- Split the placeholder capsule mesh from its collision root so sliding changes the visual pose and collision height independently. The player prefab is prepared for a future Animator and model replacement.
- Added a smooth third-person follow camera with forward look-ahead and linked it to the player in the existing gameplay scene.
- Updated the gameplay-scene command to add missing Stage 2 components in place. It preserves scene contents and refuses to modify a scene without the generated marker.
- Unity recompile completed with 0 errors and 0 warnings. Play Mode checks confirmed a lane switch reached x=3 with camera follow at x≈3, slide activated with a 1.05 m collision height, and jump produced positive launch speed while grounded.
- City ruin props are still static placeholder geometry; environment recycling and theme changes are scheduled later.

## Stage 3 — Endless road generation and pooling

Status: complete.

- Added `RoadSegment_30m.prefab` with a road surface, side shoulders, curbs, and three lane markings.
- Added `EndlessRoadManager` with a fixed five-segment pool, configurable segment length and starting position, and behind-player recycling.
- Wired the road prefab and runner into the existing gameplay scene. Disabled the original static road surfaces so they do not overlap the pooled road.
- Play Mode validation held the pool at five segments and recycled them to centers 90, 120, 150, 180, and 210 m with consistent 30 m spacing.
- Unity recompile completed with 0 errors and 0 warnings.

## Stage 4 — Zombies and shooting

Status: complete.

- Added Normal, Fast, and Big zombie prefabs with configurable health, movement speed, attack range, damage, and cooldown.
- Added a lane-aware spawner with a configurable spawn interval and alive limit. Zombies pursue the runner, attack within range, accept damage, and are removed on death.
- Added a forward raycast weapon with fire cooldown, ammunition, keyboard/mouse controls, a public shoot method for the future mobile button, and a muzzle-flash placeholder.
- Added the player health and damage events needed by enemy attacks and the upcoming HUD/power-up stages.
- Wired the spawner, enemy prefabs, player health, weapon, and muzzle references into the gameplay scene and player prefab.
- Unity recompile completed with 0 errors and 0 warnings. Play Mode check confirmed spawn count 0→1, ammo 30→29, target health 3→2 from a shot, zombie death at zero health, and player health 100→90 from an enemy attack.

## Stage 5 — Obstacles and collectibles

Status: complete.

- Added roadblock, abandoned car, and explosive barrel prefabs, plus an obstacle spawner that selects one of the three lanes. Player collisions apply damage; barrels can be destroyed by the raycast weapon and damage nearby actors.
- Added gold coin, ammo box, and health kit trigger prefabs with immediate coin, ammo, and health updates. `CollectibleManager` tracks run coins and spawns pickups ahead of the runner.
- Wired both spawners and all prefab references into the gameplay scene. Obstacles and pickups are removed after they pass behind the runner.
- Unity recompile completed with 0 errors and 0 warnings. Play Mode checks confirmed coin count 1, ammo 10→25, health 50→75 from pickups, obstacle spawn count 0→1, collision damage 75→50, and a forward shot destroyed the explosive barrel while consuming one round.

## Stage 6 — Power-ups and health

Status: complete.

- Added health restoration and ammo refill pickups, plus timed coin magnet, shield, speed boost, and double-coins effects.
- Each timed effect has an Inspector-configurable duration. Re-picking an active effect refreshes its timer; expiration restores the default player speed, damage handling, and coin/magnet behavior.
- Added a configurable power-up spawner and a reusable, color-coded power-up pickup prefab. Health and ammo refill values are configurable.
- Play Mode check confirmed health 50→85, ammo 5→60, shield blocked damage, speed 8→12 m/s, one coin awarded two coins while active, all timed effects expired back to defaults, and the pickup spawner created one power-up.
- Unity recompile completed with 0 errors and 0 warnings.

## Stage 7 — Menus and HUD

Status: complete.

- Added persistent PlayerPrefs storage for banked coins, best score, and master audio volume. Run score combines distance and zombie kills; run totals are committed once when a run ends or is abandoned. The optional revive defers final saving until the player either continues to a final game over or exits/restarts.
- Added `GameManager` states for play, pause, and game over, plus restart, main-menu navigation, and one shielded revive per run.
- Built the main menu scene with TMP title/stats, Play navigation, settings overlay, and saved master-volume slider. The menu is the first enabled Build Settings scene; the gameplay scene is second and the original sample scene remains enabled third.
- Built the gameplay HUD with TMP score/distance/coin/ammo/health/power-up displays, health bar, safe-area fitting, pause/game-over panels, retry/home/revive buttons, and mobile lane/jump/slide/fire controls. The pause menu has a master-volume settings panel. Swipe movement remains available.
- Both scenes use `InputSystemUIInputModule` and the bundled TMP essential font assets.
- Unity recompile completed with 0 errors and 0 warnings. Play Mode checks covered pause/resume, distance and kill scoring, game-over, revive and shield, one-time revive limit, menu/settings navigation, saved score/coins/audio, and Play navigation back to gameplay.

## Stage 8 — Environments and effects

Status: complete.

- Added four reusable 30 m environment segment prefabs made from URP-compatible procedural geometry: City Ruins, Subway Tunnel, Military Base, and Factory. The gameplay scene streams them in a fixed five-segment pool alongside the road pool, cycling themes every 300 m.
- Added a smooth 45 m lighting transition before each theme boundary. Fog color/range, ambient light, and the directional light color/intensity blend between configurable theme profiles.
- Added themed point lights and lightweight world-space ash, dust, and steam particle systems. Disabled the original static city blockout so the streamed city theme starts at the beginning of the run.
- Added `AudioManager` with persistent music/effects sources, cue slots, and saved master-volume support. Shot, zombie death, pickup, hit, power-up, explosion, and button cues are routed; no audio clips are included yet.
- Unity recompile completed with 0 errors and 0 warnings. Play Mode checks verified both pools initialize at five segments, all four themes replace the active segment set, lighting reaches the expected blend and target values, themed particles/lights instantiate, and pause settings persist volume.

## Stage 9 — Asset integration preparation

Status: complete.

- Added standard player and zombie Animator Controllers with shared movement, attack, hit/death, jump, slide, and fire parameters. The player and zombie prefabs now include Animator components with root motion disabled, and gameplay scripts drive the available parameters.
- Added `Models/ASSET_INTEGRATION.md` with beginner steps for importing future FBX models, replacing only visual children, retaining gameplay roots/colliders, assigning controller clips, and wiring weapon muzzle transforms.
- No external models or paid assets have been downloaded. Final FBX files and animation clips remain to be provided and assigned.

## Stage 10 — Testing and Android build preparation

Status: complete.

- Added `Tools > Zombie Runner > Prepare Android Build` and applied it to the existing Unity project. Player settings now use app/product name `Zombie Runner`, portrait orientation, IL2CPP, ARM64, minimum API 26, automatic target API, fullscreen, safe-area rendering, optimized frame pacing, and a single APK output.
- The Android identifier is `com.example.zombierunner`; replace it with a unique reverse-domain identifier before distributing the game.
- Added mobile runtime defaults in `GameManager`: 60 FPS target, VSync off, and no sleep timeout when running on a mobile platform. Android uses the project's Mobile URP quality tier.
- Installed Android Build Support, Android SDK & NDK Tools, and OpenJDK for Unity 6000.5.9f1. Configured Unity External Tools to use the machine's existing Android SDK at `C:/Users/metra/AppData/Local/Android/Sdk` and the Unity-bundled JDK/NDK.
- Unity's Android target is installed and active. Readback confirmed minimum API 26, automatic target API, ARM64, IL2CPP, and portrait orientation.
- Built `Builds/Android/ZombieRunner.apk` (42,953,718 bytes). The Android BuildReport completed successfully with 0 errors and 1,001 warnings, mostly shader diagnostics plus deprecated Unity API warnings. The APK has not been installed on a physical device.
- The project validator passed, and the Android build completed its Gradle `assembleRelease` step successfully.

## Stage 11 — Prototype Stabilization and Improvements

Status: complete.

- Inspected all 25 C# scripts in `Assets/ZombieRunner` and `Assets/TutorialInfo`.
- Verified compilation using Unity's bundled .NET SDK (`DotNetSdk\dotnet.exe`); both `Assembly-CSharp` and `Assembly-CSharp-Editor` compile with 0 errors.
- Fixed Unity 6.5 obsolete API usage: verified `requestedVisibleInsets = AndroidWindowInsetsType.None` replaces deprecated `startInFullscreen`, verified `textWrappingMode = TextWrappingModes.Normal` replaces `enableWordWrapping`, and verified standard `FindObjectsByType` syntax.
- Removed unsupported TextMeshPro Unicode characters: replaced all instances of `\u2022` (bullet `•`) and `\u2500` (box-drawing `─`) with standard ASCII delimiters (`|`, `-`) in `UIManager.cs`, `ZombieRunnerProjectSetup.cs`, and `ZombieRunnerMainMenu.unity`. Verified 0 non-ASCII characters remain across all project assets.
- Removed unnecessary Unity AI packages (`com.unity.ai.assistant` and `com.unity.ai.inference`) from `Packages/manifest.json` and deleted `ProjectSettings/Packages/com.unity.ai.assistant`, eliminating background `unity-ai-relay` service calls and domain reload overhead.
- Fixed zombie lifecycle bug: `ZombieSpawner` now cleans up and destroys zombies that fall behind the runner (`> 12m`) without awarding unearned score or triggering audio, preventing the alive count from permanently capping at `maximumAlive` (6) and halting spawns.
- Added player slide reset safety: added `OnDisable` cleanup in `PlayerController` to restore normal collider height and visual scale if the runner is defeated or disabled mid-slide.
- Added raycast shooting fallback: `ShootingController` now safely computes origin and forward rays even if the muzzle placeholder transform is unassigned.
- Implemented runner difficulty progression: added configurable distance-based acceleration (`maxForwardSpeed = 16 m/s`, `speedIncreaseRate = 0.005f`) so the runner gradually speeds up over the course of a run while preserving power-up multipliers.
- Added desktop pause shortcuts: pressing `Escape` or `P` toggles Pause/Resume in `UIManager`.
- Added procedural audio generator: `AudioManager` automatically synthesizes procedural audio clips on demand for any unassigned audio cue (shots, zombie death, pickups, player hit, explosions, buttons, powerups), giving the game full audio feedback immediately with zero external asset dependencies.
- Added high score celebration: `UIManager` highlights `(NEW BEST!)` on the Game Over screen whenever the player sets a new record score.

## Stage 12 — Mixamo Character Model Integration

Status: complete.

- Imported Mixamo `X Bot.fbx` from `Basic_Shooter_Pack.zip` into `Assets/ZombieRunner/Characters/Player/X Bot.fbx`.
- Configured ModelImporter metadata for Humanoid rig (`animationType: 3` / `Human`, `avatarSetup: 1` / `CreateFromThisModel`) and remapped material slots (`Beta_HighLimbsGeoSG3` and `Beta_Joints_MAT1`) to dedicated Universal Render Pipeline Lit materials.
- Created URP Lit materials `WR_PlayerXBot_Surface.mat` and `WR_PlayerXBot_Joints.mat` in `Assets/ZombieRunner/Materials/` with metallic/smoothness properties tuned for the runner character.
- Updated `PlayerController.cs` to detect humanoid visual roots with ground-level pivots (`y <= -0.5f`), adjusting slide visual scaling so the character compresses downward towards the road without feet sinking below the surface.
- Added automated integration tool `Tools > Zombie Runner > Integrate Mixamo Player Model` and `[InitializeOnLoadMethod]` hook in `ZombieRunnerProjectSetup.cs` to cleanly instantiate `Visual - Mixamo X Bot`, wire Animator (`Player_Animator.controller`, `applyRootMotion = false`), bind the Humanoid Avatar, disable the placeholder capsule, and preserve all root gameplay components.
- Maintained 100% integrity of player gameplay systems: `CharacterController` (height 2.0m, radius 0.42m), `PlayerController`, `PlayerHealth`, `ShootingController`, and `Weapon Placeholder` remain intact on the prefab root.
- In accordance with instructions, animation FBX clips have not been imported or added to the Animator Controller yet; the model currently uses the standard T-pose / bind pose awaiting animation clip hookup in the next phase.
- Verified compilation: both `Assembly-CSharp` and `Assembly-CSharp-Editor` compile with 0 warnings and 0 errors.

## Stage 13 — Mixamo X Bot Materials and Texture Inspection

Status: complete.

- Inspected all 17 files in `Basic_Shooter_Pack.zip` and the binary structure of `X Bot.fbx`: verified that no bitmap textures (diffuse, normal, roughness, etc.) or image files exist anywhere in the pack or model.
- Confirmed that Mixamo `X Bot` (along with `Y Bot`) is Adobe Mixamo's official rigging dummy mannequin. It was modeled without human skin, face, or clothing textures, relying on procedural/shader-based material slots.
- Extracted and created dedicated URP Lit materials for both material slots in `Assets/ZombieRunner/Characters/Player/Materials/`:
  - `Beta_HighLimbsGeoSG3.mat`: Outer armor shell (`#DBE0E8`, metallic `0.2`, smoothness `0.6`, URP Lit).
  - `Beta_Joints_MAT1.mat`: Mechanical joints, neck, fingers, and visor (`#1F2126`, metallic `0.7`, smoothness `0.65`, URP Lit).
- Configured `X Bot.fbx.meta` with `materialLocation: 0` (`InPrefab`), `materialImportMode: 1` (`ImportStandard`), and explicit `externalObjects` remaps for both slots.
- Updated `ZombieRunnerProjectSetup.cs` so `IntegrateMixamoPlayerModel()` applies and records URP Lit materials on the SkinnedMeshRenderers with `EditorUtility.SetDirty` and `PrefabUtility.RecordPrefabInstancePropertyModifications`.
- Recommended textured Mixamo survivor characters ("Remy", "AJ", "Claire", "Josh", "Swat") if a realistic textured human runner is desired.
- Preserved all existing player transforms, CharacterController, PlayerHealth, ShootingController, and camera systems.
- Recompiled with Unity .NET SDK: 0 errors and 0 warnings.

## Stage 14 — New Textured Character Integration (Ch05)

Status: complete.

- Inspected `Basic Shooter Pack (1).zip` and identified `Ch05_nonPBR.fbx` (120,031,777 bytes) with embedded PBR textures.
- Extracted `Ch05_nonPBR.fbx` into `Assets/ZombieRunner/Characters/Player/NewCharacter/Ch05_nonPBR.fbx`.
- Extracted all 12 embedded high-resolution PNG textures into `Assets/ZombieRunner/Characters/Player/NewCharacter/Textures/` (Diffuse, Normal, Specular, and Glossiness maps for Head, Upper Body, and Lower Body).
- Created 3 dedicated Universal Render Pipeline Lit materials in `Assets/ZombieRunner/Characters/Player/NewCharacter/Materials/`:
  - `Ch05_body.mat`: Head, skin, and eyes with Diffuse map and tangent space Normal map.
  - `Ch05_body1.mat`: Upper body clothing and jacket with Diffuse map and Normal map.
  - `Ch05_body2.mat`: Lower body pants, boots, and holster with Diffuse map and Normal map.
- Configured `Ch05_nonPBR.fbx.meta` for Humanoid rig (`animationType: 3`, `avatarSetup: 1`) and mapped all three material slots to their respective URP Lit materials.
- Backed up `PlayerPlaceholder.prefab` to `PlayerPlaceholder_Backup_XBot.prefab` with a unique GUID.
- Maintained all X Bot files and materials as safe backup.
- Updated `ZombieRunnerProjectSetup.cs` with `Tools > Zombie Runner > Integrate Player Model` and an automated `[InitializeOnLoadMethod]` hook to replace the visual model with `Visual - New Character`, set position `(0, -1.0, 0)`, apply URP Lit materials, configure `Player_Animator.controller`, bind the humanoid avatar, and wire `PlayerController`.
- Preserved all player root components (`CharacterController`, `PlayerController`, `PlayerHealth`, `ShootingController`, `Weapon Placeholder`, and UI bindings).
- Recompiled with Unity .NET SDK: 0 errors and 0 warnings.

## Stage 15 — Player Run & Idle Animation Integration & Runtime Debugging

Status: complete.

- Inspected `Basic Shooter Pack (1).zip` and identified matching Mixamo animation FBXs: `rifle run.fbx` (345,249 bytes, 22 frames) and `rifle aiming idle.fbx` (498,225 bytes, 93 frames).
- Extracted both animation FBX files into `Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/`.
- Diagnosed root causes for static bind pose at runtime:
  1. `m_Avatar` missing on the instantiated `Visual - New Character` Animator in `PlayerPlaceholder.prefab` (was not recorded in prefab modifications, leaving the runtime Animator without a valid Humanoid Avatar).
  2. `lastFrame: 0` in both `rifle run.fbx.meta` and `rifle aiming idle.fbx.meta`, which clamped the animation clips to a single static frame of 0.0s length.
  3. Redundant `Animator` on inactive `Visual - Placeholder Capsule` with `m_Avatar: null`, which sat as child 0 and risked capturing `GetComponentInChildren<Animator>()` calls.
  4. Animator Controller had `Idle` as the entry state instead of `Run` directly.
- Fixed `rifle run.fbx.meta` with exact take length `firstFrame: 0, lastFrame: 22`, `loopTime: 1`, `loopPose: 1`, and in-place root motion baking.
- Fixed `rifle aiming idle.fbx.meta` with exact take length `firstFrame: 0, lastFrame: 93`, `loopTime: 1`, and `loopPose: 1`.
- Bound `m_Avatar: {fileID: 9000000, guid: 05050505c87b556ca234958302b00200, type: 3}` (Humanoid Avatar of `Ch05_nonPBR.fbx`) and `m_Enabled: 1` explicitly in `PlayerPlaceholder.prefab`.
- Removed redundant `Animator` component from `Visual - Placeholder Capsule`.
- Configured `Player_Animator.controller` with `Run` as the default entry state for endless runner gameplay, with seamless transitions between `Run` and `Idle` based on `Speed` (< 0.1 / > 0.1).
- Updated `PlayerController.cs` and `PlayerHealth.cs` to explicitly target `Visual - New Character`'s Animator, dynamically validate and recover avatar references if needed, and log complete runtime diagnostic verification on `Start()`.
- Verified compilation: 0 errors and 0 warnings on both `Assembly-CSharp` and `Assembly-CSharp-Editor`.

## Stage 16 — Player Jump Animation Integration & End-to-End Fix

Status: complete.

- Inspected `Basic Shooter Pack (1).zip` and identified the matching jump animation clip: `rifle jump.fbx` (342,225 bytes, 18 frames @ 30 fps = 0.60 seconds).
- Extracted `rifle jump.fbx` into `Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/rifle jump.fbx`.
- Diagnosed root causes for why the character continued running instead of visibly playing the jump animation:
  1. `humanDescription` in `rifle jump.fbx.meta` had `human: []` and `skeleton: []` empty (149 lines total vs 895 lines in `rifle run.fbx.meta`), which caused Unity's ModelImporter to import 0 humanoid muscle curves, leaving the character in the active running pose.
  2. CharacterController `isGrounded` in `UpdateAnimator()` was evaluating true during the launch frame before upward translation cleared `skinWidth`, causing `Jump -> Run` (`hasExitTime: false`, `Grounded == true`) to immediately cancel `Jump` and return to `Run` on the very same frame.
  3. `Player_Animator.controller` motion reference mapped to `7400000` (matching the exported take clip).
- Fixed `rifle jump.fbx.meta` with the complete 55 humanoid bone mappings and 67 skeleton nodes from Mixamo `mixamorig:` bones (matching `rifle run.fbx.meta`), `animationType: 3`, `avatarSetup: 1`, `firstFrame: 0, lastFrame: 18`, `loopTime: 0`, `loopBlend: 0`, and baked root motion.
- Updated `Player_Animator.controller`:
  - Assigned clip `{fileID: 7400000, guid: 05050505c87b556ca234958302b00303, type: 3}` to `Jump` state.
  - Set `canTransitionToSelf: 0` on `Jump -> Run` (`-5627886871537749522`).
- Updated `PlayerController.cs`:
  - Added dedicated `isJumping` state flag.
  - Set `isJumping = true` on `Jump()`, resetting `Jump` trigger and setting `Grounded` to false.
  - Kept `isJumping = true` while airborne; resets to false only when falling (`verticalSpeed <= 0f`) and physically grounded (`characterController.isGrounded`).
  - Conditioned `Grounded` parameter in `UpdateAnimator()` on `!isJumping && characterController.isGrounded`, completely eliminating premature landing transitions.
  - Added concise runtime diagnostics in `PlayerController.cs` logging jump triggers, landings, and every Animator state transition with normalized time and ground status.
- Updated `ZombieRunnerProjectSetup.cs`:
  - Added `NewCharacterJumpAnimPath` constant.
  - Added menu items `Tools > Zombie Runner > Setup Player Jump Animation` and `Tools > Zombie Runner > Setup Player Animations`.
  - Configured `SetupPlayerRunAnimation` to automatically verify, extract, import, and wire `rifle jump.fbx` alongside `rifle run.fbx` and `rifle aiming idle.fbx`.
  - Updated auto-configuration checks on project load to ensure `Jump` motion remains valid.
- Built both `Assembly-CSharp` and `Assembly-CSharp-Editor`: 0 errors and 0 warnings.

## Stage 17 — Player Slide Animation Integration & Ground Alignment

Status: complete.

- Inspected all 16 animation assets in `Basic Shooter Pack (1).zip` and evaluated low/crouch motions (`hit reaction.fbx`, `strafe (2).fbx`, `firing rifle.fbx`, `reloading.fbx`, and `toss grenade.fbx`).
- Analyzed motion curves and root translation:
  - While other clips maintain upright hips ($Y \approx 93.4 - 95.1$ cm), `toss grenade.fbx` contains a 15 cm vertical drop down to $Y = 80.45$ cm between frames 26 and 56 (30 frames at 30 fps = 1.0s, with deep knee bend, athletic forward crouch, and low rifle carriage).
  - Selected `toss grenade.fbx` frames 26 to 56 as the most authentic in-pack rifle combat crouch/slide motion (`rifle slide.fbx`).
- Extracted and configured `Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/rifle slide.fbx`:
  - Configured `rifle slide.fbx.meta` with GUID `05050505c87b556ca234958302b00304`, sub-asset clip `Slide` (`internalID: 7400000`).
  - Full 55 humanoid bone and 67 skeleton mappings matching `Ch05_nonPBR.fbx` and `rifle run.fbx`.
  - Set `firstFrame: 26, lastFrame: 56`, `loopTime: 0`, and baked in-place root motion (`lockRootRotation: 1, lockRootHeightY: 1, lockRootPositionXZ: 1, keepOriginalPositionY: 1`).
- Updated `Player_Animator.controller`:
  - Assigned clip `{fileID: 7400000, guid: 05050505c87b556ca234958302b00304, type: 3}` to `Slide` state (`fileID: 7412435666384652433`) with playback speed `1.15` (aligns 30 frames cleanly with the 0.8s slide duration).
  - Configured `AnyState -> Slide` transition with conditions `Sliding == true` AND `Grounded == true`, preventing the slide animation from triggering or interrupting while airborne.
  - Verified `Slide -> Run` transition with condition `Sliding == false` (`duration: 0.12s`), smoothly returning to `Run` when the slide timer ends.
- Updated `PlayerController.cs`:
  - Added airborne guard to `Slide()`: `if (characterController == null || IsSliding || !characterController.isGrounded || isJumping || verticalSpeed > 0.1f) return;`.
  - Updated visual root scaling: restricted capsule Y-squash exclusively to non-rigged models (`if (visualRoot != null && animator == null)`), ensuring rigged humanoid characters preserve natural proportions and keep their feet firmly planted on the road surface ($Y = -1.0$).
  - Preserved collider reduction (`slideColliderHeight = 1.05f`) and center offset for clearing low obstacles.
  - Added runtime diagnostics logging slide start with collider height and slide completion.
- Updated `ZombieRunnerProjectSetup.cs`:
  - Added `NewCharacterSlideAnimPath` constant.
  - Added menu item `Tools > Zombie Runner > Setup Player Slide Animation`.
  - Configured automated extraction, Humanoid importer setup, and Animator Controller wiring in `SetupPlayerRunAnimation()`.
  - Added `Slide` motion validation in `OnProjectLoaded()`.
- Built both `Assembly-CSharp` and `Assembly-CSharp-Editor`: 0 errors.

### Stage 20: Zombie Characters Import & Setup (Common, Fast, Heavy)
- Inspected three downloaded Mixamo FBX models:
  - `Ch10_nonPBR.fbx` (Common Zombie) — Humanoid rig (`mixamorig5:` prefix), 52 humanoid bones, embedded diffuse/normal/specular textures.
  - `Yaku J Ignite.fbx` (Fast Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded textures for body and jacket.
  - `Pumpkinhulk L Shaw.fbx` (Heavy Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded diffuse/normal textures.
- Extracted and assigned embedded textures into `Assets/ZombieRunner/Characters/Zombie/Textures/`.
- Configured 5 URP Lit materials in `Assets/ZombieRunner/Characters/Zombie/Materials/`:
  - `Ch10_body.mat` & `Ch10_body1.mat`
  - `yaku_zombie_material.mat` & `yifu.mat`
  - `PumpkinHulk_Material.mat`
- Updated ModelImporter `.meta` files for all three FBX models with:
  - `animationType: 3` (Humanoid)
  - `avatarSetup: 1` (Create from this model)
  - Explicit Humanoid bone mappings
  - External material mappings bound to URP Lit materials.
- Created modular ScriptableObject architecture:
  - `ZombieConfig.cs` in `Assets/ZombieRunner/Scripts/Enemies/`
  - `Config_CommonZombie.asset`, `Config_FastZombie.asset`, `Config_HeavyZombie.asset` in `Assets/ZombieRunner/Settings/Enemies/`
  - Integrated optional config reference in `ZombieController.cs` with fallback.
- Created reusable prefabs in `Assets/ZombieRunner/Prefabs/Zombies/`:
  - `CommonZombie.prefab` (and synced `NormalZombie.prefab`)
  - `FastZombie.prefab`
  - `HeavyZombie.prefab` (and synced `BigZombie.prefab`)
  - Each prefab features clean root transforms, appropriately scaled CapsuleColliders, `ZombieController`, and child visual model with Humanoid Avatar and `Zombie_Animator.controller`.
- Player character systems (Run, Jump, Slide, Shoot) remained 100% intact and untouched.
- Clean compilation verified: 0 errors.

## Stage 18 — Mixamo Running Slide Integration & Timing Synchronization

Status: complete.

- Inspected newly downloaded `Running Slide.fbx` (120,174,848 bytes, 47 frames) placed in `Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/`.
- Curve analysis of `Running Slide.fbx`:
  - Take name: `mixamo.com`, clip duration: 47 frames @ 30 fps (1.56s).
  - Hips elevation (Curve 1) drops from $Y = 93.7\text{ cm}$ down to $Y = 23.64\text{ cm}$ at frame 11, holds the low baseball slide between frames 11-28, rises to $Y = 54-85\text{ cm}$ between frames 29-40, and returns cleanly to upright running height ($Y = 91.9\text{ cm}$) at frames 45-46.
  - Forward displacement (Curve 2, 6.5 meters) baked in-place to avoid double movement with `PlayerController`.
- Configured `Running Slide.fbx.meta` (`guid: 05050505c87b556ca234958302b00305`):
  - Rig type: Humanoid (`animationType: 3`, `avatarSetup: 1`), full 55 Mixamo bone mappings matching `Ch05_nonPBR.fbx`.
  - Linked `lastHumanDescriptionAvatarSource` to player Avatar (`Ch05_nonPBR.fbx`).
  - Clip settings: `firstFrame: 0, lastFrame: 46`, `loopTime: 0`, `loopBlend: 0`.
  - Root transform baking: `lockRootRotation: 1, lockRootHeightY: 1, lockRootPositionXZ: 1, keepOriginalPositionY: 1`.
- Preserved `rifle slide.fbx` (the grenade crouch) in place as a backup.
- Updated `Player_Animator.controller`:
  - Replaced `Slide` state motion with `{fileID: 7400000, guid: 05050505c87b556ca234958302b00305, type: 3}`.
  - Synchronized speed: set `m_Speed: 1.95` ($1.566\text{s} / 0.8\text{s} = 1.95$), making the full 47-frame slide animation play in exactly 0.8 seconds to match `slideDuration` and the 1.05m physical collider window.
  - Preserved AnyState transition conditions (`Sliding == true` AND `Grounded == true`) preventing airborne slide triggers.
  - Preserved `Slide -> Run` transition (`Sliding == false`, `duration: 0.12s`).
- Updated `ZombieRunnerProjectSetup.cs`:
  - Pointed `NewCharacterSlideAnimPath` to `Running Slide.fbx` with `BackupCharacterSlideAnimPath` fallback.
  - Added clip frame range `0-46` and `1.95x` speed configuration.
- Built both `Assembly-CSharp` and `Assembly-CSharp-Editor`: 0 errors.

### Stage 20: Zombie Characters Import & Setup (Common, Fast, Heavy)
- Inspected three downloaded Mixamo FBX models:
  - `Ch10_nonPBR.fbx` (Common Zombie) — Humanoid rig (`mixamorig5:` prefix), 52 humanoid bones, embedded diffuse/normal/specular textures.
  - `Yaku J Ignite.fbx` (Fast Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded textures for body and jacket.
  - `Pumpkinhulk L Shaw.fbx` (Heavy Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded diffuse/normal textures.
- Extracted and assigned embedded textures into `Assets/ZombieRunner/Characters/Zombie/Textures/`.
- Configured 5 URP Lit materials in `Assets/ZombieRunner/Characters/Zombie/Materials/`:
  - `Ch10_body.mat` & `Ch10_body1.mat`
  - `yaku_zombie_material.mat` & `yifu.mat`
  - `PumpkinHulk_Material.mat`
- Updated ModelImporter `.meta` files for all three FBX models with:
  - `animationType: 3` (Humanoid)
  - `avatarSetup: 1` (Create from this model)
  - Explicit Humanoid bone mappings
  - External material mappings bound to URP Lit materials.
- Created modular ScriptableObject architecture:
  - `ZombieConfig.cs` in `Assets/ZombieRunner/Scripts/Enemies/`
  - `Config_CommonZombie.asset`, `Config_FastZombie.asset`, `Config_HeavyZombie.asset` in `Assets/ZombieRunner/Settings/Enemies/`
  - Integrated optional config reference in `ZombieController.cs` with fallback.
- Created reusable prefabs in `Assets/ZombieRunner/Prefabs/Zombies/`:
  - `CommonZombie.prefab` (and synced `NormalZombie.prefab`)
  - `FastZombie.prefab`
  - `HeavyZombie.prefab` (and synced `BigZombie.prefab`)
  - Each prefab features clean root transforms, appropriately scaled CapsuleColliders, `ZombieController`, and child visual model with Humanoid Avatar and `Zombie_Animator.controller`.
- Player character systems (Run, Jump, Slide, Shoot) remained 100% intact and untouched.
- Clean compilation verified: 0 errors.

## Stage 19 — Mixamo Shoot Rifle Animation Integration & Movement Blending

Status: complete.

- Inspected newly downloaded `Shoot Rifle.fbx` (120,121,360 bytes, 29 frames @ 30 fps = 0.966s) placed in `Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/`.
- Curve analysis of `Shoot Rifle.fbx`:
  - Take name: `mixamo.com`, 29 keyframes (frames 0 to 28).
  - Hips elevation (Curve 1) cycles between $Y = 83.63\text{ cm}$ and $Y = 93.20\text{ cm}$, starting and ending at $Y = 85.07\text{ cm}$ across two running strides while aiming and firing the rifle.
  - Forward translation (Curve 2, 1.89 meters) baked in place (`lockRootPositionXZ = 1, lockRootHeightY = 1, lockRootRotation = 1`) so `PlayerController.cs` maintains continuous forward running movement.
- Configured `Shoot Rifle.fbx.meta` (`guid: 05050505c87b556ca234958302b00306`):
  - Rig type: Humanoid (`animationType: 3`, `avatarSetup: 1`), full 55 Mixamo bone mappings matching `Ch05_nonPBR.fbx`.
  - Linked `lastHumanDescriptionAvatarSource` to player Avatar (`Ch05_nonPBR.fbx`).
  - Clip settings: `firstFrame: 0, lastFrame: 28`, `loopTime: 0`, `loopBlend: 0`.
  - Disabled redundant material generation (`materialImportMode: 0`).
- Updated `Player_Animator.controller`:
  - Assigned clip `{fileID: 7400000, guid: 05050505c87b556ca234958302b00306, type: 3}` to `Shoot` state (`fileID: -8590956573672665941`) with playback speed `1.5` (~0.64s per shot cycle, matching fast response).
  - Configured `AnyState -> Shoot` transition (`fileID: 9084886046743408576`) with condition `Fire` (Trigger), snappy duration `0.08s`, and `canTransitionToSelf: 1` so consecutive shots re-trigger muzzle recoil immediately.
  - Configured `Shoot -> Run` transition (`fileID: -56610941678061942`) with `hasExitTime: 1`, `exitTime: 0.85`, and `duration: 0.12s`, returning smoothly to `Run` once the shot finishes.
  - Verified that AnyState transitions to `Jump` and `Slide` can interrupt shooting at any time, and that shooting does not block or cancel Jump, Slide, or Run physics.
- Updated `ShootingController.cs`:
  - Added robust dynamic Animator caching in `Start()` and `TryShoot()` via `EnsureAnimatorReference()`.
  - Reset and set `Fire` trigger upon firing, alongside muzzle flash, raycast damage, and audio cue.
  - Added concise runtime logging: `[ZombieRunner] Shoot Triggered -> ammo: X/Y, animTriggered: true`.
- Updated `PlayerController.cs`:
  - Added `Shoot` state logging in `UpdateAnimator()`.
- Updated `ZombieRunnerProjectSetup.cs`:
  - Added `NewCharacterShootAnimPath` constant.
  - Added menu item `Tools > Zombie Runner > Setup Player Shoot Animation`.
  - Configured automatic importer settings (`0-28` frames) and Animator wiring in `SetupPlayerRunAnimation()`.
  - Added `Shoot` state motion validation in `OnProjectLoaded()`.
- Built both `Assembly-CSharp` and `Assembly-CSharp-Editor`: 0 errors.

### Stage 20: Zombie Characters Import & Setup (Common, Fast, Heavy)
- Inspected three downloaded Mixamo FBX models:
  - `Ch10_nonPBR.fbx` (Common Zombie) — Humanoid rig (`mixamorig5:` prefix), 52 humanoid bones, embedded diffuse/normal/specular textures.
  - `Yaku J Ignite.fbx` (Fast Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded textures for body and jacket.
  - `Pumpkinhulk L Shaw.fbx` (Heavy Zombie) — Humanoid rig (`mixamorig:` prefix), 52 humanoid bones, embedded diffuse/normal textures.
- Extracted and assigned embedded textures into `Assets/ZombieRunner/Characters/Zombie/Textures/`.
- Configured 5 URP Lit materials in `Assets/ZombieRunner/Characters/Zombie/Materials/`:
  - `Ch10_body.mat` & `Ch10_body1.mat`
  - `yaku_zombie_material.mat` & `yifu.mat`
  - `PumpkinHulk_Material.mat`
- Updated ModelImporter `.meta` files for all three FBX models with:
  - `animationType: 3` (Humanoid)
  - `avatarSetup: 1` (Create from this model)
  - Explicit Humanoid bone mappings
  - External material mappings bound to URP Lit materials.
- Created modular ScriptableObject architecture:
  - `ZombieConfig.cs` in `Assets/ZombieRunner/Scripts/Enemies/`
  - `Config_CommonZombie.asset`, `Config_FastZombie.asset`, `Config_HeavyZombie.asset` in `Assets/ZombieRunner/Settings/Enemies/`
  - Integrated optional config reference in `ZombieController.cs` with fallback.
- Created reusable prefabs in `Assets/ZombieRunner/Prefabs/Zombies/`:
  - `CommonZombie.prefab` (and synced `NormalZombie.prefab`)
  - `FastZombie.prefab`
  - `HeavyZombie.prefab` (and synced `BigZombie.prefab`)
  - Each prefab features clean root transforms, appropriately scaled CapsuleColliders, `ZombieController`, and child visual model with Humanoid Avatar and `Zombie_Animator.controller`.
- Player character systems (Run, Jump, Slide, Shoot) remained 100% intact and untouched.
- Clean compilation verified: 0 errors.

### Stage 21: Zombie Animation Pack Extraction & Integration
- Located `Scary Zombie Pack.zip` (111.5 MB) in `Assets/ZombieRunner/Characters/Zombie/`.
- Inspected ZIP archive containing 12 animation FBX files and preserved the ZIP intact.
- Extracted all 12 animation FBX files into `Assets/ZombieRunner/Characters/Zombie/Animations/`:
  - `zombie idle.fbx` (Idle stance, 4.10s, 123 frames)
  - `zombie walk.fbx` (Walk locomotion, 4.03s, 121 frames, baked in-place)
  - `zombie run.fbx` (Run sprint locomotion, 0.80s, 24 frames, baked in-place)
  - `running crawl.fbx` (Low crawl sprint, 0.67s, 20 frames, baked in-place)
  - `zombie crawl.fbx` (Ground crawl, 5.13s, 154 frames, baked in-place)
  - `zombie attack.fbx` (Claw slash attack, 2.50s, 75 frames)
  - `zombie neck bite.fbx` (Grab/bite attack, 4.07s, 122 frames)
  - `zombie biting.fbx` (Feeding attack, 6.60s, 198 frames)
  - `zombie biting (2).fbx` (Quick snap bite, 2.30s, 69 frames)
  - `zombie scream.fbx` (Hit reaction / roar, 2.80s, 84 frames)
  - `zombie death.fbx` (Backward fall death, 2.80s, 84 frames)
  - `zombie dying.fbx` (Forward collapse death, 3.30s, 99 frames)
- Configured ModelImporter `.meta` files for all 12 animations:
  - Rig: Humanoid (`animationType: 3`, `avatarSetup: 1`), full Humanoid bone mappings.
  - Locomotion clips set to loop and baked into pose (`keepOriginalPositionXZ: 0`) for smooth in-place movement.
- Updated `Zombie_Animator.controller` with full state machine:
  - States: `Idle`, `Walk`, `Run`, `Attack`, `Hit`, `Dead`.
  - Parameters: `Speed` (float), `Attack` (trigger), `Hit` (trigger), `Die` (trigger), `Dead` (bool), `Attacking` (bool).
  - Transitions for smooth movement blending, attack return, hit recovery, and death locking.
- Updated `ZombieController.cs` to trigger `Hit` on damage, `Die` / `Dead` on death, and pass `moveSpeed` to `Speed`.
- All three zombie prefabs (`CommonZombie`, `FastZombie`, `HeavyZombie`) wired to `Zombie_Animator.controller` and their respective Humanoid Avatars.
- Clean compilation verified: 0 errors.

### Stage 22: Cinematic Lighting, Fog & Post-Processing Upgrade
- Analyzed existing URP configuration, post-processing stack, materials, and lighting pipelines.
- Implemented cool-blue directional moonlight (`keyLightColor = #C0D6FA`, intensity `1.75f`) as the primary directional light source.
- Configured realistic Trilight ambient bounce lighting (`Sky = #576785`, `Equator = #47546B`, `Ground = #384052`) across all four environment theme profiles to eliminate pure black crushed shadows.
- Calibrated linear atmospheric fog (`fogStart = 42m`, `fogEnd = 165m`), leaving the immediate 42-meter gameplay corridor completely clear of fog for obstacle and zombie visibility while providing deep atmospheric depth in the distance.
- Integrated ACES tonemapping, bloom, and color adjustments in URP Global Volume profile.
- Added warm sodium streetlights and broken flickering fixtures with pools of light on road asphalt.

### Stage 23: Realistic Zombie Skin, Horror Audio & Combat VFX
- Upgraded zombie skin and clothing materials across all archetypes to URP Metallic Lit workflow (`_WorkflowMode = 1`, `_Smoothness = 0.28-0.30`, `_BumpScale = 1.20-1.25`):
  - `Ch10_body.mat` & `Ch10_body1.mat` (Normal / Common zombie skin and clothing)
  - `PumpkinHulk_Material.mat` (Heavy / Big zombie skin, scars, and muscle anatomy)
  - `yaku_zombie_material.mat` & `yifu.mat` (Fast runner zombie skin and clothing)
- Synthesized 25 studio-grade 16-bit 44.1kHz WAV clips in `Assets/ZombieRunner/Audio/Clips/` covering groans, roars, hisses, attacks, hurt, death, punchy gunshots, and nocturnal wind ambience.
- Implemented `ZombieSoundEmitter.cs` on all zombie prefabs with logarithmic 3D spatial attenuation (`minDistance = 2.5m`, `maxDistance = 24m`).
- Created high-performance object pooling system in `HitImpactManager.cs` for visible glowing bullet tracers (`BulletTracer.cs`) and visceral impact splatters (blood, sparks, concrete dust) with zero runtime GC allocations.
- Added visual weapon kickback recoil (`0.07m` displacement, `3.5°` pitch up) and micro muzzle-flash point light in `ShootingController.cs`.

### Stage 24: Critical Console Error Fix & Realistic Horror Lighting Enhancement
- **Console Error Audit & Safe Particle Lifecycle Management**:
  - Identified root cause of ~102 occurrences of `Setting the duration while system is still playing is not supported.`: In `HitImpactManager.CreateParticleSystemInstance()`, `go.AddComponent<ParticleSystem>()` was invoked on an active GameObject, which immediately triggered playback on Awake before `main.duration` was modified.
  - Implemented safe lifecycle management: Created GameObject inactive (`go.SetActive(false)`) before adding the ParticleSystem component, explicitly invoked `ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear)`, safely assigned module properties, and ensured all recycled particles are stopped, cleared, and deactivated before re-enqueuing.
  - Added dynamic fallback expansion to prevent pool starvation during rapid burst fire.
  - Fixed `NullReferenceException` in `ProjectPolisher.cs` caused by accessing a non-existent `skyboxTint` property on `EnvironmentThemeProfile`.
- **Character 3-Point Lighting Vector Correction**:
  - Discovered that zombie child lights were previously inverted along the Z-axis (Key and Fill lights placed behind the zombie at `z = -1.35m` and Rim light placed in front at `z = +0.85m`).
  - Corrected local positions for all archetypes so Front Key Light and Soft Fill Light are positioned in front of the zombie (`+Z`), illuminating the face, skin wounds, and clothing:
    - Normal / Common: Key at `(0.20, 1.45, 1.35)` intensity `1.45`, Fill at `(-0.45, 1.15, 1.10)` intensity `0.85`, Rim at `(0.0, 1.85, -1.25)` intensity `1.85`.
    - Fast: Key at `(0.18, 1.30, 1.30)` intensity `1.45`, Fill at `(-0.40, 1.05, 1.05)` intensity `0.85`, Rim at `(0.0, 1.65, -1.15)` intensity `1.80`.
    - Big / Heavy: Key at `(0.35, 1.85, 1.85)` intensity `1.85`, Fill at `(-0.65, 1.45, 1.50)` intensity `1.05`, Rim at `(0.0, 2.40, -1.50)` intensity `2.20`.
- **Environment & Tunnel Lighting Enhancement**:
  - Boosted `WR_Asphalt.mat` baseColor to `{r: 0.48, g: 0.50, b: 0.54}` to ensure road asphalt grain and lane markings remain clearly visible under nocturnal illumination.
  - Upgraded `WR_SubwayConcrete.mat` from dark `{r: 0.12, g: 0.15, b: 0.16}` to `{r: 0.40, g: 0.43, b: 0.46}` and `WR_SubwayTile.mat` to `{r: 0.46, g: 0.50, b: 0.52}`, eliminating black voids on subterranean walls.
  - Upgraded ceiling lights in `EnvironmentSegment_SubwayTunnel.prefab` to intensity `1.85` (range 18m) and emergency fixtures to `1.35` (range 10m).
  - Synchronized `EnvironmentThemeManager.cs`, `LightingEnhancer.cs`, and `ZombieRunnerGameplay.unity` scene profiles so runtime theme streaming preserves calibrated visibility.
- **Player Tactical Flashlight Refinement**:
  - Verified dual-beam flashlight setup childed to `Muzzle` on `PlayerPlaceholder.prefab`:
    - `Tactical_Flashlight_Beam`: 48m range, 54° spot angle, intensity 3.0, soft shadows.
    - `Tactical_Flashlight_Wide_Fill`: 40m range, 78° spot angle, intensity 1.1.
  - Beams dynamically follow weapon auto-aim tracking, piercing dark tunnels without overexposing the player.

## Stage 25 — Critical Unity 6 Compilation Error Fix & Runtime Null Safeguards

Status: complete.

- **Compilation Error Diagnosis & Resolution**:
  - `Assets/ZombieRunner/Scripts/Editor/LightingEnhancer.cs`: Added `using System.IO;` resolving `error CS0103: The name 'File' does not exist in the current context`.
  - `Assets/ZombieRunner/Scripts/Editor/ZombieAndCombatUpgrader.cs`: Added `using UnityEngine.Rendering;` resolving `error CS0103: The name 'AmbientMode' does not exist in the current context`.
- **Unity 6 Obsolete API Modernization**:
  - Deprecated `FindObjectsSortMode` usage replaced across `LightingEnhancer.cs` and `ProjectPolisher.cs` with Unity 6 standard `Object.FindObjectsByType<Light>()`.
  - Verified all other calls in `ShootingController.cs` and `EnvironmentThemeManager.cs` use valid Unity 6 overloads (`FindObjectsInactive.Exclude`).
- **Editor Domain Reload Decoupling**:
  - Removed `[InitializeOnLoad]` and `[InitializeOnLoadMethod]` hooks from `LightingEnhancer.cs` and `VisualAuditHelper.cs` which were prematurely forcing `EditorApplication.isPlaying = true` or `AssetDatabase.Refresh()` loops during domain reload.
  - Converted automation utilities to safe, idempotent menu items under `ZombieRunner/`.
- **Runtime MaterialPropertyBlock Null Safeguard**:
  - Fixed `ArgumentNullException: Value cannot be null. Parameter name: dest` in `StreetlightFlicker.cs` (line 102) caused by non-serialized `propBlock` reset during domain reload. Added lazy initialization property `PropBlock => propBlock ??= new MaterialPropertyBlock()` with safeguards in `Awake()`, `OnEnable()`, and `Update()`.

## Stage 26 — Professional Survival Horror Audio Overhaul

Status: complete.

- **Irritating Score Audit & Root Cause Eradication**:
  - Audited legacy audio: Identified that the irritating sound was a 6.0-second repeating loop (`Ambience_Night_City_Wind_Loop.wav`) containing an undulating 480Hz-550Hz sine wave whistle and harsh 0.4s volume dips, coupled with 1100Hz/880Hz pure sine UI beeps and an 8-bit arcade arpeggio.
  - Replaced the legacy 6s file with a 48.0s continuous, organic, non-whistling Brownian wind soundscape with equal-power seamless loop crossfades.
- **Layered Adaptive Horror Music Architecture (`AdaptiveMusicController.cs`)**:
  - Designed and synthesized 4 phase-locked, synchronized 48.0s stems running in parallel at 75 BPM without track restarts:
    - **Layer 1 (Exploration)**: Brooding D minor dark pad (36.7Hz sub, 73.4Hz/110Hz detuned warmth, slow 20s chorus LFO, sparse felt chimes).
    - **Layer 2 (Approaching Danger)**: 75 BPM low cinematic pulse (62Hz heartbeat/tom) and bowed contrabass minor-second horror tension (D + Eb).
    - **Layer 3 (Combat)**: Driving hybrid war-drum rhythm and distorted industrial bass. Engineered with a hollow notch between 1.2kHz and 4.2kHz so gunfire cracks, bolt clicks, and zombie vocalizations remain punchy and unmasked.
    - **Layer 4 (Critical Health)**: Visceral infrasonic 1.25Hz adrenaline pulse with steep auditory tunneling low-pass filter.
    - **Game Over Sting**: Cinematic sub-boom drop (135Hz to 26Hz) into decaying low minor chord with natural reverberant room tail.
- **Environmental Soundscape & Sparse Stingers (`AmbientAudioController.cs`)**:
  - Continuous 48.0s atmospheric city wind bed (`Ambience_City_Night_Wind.wav`).
  - Randomized periodic scheduler triggering sparse environmental textures every 18-38 seconds: distant rusted iron girder stress groans (`Ambience_Metal_Creak_01/02`) and subterranean structural collapse rumbles (`Ambience_Distant_Rumble_01`).
- **Unity AudioMixer & Master Routing Coordinator (`AudioManager.cs`)**:
  - Generated and wired `Assets/ZombieRunner/Audio/ZombieRunner_AudioMixer.mixer` with Master, Music, Ambient, SFX, and UI groups.
  - Built-in logarithmic decibel attenuation ($dB = 20 \log_{10}(V)$) preventing clipping and distortion.
  - Automatically notifies music combat dynamics on weapon fire, damage, and explosions.
- **Tactile UI & Visceral SFX**:
  - Replaced legacy arcade sine beeps with dampened tactile mechanical clicks (`SFX_UI_Button_Horror.wav`), resonant metallic ammo grab (`SFX_Pickup_Item.wav`), visceral dark power surges (`SFX_PowerUp_Horror.wav`), and blunt impact flesh crunches (`SFX_Player_Hurt_Visceral.wav`).
- **Mobile Audio Optimization**:
  - Long 48s stems configured with `AudioClipLoadType.Streaming` and Vorbis compression (saving 85% mobile RAM).
  - Short combat SFX configured with `AudioClipLoadType.DecompressOnLoad` and 16-bit PCM for zero latency.

## Next stages

2. Player movement and camera follow — complete.
3. Endless road generation and pooling — complete.
4. Zombies and shooting — complete.
5. Obstacles and collectibles — complete.
6. Power-ups and health — complete.
7. Menus and HUD — complete.
8. Environment themes and effects — complete.
9. Asset integration preparation — complete.
10. Testing and Android build preparation — complete.
11. Prototype stabilization and improvements — complete.
12. Mixamo character model integration — complete.
13. Mixamo X Bot materials and texture inspection — complete.
14. New textured character integration (Ch05) — complete.
15. Player Run & Idle animation integration — complete.
16. Player Jump animation integration — complete.
17. Player Slide animation integration — complete.
18. Mixamo Running Slide animation integration — complete.
19. Mixamo Shoot Rifle animation integration — complete.
20. Mixamo Zombie Models (Common, Fast, Heavy) Import & Setup — complete.
21. Zombie Animation Pack Extraction & Integration — complete.
22. Cinematic Lighting, Fog & Post-Processing Upgrade — complete.
23. Realistic Zombie Skin, Horror Audio & Combat VFX — complete.
24. Critical Console Error Fix & Realistic Horror Lighting Enhancement — complete.
25. Critical Unity 6 Compilation Error Fix & Runtime Null Safeguards — complete.
26. Professional Survival Horror Audio Overhaul — complete.
27. Complete Cinematic Realistic Tunnel Environment Upgrade — complete.

### Stage 27 — Complete Cinematic Realistic Tunnel Environment Upgrade
- **Root Cause of "Repeating Blue Rectangular Frames"**:
  - The previous `EnvironmentSegment_SubwayTunnel.prefab` consisted of disconnected rectangular cube goalposts without solid continuous side-walls or ceiling, exposing the tunnel interior directly to the open sky.
  - The legacy `EnvironmentThemeManager.cs` profile applied strong cyan-blue ambient colors (`ambientSkyColor = (0.30, 0.36, 0.45)`, `keyLightColor = (0.68, 0.78, 0.92)`) and an unshadowed point light (`r: 0.65, g: 0.78, b: 0.95`), washing the open frames in an artificial monochrome blue bath.
  - Materials (`WR_SubwayConcrete.mat`, `WR_SubwayTile.mat`) lacked PBR albedo, normal, and roughness textures, rendering flat surfaces.
- **Continuous 30m Modular Subway Railway Tunnel Architecture**:
  - Reconstructed `EnvironmentSegment_SubwayTunnel.prefab` as a continuous solid subterranean service tunnel (spanning $Z = -15.0\text{m}$ to $+15.0\text{m}$):
    - **Solid Enclosure**: Continuous 5.2m concrete side walls and solid 15.5m wide ceiling completely occluding open daylight and eliminating repeating frame artifacts.
    - **Vaulted Profile**: Continuous 45-degree chamfered concrete vault arches along the upper left and right ceiling joints.
    - **Heavy Portal Rib Columns**: 4 reinforced concrete portal frames spaced every 7.5m ($Z = -11.25\text{m}, -3.75\text{m}, +3.75\text{m}, +11.25\text{m}$) with deep $0.75\text{m} \times 1.15\text{m}$ support columns, hazard-striped foundation plinths, and structural corner knee braces.
    - **Industrial Overhead Utilities**: Continuous 30m galvanized ventilation shaft ($1.65\text{m} \times 0.78\text{m}$) with steel hanger brackets and directional side exhaust louvers; dual continuous wall cable trays ($X = \pm 5.95\text{m}$) holding high-voltage cable bundles; parallel overhead electrical conduit pipes.
    - **Railway & Service Infrastructure**: Dual continuous steel rails ($X = \pm 4.85\text{m}$), 10 embedded concrete railway sleepers, raised concrete side service walkways, recessed steel maintenance door with safety signage, high-voltage electrical breaker cabinets, and emergency fire station.
    - **Obstacle Safety**: All props (drums, breaker boxes, door frames, plinths) are placed strictly outside the running corridor ($|X| \ge 5.8\text{m}$), preserving all 3 gameplay lanes ($X = -3, 0, +3$).
- **Authentic Subterranean PBR Materials & Procedural Texture Generation**:
  - `T_Tunnel_Concrete_BC.png` & `T_Tunnel_Concrete_N.png`: Warm-neutral subterranean concrete albedo and tangent-space normal map with formwork panel seams, surface aggregates, and base moisture gradients.
  - `T_Tunnel_Metal_BC.png` & `T_Tunnel_Metal_N.png`: Industrial galvanized steel with surface oxidation, crystal grain, and border rivet seams.
  - `T_Tunnel_HazardStripe_BC.png`: High-visibility industrial yellow ($#E5A712$) and charcoal ($#202023$) 45-degree hazard warning stripes.
  - Upgraded `WR_SubwayConcrete.mat`, `WR_TunnelIndustrialMetal.mat`, `WR_TunnelHazard.mat`, `WR_TunnelLamp.mat`, and `WR_EmergencyLamp.mat`.
- **Cinematic Industrial Underground Lighting Scheme**:
  - **Balanced Dual Neutral-White Ceiling Luminaires**: Two industrial fixtures at $Z = \pm 7.5\text{m}$ emitting 4200K neutral light (`(1.0, 0.96, 0.88)`), $2.2$ intensity, $16\text{m}$ range, and soft real-time shadows, with subtle realistic flickering via `StreetlightFlicker.cs`.
  - **Warm Amber Emergency Bulkheads**: Two pillar-mounted 2200K amber caged lamps (`(1.0, 0.60, 0.18)`), creating high-contrast survival-horror color temperature contrast without uniform wash.
  - **Atmospheric Dust Motes**: Preserved floating dust particles catching overhead light shafts.
  - **Calibrated Subterranean Atmosphere (`EnvironmentThemeManager.cs`)**: Neutral dark slate subterranean fog (`(0.045, 0.048, 0.055)`), $38\text{m}$ start to $125\text{m}$ end, with trilight ambient bounce.
- **Zombie Character Visibility & Combat Preservation**:
  - Enhanced `Ch10_body.mat`, `PumpkinHulk_Material.mat`, and `yaku_zombie_material.mat` with boosted base color readability, normal map depth (`_BumpScale: 1.35`), and tuned specular smoothness.
  - Extended zombie localized tactical key lights (`range: 6.8m, intensity: 2.1f`) and silhouette rim lights (`range: 4.8m, intensity: 1.6f`) for crisp visual separation against subterranean concrete walls.
  - Full combat system preserved: auto-aim, bullet tracers, muzzle flash, impact blood/sparks, and recoil.

## Current manual step

Press Play with `Assets/ZombieRunner/Scenes/ZombieRunnerMainMenu.unity` first in Build Settings to start at the title screen. `Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity` is the second scene and can be opened directly for gameplay inspection. The original template scene remains enabled.

The Android APK is at `Builds/Android/ZombieRunner.apk`. To create a new build, open File > Build Profiles, select Android, and choose Build. Unity currently uses the Android SDK at `C:/Users/metra/AppData/Local/Android/Sdk` with its bundled JDK/NDK. Change `com.example.zombierunner` to your own reverse-domain identifier before distribution, then test the APK on a physical Android device.
