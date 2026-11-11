import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from fastapi.testclient import TestClient

_TEST_ROOT = tempfile.TemporaryDirectory(prefix="sands-http-tests-")
os.environ.setdefault("SANDS_DB_PATH", str(Path(_TEST_ROOT.name) / "bootstrap.sqlite3"))

from app import main


class HttpApiFlowTests(unittest.TestCase):
    def setUp(self):
        self._db_dir = tempfile.TemporaryDirectory(prefix="sands-http-case-")
        self._db_path_patch = patch.object(
            main, "DB_PATH", Path(self._db_dir.name) / "api.sqlite3"
        )
        self._db_path_patch.start()
        main.initialize_database()
        self.client = TestClient(main.app)

    def tearDown(self):
        self.client.close()
        self._db_path_patch.stop()
        self._db_dir.cleanup()

    def _guest(self):
        response = self.client.post("/auth/guest")
        self.assertEqual(200, response.status_code, response.text)
        return response.json()

    def test_http_guest_profile_single_and_ten_summons_history_and_401(self):
        guest = self._guest()
        token = guest["access_token"]
        headers = {"Authorization": "Bearer " + token}

        self.assertEqual("bearer", guest["token_type"])
        self.assertEqual(3000, guest["profile"]["gems"])
        self.assertEqual(
            ["warrior", "paladin", "berserker", "healer", "sharpshooter"],
            guest["profile"]["formation"],
        )

        profile = self.client.get("/profile", headers=headers)
        self.assertEqual(200, profile.status_code, profile.text)
        self.assertEqual(guest["account_id"], profile.json()["account_id"])

        single = self.client.post("/summon", json={"count": 1}, headers=headers)
        self.assertEqual(200, single.status_code, single.text)
        self.assertEqual(1, len(single.json()["results"]))
        self.assertEqual(2700, single.json()["profile"]["gems"])
        self.assertEqual(
            {"COMMON": 40, "RARE": 35, "EPIC": 20, "LEGENDARY": 5},
            single.json()["rates"],
        )

        ten = self.client.post("/summon", json={"count": 10}, headers=headers)
        self.assertEqual(200, ten.status_code, ten.text)
        self.assertEqual(10, len(ten.json()["results"]))
        self.assertEqual(0, ten.json()["profile"]["gems"])

        history = self.client.get("/summon/history", headers=headers)
        self.assertEqual(200, history.status_code, history.text)
        self.assertEqual(11, len(history.json()["items"]))

        unauthorized = self.client.get("/profile")
        self.assertEqual(401, unauthorized.status_code)

    def test_http_summon_returns_409_for_short_balance_and_400_for_invalid_count(self):
        guest = self._guest()
        headers = {"Authorization": "Bearer " + guest["access_token"]}

        ten = self.client.post("/summon", json={"count": 10}, headers=headers)
        self.assertEqual(200, ten.status_code, ten.text)
        self.assertEqual(300, ten.json()["profile"]["gems"])

        insufficient = self.client.post("/summon", json={"count": 10}, headers=headers)
        self.assertEqual(409, insufficient.status_code)

        invalid = self.client.post("/summon", json={"count": 2}, headers=headers)
        self.assertEqual(400, invalid.status_code)


if __name__ == "__main__":
    unittest.main()
