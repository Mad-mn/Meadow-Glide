using Feature.TutorialModule.Scripts.Tutorials;

namespace Feature.TutorialModule.Scripts.Factory {
    public interface ITutorialFactory {
        ITutorial CreateTutorial(TutorialType tutorialType);
    }
}