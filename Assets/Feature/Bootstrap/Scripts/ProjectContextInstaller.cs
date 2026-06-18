using Feature.AddressableModule.Scripts.Installers;
using Feature.AnimationModule.Scripts.Installers;
using Feature.AssetBindingModule.Scripts.Installers;
using Feature.CameraServiceModule.Scripts.Installers;
using Feature.ChallengeModule.Scripts.Installers;
using Feature.CircleModule.Scripts.Installers;
using Feature.ColorServiceModule.Scripts.Installers;
using Feature.CoroutineRunnerModule.Scripts.Installers;
using Feature.DailyChallengeStartViewModule.Scripts.Installers;
using Feature.GameStateModule.Scripts.Installers;
using Feature.InputModule.Scripts.Installers;
using Feature.LevelInitializeModule.Scripts.Installers;
using Feature.LevelModule.Scripts.Installers;
using Feature.LevelResultModule.Scripts.Installers;
using Feature.PlayerInventoryModule.Scripts.Installers;
using Feature.PreGamePlacementModule.Scripts.Installers;
using Feature.SaveDataModule.Scripts.Installers;
using Feature.SceneLoadModule.Scripts.Installers;
using Feature.SlideAreaModule.Scripts.Installers;
using Feature.SoundModule.Scripts.Installers;
using Feature.StarModule.Scripts.Installers;
using Feature.StatusModule.Scripts.Installers;
using Feature.StripRotationModule.Scripts.Installers;
using Feature.StripsModule.Scripts.Installers;
using Feature.ToolModule.Scripts.Installers;
using Feature.TrackMoveModule.Scripts.Installers;
using Feature.TransactionModule.Scripts.Installers;
using Feature.TutorialModule.Scripts.Installers;
using Feature.UIServiceModule.Scripts.Installers;
using Feature.UndoModule.Scripts.Installers;
using Feature.VibrationModule.Scripts.Installers;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ProjectContextInstaller", menuName = "Installers/ProjectContextInstaller")]
public class ProjectContextInstaller : ScriptableObjectInstaller<ProjectContextInstaller>
{
        public override void InstallBindings()
    {
        AnimationModuleInstaller.Install(Container);
        AddressableModuleInstaller.Install(Container);
        AssetBindingModuleInstaller.Install(Container);
        CameraServiceModuleInstaller.Install(Container);
        AudioModuleInstaller.Install(Container);
        TutorialModuleInstaller.Install(Container);
        TrackMoveServiceInstaller.Install(Container);
        GameStateModuleInstaller.Install(Container);
        SaveDataModuleInstaller.Install(Container);
        SceneLoadModuleInstaller.Install(Container);
        UIModuleInstaller.Install(Container);
        ViewModelsInstaller.Install(Container);
        CircleColorModuleInstaller.Install(Container);
        LevelModuleInstaller.Install(Container);
        LevelInitializeModuleInstaller.Install(Container);
        SlideAreaModuleInstaller.Install(Container);
        InputModuleInstaller.Install(Container);
        CircleModuleInstaller.Install(Container);
        StripModuleInstaller.Install(Container);
        StripRotationModuleInstaller.Install(Container);
        StatusModuleInstaller.Install(Container);
        VibrationModuleInstaller.Install(Container);
        PreGamePlacementModuleInstaller.Install(Container);
        PlayerInventoryModuleInstaller.Install(Container);
        UndoModuleInstaller.Install(Container);
        ToolModuleInstaller.Install(Container);
        TransactionModuleInstaller.Install(Container);
        StarModuleInstaller.Install(Container);
        ChallengeModuleInstaller.Install(Container);
        CoroutineRunnerInstaller.Install(Container);
        LevelResultModuleInstaller.Install(Container);
        DailyChallengeStartViewModuleInstaller.Install(Container);
    }
}