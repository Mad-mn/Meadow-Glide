using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;

namespace Feature.TutorialViewModule.Scripts {
    public class TutorialView : ViewBase {
        [SerializeField] private TMP_Text _tutorialText;

        public void SetTutorialText(string text) {
            if (_tutorialText != null)
                _tutorialText.text = text;
        }
    }
}