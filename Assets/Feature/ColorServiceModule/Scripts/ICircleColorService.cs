using UnityEngine;

namespace Feature.ColorServiceModule.Scripts
{
    public enum CircleColorType
    {
        None,
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Cyan,
        Magenta
    }

    public interface ICircleColorService
    {
        Color GetColor(CircleColorType type);
    }
}
