using UnityEngine;

public static class RetroSfx
{
    const int SampleRate = 44100;

    static AudioSource source;
    static AudioSource musicSource;
    static AudioClip titleMusicClip;
    static AudioClip endingMusicClip;
    static AudioClip fieldMusicClip;
    static AudioClip caveMusicClip;
    static AudioClip castleMusicClip;
    static AudioClip medalClip;
    static AudioClip fireworkClip;
    static AudioClip jumpClip;
    static AudioClip coinClip;
    static AudioClip stompClip;
    static AudioClip dashClip;
    static AudioClip shootClip;
    static AudioClip trickClip;
    static AudioClip feverClip;
    static AudioClip fanfareClip;
    static AudioClip powerUpClip;
    static AudioClip gameOverClip;
    static AudioClip cutinClip;
    static AudioClip specialBoomClip;
    static AudioClip springClip;
    static AudioClip roarClip;
    static AudioClip hurtClip;

    public static void PlayJump()
    {
        if (jumpClip == null)
        {
            jumpClip = SynthClip("Sfx_Jump", 0.14f, time =>
                Square(SweepPhase(time, 240f, 620f, 0.14f)) * Decay(time, 0.14f) * 0.32f);
        }

        Play(jumpClip);
    }

    public static void PlayCoin()
    {
        if (coinClip == null)
        {
            coinClip = SynthClip("Sfx_Coin", 0.14f, time =>
            {
                float frequency = time < 0.055f ? 988f : 1319f;
                return Mathf.Sin(time * frequency * Mathf.PI * 2f) * Decay(time, 0.14f) * 0.34f;
            });
        }

        Play(coinClip);
    }

    public static void PlayStomp()
    {
        if (stompClip == null)
        {
            System.Random random = new System.Random(12345);
            stompClip = SynthClip("Sfx_Stomp", 0.19f, time =>
            {
                float body = Mathf.Sin(SweepPhase(time, 190f, 55f, 0.19f) * Mathf.PI * 2f);
                float crunch = ((float)random.NextDouble() * 2f - 1f) * 0.45f;
                float envelope = Decay(time, 0.19f);
                return (body + crunch) * envelope * envelope * 0.5f;
            });
        }

        Play(stompClip);
    }

    public static void PlayDash()
    {
        if (dashClip == null)
        {
            System.Random random = new System.Random(777);
            dashClip = SynthClip("Sfx_Dash", 0.17f, time =>
            {
                float noise = (float)random.NextDouble() * 2f - 1f;
                float swell = Mathf.Sin(Mathf.Clamp01(time / 0.17f) * Mathf.PI);
                return noise * swell * 0.2f;
            });
        }

        Play(dashClip);
    }

    public static void PlayShoot()
    {
        if (shootClip == null)
        {
            shootClip = SynthClip("Sfx_Shoot", 0.09f, time =>
                Square(SweepPhase(time, 950f, 280f, 0.09f)) * Decay(time, 0.09f) * 0.22f);
        }

        Play(shootClip);
    }

    public static void PlayTrick()
    {
        if (trickClip == null)
        {
            trickClip = SynthClip("Sfx_Trick", 0.22f, time =>
                Mathf.Sin(SweepPhase(time, 520f, 1240f, 0.22f) * Mathf.PI * 2f) * Decay(time, 0.22f) * 0.3f);
        }

        Play(trickClip);
    }

    public static void PlayFever()
    {
        if (feverClip == null)
        {
            float[] notes = { 523f, 659f, 784f, 1047f, 1319f };
            feverClip = SynthArpeggio("Sfx_Fever", notes, 0.08f, 0.28f, 0.34f);
        }

        Play(feverClip);
    }

    public static void PlayFanfare()
    {
        if (fanfareClip == null)
        {
            float[] notes = { 523f, 659f, 784f, 1047f };
            fanfareClip = SynthArpeggio("Sfx_Fanfare", notes, 0.15f, 0.5f, 0.36f);
        }

        Play(fanfareClip);
    }

    public static void PlayPowerUp()
    {
        if (powerUpClip == null)
        {
            float[] notes = { 392f, 523f, 659f, 784f, 1047f };
            powerUpClip = SynthArpeggio("Sfx_PowerUp", notes, 0.1f, 0.42f, 0.38f);
        }

        Play(powerUpClip);
    }

