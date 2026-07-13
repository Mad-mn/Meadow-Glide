using Cysharp.Threading.Tasks;

namespace Feature.WinLevelModule.Scripts {
    public interface IUnlockProgressConfigProvider {
        public UniTask Initialize();
        UnlockProgressData GetEntryForLevel(int playerLevelBeforeGame);
    }
}