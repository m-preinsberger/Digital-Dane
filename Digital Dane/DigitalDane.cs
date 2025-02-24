using GameApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Dane
{
    internal class DigitalDane
    {
        private readonly GameApiClient _api;
        private readonly ILogger<DigitalDane> _logger;
        // Aktuelle Richtung für Patrouillenbewegung (0 = Norden, 1 = Osten, 2 = Süden, 3 = Westen)
        private int _patrolDirection = 0;

        public DigitalDane(GameApiClient api, ILogger<DigitalDane> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task RunBotAsync()
        {
            _logger.LogInformation("Bot startet...");
            while (true)
            {
                // Abbruch, falls das Spiel nicht mehr läuft
                var status = await _api.GetGameStatusAsync();
                if (status == null || !status.running)
                {
                    _logger.LogInformation("Spiel beendet oder Status nicht verfügbar.");
                    break;
                }

                bool enemyFound = false;

                // Prüfe alle Richtungen (0 bis 3) mit 'peek'
                for (int direction = 0; direction < 4; direction++)
                {
                    var peekResult = await _api.PeekAsync(direction);
                    if (peekResult != null && peekResult.executed && peekResult.playersInSight > 0)
                    {
                        enemyFound = true;
                        _logger.LogInformation("Gegner via Peek in Richtung {Direction} entdeckt.", direction);

                        // Führe Scan aus, um die Position genauer zu ermitteln
                        var scanResult = await _api.ScanAsync();
                        if (scanResult == null || scanResult.error || scanResult.differenceToNearestPlayer == null)
                        {
                            // Scan hat keinen Gegner bestätigt – bewege Dich in die vermutete Richtung
                            _logger.LogInformation("Scan bestätigt keinen Gegner. Bewege in Richtung {Direction}.", direction);
                            var moveResult = await _api.MoveAsync(direction);
                            if (moveResult == null || !moveResult.move)
                            {
                                _logger.LogWarning("Bewegung in Richtung {Direction} schlug fehl.", direction);
                            }
                        }
                        else
                        {
                            // Gegner bestätigt – führe einen Nahkampfangriff aus
                            _logger.LogInformation("Gegner bestätigt. Führe Schlag in Richtung {Direction} aus.", direction);
                            var hitResult = await _api.HitAsync(direction);
                            if (hitResult == null || hitResult.hit == 0)
                            {
                                _logger.LogWarning("Angriff in Richtung {Direction} hatte keinen Treffer.", direction);
                            }
                        }
                        // Ein Peek pro Schleifendurchlauf genügt, wenn ein Gegner verarbeitet wurde
                        break;
                    }
                }

                if (!enemyFound)
                {
                    // Falls kein Gegner via Peek gefunden wurde, führe Patrouillenbewegung aus
                    _logger.LogInformation("Kein Gegner in Sicht. Patrouilliere in Richtung {PatrolDirection}.", _patrolDirection);
                    var moveResult = await _api.MoveAsync(_patrolDirection);
                    if (moveResult == null || !moveResult.move)
                    {
                        _logger.LogWarning("Bewegung in Richtung {PatrolDirection} schlug fehl. Wechsle Richtung.", _patrolDirection);
                        // Falls Bewegung nicht möglich, ändere die Patrouillenrichtung (Rotation im Uhrzeigersinn)
                        _patrolDirection = (_patrolDirection + 1) % 4;
                        await _api.MoveAsync(_patrolDirection);
                    }
                }

                // Kurze Pause, um Cooldowns zu berücksichtigen und API-Spamming zu vermeiden
                await Task.Delay(500);
            }
            _logger.LogInformation("Bot beendet.");
        }
    }
}
