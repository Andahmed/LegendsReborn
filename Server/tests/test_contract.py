import ast
import re
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


def python_assignment(source, name):
    module = ast.parse(source)
    for node in module.body:
        if isinstance(node, ast.Assign):
            if any(isinstance(target, ast.Name) and target.id == name for target in node.targets):
                return ast.literal_eval(node.value)
    raise AssertionError("Python assignment not found: " + name)


def csharp_int(source, name):
    match = re.search(r"public\s+const\s+int\s+" + re.escape(name) + r"\s*=\s*(\d+)\s*;", source)
    if not match:
        raise AssertionError("C# constant not found: " + name)
    return int(match.group(1))


class CrossLanguageSummonContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.hero_catalog = (ROOT / "Assets/Scripts/Core/HeroCatalog.cs").read_text(encoding="utf-8")
        cls.gacha = (ROOT / "Assets/Scripts/Meta/Gacha/GachaSystem.cs").read_text(encoding="utf-8")
        cls.progression = (ROOT / "Assets/Scripts/Meta/Heroes/HeroInstance.cs").read_text(encoding="utf-8")
        cls.server = (ROOT / "Server/app/main.py").read_text(encoding="utf-8")

    def test_hero_ids_and_rarities_match_server_pools(self):
        records = re.findall(
            r'new\s+HeroCardData\s*\{\s*Id="([^"]+)",.*?Rarity=Rarity\.(Common|Rare|Epic|Legendary)',
            self.hero_catalog,
            re.DOTALL,
        )
        self.assertEqual(20, len(records), "Expected 20 catalog hero records")

        expected = {"COMMON": [], "RARE": [], "EPIC": [], "LEGENDARY": []}
        rarity_names = {
            "Common": "COMMON",
            "Rare": "RARE",
            "Epic": "EPIC",
            "Legendary": "LEGENDARY",
        }
        for hero_id, rarity in records:
            expected[rarity_names[rarity]].append(hero_id)

        server_pools = python_assignment(self.server, "HERO_POOLS")
        self.assertEqual(expected, server_pools)

    def test_starters_match_between_client_and_server(self):
        match = re.search(
            r"StartingHeroes\s*\{\s*get\s*\{\s*return\s+new\[\]\s*\{([^}]+)\}",
            self.hero_catalog,
            re.DOTALL,
        )
        self.assertIsNotNone(match, "HeroCatalog.StartingHeroes not found")
        client_starters = re.findall(r'"([^"]+)"', match.group(1))
        self.assertEqual(client_starters, python_assignment(self.server, "STARTERS"))

    def test_rates_pity_costs_and_duplicate_shards_match(self):
        client_rates = {
            "COMMON": csharp_int(self.gacha, "CommonRate"),
            "RARE": csharp_int(self.gacha, "RareRate"),
            "EPIC": csharp_int(self.gacha, "EpicRate"),
            "LEGENDARY": csharp_int(self.gacha, "LegendaryRate"),
        }
        server_rates = python_assignment(self.server, "RARITY_RATES")
        self.assertEqual(client_rates, server_rates)
        self.assertEqual(100, sum(server_rates.values()))
        self.assertEqual(
            [(rarity, rate * 100) for rarity, rate in server_rates.items()],
            python_assignment(self.server, "RARITY_WEIGHTS"),
        )

        self.assertEqual(csharp_int(self.gacha, "EpicPity"), python_assignment(self.server, "EPIC_PITY"))
        self.assertEqual(csharp_int(self.gacha, "LegendaryPity"), python_assignment(self.server, "LEGENDARY_PITY"))
        self.assertEqual(csharp_int(self.gacha, "GemCostSingle"), python_assignment(self.server, "SUMMON_COST_SINGLE"))
        self.assertEqual(csharp_int(self.gacha, "GemCostTen"), python_assignment(self.server, "SUMMON_COST_TEN"))

        shard_cases = re.findall(
            r"case\s+Rarity\.(Common|Rare|Epic|Legendary):\s*return\s+(\d+)\s*;",
            self.progression,
        )
        client_shards = {
            {"Common": "COMMON", "Rare": "RARE", "Epic": "EPIC", "Legendary": "LEGENDARY"}[rarity]: int(value)
            for rarity, value in shard_cases
        }
        # Progression.DuplicateShards uses the default branch for Legendary.
        default_shards = re.search(r"default:\s*return\s*(\d+)\s*;", self.progression)
        self.assertIsNotNone(default_shards, "Legendary duplicate shard default not found")
        client_shards["LEGENDARY"] = int(default_shards.group(1))
        self.assertEqual(
            client_shards,
            python_assignment(self.server, "DUPLICATE_SHARDS"),
        )


if __name__ == "__main__":
    unittest.main()
