// =============================================================================
// FILE: Assets/Scripts/Presentation/BattleView.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Instantiate registered character prefabs into the five formation slots, play BattleSim and dispatch combat events to HeroView.
// WORK ON: Add five-member roster selection and production presentation timing after the campaign formation UI is ready.
// STATUS: 3D prefab battle presentation
// =============================================================================
using System;
using SandsOfLegends.Combat;
using UnityEngine;

namespace SandsOfLegends.Presentation
{
    public sealed class BattleView : MonoBehaviour
    {
        public event Action<BattleEvent> EventPlayed;
        public int Speed { get; private set; }
        public Transform[] PlayerModels { get; private set; }
        public Transform[] EnemyModels { get; private set; }

        private BattleSim simulation;
        private float accumulator;
        private int eventCursor;

        private void Awake() { Speed = 1; }

        public void ConfigurePresentation(
            GameObject[] playerPrefabs,
            GameObject[] enemyPrefabs,
            Transform[] playerSlots,
            Transform[] enemySlots)
        {
            if (playerSlots == null || playerSlots.Length < 5 ||
                enemySlots == null || enemySlots.Length < 5)
                throw new InvalidOperationException("Battle requires five formation slot transforms per team.");
            if (playerPrefabs == null || playerPrefabs.Length < 4 ||
                enemyPrefabs == null || enemyPrefabs.Length < 4)
                throw new InvalidOperationException("Prototype Battle requires four player and enemy prefabs.");

            PlayerModels = new Transform[4];
            EnemyModels = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                PlayerModels[i] = Spawn(playerPrefabs[i], playerSlots[i], 0, i);
                EnemyModels[i] = Spawn(enemyPrefabs[i], enemySlots[i], 1, i);
            }
        }

        private static Transform Spawn(GameObject prefab, Transform slot, int team, int index)
        {
            if (prefab == null || slot == null)
                throw new InvalidOperationException("Missing character prefab or formation slot.");
            GameObject instance = Instantiate(prefab, slot.position, Quaternion.identity, slot);
            instance.name = (team == 0 ? "Player_" : "Enemy_") + index;
            instance.transform.localPosition = Vector3.zero;
            HeroView heroView = instance.GetComponent<HeroView>();
            if (heroView == null)
                throw new InvalidOperationException("Character prefab is missing HeroView: " + prefab.name);
            heroView.Bind(team, index);
            return instance.transform;
        }

        public void Begin(BattleSim sim)
        {
            simulation = sim;
            accumulator = 0f;
            eventCursor = 0;
        }

        public void SetSpeed(int speed)
        {
            Speed = (speed == 2 || speed == 4) ? speed : 1;
        }

        private void Update()
        {
            if (simulation == null) return;
            accumulator += Time.deltaTime * Speed;
            int guard = 0;
            while (accumulator >= BattleSim.Dt && !simulation.Finished && guard++ < 64)
            {
                accumulator -= BattleSim.Dt;
                simulation.Step();
            }

            while (eventCursor < simulation.Events.Count)
            {
                BattleEvent battleEvent = simulation.Events[eventCursor++];
                DispatchToHeroes(battleEvent);
                if (EventPlayed != null) EventPlayed(battleEvent);
            }
        }

        private void DispatchToHeroes(BattleEvent battleEvent)
        {
            if (PlayerModels != null)
                for (int i = 0; i < PlayerModels.Length; i++)
                {
                    HeroView view = PlayerModels[i] != null ? PlayerModels[i].GetComponent<HeroView>() : null;
                    if (view != null) view.HandleBattleEvent(battleEvent);
                }
            if (EnemyModels != null)
                for (int i = 0; i < EnemyModels.Length; i++)
                {
                    HeroView view = EnemyModels[i] != null ? EnemyModels[i].GetComponent<HeroView>() : null;
                    if (view != null) view.HandleBattleEvent(battleEvent);
                }
        }
    }
}
