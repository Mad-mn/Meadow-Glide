using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.StatusModule.Scripts.Segments {
    [CreateAssetMenu(fileName = "SegmentStatusVisualConfig", menuName = "Configs/Visual/SegmentStatusVisualConfig")]
    public class SegmentStatusVisualConfig : ScriptableObject {
        [SerializeField] private List<SegmentStatusVisualData> _segmentStatusVisualDatas;
        
        public IReadOnlyList<SegmentStatusVisualData> SegmentStatusVisualDatas => _segmentStatusVisualDatas;
    }

    [Serializable]
    public class SegmentStatusVisualData {
        [field: SerializeField] public SegmentStatus SegmentStatus {get; private set;}
        [field: SerializeField] public Sprite StatusIcon {get; private set;}
        [field: SerializeField] public float WightCoeffiecient {get; private set;}
    }
}