using UnityEngine;
using UnityEngine.UI;

namespace Feature.AnimationModule.Scripts {
    public class SpriteAnimator : MonoBehaviour {
        [SerializeField] private Image _image;
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private float _interval = 1f;
        [SerializeField] private bool _playOnStart = true;

        private int _currentIndex;
        private float _timer;

        private void Start() {
            if (_playOnStart) Play();
        }

        private void Update() {
            if (_sprites == null || _sprites.Length == 0 || _image == null)
                return;

            _timer += Time.deltaTime;
            if (_timer >= _interval) {
                _timer -= _interval;
                _currentIndex = (_currentIndex + 1) % _sprites.Length;
                _image.sprite = _sprites[_currentIndex];
            }
        }

        public void Play() {
            _timer = 0f;
            _currentIndex = 0;
            if (_sprites.Length > 0 && _image != null)
                _image.sprite = _sprites[0];
            enabled = true;
        }

        public void Stop() {
            enabled = false;
        }

        public void SetSprites(Sprite[] sprites) {
            _sprites = sprites;
            _currentIndex = 0;
            if (_sprites.Length > 0 && _image != null)
                _image.sprite = _sprites[0];
        }
    }
}
