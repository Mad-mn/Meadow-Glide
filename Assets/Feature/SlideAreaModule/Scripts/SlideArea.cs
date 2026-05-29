using Feature.LevelModule.Scripts;
using UnityEngine;

namespace Feature.SlideAreaModule.Scripts {
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SlideArea : MonoBehaviour {
        [SerializeField] private PolygonCollider2D _polygonCollider;
        [SerializeField] private LineRenderer _leftRail;
        [SerializeField] private LineRenderer _rightRail;
        
        public int SectorIndex { get; private set; }

        private void Reset() {
            _polygonCollider = GetComponent<PolygonCollider2D>();
        }

        public void Initialize(SlideAreaConfig config, float innerRadius, float outerRadius) {
            SectorIndex = config.sectorIndex;
            if (_polygonCollider == null)
                _polygonCollider = GetComponent<PolygonCollider2D>();

            float anglePerSegment = 360f / config.totalSegments;
            float centerAngle = config.sectorIndex * anglePerSegment;
            
            // In CircleSegment, the arc is centered at the rotation angle.
            // StartAngle and EndAngle relative to the center angle.
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
            SetupRail(_leftRail, startAngle, innerRadius, outerRadius, "LeftRail");
            SetupRail(_rightRail, endAngle, innerRadius, outerRadius, "RightRail");
        }

        private void SetupRail(LineRenderer rail, float angle, float innerR, float outerR, string defaultName) {
            if (rail == null) return;

            rail.useWorldSpace = false;
            rail.positionCount = 2;
            
            Vector3 startPos = new Vector3(Mathf.Cos(angle) * innerR, Mathf.Sin(angle) * innerR, 0);
            Vector3 endPos = new Vector3(Mathf.Cos(angle) * outerR, Mathf.Sin(angle) * outerR, 0);
            
            rail.SetPosition(0, startPos);
            rail.SetPosition(1, endPos);

            // Visual styling: thin neon cyan line
            rail.startWidth = 0.05f;
            rail.endWidth = 0.05f;
            
            /*Color neonColor = new Color(0f, 1f, 1f, 0.5f); // Cyan with alpha
            rail.startColor = neonColor;
            rail.endColor = neonColor;*/
        }
    }
}