using Feature.AddressableModule.Scripts.Installers;
using Feature.AssetBindingModule.Scripts.Installers;
using Feature.CameraServiceModule.Scripts.Installers;
using Feature.CircleModule.Scripts.Installers;
using Feature.ColorServiceModule.Scripts.Installers;
using Feature.GameStateModule.Scripts.Installers;
using Feature.InputModule.Scripts.Installers;
using Feature.LevelInitializeModule.Scripts.Installers;
using Feature.LevelModule.Scripts.Installers;
using Feature.SaveDataModule.Scripts.Installers;
using Feature.SceneLoadModule.Scripts.Installers;
using Feature.SlideAreaModule.Scripts.Installers;
using Feature.StatusModule.Scripts.Installers;
using Feature.TrackMoveModule.Scripts.Installers;
using Feature.TutorialModule.Scripts.Installers;
using Feature.UIServiceModule.Scripts.Installers;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ProjectContextInstaller", menuName = "Installers/ProjectContextInstaller")]
public class ProjectContextInstaller : ScriptableObjectInstaller<ProjectContextInstaller>
{
    public override void InstallBindings()
    {
        AddressableModuleInstaller.Install(Container);
        AssetBindingModuleInstaller.Install(Container);
        CameraServiceModuleInstaller.Install(Container);
        TutorialModuleInstaller.Install(Container);
        TrackMoveServiceInstaller.Install(Container);
        GameStateModuleInstaller.Install(Container);
        SaveDataModuleInstaller.Install(Container);
        SceneLoadModuleInstaller.Install(Container);
        UIModuleInstaller.Install(Container);
        CircleColorModuleInstaller.Install(Container);
        LevelModuleInstaller.Install(Container);
        LevelInitializeModuleInstaller.Install(Container);
        SlideAreaModuleInstaller.Install(Container);
        InputModuleInstaller.Install(Container);
        CircleModuleInstaller.Install(Container);
        StatusModuleInstaller.Install(Container);
    }
}