using System.Collections.Generic;
using UnityEngine;

namespace Feature.NotificationModule.Scripts {
    [CreateAssetMenu(fileName = "NotificationConfig", menuName = "Configs/Notification/NotificationConfig")]
    public class NotificationConfig : ScriptableObject {
        [SerializeField] private List<NotificationEntry> _entries = new();

        public IReadOnlyList<NotificationEntry> Entries => _entries;

        public NotificationEntry GetEntry(NotificationType type) {
            foreach (NotificationEntry entry in _entries) {
                if (entry.Type == type)
                    return entry;
            }
            return null;
        }
    }
}
