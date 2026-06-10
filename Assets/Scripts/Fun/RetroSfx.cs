using UnityEngine;

public static class RetroSfx
{
    const int SampleRate = 44100;

    static AudioSource source;
    static AudioClip jumpClip;
    static AudioClip coinClip;
    static AudioClip stompClip;
    static AudioClip dashClip;
    static AudioClip shootClip;
    static AudioClip trickClip;
    static AudioClip feverClip;
    static AudioClip fanfareClip;
    static AudioClip gameOverClip;

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

    public static void PlayGameOver()
    {
        if (gameOverClip == null)
        {
            float[] notes = { 392f, 330f, 262f, 196f };
            gameOverClip = SynthArpeggio("Sfx_GameOver", notes, 0.2f, 0.45f, 0.3f);
        }

        Play(gameOverClip);
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
