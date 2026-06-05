using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StatusModule.Scripts.SlideAreas;
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

            Container.BindAddressableComponent<UIRoot>(AddressConstants.UIRoot);
            
            Container.BindAddressablePrefabComponent<Camera>(AddressConstants.Camera);
            Container.BindAddressablePrefabComponent<CircleController>(AddressConstants.GircleModule);
            Container.BindAddressablePrefabComponent<SlideArea>(AddressConstants.SlideArea);
        }
    }
}