    public static void PlayGameOver()
    {
        if (gameOverClip == null)
        {
            float[] notes = { 392f, 330f, 262f, 196f };
            gameOverClip = SynthArpeggio("Sfx_GameOver", notes, 0.2f, 0.45f, 0.3f);
        }

        Play(gameOverClip);
    }

    public static void PlayTitleMusic()
    {
        if (titleMusicClip == null)
        {
            // Upbeat loop: square lead + sine bass an octave below
            float[] notes = { 523f, 659f, 784f, 659f, 880f, 784f, 659f, 587f,
                              523f, 659f, 784f, 1047f, 880f, 784f, 659f, 587f };
            titleMusicClip = SynthMelody("Music_Title", notes, 0.21f, 0.2f, true);
        }

        StartMusic(titleMusicClip, 0.5f);
    }

    public static void PlayEndingMusic()
    {
        if (endingMusicClip == null)
        {
            // Gentle, slow sine melody
            float[] notes = { 392f, 440f, 523f, 587f, 659f, 587f, 523f, 440f,
                              392f, 440f, 523f, 440f, 392f, 330f, 392f, 392f };
            endingMusicClip = SynthMelody("Music_Ending", notes, 0.46f, 0.16f, false);
        }

        StartMusic(endingMusicClip, 0.55f);
    }

    public static void PlayStageMusic(int stage)
    {
        AudioClip clip;

        if (stage >= 5)
        {
            // Castle: tense low ostinato
            if (castleMusicClip == null)
            {
                float[] notes = { 196f, 196f, 233f, 196f, 175f, 196f, 147f, 165f };
                castleMusicClip = SynthMelody("Music_Castle", notes, 0.27f, 0.15f, true);
            }
            clip = castleMusicClip;
        }
        else if (stage == 4)
        {
            // Cave: slow, mysterious, soft sine
            if (caveMusicClip == null)
            {
                float[] notes = { 330f, 392f, 440f, 494f, 554f, 494f, 440f, 392f };
                caveMusicClip = SynthMelody("Music_Cave", notes, 0.5f, 0.12f, false);
            }
            clip = caveMusicClip;
        }
        else
        {
            // Field: bright and bouncy
            if (fieldMusicClip == null)
            {
                float[] notes = { 523f, 587f, 659f, 784f, 659f, 587f, 523f, 392f,
                                  440f, 523f, 659f, 880f, 784f, 659f, 587f, 523f };
                fieldMusicClip = SynthMelody("Music_Field", notes, 0.18f, 0.15f, true);
            }
            clip = fieldMusicClip;
        }

        StartMusic(clip, 0.45f);
    }

    public static void PlayMedal()
    {
        if (medalClip == null)
        {
            float[] notes = { 784f, 988f, 1319f };
            medalClip = SynthArpeggio("Sfx_Medal", notes, 0.09f, 0.32f, 0.35f);
        }

        Play(medalClip);
    }

