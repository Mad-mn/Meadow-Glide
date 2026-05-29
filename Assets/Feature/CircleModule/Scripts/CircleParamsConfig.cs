using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.CircleModule.Scripts {
    [CreateAssetMenu(fileName = "CircleParamsConfig", menuName = "Configs/CircleParamsConfig")]
    public class CircleParamsConfig : ScriptableObject {
        [SerializeField] private float _minRadius = 1f;
        [SerializeField] private float _distanceBetweenCircles = 1f;
        [SerializeField] private float _segmentWidth = 0.3f;
        [SerializeField] private float _widthExpansionCoefficient = 0.1f;

        public float DistanceBetweenCircles => _distanceBetweenCircles;

        public float GetRadius(float circleIndex) {
            float n = circleIndex;
            float d = _distanceBetweenCircles;
            float w0 = _segmentWidth;
            float c = _widthExpansionCoefficient;
            return _minRadius + n * d + n * w0 + (c * n * (n - 1)) / 2f;
        }

        public float GetRadius(int circleIndex) {
            return GetRadius((float)circleIndex);
        }

        public float GetWidth(float virtualIndex) {
            if (virtualIndex < 0) {
                // If index is negative (inner ghost), reduce width 3x faster
                return Mathf.Max(0, _segmentWidth + virtualIndex * _widthExpansionCoefficient * 3.0f);
            }
            return _segmentWidth + virtualIndex * _widthExpansionCoefficient;
        }

        public float GetWidth(int circleIndex) {
            return GetWidth((float)circleIndex);
        }

        public float GetVirtualIndex(float radius) {
            float d = _distanceBetweenCircles;
            float w0 = _segmentWidth;
            float c = _widthExpansionCoefficient;
            float r0 = _minRadius;

            if (Mathf.Abs(c) < 0.0001f) {
                float step = w0 + d;
                return step <= 0 ? 0 : (radius - r0) / step;
            }

            // Solving quadratic: 0.5*c*n^2 + (d + w0 - 0.5*c)*n + (r0 - radius) = 0
            float A = 0.5f * c;
            float B = d + w0 - 0.5f * c;
            float C_const = r0 - radius;
            
            float discriminant = B * B - 4 * A * C_const;
            if (discriminant < 0) return (radius - r0) / (d + w0);
            
            return (-B + Mathf.Sqrt(discriminant)) / (2 * A);
        }

        public float GetWidthForRadius(float radius) {
            float x = GetVirtualIndex(radius);
            return GetWidth(x);
        }
    }
}