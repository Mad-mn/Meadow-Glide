using System.Collections.Generic;
using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MessageViewModule.Scripts {
    public class MessageView : ViewBase {
        [SerializeField] private TMP_Text _messageText;
        [field: SerializeField] public Image Mask { get; private set; }
        [SerializeField] private List<RectTransform> _rectTransforms;
        
        public List<RectTransform> RectTransforms => _rectTransforms;
        public void SetMessage(string message) {
            _messageText.text = message;
        }
    }
}