    public static void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    static void StartMusic(AudioClip clip, float volume)
    {
        EnsureSource();
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    static AudioClip SynthMelody(string name, float[] notes, float noteSeconds, float volume, bool withSquareLead)
    {
        float duration = notes.Length * noteSeconds;
        return SynthClip(name, duration, time =>
        {
            int index = Mathf.Min(Mathf.FloorToInt(time / noteSeconds), notes.Length - 1);
            float noteTime = time - index * noteSeconds;
            float frequency = notes[index];

            float attack = Mathf.Clamp01(noteTime / 0.015f);
            float release = 1f - Mathf.Clamp01(noteTime / noteSeconds) * 0.55f;
            float envelope = attack * release;

            float lead = Mathf.Sin(noteTime * frequency * Mathf.PI * 2f);
            if (withSquareLead)
            {
                lead = lead * 0.6f + Square(noteTime * frequency) * 0.4f;
            }

            // Soft bass one octave below on every other note
            float bass = index % 2 == 0
                ? Mathf.Sin(noteTime * frequency * 0.5f * Mathf.PI * 2f) * 0.45f
                : 0f;

            return (lead + bass) * envelope * volume;
        });
    }

    public static void PlayFirework()
    {
        if (fireworkClip == null)
        {
            System.Random random = new System.Random(98765);
            fireworkClip = SynthClip("Sfx_Firework", 0.45f, time =>
            {
                float boom = Mathf.Sin(SweepPhase(time, 110f, 45f, 0.45f) * Mathf.PI * 2f) * Decay(time, 0.45f);
                float crackle = time > 0.1f
                    ? ((float)random.NextDouble() * 2f - 1f) * Decay(time - 0.1f, 0.35f) * 0.35f
                    : 0f;
                return (boom * 0.5f + crackle) * 0.45f;
            });
        }

        Play(fireworkClip);
    }

    public static void PlayHurt()
    {
        if (hurtClip == null)
        {
            hurtClip = SynthClip("Sfx_Hurt", 0.28f, time =>
                Square(SweepPhase(time, 520f, 130f, 0.28f)) * Decay(time, 0.28f) * 0.3f);
        }

        Play(hurtClip);
    }

    public static void PlayRoar()
    {
        if (roarClip == null)
        {
            roarClip = SynthClip("Sfx_Roar", 0.55f, time =>
            {
                float vibrato = 1f + Mathf.Sin(time * 30f * Mathf.PI * 2f) * 0.06f;
                float growl = Square(SweepPhase(time, 130f * vibrato, 55f, 0.55f));
                float envelope = Mathf.Sin(Mathf.Clamp01(time / 0.55f) * Mathf.PI);
                return growl * envelope * 0.3f;
            });
        }

        Play(roarClip);
    }

    public static void PlaySpring()
    {
        if (springClip == null)
        {
            springClip = SynthClip("Sfx_Spring", 0.2f, time =>
                Mathf.Sin(SweepPhase(time, 160f, 950f, 0.2f) * Mathf.PI * 2f) * Decay(time, 0.2f) * 0.34f);
        }

        Play(springClip);
    }

    public static void PlayCutin()
    {
        if (cutinClip == null)
        {
            cutinClip = SynthClip("Sfx_Cutin", 0.7f, time =>
            {
                float riser = Mathf.Sin(SweepPhase(time, 180f, 1500f, 0.7f) * Mathf.PI * 2f);
                float tremolo = 0.7f + 0.3f * Mathf.Sin(time * 42f * Mathf.PI * 2f);
                return riser * tremolo * Mathf.Clamp01(time / 0.05f) * 0.3f;
            });
        }

        Play(cutinClip);
    }

    public static void PlaySpecialBoom()
    {
        if (specialBoomClip == null)
        {
            System.Random random = new System.Random(424242);
            specialBoomClip = SynthClip("Sfx_SpecialBoom", 0.65f, time =>
            {
                float body = Mathf.Sin(SweepPhase(time, 150f, 35f, 0.65f) * Mathf.PI * 2f);
                float noise = ((float)random.NextDouble() * 2f - 1f) * 0.55f;
                float envelope = Decay(time, 0.65f);
                return (body + noise * envelope) * envelope * 0.6f;
            });
        }

        Play(specialBoomClip);
    }

    static void Play(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        EnsureSource();
        source.PlayOneShot(clip);
    }

    static void EnsureSource()
    {
        if (source != null)
        {
            return;
        }

        GameObject host = new GameObject("RetroSfx");
        Object.DontDestroyOnLoad(host);
        source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = 0.85f;

        musicSource = host.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
    }

    static AudioClip SynthArpeggio(string name, float[] notes, float noteSeconds, float lastNoteSeconds, float volume)
    {
        float duration = noteSeconds * (notes.Length - 1) + lastNoteSeconds;
        return SynthClip(name, duration, time =>
        {
            int index = Mathf.Min(Mathf.FloorToInt(time / noteSeconds), notes.Length - 1);
            float noteTime = time - index * noteSeconds;
            float noteLength = index == notes.Length - 1 ? lastNoteSeconds : noteSeconds;
            float tone = Mathf.Sin(noteTime * notes[index] * Mathf.PI * 2f)
                + Square(noteTime * notes[index]) * 0.25f;
            return tone * Decay(noteTime, noteLength) * volume;
        });
    }

    static AudioClip SynthClip(string name, float duration, System.Func<float, float> wave)
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Clamp(wave((float)i / SampleRate), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static float SweepPhase(float time, float startFrequency, float endFrequency, float duration)
    {
        return startFrequency * time + (endFrequency - startFrequency) * time * time / (2f * duration);
    }

    static float Square(float phase)
    {
        return Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f;
    }

    static float Decay(float time, float duration)
    {
        float progress = Mathf.Clamp01(time / duration);
        return (1f - progress) * (1f - progress);
    }
}
