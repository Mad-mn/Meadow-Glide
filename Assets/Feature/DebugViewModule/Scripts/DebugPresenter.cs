using System;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.UIServiceModule.Scripts;

namespace Feature.DebugViewModule.Scripts {
    public class DebugPresenter : PresenterBase<DebugView> {
        private readonly SaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IViewService _viewService;
        private readonly IPlayerInventoryService _playerInventoryService;

        public DebugPresenter(DebugView view, SaveDataModel saveDataModel, ISaveDataService saveDataService,
            IViewService viewService, IPlayerInventoryService playerInventoryService) : base(view) {
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _viewService = viewService;
            _playerInventoryService = playerInventoryService;
        }

        public override void Initialize() {
            View.GoToLevelButton.onClick.AddListener(GoToLevel);
            View.CloseDebugButton.onClick.AddListener(CloseDebug);
            View.Add100CoinsBUtton.onClick.AddListener(Add100Coins);
        }

        private void Add100Coins() {
            _playerInventoryService.Add(ResourceType.Coins, 100);
        }

        private void CloseDebug() {
            _viewService.HideView(ViewType.DebugView);
        }

        private void GoToLevel() {
            if (Int32.TryParse(View.GoToLevelInputField.text, out int level)) {
                _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                    .Level = level;
                _saveDataService.Save(SaveDataType.PlayerProgress);
            }
        }
    }
}