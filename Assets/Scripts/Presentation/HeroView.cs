// =============================================================================
// FILE: Assets/Scripts/Presentation/HeroView.cs
// OWNER ROLE: Animation/VFX Artist
// RESPONSIBILITY: Play the seven humanoid animation states and faction VFX from BattleView events.
// WORK ON: Add authored hit-stop, root motion and faction-specific VFX timing after visual playtesting.
// STATUS: Prototype implementation
// =============================================================================
using SandsOfLegends.Combat;
using UnityEngine;

namespace SandsOfLegends.Presentation
{
    public sealed class HeroView : MonoBehaviour
    {
        public Animator animator;
        public Transform hpBarAnchor;
        public ParticleSystem skillVfx;
        public ParticleSystem ultimateVfx;
        private int team;
        private int slot;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        public void Bind(int teamIndex, int slotIndex)
        {
            team = teamIndex;
            slot = slotIndex;
            transform.rotation = Quaternion.Euler(0f, team == 0 ? 90f : -90f, 0f);
        }

        public void HandleBattleEvent(BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.Death &&
                battleEvent.TargetTeam == team && battleEvent.TargetSlot == slot)
            {
                Trigger("Death");
                return;
            }

            if (battleEvent.SourceTeam == team && battleEvent.SourceSlot == slot)
            {
                switch (battleEvent.Type)
                {
                    case BattleEventType.Attack: Trigger("Attack"); break;
                    case BattleEventType.Skill: Trigger("Skill"); PlayVfx(skillVfx); break;
                    case BattleEventType.Ultimate: Trigger("Ultimate"); PlayVfx(ultimateVfx); break;
                }
            }

            if (battleEvent.TargetTeam == team && battleEvent.TargetSlot == slot &&
                battleEvent.Type != BattleEventType.Heal &&
                battleEvent.Type != BattleEventType.Death)
                Trigger("Hit");
        }

        private void Trigger(string state)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.SetTrigger(state);
        }

        private void PlayVfx(ParticleSystem source)
        {
            if (source == null) return;
            ParticleSystem instance = Instantiate(source, transform.position + Vector3.up, Quaternion.identity, transform);
            instance.Play();
            Destroy(instance.gameObject, 2f);
        }
    }
}
