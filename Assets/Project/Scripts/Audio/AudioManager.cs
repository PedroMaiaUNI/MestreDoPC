using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MestreDoPC
{
    public enum Sfx { Click, Pick, Snap, Error, Purchase, Success, Fail, Warning }

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Opcional: áudios reais, na ordem do enum Sfx (vazio = bipe gerado)")]
        [SerializeField] private AudioClip[] sfxOverrides = new AudioClip[8];
        [SerializeField] private AudioClip musicClip;

        private AudioSource sfxSource;
        private AudioSource musicSource;
        private readonly Dictionary<Sfx, AudioClip> generated = new Dictionary<Sfx, AudioClip>();

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            generated[Sfx.Click]    = Tone("click", 880f, 0.05f);
            generated[Sfx.Pick]     = Tone("pick", 520f, 0.08f);
            generated[Sfx.Snap]     = Tone("snap", 330f, 0.12f, 220f);
            generated[Sfx.Error]    = Tone("error", 160f, 0.25f, 110f);
            generated[Sfx.Purchase] = Tone("purchase", 660f, 0.14f, 990f);
            generated[Sfx.Success]  = Tone("success", 523f, 0.40f, 1046f);
            generated[Sfx.Fail]     = Tone("fail", 300f, 0.50f, 100f);
            generated[Sfx.Warning]  = Tone("warning", 440f, 0.18f);

            if (musicClip != null) PlayMusic(musicClip);
        }

        private void Update()
        {
            // Clique em qualquer elemento de UI = som de clique.
            if (Input.GetMouseButtonDown(0) && EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                Play(Sfx.Click);
        }

        // Seguro de chamar de qualquer lugar (não faz nada se não houver AudioManager, ex.: testando sem a Boot).
        public static void Play(Sfx sfx)
        {
            if (Instance != null) Instance.PlayInternal(sfx);
        }

        public static void PlayMusic(AudioClip clip)
        {
            if (Instance == null || clip == null) return;
            Instance.musicSource.clip = clip;
            Instance.musicSource.volume = Instance.MusicVolume;
            Instance.musicSource.Play();
        }

        private void PlayInternal(Sfx sfx)
        {
            int i = (int)sfx;
            var clip = i < sfxOverrides.Length && sfxOverrides[i] != null ? sfxOverrides[i] : generated[sfx];
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        // Respeita os volumes que o menu de Opções grava no SaveData.
        private float SfxVolume => SaveManager.Instance != null
            ? SaveManager.Instance.Data.masterVolume * SaveManager.Instance.Data.sfxVolume : 1f;
        private float MusicVolume => SaveManager.Instance != null
            ? SaveManager.Instance.Data.masterVolume * SaveManager.Instance.Data.musicVolume : 1f;

        // Bipe senoidal com início curto e decaimento (freqEnd > 0 faz um "deslize" de frequência).
        private static AudioClip Tone(string name, float freq, float seconds, float freqEnd = -1f)
        {
            const int sampleRate = 44100;
            int count = Mathf.CeilToInt(sampleRate * seconds);
            var data = new float[count];
            float phase = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float f = freqEnd > 0f ? Mathf.Lerp(freq, freqEnd, t) : freq;
                phase += 2f * Mathf.PI * f / sampleRate;
                float envelope = Mathf.Min(1f, t * 20f) * (1f - t);
                data[i] = Mathf.Sin(phase) * 0.35f * envelope;
            }

            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
