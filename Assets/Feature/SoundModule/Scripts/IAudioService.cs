namespace Feature.SoundModule.Scripts {
    public interface IAudioService {
        void Initialize();
        void PlaySound(AudioType audioType, float volume = 1f);
        void PlayMusic(AudioType audioType, bool loop = true, float fadeDuration = 1f);
        void StopMusic(float fadeDuration = 1f);
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void PauseAll();
        void ResumeAll();
        void StopAll();
        void ChangeEnabledState(bool enabled);
        void ChangeEnabledMusicState(bool enabled);
    }
}