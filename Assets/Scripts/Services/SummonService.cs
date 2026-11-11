// =============================================================================
// FILE: Assets/Scripts/Services/SummonService.cs
// OWNER ROLE: Client Programmer - Networking
// RESPONSIBILITY: Server-authoritative guest auth and summon requests.
// WORK ON: Replace the development URL through inspector/configuration before deployment.
// STATUS: Implemented (not Unity-verified)
// =============================================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SandsOfLegends.Meta;

namespace SandsOfLegends.Services
{
    /// <summary>Small coroutine client for the live summon contract. The bearer token is memory-only.</summary>
    public sealed class SummonService : MonoBehaviour
    {
        [Serializable] private sealed class GuestRequest { }
        [Serializable] private sealed class SummonRequest { public int count; }
        [Serializable] private sealed class GuestResponse { public string access_token; }
        [Serializable] private sealed class SummonEnvelope
        {
            public SummonResultDto[] results;
            public ProfileStateDto profile_state;
            public PityDto pity;
        }
        [Serializable] private sealed class SummonResultDto
        {
            public string hero_id;
            public string rarity;
            public bool is_new;
            public int shards_gained;
        }
        [Serializable] private sealed class PityDto { public int epic; public int legendary; }
        [Serializable] private sealed class ProfileStateDto
        {
            public long gems;
            public long gold;
            public long exp;
            public long gear_materials;
            public HeroStateDto[] heroes;
        }
        [Serializable] private sealed class HeroStateDto
        {
            public string hero_id;
            public int level;
            public int stars;
            public int shards;
        }

        private string serverBaseUrl = "http://localhost:8000";
        private string accessToken;

        public void Configure(string baseUrl)
        {
            if (!string.IsNullOrEmpty(baseUrl)) serverBaseUrl = baseUrl.TrimEnd('/');
        }

        public void Summon(int count, Action<SummonResponse> onSuccess, Action<string> onFailure)
        {
            StartCoroutine(SummonRoutine(count, onSuccess, onFailure));
        }

        private IEnumerator SummonRoutine(int count, Action<SummonResponse> onSuccess, Action<string> onFailure)
        {
            if (count != 1 && count != 10)
            {
                onFailure("Summon count must be 1 or 10.");
                yield break;
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                yield return PostJson("/auth/guest", JsonUtility.ToJson(new GuestRequest()), null,
                    text => { },
                    error => onFailure(error));
                if (string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(_lastToken)) accessToken = _lastToken;
                _lastToken = null;
                if (string.IsNullOrEmpty(accessToken)) yield break;
            }

            string responseText = null;
            string failure = null;
            yield return PostJson("/summon", JsonUtility.ToJson(new SummonRequest { count = count }), accessToken,
                text => responseText = text, error => failure = error);
            if (!string.IsNullOrEmpty(failure)) { onFailure(failure); yield break; }

            SummonEnvelope envelope;
            try { envelope = JsonUtility.FromJson<SummonEnvelope>(responseText); }
            catch (Exception e) { onFailure("Invalid summon response: " + e.Message); yield break; }
            if (envelope == null || envelope.results == null || envelope.profile_state == null)
            {
                onFailure("Server returned an incomplete summon response.");
                yield break;
            }
            onSuccess(new SummonResponse(envelope));
        }

        // Temporary handoff because the coroutine callback runs before the iterator resumes.
        private string _lastToken;

        private IEnumerator PostJson(string path, string json, string bearer,
            Action<string> onSuccess, Action<string> onFailure)
        {
            using (UnityWebRequest request = new UnityWebRequest(serverBaseUrl + path, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(bearer)) request.SetRequestHeader("Authorization", "Bearer " + bearer);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onFailure(request.error + " (HTTP " + request.responseCode + ")");
                    yield break;
                }
                if (path == "/auth/guest")
                {
                    try { _lastToken = JsonUtility.FromJson<GuestResponse>(request.downloadHandler.text).access_token; }
                    catch (Exception e) { onFailure("Invalid guest auth response: " + e.Message); yield break; }
                }
                onSuccess(request.downloadHandler.text);
            }
        }

        public sealed class SummonResponse
        {
            public readonly PullResult[] Results;
            public readonly long Gems;
            public readonly long Gold;
            public readonly long Exp;
            public readonly long GearMaterials;
            public readonly int SinceEpic;
            public readonly int SinceLegendary;
            public readonly HeroInstance[] Heroes;

            private SummonResponse(SummonEnvelope envelope)
            {
                Results = new PullResult[envelope.results.Length];
                for (int i = 0; i < Results.Length; i++)
                {
                    SummonResultDto source = envelope.results[i];
                    Results[i] = new PullResult {
                        HeroId = source.hero_id,
                        Rarity = ParseRarity(source.rarity),
                        IsNew = source.is_new,
                        ShardsGained = source.shards_gained
                    };
                }
                ProfileStateDto profile = envelope.profile_state;
                Gems = profile.gems; Gold = profile.gold; Exp = profile.exp; GearMaterials = profile.gear_materials;
                SinceEpic = envelope.pity.epic; SinceLegendary = envelope.pity.legendary;
                Heroes = new HeroInstance[profile.heroes != null ? profile.heroes.Length : 0];
                for (int i = 0; i < Heroes.Length; i++)
                    Heroes[i] = new HeroInstance { HeroId = profile.heroes[i].hero_id, Level = profile.heroes[i].level,
                        Stars = profile.heroes[i].stars, Shards = profile.heroes[i].shards };
            }

            private static SandsOfLegends.Combat.Rarity ParseRarity(string value)
            {
                try { return (SandsOfLegends.Combat.Rarity)Enum.Parse(typeof(SandsOfLegends.Combat.Rarity), value, true); }
                catch { return SandsOfLegends.Combat.Rarity.Common; }
            }
        }
    }
}
