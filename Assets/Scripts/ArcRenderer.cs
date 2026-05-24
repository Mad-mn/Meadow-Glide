using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class ArcRenderer : MonoBehaviour
{
    [Header("Arc Configuration")]
    [Tooltip("Radius of the arc")]
    public float radius = 2f;
    
    [Range(0, 360)]
    [Tooltip("Degrees of the arc")]
    public float arcAngle = 60f;
    
    [Tooltip("Rotation offset in degrees")]
    public float offsetAngle = 0f;
    
    [Range(3, 120)]
    [Tooltip("Number of segments for smoothness")]
    public int segments = 60;

    private LineRenderer lr;

    private void OnValidate()
    {
        UpdateArc();
    }

    private void Awake()
    {
        UpdateArc();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // This ensures the line updates if variables are changed via other scripts or animation in Editor
        if (!Application.isPlaying)
        {
            UpdateArc();
        }
    }
#endif

    [ContextMenu("Update Arc")]
    public void UpdateArc()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        if (lr == null) return;

        // Ensure we have enough points
        lr.positionCount = segments + 1;
        lr.useWorldSpace = false; // Positions are relative to this GameObject

        for (int i = 0; i <= segments; i++)
        {
            float progress = (float)i / segments;
            // Calculate angle in radians
            float currentAngle = (progress * arcAngle + offsetAngle) * Mathf.Deg2Rad;
            
            // Calculate point on circle
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}
