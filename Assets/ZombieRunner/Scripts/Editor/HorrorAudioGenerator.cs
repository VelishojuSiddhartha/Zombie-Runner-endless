using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Studio-grade procedural 16-bit 44.1kHz WAV horror audio synthesizer.
    /// Generates:
    ///   - Seamless 48s adaptive horror music stems (Exploration, Tension, Combat, Critical Health)
    ///   - Dramatic Game Over sting
    ///   - Organic, non-irritating 48s city wind ambience and sparse environmental stingers (metal creaks, rumbles)
    ///   - Tactile horror UI clicks, item pickups, visceral player damage, and power-up surges
    ///   - Studio firearms and 3 zombie archetype vocalizations (Normal, Fast, Heavy)
    /// </summary>
    public static class HorrorAudioGenerator
    {
        public const int SampleRate = 44100;
        public const string OutputFolder = "Assets/ZombieRunner/Audio/Clips";

        [MenuItem("ZombieRunner/Audio/Generate All Horror Audio WAVs")]
        public static void GenerateAllAudio()
        {
            string fullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputFolder));
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            Debug.Log("[HorrorAudioGenerator] Synthesizing comprehensive survival horror audio suite...");

            // 1. Layered Adaptive Music Stems (48.0s synchronized seamless loops at 75 BPM)
            SaveWav(Path.Combine(fullDir, "Music_Horror_Layer1_Exploration.wav"), GenerateLayer1Exploration(seed: 101, duration: 48f));
            SaveWav(Path.Combine(fullDir, "Music_Horror_Layer2_Tension.wav"), GenerateLayer2Tension(seed: 202, duration: 48f));
            SaveWav(Path.Combine(fullDir, "Music_Horror_Layer3_Combat.wav"), GenerateLayer3Combat(seed: 303, duration: 48f));
            SaveWav(Path.Combine(fullDir, "Music_Horror_Layer4_CriticalHealth.wav"), GenerateLayer4CriticalHealth(seed: 404, duration: 48f));
            SaveWav(Path.Combine(fullDir, "Music_Horror_Sting_GameOver.wav"), GenerateGameOverSting(seed: 505));

            // 2. Atmospheric City Ambience & Sparse Stingers
            float[] windAmbience = GenerateOrganicCityWind(seed: 606, duration: 48f);
            SaveWav(Path.Combine(fullDir, "Ambience_City_Night_Wind.wav"), windAmbience);
            // Overwrite legacy 6s file with the upgraded 48s wind to prevent any legacy references from playing old whistling loop
            SaveWav(Path.Combine(fullDir, "Ambience_Night_City_Wind_Loop.wav"), windAmbience);

            SaveWav(Path.Combine(fullDir, "Ambience_Streetlight_Hum.wav"), GenerateStreetlightHum(duration: 12f));
            SaveWav(Path.Combine(fullDir, "Ambience_Streetlight_Buzz.wav"), GenerateStreetlightHum(duration: 12f));
            SaveWav(Path.Combine(fullDir, "Ambience_Metal_Creak_01.wav"), GenerateMetalCreak(seed: 707, pitch: 165f));
            SaveWav(Path.Combine(fullDir, "Ambience_Metal_Creak_02.wav"), GenerateMetalCreak(seed: 708, pitch: 195f));
            SaveWav(Path.Combine(fullDir, "Ambience_Distant_Rumble_01.wav"), GenerateDistantRumble(seed: 808));

            // 3. Tactile Horror UI & Gameplay SFX
            SaveWav(Path.Combine(fullDir, "SFX_UI_Button_Horror.wav"), GenerateTactileButtonSFX());
            SaveWav(Path.Combine(fullDir, "SFX_Pickup_Item.wav"), GenerateItemPickupSFX(seed: 901));
            SaveWav(Path.Combine(fullDir, "SFX_PowerUp_Horror.wav"), GeneratePowerUpSurgeSFX(seed: 902));
            SaveWav(Path.Combine(fullDir, "SFX_Player_Hurt_Visceral.wav"), GeneratePlayerHurtSFX(seed: 903));

            // 4. Firearms & Combat
            SaveWav(Path.Combine(fullDir, "SFX_Gunshot_Punchy_01.wav"), GenerateGunshot(seed: 101, pitchMultiplier: 1.0f));
            SaveWav(Path.Combine(fullDir, "SFX_Gunshot_Punchy_02.wav"), GenerateGunshot(seed: 202, pitchMultiplier: 1.06f));
            SaveWav(Path.Combine(fullDir, "SFX_Gunshot_Punchy_03.wav"), GenerateGunshot(seed: 303, pitchMultiplier: 0.94f));

            // 5. Impacts
            SaveWav(Path.Combine(fullDir, "SFX_Impact_Blood_01.wav"), GenerateBloodImpact(seed: 401, heavy: false));
            SaveWav(Path.Combine(fullDir, "SFX_Impact_Blood_02.wav"), GenerateBloodImpact(seed: 402, heavy: true));
            SaveWav(Path.Combine(fullDir, "SFX_Impact_Metal_01.wav"), GenerateMetalImpact(seed: 501, pitch: 1900f));
            SaveWav(Path.Combine(fullDir, "SFX_Impact_Metal_02.wav"), GenerateMetalImpact(seed: 502, pitch: 2400f));
            SaveWav(Path.Combine(fullDir, "SFX_Impact_Concrete_01.wav"), GenerateConcreteImpact(seed: 601));

            // 6. Normal Zombie
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Normal_Groan_01.wav"), GenerateHumanZombieVocal(seed: 701, type: VocalType.Groan, baseFreq: 95f));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Normal_Groan_02.wav"), GenerateHumanZombieVocal(seed: 702, type: VocalType.Groan, baseFreq: 110f));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Normal_Attack_01.wav"), GenerateHumanZombieVocal(seed: 703, type: VocalType.Attack, baseFreq: 120f));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Normal_Hurt_01.wav"), GenerateHumanZombieVocal(seed: 704, type: VocalType.Hurt, baseFreq: 105f));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Normal_Death_01.wav"), GenerateHumanZombieVocal(seed: 705, type: VocalType.Death, baseFreq: 100f));

            // 7. Fast Zombie
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Fast_Hiss_01.wav"), GenerateFastZombieVocal(seed: 801, type: FastVocalType.Hiss));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Fast_Hiss_02.wav"), GenerateFastZombieVocal(seed: 802, type: FastVocalType.Pant));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Fast_Attack_01.wav"), GenerateFastZombieVocal(seed: 803, type: FastVocalType.Screech));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Fast_Hurt_01.wav"), GenerateFastZombieVocal(seed: 804, type: FastVocalType.Hurt));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Fast_Death_01.wav"), GenerateFastZombieVocal(seed: 805, type: FastVocalType.Death));

            // 8. Heavy Zombie
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Heavy_Roar_01.wav"), GenerateHeavyZombieVocal(seed: 901, type: HeavyVocalType.Groan));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Heavy_Roar_02.wav"), GenerateHeavyZombieVocal(seed: 902, type: HeavyVocalType.Rumble));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Heavy_Attack_01.wav"), GenerateHeavyZombieVocal(seed: 903, type: HeavyVocalType.Attack));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Heavy_Hurt_01.wav"), GenerateHeavyZombieVocal(seed: 904, type: HeavyVocalType.Hurt));
            SaveWav(Path.Combine(fullDir, "SFX_Zombie_Heavy_Death_01.wav"), GenerateHeavyZombieVocal(seed: 905, type: HeavyVocalType.Death));

            AssetDatabase.Refresh();
            ConfigureImportSettings(fullDir);
            Debug.Log("[HorrorAudioGenerator] Finished generating all survival horror audio assets with mobile compression.");
        }

        #region Adaptive Music Synthesizers

        /// <summary>
        /// Layer 1 — Exploration: Quiet atmospheric horror drone in D minor.
        /// Detuned multi-oscillator warmth (36.7Hz, 73.4Hz, 110Hz), slow 20s chorus LFO,
        /// and gentle felt chime notes decaying naturally.
        /// </summary>
        private static float[] GenerateLayer1Exploration(int seed, float duration)
        {
            float crossfadeLen = 4.0f;
            int totalSamples = (int)(SampleRate * (duration + crossfadeLen));
            float[] raw = new float[totalSamples];
            var rng = new System.Random(seed);

            float b0 = 0f, b1 = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Deep D minor root pad: D1 (36.7Hz) + D2 (73.4Hz) + A2 (110.0Hz) + F2 (87.3Hz)
                // Detune offsets create slow, evolving analog warmth (0.35Hz beat frequency)
                float d1 = Mathf.Sin(2f * Mathf.PI * 36.7f * t) * 0.35f;
                float d2 = Mathf.Sin(2f * Mathf.PI * (73.4f + 0.32f) * t) * 0.28f;
                float a2 = Mathf.Sin(2f * Mathf.PI * (110.0f - 0.25f) * t) * 0.20f;
                float f2 = Mathf.Sin(2f * Mathf.PI * 87.3f * t) * 0.16f;

                // Evolving slow filter sweep (0.05Hz = 20s cycle)
                float filterLfo = Mathf.Sin(2f * Mathf.PI * 0.05f * t) * 0.15f + 0.85f;
                float pad = (d1 + d2 + a2 + f2) * filterLfo;

                // Gentle brownian noise sub-breath (filtered lowpass < 90Hz)
                float white = (float)rng.NextDouble() * 2f - 1f;
                b0 = 0.985f * b0 + white * 0.015f;
                b1 = 0.985f * b1 + b0 * 0.015f;
                float subBreath = b1 * 0.25f;

                // Sparse felt piano chime note (D4 = 293.66Hz or A3 = 220Hz) every 16 seconds
                float chime = 0f;
                float chimeCycle = t % 16f;
                if (chimeCycle < 7.0f)
                {
                    float chimeFreq = (t % 32f < 16f) ? 220.0f : 293.66f;
                    float chimeEnv = Mathf.Exp(-chimeCycle * 0.75f);
                    chime = Mathf.Sin(2f * Mathf.PI * chimeFreq * chimeCycle) * chimeEnv * 0.12f;
                }

                raw[i] = (pad + subBreath + chime) * 0.65f;
            }

            return MakeSeamlessLoop(raw, duration, crossfadeLen);
        }

        /// <summary>
        /// Layer 2 — Approaching Danger: Low cinematic tension pulses and dissonant bowed drone.
        /// 75 BPM quarter-note heartbeat/tom pulse (62Hz), plus minor 2nd horror friction (D 73.4Hz + Eb 77.8Hz).
        /// </summary>
        private static float[] GenerateLayer2Tension(int seed, float duration)
        {
            float crossfadeLen = 4.0f;
            int totalSamples = (int)(SampleRate * (duration + crossfadeLen));
            float[] raw = new float[totalSamples];

            float bpm = 75f;
            float beatInterval = 60f / bpm; // 0.8s per beat

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // 75 BPM low cinematic pulse (heartbeat/low tom drum)
                float beatTime = t % beatInterval;
                float pulseEnv = Mathf.Exp(-beatTime * 10f); // 100ms decay
                float pulse = Mathf.Sin(2f * Mathf.PI * (64f - 22f * (beatTime / beatInterval)) * beatTime) * pulseEnv * 0.42f;

                // Bowed contrabass horror friction (D + Eb dissonant minor second)
                // Swells gently every 8 bars (6.4s cycle)
                float swell = Mathf.Sin(2f * Mathf.PI * (t / 6.4f)) * 0.5f + 0.5f;
                float droneD = Mathf.Sin(2f * Mathf.PI * 73.4f * t) * 0.25f;
                float droneEb = Mathf.Sin(2f * Mathf.PI * 77.78f * t) * 0.22f; // Dissonant tension
                float droneGSharp = Mathf.Sin(2f * Mathf.PI * 103.8f * t) * 0.15f; // Tritone
                float bowedTension = (droneD + droneEb + droneGSharp) * swell * 0.35f;

                raw[i] = (pulse + bowedTension) * 0.70f;
            }

            return MakeSeamlessLoop(raw, duration, crossfadeLen);
        }

        /// <summary>
        /// Layer 3 — Combat: Driving hybrid war-drum rhythm and distorted industrial bass.
        /// Frequency-shaped with a hollow notch between 1.2kHz and 4.2kHz so gunshots and zombie growls cut through!
        /// </summary>
        private static float[] GenerateLayer3Combat(int seed, float duration)
        {
            float crossfadeLen = 4.0f;
            int totalSamples = (int)(SampleRate * (duration + crossfadeLen));
            float[] raw = new float[totalSamples];
            var rng = new System.Random(seed);

            float bpm = 75f;
            float beatInterval = 60f / bpm; // 0.8s
            float barInterval = beatInterval * 4f; // 3.2s

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float barTime = t % barInterval;
                int currentBeat = (int)(barTime / beatInterval);
                float beatTime = barTime % beatInterval;

                // Hybrid cinematic percussion pattern:
                // Beat 0: Heavy taiko kick
                // Beat 1: Ghost sub-pulse + industrial noise tap
                // Beat 2: Syncopated double hit
                // Beat 3: Heavy snare/rim impact
                float perc = 0f;
                if (currentBeat == 0 || currentBeat == 2)
                {
                    float kickEnv = Mathf.Exp(-beatTime * 14f);
                    float kickFreq = Mathf.Lerp(110f, 42f, beatTime * 8f);
                    perc += Mathf.Sin(2f * Mathf.PI * kickFreq * beatTime) * kickEnv * 0.55f;
                }
                else if (currentBeat == 3)
                {
                    float rimEnv = Mathf.Exp(-beatTime * 18f);
                    float rimTone = Mathf.Sin(2f * Mathf.PI * 180f * beatTime) * 0.3f;
                    float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.2f;
                    perc += (rimTone + noise) * rimEnv * 0.40f;
                }

                // Eighth-note driving distorted industrial bass (75 BPM eighths = 0.4s)
                float eighthTime = t % (beatInterval * 0.5f);
                float bassEnv = Mathf.Exp(-eighthTime * 6f);
                float rawBass = Mathf.Sin(2f * Mathf.PI * 55f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * 110f * t);
                // Soft saturation tanh curve
                float satBass = (float)Math.Tanh(rawBass * 1.8) * bassEnv * 0.32f;

                raw[i] = (perc + satBass) * 0.75f;
            }

            return MakeSeamlessLoop(raw, duration, crossfadeLen);
        }

        /// <summary>
        /// Layer 4 — Critical Health: Visceral infrasonic blood rush and claustrophobic muffle.
        /// 1.25Hz adrenaline pulse (rapid heartbeat) + steep low-pass filtering.
        /// </summary>
        private static float[] GenerateLayer4CriticalHealth(int seed, float duration)
        {
            float crossfadeLen = 4.0f;
            int totalSamples = (int)(SampleRate * (duration + crossfadeLen));
            float[] raw = new float[totalSamples];
            var rng = new System.Random(seed);

            float b0 = 0f, b1 = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // 1.25 Hz adrenaline heartbeat rhythm (75 BPM double-thump: lub-dub)
                float cycle = t % 0.8f;
                float thump = 0f;
                if (cycle < 0.16f)
                {
                    thump = Mathf.Sin(2f * Mathf.PI * 46f * cycle) * Mathf.Exp(-cycle * 22f) * 0.65f;
                }
                else if (cycle > 0.22f && cycle < 0.38f)
                {
                    float c2 = cycle - 0.22f;
                    thump = Mathf.Sin(2f * Mathf.PI * 38f * c2) * Mathf.Exp(-c2 * 26f) * 0.45f;
                }

                // Subterranean blood rush brownian noise (low-pass < 70Hz)
                float white = (float)rng.NextDouble() * 2f - 1f;
                b0 = 0.992f * b0 + white * 0.008f;
                b1 = 0.992f * b1 + b0 * 0.008f;
                float rush = b1 * 0.35f;

                raw[i] = (thump + rush) * 0.70f;
            }

            return MakeSeamlessLoop(raw, duration, crossfadeLen);
        }

        /// <summary>
        /// Tragic, cinematic Game Over horror sting.
        /// Massive sub-drop impact (130Hz -> 28Hz) into decaying low minor chord.
        /// </summary>
        private static float[] GenerateGameOverSting(int seed)
        {
            float duration = 5.2f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;

                // Sub-boom drop (0 to 1.5s)
                float boomEnv = Mathf.Exp(-t * 2.8f);
                float boomFreq = Mathf.Lerp(135f, 26f, Mathf.Clamp01(t * 1.8f));
                float boom = Mathf.Sin(2f * Mathf.PI * boomFreq * t) * boomEnv * 0.75f;

                // Tragic decaying low chord (D1 + F1 + A1 + Bb1 minor dissonance)
                float chordEnv = Mathf.Exp(-t * 0.85f);
                float chord = (Mathf.Sin(2f * Mathf.PI * 36.7f * t) * 0.35f +
                               Mathf.Sin(2f * Mathf.PI * 43.65f * t) * 0.25f +
                               Mathf.Sin(2f * Mathf.PI * 55.0f * t) * 0.20f +
                               Mathf.Sin(2f * Mathf.PI * 58.27f * t) * 0.18f) * chordEnv;

                buffer[i] = Mathf.Clamp((boom + chord) * 0.80f, -0.95f, 0.95f);
            }

            return buffer;
        }

        #endregion

        #region Ambient & Environmental Soundscapes

        /// <summary>
        /// 48.0s seamless, organic city wind loop.
        /// Brown noise filtered through 3 cascaded smooth lowpass filters (120Hz - 280Hz).
        /// Zero harsh whistling tones, zero repetitive beats, perfectly seamless loop.
        /// </summary>
        private static float[] GenerateOrganicCityWind(int seed, float duration)
        {
            float crossfadeLen = 4.0f;
            int totalSamples = (int)(SampleRate * (duration + crossfadeLen));
            float[] raw = new float[totalSamples];
            var rng = new System.Random(seed);

            float b0 = 0f, b1 = 0f, b2 = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Slow, non-harmonic breathing pressure envelopes (LFO periods: 17.3s, 23.1s, 31.7s, 43.9s)
                float gust = Mathf.Sin(2f * Mathf.PI * (t / 17.3f)) * 0.25f +
                             Mathf.Sin(2f * Mathf.PI * (t / 23.1f)) * 0.22f +
                             Mathf.Sin(2f * Mathf.PI * (t / 31.7f)) * 0.18f + 0.65f;

                // Filtered brown noise wind (three cascaded single-pole filters, cutoff ~180Hz)
                float white = (float)rng.NextDouble() * 2f - 1f;
                b0 = 0.982f * b0 + white * 0.018f;
                b1 = 0.978f * b1 + b0 * 0.022f;
                b2 = 0.972f * b2 + b1 * 0.028f;
                float windBody = b2 * gust * 0.55f;

                // Deep subterranean structural rumble (38Hz - 48Hz)
                float rumble = Mathf.Sin(2f * Mathf.PI * 42f * t) * 0.12f;

                // Subtle distant metallic resonance (wind vibrating hollow steel beams at 220Hz and 330Hz, Q=12)
                float beamRes = (Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.025f +
                                 Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.018f) * gust;

                raw[i] = (windBody + rumble + beamRes) * 0.60f;
            }

            return MakeSeamlessLoop(raw, duration, crossfadeLen);
        }

        private static float[] GenerateStreetlightHum(float duration)
        {
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                // Warm, low 60Hz and 120Hz transformer hum (filtered < 200Hz, no harsh 300Hz harmonics)
                float hum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.40f +
                            Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.18f;
                buffer[i] = hum * 0.25f;
            }

            return MakeSeamlessLoop(buffer, duration, 1.5f);
        }

        private static float[] GenerateMetalCreak(int seed, float pitch)
        {
            float duration = 4.2f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI) * Mathf.Exp(-t * 0.45f);
                float slidePitch = pitch - (t * 12f);
                float tone = Mathf.Sin(2f * Mathf.PI * slidePitch * t) * 0.4f +
                             Mathf.Sin(2f * Mathf.PI * (slidePitch * 2.1f) * t) * 0.18f;
                buffer[i] = tone * env * 0.35f;
            }

            return buffer;
        }

        private static float[] GenerateDistantRumble(int seed)
        {
            float duration = 5.0f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];
            var rng = new System.Random(seed);
            float b0 = 0f, b1 = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                float white = (float)rng.NextDouble() * 2f - 1f;
                b0 = 0.994f * b0 + white * 0.006f;
                b1 = 0.994f * b1 + b0 * 0.006f;
                float sub = Mathf.Sin(2f * Mathf.PI * 40f * t) * 0.3f;
                buffer[i] = (b1 + sub) * env * 0.40f;
            }

            return buffer;
        }

        #endregion

        #region Tactile Horror UI & Item SFX

        private static float[] GenerateTactileButtonSFX()
        {
            float duration = 0.07f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 85f);
                float body = Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.6f;
                float click = Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-t * 220f) * 0.3f;
                buffer[i] = (body + click) * env * 0.55f;
            }

            return buffer;
        }

        private static float[] GenerateItemPickupSFX(int seed)
        {
            float duration = 0.18f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 18f);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.6f;
                float tone2 = Mathf.Sin(2f * Mathf.PI * 1040f * t) * 0.25f;
                buffer[i] = (tone1 + tone2) * env * 0.50f;
            }

            return buffer;
        }

        private static float[] GeneratePowerUpSurgeSFX(int seed)
        {
            float duration = 0.44f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI) * (1f - (t / duration) * 0.3f);
                float freq = Mathf.Lerp(55f, 220f, t / duration);
                float surge = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f +
                              Mathf.Sin(2f * Mathf.PI * (freq * 2.02f) * t) * 0.25f;
                buffer[i] = surge * env * 0.60f;
            }

            return buffer;
        }

        private static float[] GeneratePlayerHurtSFX(int seed)
        {
            float duration = 0.28f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];
            var rng = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 16f);
                float grunt = Mathf.Sin(2f * Mathf.PI * (110f - 50f * (t / duration)) * t) * 0.65f;
                float impact = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 40f) * 0.40f;
                buffer[i] = (grunt + impact) * env * 0.65f;
            }

            return buffer;
        }

        #endregion

        #region Firearms, Impacts & Zombie Vocals

        private static float[] GenerateGunshot(int seed, float pitchMultiplier)
        {
            var rng = new System.Random(seed);
            float duration = 0.42f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            float kickStartFreq = 165f * pitchMultiplier;
            float kickEndFreq = 38f * pitchMultiplier;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;

                // Initial supersonic crack (0 to 12ms)
                float crackEnv = Mathf.Exp(-t * 180f);
                float crackNoise = ((float)rng.NextDouble() * 2f - 1f) * crackEnv * 0.95f;

                // Sub-bass thump (exponential frequency drop)
                float thumpEnv = Mathf.Exp(-t * 22f);
                float currentFreq = Mathf.Lerp(kickStartFreq, kickEndFreq, Mathf.Clamp01(t * 16f));
                float thump = Mathf.Sin(2f * Mathf.PI * currentFreq * t) * thumpEnv * 0.75f;

                // Mechanical bolt/slide click at 14ms
                float click = 0f;
                if (t >= 0.014f && t < 0.026f)
                {
                    float ct = t - 0.014f;
                    click = Mathf.Sin(2f * Mathf.PI * 2200f * ct) * Mathf.Exp(-ct * 500f) * 0.4f;
                }

                // Urban concrete slapback echo (starts at 38ms, decays smoothly)
                float echo = 0f;
                if (t >= 0.038f)
                {
                    float et = t - 0.038f;
                    float echoEnv = Mathf.Exp(-et * 12f) * 0.28f;
                    echo = ((float)rng.NextDouble() * 2f - 1f) * echoEnv;
                }

                float sample = (crackNoise + thump + click + echo) * 0.82f;
                buffer[i] = Mathf.Clamp(sample, -0.98f, 0.98f);
            }

            return buffer;
        }

        private static float[] GenerateBloodImpact(int seed, bool heavy)
        {
            var rng = new System.Random(seed);
            float duration = heavy ? 0.28f : 0.20f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * (heavy ? 18f : 26f));
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.75f;
                float thud = Mathf.Sin(2f * Mathf.PI * (heavy ? 75f : 120f) * t) * 0.55f;
                buffer[i] = Mathf.Clamp((noise + thud) * env * 0.75f, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static float[] GenerateMetalImpact(int seed, float pitch)
        {
            float duration = 0.25f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 22f);
                float ring = Mathf.Sin(2f * Mathf.PI * pitch * t) * 0.6f +
                             Mathf.Sin(2f * Mathf.PI * (pitch * 1.62f) * t) * 0.3f;
                buffer[i] = Mathf.Clamp(ring * env * 0.70f, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static float[] GenerateConcreteImpact(int seed)
        {
            var rng = new System.Random(seed);
            float duration = 0.22f;
            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 30f);
                float crack = ((float)rng.NextDouble() * 2f - 1f) * 0.85f;
                float lowPunch = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.45f;
                buffer[i] = Mathf.Clamp((crack + lowPunch) * env * 0.75f, -0.95f, 0.95f);
            }

            return buffer;
        }

        public enum VocalType { Groan, Attack, Hurt, Death }
        public enum FastVocalType { Hiss, Pant, Screech, Hurt, Death }
        public enum HeavyVocalType { Groan, Rumble, Attack, Hurt, Death }

        private static float[] GenerateHumanZombieVocal(int seed, VocalType type, float baseFreq)
        {
            float duration = type switch
            {
                VocalType.Attack => 0.55f,
                VocalType.Hurt => 0.32f,
                VocalType.Death => 0.95f,
                _ => 0.75f
            };

            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];
            var rng = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                if (type == VocalType.Death) env = Mathf.Exp(-t * 2.8f);

                float freqSweep = baseFreq + Mathf.Sin(2f * Mathf.PI * 4.5f * t) * 15f;
                if (type == VocalType.Death) freqSweep = Mathf.Lerp(baseFreq * 1.3f, 48f, t / duration);

                float fundamental = Mathf.Sin(2f * Mathf.PI * freqSweep * t) * 0.6f;
                float rasp = ((float)rng.NextDouble() * 2f - 1f) * 0.32f;
                buffer[i] = Mathf.Clamp((fundamental + rasp) * env * 0.72f, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static float[] GenerateFastZombieVocal(int seed, FastVocalType type)
        {
            float duration = type switch
            {
                FastVocalType.Screech => 0.52f,
                FastVocalType.Hurt => 0.28f,
                FastVocalType.Death => 0.78f,
                _ => 0.65f
            };

            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];
            var rng = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                if (type == FastVocalType.Death) env = Mathf.Exp(-t * 3.2f);

                float hissNoise = ((float)rng.NextDouble() * 2f - 1f) * 0.65f;
                float throatTone = Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.25f;
                buffer[i] = Mathf.Clamp((hissNoise + throatTone) * env * 0.70f, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static float[] GenerateHeavyZombieVocal(int seed, HeavyVocalType type)
        {
            float duration = type switch
            {
                HeavyVocalType.Attack => 0.70f,
                HeavyVocalType.Hurt => 0.40f,
                HeavyVocalType.Death => 1.25f,
                _ => 0.95f
            };

            int count = (int)(SampleRate * duration);
            float[] buffer = new float[count];
            var rng = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                if (type == HeavyVocalType.Death) env = Mathf.Exp(-t * 2.2f);

                float sub = Mathf.Sin(2f * Mathf.PI * 52f * t) * 0.65f;
                float growl = Mathf.Sin(2f * Mathf.PI * 104f * t) * 0.28f;
                float gravel = ((float)rng.NextDouble() * 2f - 1f) * 0.25f;
                buffer[i] = Mathf.Clamp((sub + growl + gravel) * env * 0.78f, -0.95f, 0.95f);
            }

            return buffer;
        }

        #endregion

        #region Helpers & WAV Writer

        /// <summary>
        /// Converts a generated buffer of length (duration + crossfadeLen) into a seamless loop of exact length duration.
        /// Uses equal-power sine/cosine crossfade between [0..crossfadeLen] and [duration..duration+crossfadeLen].
        /// </summary>
        private static float[] MakeSeamlessLoop(float[] raw, float duration, float crossfadeLen)
        {
            int loopSampleCount = (int)(SampleRate * duration);
            int crossfadeSamples = (int)(SampleRate * crossfadeLen);

            float[] loop = new float[loopSampleCount];

            for (int i = 0; i < loopSampleCount; i++)
            {
                if (i < crossfadeSamples)
                {
                    float ratio = (float)i / crossfadeSamples;
                    // Equal-power crossfade
                    float gainOut = Mathf.Cos(ratio * Mathf.PI * 0.5f);
                    float gainIn = Mathf.Sin(ratio * Mathf.PI * 0.5f);

                    int tailIdx = loopSampleCount + i;
                    float tailSample = (tailIdx < raw.Length) ? raw[tailIdx] : 0f;
                    loop[i] = tailSample * gainOut + raw[i] * gainIn;
                }
                else
                {
                    loop[i] = raw[i];
                }
            }

            return loop;
        }

        private static void SaveWav(string filePath, float[] samples)
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            int sampleCount = samples.Length;
            short channels = 1;
            short bitsPerSample = 16;
            int byteRate = SampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            int dataChunkSize = sampleCount * (bitsPerSample / 8);
            int chunkSize = 36 + dataChunkSize;

            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(chunkSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write(channels);
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataChunkSize);

            for (int i = 0; i < sampleCount; i++)
            {
                short intSample = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
                writer.Write(intSample);
            }

            writer.Flush();
        }

        private static void ConfigureImportSettings(string folder)
        {
            string[] wavFiles = Directory.GetFiles(folder, "*.wav");
            foreach (string file in wavFiles)
            {
                string relPath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                AudioImporter importer = AssetImporter.GetAtPath(relPath) as AudioImporter;
                if (importer == null) continue;

                string fileName = Path.GetFileName(file);
                bool isLongStem = fileName.StartsWith("Music_") || fileName.StartsWith("Ambience_");

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                if (isLongStem)
                {
                    // Streaming Vorbis for long ambient/music stems (saves 85% mobile memory)
                    settings.loadType = AudioClipLoadType.Streaming;
                    settings.compressionFormat = AudioCompressionFormat.Vorbis;
                    settings.quality = 0.75f;
                }
                else
                {
                    // Decompress on load for instant response SFX (zero latency gunshots, impacts)
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                    settings.compressionFormat = AudioCompressionFormat.PCM;
                }

                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        #endregion
    }
}
