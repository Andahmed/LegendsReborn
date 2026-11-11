// =============================================================================
// FILE: Assets/Scripts/Presentation/HeroView.cs
// OWNER ROLE: Animation/VFX Artist
// RESPONSIBILITY: Bind a hero prefab to battle slots and play event-driven prototype motion/VFX.
// WORK ON: Replace procedural placeholder motion with Humanoid Animator clips: Idle/Run/Attack/Skill/Ultimate/Hit/Death.
// STATUS: Prototype presentation; production animation clips not supplied
// =============================================================================
using System.Collections;
using SandsOfLegends.Combat;
using UnityEngine;

namespace SandsOfLegends.Presentation
{
    public sealed class HeroView : MonoBehaviour
    {
        public Animator animator;
        public Transform visualRoot;
        public Transform hpBarAnchor;
        public ParticleSystem skillVfx;
        public ParticleSystem ultimateVfx;

        private int _team;
        private int _slot;
        private float _facing = 1f;
        private Vector3 _restPosition;
        private Quaternion _restRotation;
        private Coroutine _motion;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (visualRoot == null) visualRoot = transform;
            _restPosition = visualRoot.localPosition;
            _restRotation = visualRoot.localRotation;
        }

        public void Bind(int team, int slot)
        {
            _team = team;
            _slot = slot;
            _facing = team == 0 ? 1f : -1f;
            transform.rotation = Quaternion.Euler(0f, team == 0 ? 0f : 180f, 0f);
        }

        public void HandleBattleEvent(BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.Death &&
                battleEvent.TargetTeam == _team && battleEvent.TargetSlot == _slot)
            {
                PlayMotion("Death", 0f, true);
                return;
            }

            if (battleEvent.SourceTeam == _team && battleEvent.SourceSlot == _slot)
            {
                if (battleEvent.Type == BattleEventType.Attack) PlayMotion("Attack", 0.22f, false);
                else if (battleEvent.Type == BattleEventType.Skill)
                {
                    PlayMotion("Skill", 0.3f, false);
                    PlayVfx(skillVfx);
                }
                else if (battleEvent.Type == BattleEventType.Ultimate)
                {
                    PlayMotion("Ultimate", 0.42f, false);
                    PlayVfx(ultimateVfx);
                }
            }

            if (battleEvent.Type != BattleEventType.Heal &&
                battleEvent.TargetTeam == _team && battleEvent.TargetSlot == _slot)
                PlayMotion("Hit", 0.14f, false);
        }

        private void PlayVfx(ParticleSystem source)
        {
            if (source == null) return;
            ParticleSystem instance = Instantiate(source, transform.position + Vector3.up, Quaternion.identity, transform);
            instance.Play();
            Destroy(instance.gameObject, 2f);
        }

        private void PlayMotion(string state, float distance, bool death)
        {
            if (animator != null && animator.runtimeAnimatorController != null &&
                HasParameter(animator, state))
                animator.SetTrigger(state);

            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(Motion(distance, death));
        }

        private IEnumerator Motion(float distance, bool death)
        {
            if (death)
            {
                float elapsedDeath = 0f;
                Vector3 startScale = visualRoot.localScale;
                while (elapsedDeath < 0.45f)
                {
                    elapsedDeath += Time.deltaTime;
                    visualRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsedDeath / 0.45f);
                    yield return null;
                }
                visualRoot.localScale = Vector3.zero;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < 0.16f)
            {
                elapsed += Time.deltaTime;
                float amount = Mathf.Sin(Mathf.Clamp01(elapsed / 0.16f) * Mathf.PI);
                visualRoot.localPosition = _restPosition + new Vector3(0f, 0.04f * amount, distance * _facing * amount);
                visualRoot.localRotation = _restRotation * Quaternion.Euler(-8f * amount, 0f, 0f);
                yield return null;
            }
            visualRoot.localPosition = _restPosition;
            visualRoot.localRotation = _restRotation;
        }

        private static bool HasParameter(Animator target, string parameter)
        {
            AnimatorControllerParameter[] parameters = target.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == parameter && parameters[i].type == AnimatorControllerParameterType.Trigger)
                    return true;
            return false;
        }
    }
}
