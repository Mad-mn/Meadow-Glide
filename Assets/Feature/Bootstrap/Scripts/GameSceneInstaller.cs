using Feature.LevelInitializeModule.Scripts.Installers;
using UnityEngine;
using Zenject;

namespace Feature.Bootstrap.Scripts {
    [CreateAssetMenu(fileName = "GameSceneInstaller", menuName = "Installers/GameSceneInstaller")]
    public class GameSceneInstaller : ScriptableObjectInstaller<GameSceneInstaller>
    {
        public override void InstallBindings() {
            LevelInitializeModuleInstaller.Install(Container);
        }
    }
}