using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Feature.AnimationModule.Scripts {
    public interface IAnimationService {
        Tween PlayFly(Transform target, Vector3 endPos, Action onComplete = null);
        Tween PlayLand(Transform target, Action onComplete = null);
        Tween PlayPunchScale(Transform target, Vector3 punch, float duration, int vibrato, float elasticity, Action onComplete = null);
        void PlayShake(IEnumerable<Transform> targets, float duration, float strength, int vibrato);
        Sequence PlayLockIconBreak(GameObject lockIcon, float shakeDuration = 2f, float explodeDuration = 0.3f);
    }
}
