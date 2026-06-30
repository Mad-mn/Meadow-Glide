using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.AnimationModule.Scripts {
    public class SpriteAnimator : MonoBehaviour {
        [SerializeField] private Image _image;
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private float _interval = 1f;
        [SerializeField] private bool _playOnStart = true;

        private Sequence _sequence;

        private void Start() {
            if (_playOnStart) Play();
        }

        private void OnDestroy() {
            _sequence?.Kill();
        }

        public void Play() {
            _sequence?.Kill();
            if (_sprites == null || _sprites.Length == 0 || _image == null) return;

            _image.sprite = _sprites[0];
            _sequence = DOTween.Sequence().SetLoops(-1);
            for (int i = 0; i < _sprites.Length; i++) {
                var sprite = _sprites[i];
                _sequence.AppendCallback(() => _image.sprite = sprite);
                if (i < _sprites.Length - 1)
                    _sequence.AppendInterval(_interval);
            }
            _sequence.AppendInterval(_interval);
        }

        public void Stop() {
            _sequence?.Kill();
            _sequence = null;
        }

        public void SetSprites(Sprite[] sprites) {
            _sprites = sprites;
            if (_sprites.Length > 0 && _image != null)
                _image.sprite = _sprites[0];
        }
    }
}
