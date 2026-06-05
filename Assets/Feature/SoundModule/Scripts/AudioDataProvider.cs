using Cysharp.Threading.Tasks;

namespace Feature.SoundModule.Scripts {
    public class AudioDataProvider : IAudioDataProvider {
        private readonly UniTask<AudioConfig> _audioConfigTask;

        public AudioDataProvider(UniTask<AudioConfig> audioConfigTask) {
            _audioConfigTask = audioConfigTask;
        }

        public async UniTask Initialize() {
            AudioConfig = await _audioConfigTask;
        }

        public AudioConfig AudioConfig { get; private set; }
    }
}