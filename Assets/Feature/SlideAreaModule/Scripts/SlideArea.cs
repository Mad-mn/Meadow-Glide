using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.ColorServiceModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SlideArea : MonoBehaviour {
        [SerializeField] private PolygonCollider2D _polygonCollider;
        [SerializeField] private LineRenderer _leftRail;
        [SerializeField] private LineRenderer _rightRail;
        [SerializeField] private SlideAreaAnimationController _slideAreaAnimation;
        
        private SlideAreaData _slideAreaData;
        private ICircleColorService _colorService;
        private List<CircleColorType> _colors;

        public int SectorIndex { get; private set; }
        public int StartCircleIndex { get; private set; }
        public int EndCircleIndex { get; private set; }

        public SlideAreaStatus Status => _slideAreaData?.SlideAreaStatus ?? SlideAreaStatus.Default;
        public List<CircleColorType> FilterColors => _colors;

        private bool _isAnimating;

        [Inject]
        private void InjectDependencies(ICircleColorService colorService) {
            _colorService = colorService;
        }

        public void TriggerBlockedAnimation() {
          _slideAreaAnimation.PlayBlockedAnimation();
        }

        private void Reset() {
            _polygonCollider = GetComponent<PolygonCollider2D>();
        }

        public void Initialize(SlideAreaConfig config, SlideAreaData slideAreaData, float innerRadius, float outerRadius) {
            SectorIndex = config.sectorIndex;
            StartCircleIndex = config.startCircleIndex;
            EndCircleIndex = config.endCircleIndex;
            _slideAreaData = slideAreaData;
            _colors = config.Colors;

            if (_polygonCollider == null)
                _polygonCollider = GetComponent<PolygonCollider2D>();

            float anglePerSegment = 360f / config.totalSegments;
            float centerAngle = config.sectorIndex * anglePerSegment;

            float startAngle = (centerAngle - anglePerSegment / 2f) * Mathf.Deg2Rad;
            float endAngle = (centerAngle + anglePerSegment / 2f) * Mathf.Deg2Rad;

            SetupRails(startAngle, endAngle, innerRadius, outerRadius);

            // Points order for collider: LB, RB, RT, LT
            float radCenterAngle = centerAngle * Mathf.Deg2Rad;

            Vector2[] points = new Vector2[5];

            points[0] = new Vector2(Mathf.Cos(startAngle) * innerRadius, Mathf.Sin(startAngle) * innerRadius);
            points[1] = new Vector2(Mathf.Cos(endAngle) * innerRadius, Mathf.Sin(endAngle) * innerRadius);
            points[2] = new Vector2(Mathf.Cos(endAngle) * outerRadius, Mathf.Sin(endAngle) * outerRadius);
            points[3] = new Vector2(Mathf.Cos(radCenterAngle) * outerRadius, Mathf.Sin(radCenterAngle) * outerRadius);
            points[4] = new Vector2(Mathf.Cos(startAngle) * outerRadius, Mathf.Sin(startAngle) * outerRadius);

            _polygonCollider.points = points;
        }

        private void SetupRails(float startAngle, float endAngle, float innerRadius, float outerRadius) {
            SetupRail(_leftRail, startAngle, innerRadius, outerRadius);
            SetupRail(_rightRail, endAngle, innerRadius, outerRadius);
        }

        private void SetupRail(LineRenderer rail, float angle, float innerR, float outerR) {
            if (rail == null)
                return;

            rail.useWorldSpace = false;
            
            int pointsCount = 10; // Increased points to handle sharp gradient transitions
            rail.positionCount = pointsCount;

            Vector3 startPos = new Vector3(Mathf.Cos(angle) * innerR, Mathf.Sin(angle) * innerR, 0);
            Vector3 endPos = new Vector3(Mathf.Cos(angle) * outerR, Mathf.Sin(angle) * outerR, 0);

            for (int i = 0; i < pointsCount; i++) {
                float t = (float)i / (pointsCount - 1);
                rail.SetPosition(i, Vector3.Lerp(startPos, endPos, t));
            }

            rail.material = _slideAreaData.Material;

            if (_slideAreaData.SlideAreaStatus == SlideAreaStatus.FilterColors && _colors != null && _colors.Count > 0) {
                Gradient gradient = new Gradient();
                gradient.mode = GradientMode.Fixed;
                
                int colorCount = Mathf.Min(_colors.Count, 8); // Unity supports max 8 keys
                GradientColorKey[] colorKeys = new GradientColorKey[colorCount];
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorCount];

                for (int i = 0; i < colorCount; i++) {
                    // In Fixed mode, the color is used for the segment BEFORE the key.
                    // So we place keys at the end of each segment (1/N, 2/N, ... 1.0)
                    float time = (float)(i + 1) / colorCount;
                    colorKeys[i] = new GradientColorKey(_colorService.GetColor(_colors[i]), time);
                    alphaKeys[i] = new GradientAlphaKey(1.0f, time);
                }

                gradient.SetKeys(colorKeys, alphaKeys);
                rail.colorGradient = gradient;
            }
        }
    }
}