// =============================================================================
// FILE: Assets/Scripts/Presentation/PrototypeBattleController.cs
// OWNER ROLE: Client Programmer - Combat
// RESPONSIBILITY: Supply hero and stage enemy prefabs, five formation slots, environment and HUD data to BattleView.
// WORK ON: Replace Stage_001 prototype data with campaign stage selection and production battle HUD.
// STATUS: 3D prefab battle integration prototype
// =============================================================================
using SandsOfLegends.Combat;
using SandsOfLegends.Core;
using SandsOfLegends.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace SandsOfLegends.Presentation
{
    public sealed class PrototypeBattleController : MonoBehaviour
    {
        public HeroDefinition[] heroes = new HeroDefinition[4];
        public StageDefinition stageDefinition;
        public GameObject[] biomePrefabs = new GameObject[5];
        public Transform environmentRoot;
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

        private BattleSim simulation;

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

            SpawnStageEnvironment();
            var playerTeam = new UnitSpec[4];
            var enemyTeam = new UnitSpec[4];
            var playerPrefabs = new GameObject[4];
            var enemyPrefabs = new GameObject[4];

            for (int i = 0; i < 4; i++)
            {
                if (heroes[i] == null || heroes[i].prefab == null)
                    throw new System.InvalidOperationException("HeroDefinition or hero prefab is missing at slot " + i + ".");

                playerTeam[i] = heroes[i].ToSpec();
                playerPrefabs[i] = heroes[i].prefab;
                UnitSpec source = playerTeam[i];
                enemyTeam[i] = new UnitSpec
                {
                    Id = "enemy_" + source.Id,
                    Name = stageDefinition != null ? "Raider " + (i + 1) : "Echo " + source.Name,
                    Faction = source.Faction,
                    Role = source.Role,
                    Hp = source.Hp,
                    Atk = source.Atk,
                    Def = source.Def,
                    Spd = source.Spd,
                    CritChance = source.CritChance,
                    Skill = source.Skill,
                    Ultimate = source.Ultimate
                };

                enemyPrefabs[i] = stageDefinition != null &&
                    stageDefinition.enemyPrefabs != null &&
                    i < stageDefinition.enemyPrefabs.Length &&
                    stageDefinition.enemyPrefabs[i] != null
                    ? stageDefinition.enemyPrefabs[i]
                    : heroes[i].prefab;
            }

            battleView.ConfigurePresentation(playerPrefabs, enemyPrefabs, playerModels, enemyModels);
            playerModels = battleView.PlayerModels;
            enemyModels = battleView.EnemyModels;
            simulation = new BattleSim(playerTeam, enemyTeam, 20261111);
            battleView.Begin(simulation);

            if (speed1Button != null) speed1Button.onClick.AddListener(SetSpeed1);
            if (speed2Button != null) speed2Button.onClick.AddListener(SetSpeed2);
            if (speed4Button != null) speed4Button.onClick.AddListener(SetSpeed4);

            RefreshVisuals();
            SetStatus("Battle running - x1");
        }

        private void SpawnStageEnvironment()
        {
            if (environmentRoot == null) return;
            int stageNumber = stageDefinition != null ? Mathf.Max(1, stageDefinition.stageNumber) : 1;
            int biomeIndex = Mathf.Clamp(CampaignProgress.BiomeOf(stageNumber), 0, 4);
            GameObject environment = biomePrefabs != null && biomeIndex < biomePrefabs.Length
                ? biomePrefabs[biomeIndex]
                : null;
            if (environment == null && stageDefinition != null) environment = stageDefinition.environmentPrefab;
            if (environment != null) Instantiate(environment, environmentRoot.position, Quaternion.identity, environmentRoot);
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
            SetStatus(simulation != null && simulation.Finished ? "Battle finished" : "Battle running - x" + speed);
        }

        private void HandleBattleEvent(BattleEvent battleEvent)
        {
            RefreshVisuals();
            if (simulation != null && simulation.Finished)
                SetStatus(simulation.Winner == 0 ? "Victory" : "Defeat");
        }

        private void RefreshVisuals()
        {
            if (simulation == null) return;
            for (int i = 0; i < 4; i++)
            {
                RefreshSide(0, i, playerModels, playerHealth, playerLabels);
                RefreshSide(1, i, enemyModels, enemyHealth, enemyLabels);
            }
        }

        private void RefreshSide(int team, int slot, Transform[] models, Image[] health, Text[] labels)
        {
            BattleUnit unit = simulation.Get(team, slot);
            if (unit == null) return;
            if (models != null && slot < models.Length && models[slot] != null)
                models[slot].gameObject.SetActive(true);
            if (health != null && slot < health.Length && health[slot] != null)
                health[slot].fillAmount = unit.MaxHp > 0 ? (float)unit.Hp / unit.MaxHp : 0f;
            if (labels != null && slot < labels.Length && labels[slot] != null)
                labels[slot].text = unit.Alive
                    ? unit.Spec.Name + "  " + unit.Hp + "/" + unit.MaxHp
                    : unit.Spec.Name + "  defeated";
        }

        private void SetStatus(string value)
        {
            if (battleStatus != null) battleStatus.text = value;
        }
    }
}
