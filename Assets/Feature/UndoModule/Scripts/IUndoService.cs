using Cysharp.Threading.Tasks;
using Feature.UndoModule.Scripts.Actions;

namespace Feature.UndoModule.Scripts
{
    public interface IUndoService
    {
        void Record(IUndoableAction action);
        UniTask Undo();
        bool CanUndo { get; }
        void Clear();
    }
}
