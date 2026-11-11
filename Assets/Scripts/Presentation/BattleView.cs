// =============================================================================
// FILE: Assets/Scripts/Presentation/BattleView.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Plays a BattleSim in real time: steps the sim at x1/x2/x4 and publishes each BattleEvent once for animations/VFX/HUD.
// WORK ON: Subscribe HeroView/DamageNumbers/HP bars to EventPlayed; manual ultimate button; skip/auto buttons.
// STATUS: Implemented
// =============================================================================
using System;
using UnityEngine;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Presentation
{
    public sealed class BattleView : MonoBehaviour
    {
        public event Action<BattleEvent> EventPlayed;
        public int Speed { get; private set; }

        private BattleSim _sim;
        private float _acc;
        private int _cursor;

        private void Awake() { Speed = 1; }

        public void Begin(BattleSim sim) { _sim = sim; _acc = 0f; _cursor = 0; }
        public void SetSpeed(int speed) { Speed = (speed == 2 || speed == 4) ? speed : 1; }

        private void Update()
        {
            if (_sim == null) return;
            _acc += Time.deltaTime * Speed;
            int guard = 0;
            while (_acc >= BattleSim.Dt && !_sim.Finished && guard++ < 64)
            {
                _acc -= BattleSim.Dt;
                _sim.Step();
            }
            while (_cursor < _sim.Events.Count)
                if (EventPlayed != null) EventPlayed(_sim.Events[_cursor++]); else _cursor++;
        }
    }
}
