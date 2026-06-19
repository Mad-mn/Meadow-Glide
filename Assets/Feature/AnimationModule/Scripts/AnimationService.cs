using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Feature.AnimationModule.Scripts {
    public class AnimationService : IAnimationService {
        private const float FlyDuration = 0.35f;
        private const float ArcHeightMultiplier = 0.3f;
        private const float ArcHeightMin = 0.5f;
        private const float FlyScaleTarget = 0.7f;
        private const float FlyScaleDurationMultiplier = 0.8f;
        private const Ease FlyXEase = Ease.InOutQuad;
        private const Ease FlyYEase = Ease.OutQuad;
        private const float LandPunchStrength = 0.3f;
        private const float LandPunchDuration = 0.3f;
        private const int LandPunchVibrato = 10;
        private const float LandPunchElasticity = 0.5f;

        public Tween PlayFly(Transform target, Vector3 endPos, Action onComplete = null) {
            Vector3 startPos = target.position;
            float arcHeight = Mathf.Abs(endPos.y - startPos.y) * ArcHeightMultiplier + ArcHeightMin;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(target.DOJump(endPos, arcHeight, 1, FlyDuration).SetEase(FlyYEase));
            sequence.Join(target.DOMoveX(endPos.x, FlyDuration).SetEase(FlyXEase));
            sequence.Join(target.DOScale(FlyScaleTarget, FlyDuration * FlyScaleDurationMultiplier).SetEase(Ease.InQuad));
            sequence.OnComplete(() => {
                target.position = endPos;
                target.localScale = Vector3.one;
                onComplete?.Invoke();
            });

            return sequence;
        }

        public Tween PlayLand(Transform target, Action onComplete = null) {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(target.DOPunchScale(Vector3.one * LandPunchStrength, LandPunchDuration, LandPunchVibrato, LandPunchElasticity));
            sequence.OnComplete(() => onComplete?.Invoke());
            return sequence;
        }

        public Tween PlayPunchScale(Transform target, Vector3 punch, float duration, int vibrato, float elasticity, Action onComplete = null) {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(target.DOPunchScale(punch, duration, vibrato, elasticity));
            sequence.OnComplete(() => onComplete?.Invoke());
            return sequence;
        }

        public void PlayShake(IEnumerable<Transform> targets, float duration, float strength, int vibrato) {
            foreach (var target in targets) {
                if (target == null) continue;
                target.DOShakePosition(duration, strength, vibrato);
            }
        }

        public Sequence PlayLockIconBreak(GameObject lockIcon, float shakeDuration = 2f, float explodeDuration = 0.3f) {
            if (lockIcon == null)
                return null;

            lockIcon.SetActive(true);

            CanvasGroup canvasGroup = lockIcon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = lockIcon.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;

            Transform iconTransform = lockIcon.transform;
            iconTransform.localScale = Vector3.one;

            const float shakeStrength = 8f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(iconTransform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, shakeStrength, 0), 20, 90, false, true));
            sequence.Join(iconTransform.DOShakeScale(shakeDuration, new Vector3(0.15f, 0.15f, 0), 10, 90, false));
            sequence.AppendCallback(() => {
                iconTransform.DOScale(Vector3.one * 1.8f, explodeDuration).SetEase(Ease.OutQuad);
                canvasGroup.DOFade(0f, explodeDuration).SetEase(Ease.OutQuad);
            });

            return sequence;
        }
    }
}
