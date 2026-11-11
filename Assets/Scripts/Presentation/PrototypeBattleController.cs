// =============================================================================
// FILE: Assets/Scripts/Presentation/PrototypeBattleController.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Runtime wiring for the 4v4 prototype battle, hero prefabs, HP bars, and x1/x2/x4 controls.
// WORK ON: Expand the presentation to five authored slot transforms per team and bind stage-specific enemies/environments.
// STATUS: Prototype implementation
// =============================================================================
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.Presentation
{
    public sealed class PrototypeBattleController : MonoBehaviour
    {
        public HeroDefinition[] heroes = new HeroDefinition[4];
        public BattleView battleView;
        public Transform[] playerModels = new Transform[5];
        public Transform[] enemyModels = new Transform[5];
        public Image[] playerHealth = new Image[4];
        public Image[] enemyHealth = new Image[4];
        public Text[] playerLabels = new Text[4];
        public Text[] enemyLabels = new Text[4];
        public Text battleStatus;
        public Button speed1Button;
        public Button speed2Button;
        public Button speed4Button;

        private BattleSim _simulation;
        private readonly HeroView[] _playerViews = new HeroView[4];
        private readonly HeroView[] _enemyViews = new HeroView[4];

        private void OnEnable()
        {
            if (battleView != null) battleView.EventPlayed += HandleBattleEvent;
        }

        private void Start()
        {
            if (heroes == null || heroes.Length < 4)
                throw new System.InvalidOperationException("Prototype battle needs four HeroDefinition assets.");
            if (battleView == null)
                throw new System.InvalidOperationException("Prototype battle needs a BattleView.");

            var playerTeam = new UnitSpec[4];
            var enemyTeam = new UnitSpec[4];
            for (int i = 0; i < 4; i++)
            {
                if (heroes[i] == null)
                    throw new System.InvalidOperationException("A prototype HeroDefinition reference is missing at slot " + i + ".");
                if (heroes[i].prefab == null)
                    throw new System.InvalidOperationException("Hero prefab reference is missing for " + heroes[i].id + ".");

                playerTeam[i] = heroes[i].ToSpec();
                UnitSpec source = playerTeam[i];
                enemyTeam[i] = new UnitSpec
                {
                    Id = "enemy_" + source.Id, Name = "Echo " + source.Name,
                    Faction = source.Faction, Role = source.Role, Hp = source.Hp,
                    Atk = source.Atk, Def = source.Def, Spd = source.Spd,
                    CritChance = source.CritChance, Skill = source.Skill, Ultimate = source.Ultimate
                };

                _playerViews[i] = SpawnHero(heroes[i].prefab, playerModels, i, 0);
                _enemyViews[i] = SpawnHero(heroes[i].prefab, enemyModels, i, 1);
            }

            if (playerModels != null && playerModels.Length > 4 && playerModels[4] != null) playerModels[4].gameObject.SetActive(false);
            if (enemyModels != null && enemyModels.Length > 4 && enemyModels[4] != null) enemyModels[4].gameObject.SetActive(false);

            _simulation = new BattleSim(playerTeam, enemyTeam, 20261111);
            battleView.Begin(_simulation);

            if (speed1Button != null) speed1Button.onClick.AddListener(SetSpeed1);
            if (speed2Button != null) speed2Button.onClick.AddListener(SetSpeed2);
            if (speed4Button != null) speed4Button.onClick.AddListener(SetSpeed4);

            RefreshVisuals();
            SetStatus("Battle running - x1");
        }

        private HeroView SpawnHero(GameObject prefab, Transform[] slots, int slot, int team)
        {
            if (slots == null || slot >= slots.Length || slots[slot] == null)
                throw new System.InvalidOperationException("Missing battle slot transform for team " + team + ", slot " + slot + ".");
            Transform slotTransform = slots[slot];
            Vector3 position = slotTransform.position;
            Transform parent = slotTransform.parent;
            slotTransform.gameObject.SetActive(false);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, parent);
            instance.name = (team == 0 ? "Player_" : "Enemy_") + slot;
            HeroView view = instance.GetComponent<HeroView>();
            if (view == null) throw new System.InvalidOperationException("Hero prefab is missing HeroView.");
            view.Bind(team, slot);
            slots[slot] = instance.transform;
            return view;
        }

        private void OnDisable()
        {
            if (battleView != null) battleView.EventPlayed -= HandleBattleEvent;
            if (speed1Button != null) speed1Button.onClick.RemoveListener(SetSpeed1);
            if (speed2Button != null) speed2Button.onClick.RemoveListener(SetSpeed2);
            if (speed4Button != null) speed4Button.onClick.RemoveListener(SetSpeed4);
        }

        public void SetSpeed1() { SetSpeed(1); }
        public void SetSpeed2() { SetSpeed(2); }
        public void SetSpeed4() { SetSpeed(4); }

        private void SetSpeed(int speed)
        {
            if (battleView == null) return;
            battleView.SetSpeed(speed);
            SetStatus(_simulation != null && _simulation.Finished ? "Battle finished" : "Battle running - x" + speed);
        }

        private void HandleBattleEvent(BattleEvent battleEvent)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_playerViews[i] != null) _playerViews[i].HandleBattleEvent(battleEvent);
                if (_enemyViews[i] != null) _enemyViews[i].HandleBattleEvent(battleEvent);
            }
            RefreshVisuals();
            if (_simulation != null && _simulation.Finished)
                SetStatus(_simulation.Winner == 0 ? "Victory" : "Defeat");
        }

        private void RefreshVisuals()
        {
            if (_simulation == null) return;
            for (int i = 0; i < 4; i++)
            {
                RefreshSide(0, i, playerModels, playerHealth, playerLabels);
                RefreshSide(1, i, enemyModels, enemyHealth, enemyLabels);
            }
        }

        private void RefreshSide(int team, int slot, Transform[] models, Image[] health, Text[] labels)
        {
            BattleUnit unit = _simulation.Get(team, slot);
            if (unit == null) return;
            if (models != null && slot < models.Length && models[slot] != null)
                models[slot].gameObject.SetActive(true);
            if (health != null && slot < health.Length && health[slot] != null)
                health[slot].fillAmount = unit.MaxHp > 0 ? (float)unit.Hp / unit.MaxHp : 0f;
            if (labels != null && slot < labels.Length && labels[slot] != null)
                labels[slot].text = unit.Alive ? unit.Spec.Name + "  " + unit.Hp + "/" + unit.MaxHp : unit.Spec.Name + "  defeated";
        }

        private void SetStatus(string value)
        {
            if (battleStatus != null) battleStatus.text = value;
        }
    }
}
