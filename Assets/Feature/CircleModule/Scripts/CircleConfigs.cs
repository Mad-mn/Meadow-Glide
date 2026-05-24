using System;
using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using UnityEngine;

namespace Feature.CircleModule.Scripts
{
    [CreateAssetMenu(fileName = "CircleConfig", menuName = "Configs/CircleConfig")]
    public class CircleConfig : ScriptableObject
    {
        public int segmentCount = 4;
        public float radius = 2f;
        public List<SegmentConfig> segments = new List<SegmentConfig>();
    }

    [Serializable]
    public class SegmentConfig
    {
        public CircleColorType colorType;
        public float radius;
        public float angle;
    }
}
