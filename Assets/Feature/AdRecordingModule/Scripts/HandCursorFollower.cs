#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace Feature.AdRecordingModule.Scripts {
    public class HandCursorFollower : MonoBehaviour {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Camera _camera;
        private Sprite _releasedSprite;
        private Sprite _pressedSprite;
        private InputAction _pointerDownAction;
        private InputAction _pointerUpAction;

        public void Initialize(AdRecordingConfig config) {
            _releasedSprite = config.ReleasedSprite;
            _pressedSprite = config.PressedSprite;

            _spriteRenderer.sprite = _releasedSprite;
            _spriteRenderer.sortingOrder = 9999;

            var gameplayActions = new InputActionMap("AdRecording_Cursor");
            _pointerDownAction = gameplayActions.AddAction("PointerDown", type: InputActionType.Button);
            _pointerDownAction.AddBinding("<Mouse>/leftButton");
            _pointerUpAction = gameplayActions.AddAction("PointerUp", type: InputActionType.Button);
            _pointerUpAction.AddBinding("<Mouse>/leftButton");
            gameplayActions.Enable();

            _pointerDownAction.performed += OnPointerDown;
            _pointerUpAction.canceled += OnPointerUp;
        }

        private void Update() {
            if (_camera == null) {
                _camera = FindObjectOfType<Camera>();
                if (_camera == null) return;
            }

            if (Mouse.current == null) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = -_camera.transform.position.z;
            transform.position = _camera.ScreenToWorldPoint(mousePos);
        }

        private void OnPointerDown(InputAction.CallbackContext ctx) {
            _spriteRenderer.sprite = _pressedSprite;
        }

        private void OnPointerUp(InputAction.CallbackContext ctx) {
            _spriteRenderer.sprite = _releasedSprite;
        }

        private void OnDisable() {
            if (_pointerDownAction != null)
                _pointerDownAction.performed -= OnPointerDown;
            if (_pointerUpAction != null)
                _pointerUpAction.canceled -= OnPointerUp;
        }
    }
}
#endif
