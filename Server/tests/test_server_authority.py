import unittest

from app.main import calculate_idle_reward, utc_day


class ServerAuthorityTests(unittest.TestCase):
    def test_idle_reward_uses_elapsed_time_and_stage(self):
        reward = calculate_idle_reward(1_000, 4_600, 10)
        self.assertEqual(reward["elapsed_seconds"], 3_600)
        self.assertEqual(reward["gold"], 50)
        self.assertEqual(reward["exp"], 30)

    def test_idle_reward_caps_offline_time_at_twelve_hours(self):
        reward = calculate_idle_reward(1_000, 1_000 + 48 * 3_600, 0)
        self.assertEqual(reward["elapsed_seconds"], 12 * 3_600)
        self.assertEqual(reward["gold"], 240)
        self.assertEqual(reward["exp"], 120)

    def test_idle_reward_does_not_award_negative_elapsed_time(self):
        reward = calculate_idle_reward(2_000, 1_000, 0)
        self.assertEqual(reward["elapsed_seconds"], 0)
        self.assertEqual(reward["gold"], 0)
        self.assertEqual(reward["exp"], 0)

    def test_stage_rate_is_clamped_to_campaign_range(self):
        reward = calculate_idle_reward(1_000, 4_600, 10_000)
        self.assertEqual(reward["gold"], 620)
        self.assertEqual(reward["exp"], 410)

    def test_daily_key_is_utc_not_local_timezone(self):
        self.assertEqual(utc_day(0), "1970-01-01")


if __name__ == "__main__":
    unittest.main()
