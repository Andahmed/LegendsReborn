import os
import tempfile
import unittest
from unittest.mock import patch

_TEST_DIR = tempfile.TemporaryDirectory(prefix="sands-test-")
os.environ["SANDS_DB_PATH"] = os.path.join(_TEST_DIR.name, "test.sqlite3")

from app import main


class ServerFlowTests(unittest.TestCase):
    def test_guest_profile_and_audited_ten_pull(self):
        login = main.guest_login()
        authorization = "Bearer " + login.access_token
        account_id = main.require_account(authorization)

        self.assertEqual(5, len(login.profile["heroes"]))
        response = main.summon(main.SummonRequest(count=10), authorization)
        self.assertEqual(10, len(response["results"]))
        self.assertEqual(300, response["profile"]["gems"])

        history = main.summon_history(limit=20, authorization=authorization)
        self.assertEqual(10, len(history["items"]))
        self.assertTrue(all(item["hero_id"] for item in history["items"]))
        self.assertTrue(account_id)

    def test_gacha_pity_guarantees_epic_or_better(self):
        profile = {"pity_epic": 29, "pity_legendary": 12}
        with patch.object(main.secrets, "randbelow", return_value=100):
            self.assertIn(main.roll_rarity(profile), ("EPIC", "LEGENDARY"))

    def test_gacha_pity_guarantees_legendary(self):
        profile = {"pity_epic": 29, "pity_legendary": 89}
        with patch.object(main.secrets, "randbelow", return_value=0):
            self.assertEqual("LEGENDARY", main.roll_rarity(profile))

    def test_invalid_pull_count_is_rejected(self):
        login = main.guest_login()
        with self.assertRaises(Exception):
            main.summon(main.SummonRequest(count=2), "Bearer " + login.access_token)

    def test_unknown_token_is_rejected(self):
        with self.assertRaises(Exception):
            main.require_account("Bearer not-a-real-token")


if __name__ == "__main__":
    unittest.main()
