using Zenject;

namespace Feature.TrackMoveModule.Scripts.Installers {
    public class TrackMoveServiceInstaller : Installer<TrackMoveServiceInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<MoveTrackService>()
                .AsSingle();
        }
    }
}