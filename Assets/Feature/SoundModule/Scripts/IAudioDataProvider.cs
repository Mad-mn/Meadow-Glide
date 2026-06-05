using Cysharp.Threading.Tasks;

namespace Feature.SoundModule.Scripts {
    public interface IAudioDataProvider {
        UniTask Initialize();
        public AudioConfig AudioConfig { get; }
    }
}