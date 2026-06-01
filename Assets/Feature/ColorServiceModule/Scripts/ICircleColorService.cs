using UnityEngine;

namespace Feature.ColorServiceModule.Scripts {
    public enum CircleColorType {
        None = 0,
        Red = 2,
        Blue = 3,
        Green = 4,
        Yellow = 5,
        Cyan = 6,
        Magenta = 7
    }

    public interface ICircleColorService {
        Color GetColor(CircleColorType type);
    }
}