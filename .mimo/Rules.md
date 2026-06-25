# Unity Project Rules

Tech Stack:
- Unity 6
- C#
- UniTask
- DOTween
- Firebase
- Mobile (Android/iOS)

Code Style:
- Use private fields with _prefix
- Use var only when type is obvious
- Avoid LINQ in gameplay code
- Avoid allocations in Update
- Prefer UniTask over Coroutines
- Use ScriptableObjects for configuration

Architecture:
- Prefer composition over inheritance
- Keep MonoBehaviours thin
- Separate data and presentation
- Avoid singleton abuse

Performance:
- Mobile first
- Minimize GC allocations
- Cache references
- Avoid FindObjectOfType