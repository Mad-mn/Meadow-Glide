using System;
using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.CircleModule.Scripts
{
    [CreateAssetMenu(fileName = "CircleConfig", menuName = "Configs/CircleConfig")]
    public class CircleConfig : ScriptableObject
    {
        public int SegmentCount = 4;
        public List<SegmentConfig> Segments = new List<SegmentConfig>();
    }

    [Serializable]
    public class SegmentConfig
    {
        public CircleColorType ColorType;
        public float Radius;
        public float Angle;
        public SegmentStatus SegmentStatus;
    }
}
