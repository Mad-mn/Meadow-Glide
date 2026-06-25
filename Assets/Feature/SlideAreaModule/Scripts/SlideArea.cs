using System.Collections.Generic;
using Feature.CircleModule.Scripts;
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
        public int TotalSegments { get; private set; }

        public SlideAreaStatus Status => _slideAreaData?.SlideAreaStatus ?? SlideAreaStatus.Default;
        public List<CircleColorType> FilterColors => _colors;

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

        public void Initialize(SlideAreaConfig config, SlideAreaData slideAreaData, float innerBoundaryY, float outerBoundaryY,
            float leftX, float rightX) {
            SectorIndex = config.sectorIndex;
            StartCircleIndex = config.startCircleIndex;
            EndCircleIndex = config.endCircleIndex;
            TotalSegments = config.totalSegments;
            _slideAreaData = slideAreaData;
            _colors = config.Colors;

            if (_polygonCollider == null)
                _polygonCollider = GetComponent<PolygonCollider2D>();

            SetupRails(leftX, rightX, innerBoundaryY, outerBoundaryY);

            Vector2[] points = {
                new Vector2(leftX, innerBoundaryY),
                new Vector2(rightX, innerBoundaryY),
                new Vector2(rightX, outerBoundaryY),
                new Vector2(leftX, outerBoundaryY)
            };

            _polygonCollider.points = points;
        }

        private void SetupRails(float leftX, float rightX, float innerY, float outerY) {
            SetupRail(_leftRail, leftX, innerY, outerY);
            SetupRail(_rightRail, rightX, innerY, outerY);
        }

        private void SetupRail(LineRenderer rail, float x, float innerY, float outerY) {
            if (rail == null)
                return;

            rail.useWorldSpace = false;
            const int pointsCount = 10;
            rail.positionCount = pointsCount;

            Vector3 startPos = new Vector3(x, innerY, 0);
            Vector3 endPos = new Vector3(x, outerY, 0);

            for (int i = 0; i < pointsCount; i++) {
                float t = (float)i / (pointsCount - 1);
                rail.SetPosition(i, Vector3.Lerp(startPos, endPos, t));
            }

            rail.material = _slideAreaData.Material;

            if (_slideAreaData.SlideAreaStatus == SlideAreaStatus.FilterColors && _colors != null && _colors.Count > 0) {
                Gradient gradient = new Gradient();
                gradient.mode = GradientMode.Fixed;

                int colorCount = Mathf.Min(_colors.Count, 8);
                GradientColorKey[] colorKeys = new GradientColorKey[colorCount];
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorCount];

                for (int i = 0; i < colorCount; i++) {
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
