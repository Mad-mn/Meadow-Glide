using System.Collections;
using System.Linq;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using UnityEngine;
using Zenject;

namespace Feature.SoundModule.Scripts {
    public class AudioService : IAudioService {
        private readonly IAudioDataProvider _audioDataProvider;
        private readonly DiContainer _diContainer;
        private readonly SaveDataModel _saveDataModel;
        private AudioConfig _audioConfig;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _canPlay = true;
        private bool _canPlayMusic = true;

        private Coroutine _currentFadeCoroutine;

        public AudioService(IAudioDataProvider audioDataProvider, DiContainer diContainer,
            SaveDataModel saveDataModel) {
            _audioDataProvider = audioDataProvider;
            _diContainer = diContainer;
            _saveDataModel = saveDataModel;
        }

        public void Initialize() {
            _musicSource = _diContainer.InstantiatePrefab(_audioDataProvider.AudioConfig.MusicSource).GetComponent<AudioSource>();
            _sfxSource = _diContainer.InstantiatePrefab(_audioDataProvider.AudioConfig.AudioSource).GetComponent<AudioSource>();
            _audioConfig = _audioDataProvider.AudioConfig;
            
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            ChangeEnabledState(_saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings).SoundsEnabled);
            ChangeEnabledMusicState(_saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings).MusicEnabled);
            PlayMusic(AudioType.Music);
        }

        public void PlaySound(AudioType audioType, float volume = 1f) {
            if(!_canPlay)
                return;
            
            AudioClip clip = GetClip(audioType);
            if (clip == null)
                return;

            _sfxSource.clip = clip;
            _sfxSource.volume = volume * _sfxVolume * _masterVolume;
            _sfxSource.Play();
        }

        public void PlayMusic(AudioType audioType, bool loop = true, float fadeDuration = 1f) {
            if(!_canPlayMusic)
                return;
            
            AudioClip clip = GetClip(audioType);
            if (clip == null)
                return;

            if (_musicSource.isPlaying) {
                StopMusic(fadeDuration);
            }

            if (_currentFadeCoroutine != null) {
                MonoBehaviour monoBehaviour = GetMonoBehaviour();
                if (monoBehaviour != null) {
                    monoBehaviour.StopCoroutine(_currentFadeCoroutine);
                }
            }

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = 0f; 
            _musicSource.Play();

            FadeIn(fadeDuration);
        }

        public void StopMusic(float fadeDuration = 1f) {
            if (!_musicSource.isPlaying)
                return;
            MonoBehaviour monoBehaviour = GetMonoBehaviour();
            if (_currentFadeCoroutine != null) {
                
                if (monoBehaviour != null) {
                    monoBehaviour.StopCoroutine(_currentFadeCoroutine);
                }
            }

            _currentFadeCoroutine = monoBehaviour.StartCoroutine(FadeOutAndStop(fadeDuration));
        }

        public void SetMasterVolume(float volume) {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume) {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume) {
            _sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void PauseAll() {
            _musicSource.Pause();
            _sfxSource.Pause();
        }

        public void ResumeAll() {
            _musicSource.UnPause();
            _sfxSource.UnPause();
        }

        public void StopAll() {
            _musicSource.Stop();
            _sfxSource.Stop();
        }

        public void ChangeEnabledState(bool enabled) {
            _canPlay = enabled;
        }
        public void ChangeEnabledMusicState(bool enabled) {
            _canPlayMusic = enabled;
        }

        private AudioClip GetClip(AudioType audioType) {
            AudioClip clip = _audioConfig.AudioData.First(data=>data.AudioType == audioType).AudioClip;
            if (clip == null) {
                Debug.LogError($"Audio clip for type {audioType} not exist");
                return null;
            }

            return clip;
        }
        private void UpdateVolumes() {
            if (_musicSource.isPlaying) {
                _musicSource.volume = _musicVolume * _masterVolume;
            }

            _sfxSource.volume = _sfxVolume * _masterVolume;
        }

        private void FadeIn(float duration) {
            MonoBehaviour monoBehaviour = GetMonoBehaviour();
            if (monoBehaviour == null)
                return;

            if (_currentFadeCoroutine != null) {
                monoBehaviour.StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = monoBehaviour.StartCoroutine(FadeInCoroutine(duration));
        }

        private IEnumerator FadeInCoroutine(float duration) {
            float elapsed = 0f;
            float targetVolume = _musicVolume * _masterVolume;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }

        private IEnumerator FadeOutAndStop(float duration) {
            float elapsed = 0f;
            float startVolume = _musicSource.volume;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.volume = 0f;
            _currentFadeCoroutine = null;
        }

        private MonoBehaviour GetMonoBehaviour() {
            return Object.FindObjectOfType<MonoBehaviour>();
        }
    }
}