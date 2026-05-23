using Feature.GameStateModule.Scripts.Installers;
using Feature.SceneLoadModule.Scripts.Installers;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ProjectContextInstaller", menuName = "Installers/ProjectContextInstaller")]
public class ProjectContextInstaller : ScriptableObjectInstaller<ProjectContextInstaller>
{
    public override void InstallBindings()
    {
        GameStateModuleInstaller.Install(Container);
        SceneLoadModuleInstaller.Install(Container);
    }
}