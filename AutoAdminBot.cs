using PRoCon.Core;
using PRoCon.Core.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

public class AutoAdminBot : PRoConPluginAPI
{
    private List<string> badTags = new List<string> { "hack", "cheat", "aimbot" };
    private List<string> badWords = new List<string> { "fdp", "merda", "lixo", "noob" };
    private int maxPing = 150;
    private Timer messageTimer;

    public override void OnPluginLoaded(string hostName, string port, string proconVersion)
    {
        this.ExecuteCommand("procon.protected.pluginconsole.write", "AutoAdminBot carregado.");
    }

    public override void OnPluginEnable()
    {
        this.ExecuteCommand("procon.protected.pluginconsole.write", "AutoAdminBot ativado.");

        messageTimer = new Timer(300000); // 5 minutos
        messageTimer.Elapsed += SendPeriodicMessage;
        messageTimer.Start();
    }

    public override void OnPluginDisable()
    {
        messageTimer?.Stop();
        this.ExecuteCommand("procon.protected.pluginconsole.write", "AutoAdminBot desativado.");
    }

    public override void OnPlayerJoin(string playerName)
    {
        this.ExecuteCommand("admin.say", $"Bem-vindo ao servidor, {playerName}! Jogue limpo!", "player", playerName);

        foreach (var tag in badTags)
        {
            if (playerName.ToLower().Contains(tag))
            {
                this.ExecuteCommand("admin.kickPlayer", playerName, $"Nome inválido detectado: {tag}");
                this.ExecuteCommand("admin.say", $"{playerName} foi expulso por nome impróprio.", "all");
                LogAction($"{playerName} kickado por nome com tag proibida: {tag}");
                break;
            }
        }
    }

    public override void OnPlayerPing(string playerName, int ping)
    {
        if (ping > maxPing)
        {
            this.ExecuteCommand("admin.kickPlayer", playerName, $"Ping acima do limite ({ping}ms)");
            this.ExecuteCommand("admin.say", $"{playerName} foi expulso por ping alto.", "all");
            LogAction($"{playerName} kickado por ping alto: {ping}ms");
        }
    }

    public override void OnGlobalChat(string speaker, string message)
    {
        string lowerMessage = message.ToLower();

        // Filtro de palavrões
        foreach (var word in badWords)
        {
            if (lowerMessage.Contains(word))
            {
                this.ExecuteCommand("admin.say", $"{speaker} foi banido por linguagem inadequada.", "all");
                this.ExecuteCommand("banList.add", speaker, "perm", "AutoAdminBot", $"Palavra proibida detectada: {word}");
                LogAction($"{speaker} banido por palavra proibida: {word}");
                return;
            }
        }

        // Comando !help
        if (message.StartsWith("!help"))
        {
            this.ExecuteCommand("admin.say", $"{speaker}, este servidor é monitorado pelo AutoAdminBot. Respeite as regras!", "player", speaker);
        }
    }

    private void SendPeriodicMessage(object sender, ElapsedEventArgs e)
    {
        this.ExecuteCommand("admin.say", "Respeite os jogadores e jogue limpo. Admin Bot ativo!", "all");
    }

    private void LogAction(string action)
    {
        string logDir = "logs";
        string logPath = Path.Combine(logDir, "AutoAdminLog.txt");
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";

        try
        {
            Directory.CreateDirectory(logDir);
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", $"Erro ao gravar log: {ex.Message}");
        }
    }
}