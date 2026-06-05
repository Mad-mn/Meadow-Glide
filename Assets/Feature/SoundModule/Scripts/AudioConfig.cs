using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.SoundModule.Scripts {
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject {
        [SerializeField] private List<AudioData> _audioData = new List<AudioData>();
        [field: SerializeField] public GameObject AudioSource { get; private set; }
        [field: SerializeField] public GameObject MusicSource { get; private set; }
        
        public IReadOnlyList<AudioData> AudioData => _audioData;
    }

    [Serializable]
    public class AudioData {
        public AudioType AudioType;
        public AudioClip AudioClip;
    }
}