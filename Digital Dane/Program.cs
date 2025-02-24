using GameApi;
using Microsoft.Extensions.Logging;

namespace Digital_Dane
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Logging-Setup: Erzeugt einen Logger, der Konsolenausgaben erzeugt.
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            // API-Key und Basis-URL festlegen.
            string apiKey = "b681e1c7-aa00-47d1-b37c-229b6ac34a97";
            string baseUrl = "https://game-dd.countit.at";

            logger.LogInformation("Initialisiere GameApiClient mit API-Key {ApiKey}", apiKey);
            var gameApiClient = new GameApiClient(baseUrl, apiKey, loggerFactory.CreateLogger<GameApiClient>());

            // Spiel erstellen
            logger.LogInformation("Erstelle Spiel...");
            var createGameResult = await gameApiClient.CreateGameAsync();
            if (createGameResult == null || createGameResult.error)
            {
                logger.LogError("Spielerstellung fehlgeschlagen: {Description}", createGameResult?.description ?? "Unbekannter Fehler");

                // Falls bereits ein Spiel läuft, versuche den aktuellen Status abzurufen.
                var status = await gameApiClient.GetGameStatusAsync();
                if (status != null && status.running)
                {
                    logger.LogInformation("Ein laufendes Spiel wurde gefunden: {GameId}", status.gameid);
                }
                else
                {
                    logger.LogError("Kein gültiges Spiel gefunden. Beende Programm.");
                    return;
                }
            }
            else
            {
                logger.LogInformation("Spiel erfolgreich erstellt: {GameId}", createGameResult.gameid);
            }

            // Überprüfe den Spielstatus
            var gameStatus = await gameApiClient.GetGameStatusAsync();
            if (gameStatus == null || !gameStatus.running)
            {
                logger.LogError("Spielstatus ungültig oder Spiel nicht laufend, beende Programm.");
                return;
            }

            // Starte die Bot-Logik, solange das Spiel läuft.
            logger.LogInformation("Spiel läuft, starte Bot-Logik...");
            var botLogger = loggerFactory.CreateLogger<DigitalDane>();
            var bot = new DigitalDane(gameApiClient, botLogger);
            await bot.RunBotAsync();

            // Nachdem die Bot-Logik beendet wurde, schließe das Spiel.
            logger.LogInformation("Beende Spiel...");
            var closeResult = await gameApiClient.CloseGameAsync();
            if (closeResult == null || closeResult.error)
            {
                logger.LogError("Spiel konnte nicht geschlossen werden: {Description}", closeResult?.description ?? "Unbekannter Fehler");
            }
            else
            {
                logger.LogInformation("Spiel erfolgreich geschlossen: {GameId}", closeResult.gameid);
            }
        }
    }
}
