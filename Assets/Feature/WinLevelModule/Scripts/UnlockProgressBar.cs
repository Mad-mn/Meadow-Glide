using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockProgressBar : MonoBehaviour {
    [field: SerializeField] public TMP_Text Title {get; private set;}
    [field: SerializeField] public Image Fill {get; private set;}
}