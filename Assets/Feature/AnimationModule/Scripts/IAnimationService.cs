using System;
using DG.Tweening;
using UnityEngine;

namespace Feature.AnimationModule.Scripts {
    public interface IAnimationService {
        Tween PlayFly(Transform target, Vector3 endPos, Action onComplete = null);
        Tween PlayLand(Transform target, Action onComplete = null);
        Tween PlayPunchScale(Transform target, Vector3 punch, float duration, int vibrato, float elasticity, Action onComplete = null);
    }
}
