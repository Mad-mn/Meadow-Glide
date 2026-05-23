using Cysharp.Threading.Tasks;

namespace Feature.UIServiceModule.Scripts {
    public interface IViewService {
        void ShowView<T>(ViewType viewType) where T : ViewBase;
        void HideView(ViewType viewType);
        bool IsViewOpen(ViewType viewType);
    }
}