using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainTutorialViewModule.Scripts {
    public class MainTutorialView : ViewBase {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public GameObject First { get; private set; }
        [field: SerializeField] public GameObject Second { get; private set; }
        [field: SerializeField] public GameObject Third { get; private set; }
    }
}