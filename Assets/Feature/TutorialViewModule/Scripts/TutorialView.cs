using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;

namespace Feature.TutorialViewModule.Scripts {
    public class TutorialView : ViewBase {
        [SerializeField] private TMP_Text[] _textZones = new TMP_Text[3];

        public void SetTutorialText(string text, int zone) {
            for (int i = 0; i < _textZones.Length; i++) {
                if (_textZones[i] == null) continue;
                if (i == zone) {
                    _textZones[i].text = text;
                    _textZones[i].gameObject.SetActive(true);
                }
                else {
                    _textZones[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
