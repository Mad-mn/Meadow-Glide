using System;
using Feature.TransactionModule.Scripts;
using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.ConfirmBuyViewModule.Scripts {
    public class ConfirmBuyView : ViewBase {
        [field: SerializeField] public Button NoButton { get; private set; }
        [field: SerializeField] public Button YesButton { get; private set; }
        [field: SerializeField] public TMP_Text PriceText { get; private set; }
        [field: SerializeField] public TMP_Text TitleText { get; private set; }
        [field: SerializeField] public TMP_Text PlayerCoinsAmountText { get; private set; }
    }
}