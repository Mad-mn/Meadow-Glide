using Feature.LevelModule.Scripts;
using UnityEngine;

namespace Feature.SlideAreaModule.Scripts {
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SlideArea : MonoBehaviour {
        [SerializeField] private PolygonCollider2D _polygonCollider;
        
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

            // Points order: LB, RB, RT, LT
            // LB: inner radius, start angle
            // RB: inner radius, end angle
            // RT: outer radius, end angle
            // LT: outer radius, start angle
            
            float radCenterAngle = centerAngle * Mathf.Deg2Rad;
            
            Vector2[] points = new Vector2[5];
            
            points[0] = new Vector2(Mathf.Cos(startAngle) * innerRadius, Mathf.Sin(startAngle) * innerRadius);
            points[1] = new Vector2(Mathf.Cos(endAngle) * innerRadius, Mathf.Sin(endAngle) * innerRadius);
            points[2] = new Vector2(Mathf.Cos(endAngle) * outerRadius, Mathf.Sin(endAngle) * outerRadius);
            points[3] = new Vector2(Mathf.Cos(radCenterAngle) * outerRadius, Mathf.Sin(radCenterAngle) * outerRadius);
            points[4] = new Vector2(Mathf.Cos(startAngle) * outerRadius, Mathf.Sin(startAngle) * outerRadius);

            _polygonCollider.points = points;
        }
    }
}