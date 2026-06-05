using System;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.UIServiceModule.Scripts;

namespace Feature.DebugViewModule.Scripts {
    public class DebugPresenter : PresenterBase<DebugView> {
        private readonly SaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IViewService _viewService;

        public DebugPresenter(DebugView view, SaveDataModel saveDataModel, ISaveDataService saveDataService,
            IViewService viewService) : base(view) {
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _viewService = viewService;
        }

        public override void Initialize() {
            View.GoToLevelButton.onClick.AddListener(GoToLevel);
            View.CloseDebugButton.onClick.AddListener(CloseDebug);
        }

        private void CloseDebug() {
            _viewService.HideView(ViewType.DebugView);
        }

        private void GoToLevel() {
            if (Int32.TryParse(View.GoToLevelInputField.text, out int level)) {
                _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                    .Level = level;
                _saveDataService.SaveAll();
            }
        }
    }
}