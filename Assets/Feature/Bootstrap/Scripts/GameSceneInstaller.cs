using Feature.CircleModule.Scripts.Installers;
using Feature.TrackMoveModule.Scripts.Installers;
using UnityEngine;
using Zenject;

namespace Feature.Bootstrap.Scripts {
    [CreateAssetMenu(fileName = "GameSceneInstaller", menuName = "Installers/GameSceneInstaller")]
    public class GameSceneInstaller : ScriptableObjectInstaller<GameSceneInstaller>
    {
        public override void InstallBindings() {
            TrackMoveServiceInstaller.Install(Container);
        }
    }
}