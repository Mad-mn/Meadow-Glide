using System;
using UnityEngine;

namespace Feature.InputModule.Scripts {
    public interface IInputService {
        Vector2 PointerPosition { get; }
        bool IsPointerPressed { get; }
        bool IsRightClickPressed { get; }
        event Action PointerDown;
        event Action PointerUp;
    }
}