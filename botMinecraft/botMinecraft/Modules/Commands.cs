using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using System.Net;
using CoreRCON;
using System.Text.RegularExpressions;

namespace Program.Modules
{
  /// <summary>
  /// Les commandes préfix arg Pos (!)
  /// </summary>
  public class Commands : ModuleBase<SocketCommandContext>
  {
    public static Process? minecraftProcess;

    private IPAddress ip;
    private ushort port;
    private string? password;

    /// <summary>
    /// Constructeur du Rcon
    /// </summary>
    public Commands()
    {
      this.ip = IPAddress.Parse("127.0.0.1");   // Adresse IP du server mc
      this.port = 25575; // Port du Rcon
      this.password = Environment.GetEnvironmentVariable("RCON_PASSWORD", EnvironmentVariableTarget.User);   // Mdp du Rcon      
    }

    /// <summary>
    /// Ouvre le serveur
    /// </summary>
    /// <returns></returns>
    [Command("start")]
    public async Task RunServMC()
    {
      try
      {
        if (minecraftProcess != null && !minecraftProcess.HasExited)
        {
          await ReplyAsync("Le serveur est déjà en cours d'éxecution!");
          return;
        }

        minecraftProcess = Process.Start(new ProcessStartInfo()
        {
          FileName = "run.bat",
          UseShellExecute = true,
          WorkingDirectory = @"C:\Users\Utilisateur\Desktop\Serveur Minecraft"
        });

        await ReplyAsync("Le serveur est en lancement...");
        await Task.Delay(25000);
        await ReplyAsync("Le serveur est lancé!");
      }

      catch (Exception ex)
      {
        Console.WriteLine("Erreur : " + ex.Message);
        await ReplyAsync($"Erreur lors du lancement du serveur : {ex.Message}!");
      }
    }

    /// <summary>
    /// Ferme le serveur
    /// </summary>
    /// <returns></returns>
    [Command("stop")]
    public async Task StopServMC()
    {
      try
      {
        using var rcon = new RCON(this.ip, this.port, this.password);

        await rcon.ConnectAsync();
        Console.WriteLine("Connection réussie!");
        await ReplyAsync("Fermeture en cours...");

        string? response = await rcon.SendCommandAsync("list");
        var match = Regex.Match(response, @"There are (\d+) of a max");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int listePlayer))
        {
          if (listePlayer == 0)
          {
            await rcon.SendCommandAsync("stop");
            await ReplyAsync("Fermeture réussi");           
          }
          else if (rcon.Connected && listePlayer == 0)
          {           
              await ReplyAsync("Serveur toujours actif, arrêt forcé...");
              this.KillServerMC();
          }
          else
          {
            await ReplyAsync("Des joueurs sont connectés, arrêt du serveur annulé!");
          }
        }       
      }

      catch (Exception ex)
      {
        await ReplyAsync("Erreur de la connexion au RCON: " + ex.Message);
        await ReplyAsync($"Erreur lors de la fermeture du serveur : {ex.Message}!");
      }
    }

    //Kill le serveur dans le cas ou il ne s'est pas arrêter correctement
    private async void KillServerMC()
    {
      Process[] processes = Process.GetProcessesByName("java");
      foreach (Process process in processes)
      {
        await ReplyAsync("Le serveur est en cours de fermeture...");
        process.Kill();

        await Task.Delay(3000);
        await ReplyAsync("Le serveur est fermé!");
      }
    }

    /// <summary>
    /// Affiche l'état du serveur
    /// </summary>
    /// <returns></returns>
    [Command("status")]
    public async Task StatusServMC()
    {    
      bool estLigne = Process.GetProcessesByName("java").Length > 0;
      await ReplyAsync(estLigne == false ? "Le serveur est hors ligne!" : "Le serveur est en ligne!");
    }

    /// <summary>
    /// Affiche la liste des joueurs qui joue sur le serveur mincraft
    /// </summary>
    /// <returns></returns>
    [Command("list")]
    public async Task ListePlayersAsync()
    {
      //Création du rcon
      using var rcon = new RCON(this.ip, this.port, this.password);     

      //Test de connection au rcon
      try
      {
        await rcon.ConnectAsync();
        string? reponse = await rcon.SendCommandAsync("list");
        await ReplyAsync($"{reponse}");
      }

      //ERREUR
      catch (Exception ex)
      {
        await ReplyAsync($"Erreur RCON : {ex.Message}");
      }
    }

    /// <summary>
    /// Active et|ou désactive le keepInventory
    /// </summary>
    /// <param name="value">true|false</param>
    /// <returns>retourne que la commande à bien fonctionné</returns>
    [Command("keepInv")]
    public async Task SetKeepInventoryAsync(string? value)
    {
      if (value != "true" && value != "false")
      {
        await ReplyAsync("Utilisation : '!keepInv on' ou '!keepInv off'");
        return;
      }

      string rconCommand = $"gamerule keepInventory {value}";

      try
      {
        using var rcon = new RCON(this.ip, this.port, this.password);
        await rcon.ConnectAsync();

        var response = await rcon.SendCommandAsync(rconCommand);
        await ReplyAsync($"KeepInventory {value} appliqué sur le serveur!");
      }

      catch (Exception ex)
      {
        await ReplyAsync($"Erreur lors de la connexion au serveur : {ex.Message}");
      }
    }
  }
}
