# Zombie Runner — Audio Suite Credits & Integration Documentation

## 1. User-Provided Primary Audio Pack
Extracted safely to: `Assets/ZombieRunner/Audio/ProvidedAudio/music/`  
Original ZIP preserved at: `C:/Users/metra/.gemini/antigravity/brain/dc4d155a-c540-4f2f-817a-adb1d23fd8fc/.user_uploaded/media_1793864691799_a561db67.zip`

| Provided Asset | Size | Sample Rate / Channels | Game Mapping | Role in Zombie Runner |
| :--- | :--- | :--- | :--- | :--- |
| **`main screen intro music.mp3`** | 1.53 MB | 44.1 kHz, Stereo | `MenuAudioController.menuMusicClip` | Main Menu Intro & Background Music (smooth fade-in, seamless loop, non-restarting on sub-panels) |
| **`city sound.mp3`** | 1.94 MB | 44.1 kHz, Stereo | `MenuAudioController.cityAmbienceClip` & `AmbientAudioController.windAmbienceClip` | City Environmental Ambience Bed (open-air ruined streetscape atmosphere) |
| **`gun shot.mp3`** | 36.8 KB | 44.1 kHz, Mono | `AudioManager.gunshotClips` & `GameAudioCue.Shot` | Primary Firearm Gunshot (Tommy Gun automatic fire) |
| **`zombie-voice-growl.mp3`** | 71.9 KB | 44.1 kHz, Stereo | `ZombieSoundEmitter.normalGroans`, `fastHisses`, `heavyRoars` | Zombie Voice / Idle Patrol / Alert Growl across all 5 zombie archetypes |
| **`zombie-attack.mp3`** | 16.8 KB | 48.0 kHz, Stereo | `ZombieSoundEmitter.normalAttack`, `fastAttack`, `heavyAttack` | Zombie Lunge & Melee Attack Vocalization |
| **`zombie death.mp3`** | 61.0 KB | 44.1 kHz, Stereo | `ZombieSoundEmitter.normalDeath`, `fastDeath`, `heavyDeath` & `GameAudioCue.ZombieDeath` | Zombie Expiration & Death Scream |

---

## 2. Missing-Audio Checklist & Gap Filling
The user's ZIP provided the 6 core pillars (Menu Music, City Ambience, Gunshot, Zombie Growl, Zombie Attack, Zombie Death). The table below lists the remaining game events and how they are satisfied:

| Game Event | Status in Provided Pack | Filled Asset & Path | Source / License | Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Subway Tunnel Ambience** | Not in ZIP | `Ambience_Streetlight_Hum.wav` (`Clips/`) | Algorithmic Subterranean Synthesizer (CC0) | Subterranean concrete tunnel reverberation drone during tunnel sections. |
| **Zombie Hurt / Bullet Strike** | Not in ZIP | `SFX_Zombie_Normal_Hurt_01.wav` (`Clips/`) | Algorithmic Formant Synthesizer (CC0) | Grunt of pain when a zombie is hit by bullets before dying. |
| **Flesh / Blood Bullet Impact** | Not in ZIP | `SFX_Impact_Blood_01.wav` & `_02.wav` (`Clips/`) | Algorithmic Impact Synthesizer (CC0) | Visceral audio feedback when bullets hit zombie bodies. |
| **Surface Bullet Impact (Metal/Concrete)** | Not in ZIP | `SFX_Impact_Concrete_01.wav`, `SFX_Impact_Metal_01.wav` (`Clips/`) | Algorithmic Impact Synthesizer (CC0) | Acoustic strike when bullets ricochet or impact ruined cars/roadblocks. |
| **Player Damage / Hurt** | Not in ZIP | `SFX_Player_Hurt_Visceral.wav` (`Clips/`) | Algorithmic Vocal Synthesizer (CC0) | Visceral impact grunt when the player collides with obstacles. |
| **Item Pickups (Coins / Ammo)** | Not in ZIP | `SFX_Pickup_Item.wav` (`Clips/`) | Algorithmic Acoustic Resonator (CC0) | Clean tactile chime when picking up coins or ammo boxes. |
| **Power-Up Activation** | Not in ZIP | `SFX_PowerUp_Horror.wav` (`Clips/`) | Algorithmic Sub Surge Synthesizer (CC0) | Dark resonant surge when activating invulnerability/magnet power-ups. |
| **Tactile UI Button Click** | Not in ZIP | `SFX_UI_Button_Horror.wav` (`Clips/`) | Algorithmic Acoustic Click (CC0) | Non-intrusive tactile click on menu button selection. |
| **Gameplay Adaptive Music** | Not in ZIP | `Music_Horror_Layer1..4_*.wav` (`Clips/`) | 48s Synchronized Horror Stems (CC0) | Synchronized 4-stem adaptive score that dynamically crossfades based on zombie distance and player HP. |

---

## 3. Audio Architecture & Routing
- **AudioMixer (`ZombieRunner_AudioMixer.mixer`):**
  - **Master Group:** Controls overall output volume (bound to `SaveManager.MasterVolume` / `AudioListener.volume`).
  - **Music Group:** Sub-bus for menu intro music and gameplay adaptive stems.
  - **Ambient Group:** Sub-bus for city soundscape bed and subterranean tunnel transitions.
  - **SFX Group:** Sub-bus for gunshots, zombie vocals, impacts, pickups, and UI clicks.
- **Dynamic Transition System (`AmbientAudioController`):**
  - Automatically queries `EnvironmentThemeManager.BlendFactor` (0 = City, 1 = Tunnel).
  - Smoothly crossfades `windSource` (user's `city sound.mp3`) with `tunnelSource` as the player enters or exits subterranean tunnel portals.
- **Main Menu Music Logic (`MenuAudioController`):**
  - Uses `main screen intro music.mp3` with smooth 3.5s unscaled fade-in.
  - Loops seamlessly without restarts when opening Settings or Credits.
  - Fades out smoothly (0.4s) before loading gameplay when the user clicks **Play Survival**.

---

## 4. Windows Audio Output Compatibility
- Unity’s native audio backend on Windows uses WASAPI (Windows Audio Session API) / DirectSound.
- Audio is dynamically routed to the user's default active Windows playback device without requiring game restarts:
  - Built-in laptop speakers
  - 3.5mm analog earphones and headphones
  - USB DACs / USB Gaming Headsets
  - Bluetooth Stereo Headsets (A2DP profile)
  - External HDMI / Optical audio receivers
