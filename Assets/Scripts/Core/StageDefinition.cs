// =============================================================================
// FILE: Assets/Scripts/Core/StageDefinition.cs
// OWNER ROLE: Systems Designer
// RESPONSIBILITY: Stage data references enemy character prefabs and the stage biome environment prefab.
// WORK ON: Populate the full campaign stage catalogue and stage-specific enemy formations.
// STATUS: Prototype stage data
// =============================================================================
using UnityEngine;

namespace SandsOfLegends.Core
{
    [CreateAssetMenu(menuName = "SandsOfLegends/Stage", fileName = "Stage_")]
    public sealed class StageDefinition : ScriptableObject
    {
        public int stageNumber = 1;
        public GameObject[] enemyPrefabs = new GameObject[4];
        public GameObject environmentPrefab;
    }
}
