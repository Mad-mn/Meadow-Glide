using System.Collections.Generic;
using UnityEngine;

namespace Feature.StatusModule.Scripts.SlideAreas {
    [CreateAssetMenu (fileName = "SlideAreaStatusDataConfig", menuName = "Configs/Visual/SlideAreaStatusDataConfig")]
    public class SlideAreaStatusDataConfig : ScriptableObject {
        [SerializeField] private List<SlideAreaData> _slideAreaDatas;
        public IReadOnlyList<SlideAreaData> SlideAreaDatas => _slideAreaDatas;
    }
}