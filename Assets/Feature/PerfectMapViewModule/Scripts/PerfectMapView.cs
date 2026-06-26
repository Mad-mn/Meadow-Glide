using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.PerfectMapViewModule.Scripts {
    public class PerfectMapView : ViewBase {
        [field: SerializeField] public LevelInfoPerfectChallenge InfoPrefab { get; private set; }
        [field: SerializeField] public RectTransform InfoParent { get; private set; }
        [field: SerializeField] public ScrollRect ScrollRect { get; private set; }
        [field: SerializeField] public Button CloseButton { get; private set; }
    }
}