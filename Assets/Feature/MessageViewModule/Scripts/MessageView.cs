using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MessageViewModule.Scripts {
    public class MessageView : ViewBase {
        [SerializeField] private TMP_Text _messageText;
        [field: SerializeField] public Image Mask { get; private set; }

        public void SetMessage(string message) {
            _messageText.text = message;
        }
    }
}
