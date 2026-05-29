using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.CircleModule.Scripts {
    [CreateAssetMenu(fileName = "CircleParamsConfig", menuName = "Configs/CircleParamsConfig")]
    public class CircleParamsConfig : ScriptableObject {
        [SerializeField] private float _minRadius = 1f;
        [SerializeField] private float _distanceBetweenCircles = 1f;
        [SerializeField] private float _segmentWight = 0.3f;

        public float GetRadius(int circleIndex) {
            float m = _minRadius + _segmentWight * circleIndex + _distanceBetweenCircles * circleIndex;
            return m; //_minRadius * (circleIndex + 1) * _distanceBetweenCircles;
        }
    }
}