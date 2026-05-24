using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.CircleModule.Scripts {
    [CreateAssetMenu(fileName = "CircleParamsConfig", menuName = "Configs/CircleParamsConfig")]
    public class CircleParamsConfig : ScriptableObject {
        [SerializeField] private float _minRadius = 1f;
        [SerializeField] private float _distanceCoefficient = 1f;

        public float GetRadius(int circleIndex) {
            return _minRadius * (circleIndex + 1) * _distanceCoefficient;
        }
    }
}