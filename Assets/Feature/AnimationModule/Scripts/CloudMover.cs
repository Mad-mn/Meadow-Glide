using DG.Tweening;
using UnityEngine;

namespace Feature.AnimationModule.Scripts {
    public class CloudMover : MonoBehaviour {
        [SerializeField] private RectTransform[] _clouds;
        [SerializeField] private RectTransform _respawnPoint;
        [SerializeField] private RectTransform _destination;
        [SerializeField] private float _speed = 30f;

        private void Start() {
            foreach (var cloud in _clouds)
                MoveCloud(cloud, cloud.anchoredPosition);
        }

        private void OnDestroy() {
            foreach (var cloud in _clouds)
                cloud.DOKill();
        }

        private void MoveCloud(RectTransform cloud, Vector2 from) {
            float distance = Vector2.Distance(from, _destination.anchoredPosition);
            float duration = distance / _speed;

            cloud.DOAnchorPos(_destination.anchoredPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    cloud.anchoredPosition = _respawnPoint.anchoredPosition;
                    MoveCloud(cloud, _respawnPoint.anchoredPosition);
                });
        }
    }
}
