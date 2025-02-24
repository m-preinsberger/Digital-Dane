using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GameApi
{
    public class GameApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _key;
        private readonly ILogger<GameApiClient> _logger;

        public GameApiClient(string baseUrl, string key, ILogger<GameApiClient> logger)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _key = key;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        #region Spielverwaltung

        public async Task<GameStatusResponse> CreateGameAsync()
        {
            var url = $"{_baseUrl}/api/game/{_key}/create";
            return await SendRequestAsync<GameStatusResponse>(() => _httpClient.PostAsync(url, null), "CreateGameAsync", url);
        }

        public async Task<GameStatusResponse> GetGameStatusAsync()
        {
            var url = $"{_baseUrl}/api/game/{_key}/status";
            return await SendRequestAsync<GameStatusResponse>(() => _httpClient.GetAsync(url), "GetGameStatusAsync", url);
        }

        public async Task<GameStatusResponse> CloseGameAsync()
        {
            var url = $"{_baseUrl}/api/game/{_key}/close";
            return await SendRequestAsync<GameStatusResponse>(() => _httpClient.PostAsync(url, null), "CloseGameAsync", url);
        }

        #endregion

        #region Bewegungsmechaniken

        public async Task<MoveResponse> MoveAsync(int direction)
        {
            var url = $"{_baseUrl}/api/player/{_key}/move/{direction}";
            return await SendRequestAsync<MoveResponse>(() => _httpClient.PostAsync(url, null), "MoveAsync", url);
        }

        public async Task<DashResponse> DashAsync(int direction)
        {
            var url = $"{_baseUrl}/api/player/{_key}/dash/{direction}";
            return await SendRequestAsync<DashResponse>(() => _httpClient.PostAsync(url, null), "DashAsync", url);
        }

        public async Task<TeleportResponse> TeleportAsync(int x, int y)
        {
            var url = $"{_baseUrl}/api/player/{_key}/teleport/{x}/{y}";
            return await SendRequestAsync<TeleportResponse>(() => _httpClient.GetAsync(url), "TeleportAsync", url);
        }

        #endregion

        #region Angriffsmechaniken

        public async Task<HitResponse> HitAsync(int direction)
        {
            var url = $"{_baseUrl}/api/player/{_key}/hit/{direction}";
            return await SendRequestAsync<HitResponse>(() => _httpClient.PostAsync(url, null), "HitAsync", url);
        }

        public async Task<SpecialAttackResponse> SpecialAttackAsync()
        {
            var url = $"{_baseUrl}/api/player/{_key}/specialattack";
            return await SendRequestAsync<SpecialAttackResponse>(() => _httpClient.PostAsync(url, null), "SpecialAttackAsync", url);
        }

        public async Task<ShootResponse> ShootAsync(int direction)
        {
            var url = $"{_baseUrl}/api/player/{_key}/shoot/{direction}";
            return await SendRequestAsync<ShootResponse>(() => _httpClient.PostAsync(url, null), "ShootAsync", url);
        }

        #endregion

        #region Umgebungserkundung

        public async Task<RadarResponse> RadarAsync()
        {
            var url = $"{_baseUrl}/api/player/{_key}/radar";
            return await SendRequestAsync<RadarResponse>(() => _httpClient.GetAsync(url), "RadarAsync", url);
        }

        public async Task<PeekResponse> PeekAsync(int direction)
        {
            var url = $"{_baseUrl}/api/player/{_key}/peek/{direction}";
            return await SendRequestAsync<PeekResponse>(() => _httpClient.GetAsync(url), "PeekAsync", url);
        }

        public async Task<ScanResponse> ScanAsync()
        {
            var url = $"{_baseUrl}/api/player/{_key}/scan";
            return await SendRequestAsync<ScanResponse>(() => _httpClient.GetAsync(url), "ScanAsync", url);
        }

        #endregion

        /// <summary>
        /// Führt einen HTTP‑Request aus, loggt etwaige Fehler und versucht, die Antwort als JSON zu deserialisieren.
        /// Falls ein Fehler auftritt und das Antwortobjekt über "error" und "description" verfügt, werden diese Felder gesetzt.
        /// </summary>
        private async Task<T> SendRequestAsync<T>(Func<Task<HttpResponseMessage>> sendRequest, string methodName, string url) where T : new()
        {
            try
            {
                HttpResponseMessage response = await sendRequest();
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Method {MethodName} - Request to {Url} failed with status {StatusCode}: {ResponseJson}", methodName, url, response.StatusCode, json);
                }
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {MethodName} when calling {Url}", methodName, url);
                T errorResponse = new T();

                // Wenn möglich, setze "error" und "description" in der Antwort
                var errorProp = typeof(T).GetProperty("error");
                var descProp = typeof(T).GetProperty("description");
                if (errorProp != null && descProp != null)
                {
                    errorProp.SetValue(errorResponse, true);
                    descProp.SetValue(errorResponse, ex.Message);
                }
                return errorResponse;
            }
        }
    }

    #region Antwortklassen

    // Gemeinsame Spielstatus-Antwort (auch für Fehlerfälle)
    public class GameStatusResponse
    {
        public string gameid { get; set; }
        public bool running { get; set; }
        public string level { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    // Bewegung
    public class MoveResponse
    {
        public string action { get; set; }
        public bool move { get; set; }
        public bool executed { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    public class DashResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public int blocksDashed { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    public class TeleportResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public bool landedInWall { get; set; }
    }

    // Angriff
    public class HitResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public int hit { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    public class SpecialAttackResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public string description { get; set; }
    }

    public class ShootResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public bool hitSomeone { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    // Umgebungserkundung
    public class RadarResponse
    {
        public string action { get; set; }
        public RadarResults radarResults { get; set; }
    }

    public class RadarResults
    {
        public int north { get; set; }
        public int east { get; set; }
        public int south { get; set; }
        public int west { get; set; }
    }

    public class PeekResponse
    {
        public string action { get; set; }
        public bool executed { get; set; }
        public int playersInSight { get; set; }
        public int? sightedPlayerDistance { get; set; }
    }

    public class ScanResponse
    {
        public string action { get; set; }
        public DifferenceToNearestPlayer differenceToNearestPlayer { get; set; }
        public bool error { get; set; }
        public string description { get; set; }
    }

    public class DifferenceToNearestPlayer
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    #endregion
}
