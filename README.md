# ZOMBIE RUNNER

A 3D endless survival-horror runner game developed using Unity.

![Unity 6](https://img.shields.io/badge/Unity-6000.5.9f1-blue.svg)
![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%2017.5.0-orange.svg)
![Target Platform](https://img.shields.io/badge/Platform-Windows%20PC-success.svg)
![Git LFS](https://img.shields.io/badge/Git%20LFS-Enabled-purple.svg)

---

## Overview

**Zombie Runner** is a fast-paced 3D endless survival-horror runner combining high-speed lane-based traversal with responsive real-time combat, dynamic horror audio, and cinematic nocturnal visuals. Players sprint through apocalyptic city ruins and subterranean railway service tunnels, vaulting obstacles, sliding under barriers, scavenging ammunition and gold, and neutralizing aggressive undead threats.

---

## Features

- **Endless City & Subterranean Tunnel Environments:** Dynamically streamed 30m modular environment segments recycling seamlessly with smooth atmospheric fog, trilight ambient bounce, and cinematic lighting transitions.
- **Zombie Enemies & AI:** Multiple distinct zombie archetypes (Common Runner, Fast Sprinter, and Heavy Brute) featuring dynamic lane-aware pursuit, melee attack lunges, damage reaction flinches, and death states.
- **Weapon-Based Combat & Hit Detection:** Responsive raycast shooting controller with visual kickback recoil, dynamic auto-aim tracking, illuminated bullet tracers, and pooled blood/impact VFX.
- **Player Movement, Jumping & Sliding:** Responsive three-lane runner controller supporting instant lane shifting, grounded jump physics, low slide clearance under obstacles, and keyboard/mouse controls.
- **Coins, Ammunition & Scoring:** Real-time distance tracking and kill scoring, persistent PlayerPrefs high-score saving with record celebration, and collectible gold coins, ammo boxes, and health packs.
- **Health & Revive System:** Player health management with damage feedback, invulnerability frames, shield protection, and a one-time revive mechanic per run.
- **Cinematic Horror Atmosphere:** Cool-blue moonlight, warm sodium emergency lighting, volumetric linear fog, ACES tonemapping, and dynamic post-processing.
- **Layered Horror Audio & Dynamic Music:** 4-layer adaptive horror soundtrack crossfading across exploration, tension, combat, and critical health states, coupled with 3D spatialized positional zombie audio and tactile mechanical UI sound effects.
- **Main Menu & Settings:** Comprehensive title screen with survivor camera staging, audio volume persistence, graphics toggles, control reference guide, and seamless scene navigation.
- **Windows PC Support:** Fully tested on Windows PC with native keyboard/mouse controls and WASAPI/DirectSound multi-device audio routing.

---

## Technology Stack

- **Engine:** Unity 6 (`6000.5.9f1`)
- **Render Pipeline:** Universal Render Pipeline (URP `17.5.0`)
- **Language:** C# (.NET Standard 2.1)
- **Input System:** Unity Input System (`1.20.0`) & Legacy UI module fallback
- **Animation:** Unity Mecanim Animator Controllers with Humanoid Avatars
- **Audio:** Unity AudioMixer with custom logarithmic attenuation and procedural audio generator
- **UI:** TextMeshPro (uGUI `2.5.0` bundled runtime) with 9-sliced procedural UI sprites

---

## Project Structure

```text
zombie_runner/
├── Assets/
│   ├── Resources/                     # Runtime resource assets
│   ├── Scenes/                        # Template backup scenes (SampleScene.unity)
│   ├── Settings/                      # Universal Render Pipeline configuration assets & quality tiers
│   ├── TextMesh Pro/                  # TextMeshPro fonts, shaders, and essential resources
│   ├── TutorialInfo/                  # URP template starter metadata
│   └── ZombieRunner/                  # Primary game assets and authored content
│       ├── Animations/                # Mecanim controllers (MenuIdle, Player, Zombie)
│       ├── Audio/                     # AudioMixer, WAV SFX, MP3 tracks, and adaptive music stems
│       ├── Characters/                # Humanoid character models (Ch05, X Bot, Zombies) & animations
│       ├── Editor/                    # Project setup tools, audit helpers, and scene builders
│       ├── Materials/                 # URP Lit materials for environment, props, and characters
│       ├── Models/                    # Weapon models (Tommy Gun 3D model) and integration guides
│       ├── Prefabs/                   # Player, Zombie archetypes, Obstacles, Collectibles, and Segments
│       ├── Scenes/                    # MainMenu and Gameplay scenes
│       ├── Scripts/                   # Modular C# gameplay architecture
│       │   ├── Audio/                 # Audio managers, adaptive music controllers
│       │   ├── Collectibles/          # Coin, ammo, and health kit mechanics
│       │   ├── Core/                  # GameManager, SaveManager, ScoreManager
│       │   ├── Editor/                # Custom editor tools and lighting enhancers
│       │   ├── Enemies/               # Zombie AI, spawner, and 3D sound emitters
│       │   ├── Player/                # Player locomotion, health, and camera follow
│       │   ├── PowerUps/              # Timed shields, magnet, speed boost, double coins
│       │   ├── UI/                    # HUD, Main Menu, Pause menu, GameOver UI
│       │   ├── Utilities/             # Interfaces (IDamageable) and helpers
│       │   ├── Weapons/               # Raycast shooting, bullet tracers, hit impacts
│       │   └── World/                 # Endless road pooling, lighting transitions, flicker
│       ├── Settings/                  # Enemy ScriptableObject configurations
│       └── Textures/                  # Environment PBR textures and UI sprites
├── Packages/                          # Package manager manifest and lock file
├── ProjectSettings/                   # Engine, physics, graphics, tags, and project configuration
├── .gitattributes                     # Git LFS tracking rules for binary assets
├── .gitignore                         # Comprehensive Unity exclusion rules
├── DEVELOPMENT_STATUS.md              # Detailed stage-by-stage engineering log
└── README.md                          # Project documentation
```

---

## Controls

| Action | Primary Keyboard Key | Secondary Key / Mouse |
| :--- | :--- | :--- |
| **Move Left / Right** | `A` / `D` | `Left Arrow` / `Right Arrow` |
| **Jump** | `W` / `Space` | `Up Arrow` |
| **Slide** | `S` | `Down Arrow` |
| **Shoot** | `F` | `Left Mouse Button` |
| **Pause / Resume** | `Escape` | `P` |

---

## How to Open and Run the Project

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/Jagarlapudiradhakrishna0/zombie_runner.git
   cd zombie_runner
   git lfs pull
   ```
2. **Open Unity Hub:**
   Launch the Unity Hub application on your computer.
3. **Select Add Project from Disk:**
   Click **Add** (or **Add project from disk**) in the top-right corner of Unity Hub.
4. **Choose Project Folder:**
   Navigate to and select the cloned `zombie_runner` root directory (containing `Assets/`, `Packages/`, and `ProjectSettings/`).
5. **Open with Unity Version:**
   Select Unity Editor version **`6000.5.9f1`** (Unity 6) as specified in `ProjectSettings/ProjectVersion.txt`.
6. **Load Game Scene:**
   - For the full game experience: Open [`Assets/ZombieRunner/Scenes/ZombieRunnerMainMenu.unity`](file:///e:/zombie%20runner/Assets/ZombieRunner/Scenes/ZombieRunnerMainMenu.unity) and click **Play**.
   - For direct gameplay testing: Open [`Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity`](file:///e:/zombie%20runner/Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity).

---

## Development Team

- **Collaborator 1:** `[Name / Role / GitHub Profile]`
- **Collaborator 2:** `[Name / Role / GitHub Profile]`
- **Collaborator 3:** `[Name / Role / GitHub Profile]`
- **Collaborator 4:** `[Name / Role / GitHub Profile]`

---

## Credits and Licenses

### 3D Characters & Skeletal Animations
- **Mixamo (Adobe Systems):**
  - Player Survivor Model: Mixamo "Ch05" textured Humanoid Character Model.
  - Player Auxiliary Model: Mixamo "X Bot" Humanoid Rigging Model.
  - Zombie Archetypes: Mixamo "Ch10" (Common), "Yaku J Ignite" (Fast Runner), "Pumpkinhulk L Shaw" (Heavy Brute).
  - Animation Clips: Mixamo Shooter Pack and Scary Zombie Pack locomotion and combat animations (Run, Aiming Idle, Jump, Slide, Shoot Rifle, Walk, Attack, Bite, Death).
  - License: Adobe Mixamo Software License (royalty-free for commercial and non-commercial game development).

### Weapon Models & Textures
- **Tommy Gun (Submachine Gun):**
  - High-detail 3D Submachine Gun model with PBR metallic-roughness workflow textures.
  - Location: `Assets/ZombieRunner/Models/Weapons/TommyGun/`.

### Audio Suite
- **User-Provided Audio Tracks (`Assets/ZombieRunner/Audio/ProvidedAudio/`):**
  - Main Menu Music (`main screen intro music.mp3`)
  - Nocturnal City Soundscape (`city sound.mp3`)
  - Primary Gunshot (`gun shot.mp3`)
  - Zombie Growl & Patrol Voice (`zombie-voice-growl.mp3`)
  - Zombie Melee Lunge (`zombie-attack.mp3`)
  - Zombie Death Screams (`zombie death.mp3`)
- **Procedural / Algorithmic Audio (`Assets/ZombieRunner/Audio/Clips/`):**
  - 4-layer synchronized adaptive horror soundtrack stems.
  - Subterranean concrete reverberation drone, flesh impact splatters, concrete ricochets, item pickup chimes, tactile UI clicks.
  - License: Dedicated algorithmic audio assets released under CC0 / Public Domain. See `Assets/ZombieRunner/Audio/AUDIO_CREDITS.md` for full breakdown.

### Engine & Packages
- Unity Technologies: Universal Render Pipeline, Input System, TextMeshPro, Universal Shaders.
