using Cysharp.Threading.Tasks;

namespace Feature.UndoModule.Scripts.Actions
{
    public interface IUndoableAction
    {
        UniTask ExecuteReverse();
        void RestoreState();
    }
}
