using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.CircleModule.Scripts
{
    public class SegmentStatusAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeAmount = 0.1f;
        
        private bool _isAnimating;
        private Vector3 _originalLocalPos;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
                _originalLocalPos = _target.localPosition;
        }

        public async UniTaskVoid PlayShake()
        {
            if (_isAnimating || _target == null) return;
            
            _isAnimating = true;
            _originalLocalPos = _target.localPosition;
            float elapsed = 0;

            while (elapsed < _shakeDuration)
            {
                elapsed += Time.deltaTime;
                float strength = 1f - (elapsed / _shakeDuration);
                Vector3 randomOffset = Random.insideUnitSphere * _shakeAmount * strength;
                randomOffset.z = 0;
                _target.localPosition = _originalLocalPos + randomOffset;
                await UniTask.Yield();
            }

            _target.localPosition = _originalLocalPos;
            _isAnimating = false;
        }
    }
}
