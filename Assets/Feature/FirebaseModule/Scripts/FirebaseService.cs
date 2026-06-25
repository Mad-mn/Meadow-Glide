using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace Feature.FirebaseModule.Scripts {
    public class FirebaseService : IFirebaseService {
        private FirebaseApp _app;

        public bool IsInitialized { get; private set; }

        public UniTask Initialize() {
            var tcs = new UniTaskCompletionSource();

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;

                if (dependencyStatus == DependencyStatus.Available) {
                    _app = FirebaseApp.DefaultInstance;
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Crashlytics.IsCrashlyticsCollectionEnabled = true;
                    IsInitialized = true;
                    Debug.Log("Firebase initialized successfully");
                } else {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }

                tcs.TrySetResult();
            });

            return tcs.Task;
        }
    }
}
