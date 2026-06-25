using UnityEngine;

namespace Feature.CircleModule.Scripts {
    public class SegmentAnimationController : MonoBehaviour {
        [SerializeField] private SegmentStatusAnimator _statusAnimator;

        private bool _zoomed;

        public void TriggerBlockedAnimation()
        {
            if (_statusAnimator != null)
                _statusAnimator.PlayShake().Forget();
        }
    }
}