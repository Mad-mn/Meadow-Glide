using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.ColorServiceModule.Scripts
{
    [CreateAssetMenu(fileName = "CircleColorProvider", menuName = "Configs/CircleColorProvider")]
    public class CircleColorProvider : ScriptableObject
    {
        [SerializeField] private List<ColorMapping> mappings;

        public IReadOnlyList<ColorMapping> Mappings => mappings;
    }
    
    [Serializable]
    public struct ColorMapping
    {
        public CircleColorType Type;
        public Color Color;
    }
}
