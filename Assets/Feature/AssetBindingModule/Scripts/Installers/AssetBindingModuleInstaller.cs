using Cysharp.Threading.Tasks;
using Feature.ChallengeModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.DailyChallengeStartViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.PerfectMapViewModule.Scripts.Configs;
using Feature.PlayerInventoryModule.Configs;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.StripsModule.Scripts;
using Feature.ToolModule.Scripts;
using Feature.TransactionModule.Scripts.Configs;
using Feature.TutorialModule.Scripts.Configs;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.AssetBindingModule.Scripts.Installers {
    public class AssetBindingModuleInstaller : Installer<AssetBindingModuleInstaller> {
        public override void InstallBindings() {
            Container.BindAddressableAsset<CircleColorProvider>(AddressConstants.CircleColorProvider);
            Container.BindAddressableAsset<LevelConfigProvider>(AddressConstants.LevelConfigProvider);
            Container.BindAddressableAsset<ViewSettings>(AddressConstants.ViewSettings);
            Container.BindAddressableAsset<CircleParamsConfig>(AddressConstants.CircleParamsConfig);
            Container.BindAddressableAsset<SegmentStatusVisualConfig>(AddressConstants.SegmentStatusVisualConfig);
            Container.BindAddressableAsset<SlideAreaStatusDataConfig>(AddressConstants.SlideAreaStatusDataConfig);
            Container.BindAddressableAsset<TutorialAssetsConfig>(AddressConstants.TutorialAssetsConfig);
            Container.BindAddressableAsset<AudioConfig>(AddressConstants.AudioConfig);
            Container.BindAddressableAsset<EconomyConfig>(AddressConstants.EconomyConfig);
            Container.BindAddressableAsset<ToolConfig>(AddressConstants.ToolConfig);
            Container.BindAddressableAsset<TransactionConfigs>(AddressConstants.TransactionConfigs);
            Container.BindAddressableAsset<ChallengeConfigs>(AddressConstants.DailyChallengeConfigs);
            Container.BindAddressableAsset<ResourceInfoConfig>(AddressConstants.ResourceInfoConfig);
            Container.BindAddressableAsset<PerfectMapRewardConfigs>(AddressConstants.PerfectMapRewardConfigs);

            Container.BindAddressableComponent<UIRoot>(AddressConstants.UIRoot);

            Container.BindAddressablePrefabComponent<GameBack>(AddressConstants.GameBack);
            Container.BindAddressablePrefabComponent<Camera>(AddressConstants.Camera);
            Container.BindAddressablePrefabComponent<CircleController>(AddressConstants.GircleModule);
            Container.BindAddressablePrefabComponent<StripController>(AddressConstants.Strip);
            Container.BindAddressablePrefabComponent<SlideArea>(AddressConstants.SlideArea);
            Container.BindAddressablePrefabComponent<EmptySlotsBack>(AddressConstants.EmptySlotsBack);
        }
    }
}