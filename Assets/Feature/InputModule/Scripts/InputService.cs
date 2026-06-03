using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Feature.InputModule.Scripts {
    public class InputService : IInputService, IInitializable, IDisposable {
        private InputAction _pointAction;
        private InputAction _clickAction;

        public Vector2 PointerPosition => _pointAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool IsPointerPressed => _clickAction?.IsPressed() ?? false;
        public bool IsRightClickPressed => _rightCick?.IsPressed() ?? false;

        public event Action PointerDown;
        public event Action PointerUp;

        public void Initialize() {
            var actions = InputSystem.actions;
            if (actions == null) {
                Debug.LogError("InputService: No project-wide Input Actions asset assigned!");
                return;
            }

            var uiMap = actions.FindActionMap("UI");
            if (uiMap != null) {
                uiMap.Enable();
            } else {
                actions.Enable();
            }

            _pointAction = actions.FindAction("UI/Point");
            _clickAction = actions.FindAction("UI/Click");
            _rightCick = actions.FindAction("UI/RightClick");

            if (_clickAction != null) {
                _clickAction.performed += OnClickPerformed;
                _clickAction.canceled += OnClickCanceled;
            }
        }

        private bool _wasPressed;
        private InputAction _rightCick;

        private void OnClickPerformed(InputAction.CallbackContext context) {
            bool pressed = context.ReadValueAsButton();
            if (pressed && !_wasPressed) {
                _wasPressed = true;
                PointerDown?.Invoke();
            } else if (!pressed && _wasPressed) {
                _wasPressed = false;
                PointerUp?.Invoke();
            }
        }

        private void OnClickCanceled(InputAction.CallbackContext context) {
            if (_wasPressed) {
                _wasPressed = false;
                PointerUp?.Invoke();
            }
        }

        public void Dispose() {
            if (_clickAction != null) {
                _clickAction.performed -= OnClickPerformed;
                _clickAction.canceled -= OnClickCanceled;
            }
        }
    }
}