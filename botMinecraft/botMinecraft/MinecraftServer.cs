using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using Discord.Interactions.Builders;
using Newtonsoft.Json;
using Discord.Net;

/// <summary>
/// Initialise le Bot
/// </summary>
public class MinecraftServer
{
  private DiscordSocketClient client;
  private CommandService commands;

  /// <summary>
  /// Donne accès au client
  /// </summary>
  public DiscordSocketClient Client
  {
    get { return this.Client; }
  }

  public static void Main(string[] args)
    => new MinecraftServer().MainAsync().GetAwaiter().GetResult();

  /// <summary>
  /// Connecte le client
  /// </summary>
  /// <returns>Retour un Async</returns>
  public async Task MainAsync()
  {
    //Connecte le bot discord
    client = new DiscordSocketClient(new DiscordSocketConfig
    {
      LogLevel = LogSeverity.Debug,
      GatewayIntents = GatewayIntents.All
    });

    //Attache le Handler une seule fois à RepondFeur
    this.client.MessageReceived += this.RepondFeurAsync;

    //Créer les commandes pour pouvoir les utiliser
    commands = new CommandService();

    //Pour gérer les commandes slash
    client.SlashCommandExecuted += this.SlashCommandHandler;

    client.Log += Log;

    await InstallCommandsAsync();

    //Se connecte au bon bot en récupérant son token contenue dans une de mes variables environnement pour plus de sécurité
    await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TokenMC", EnvironmentVariableTarget.User));
    await client.StartAsync();

    await Task.Delay(-1);

    Discord.SlashCommandBuilder globalCommandAPI = new Discord.SlashCommandBuilder()
      .WithName("info")
      .WithDescription("Affiches les informations du bot !");

    try
    {
      await this.client.CreateGlobalApplicationCommandAsync(globalCommandAPI.Build());
      Console.WriteLine("Commandes Slash crées avec succès !");
    }
    catch (HttpException ex)
    {
      var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
      Console.WriteLine($"Erreur lors de la création des commandes : {json}");
    }
  }

  /// <summary>
  /// Réponds Feur ! lorsque le dernier message se finit par quoi
  /// </summary>
  /// <param name="message">Le message à écouter</param>
  /// <returns></returns>
  private async Task RepondFeurAsync(SocketMessage message)
  {
      //Ignore les messages du bot lui-même
      if (message.Author.IsBot) return;

      string? contentMsg = message.Content.Trim().ToLower();

      //Si le message se termine par "quoi" alors il réponds "Feur !"
      if (contentMsg.EndsWith("quoi"))
      {
        await message.Channel.SendMessageAsync("Feur !");
      }
  }

  /// <summary>
  /// Pour l'intéraction avec les commandes slashs
  /// </summary>
  /// <param name="command"></param>
  /// <returns></returns>
  private async Task SlashCommandHandler(SocketSlashCommand command)
  {
    if (command.Data.Name == "info")
    {
      EmbedBuilder embed = new EmbedBuilder()
        .WithTitle("Information du BotMC")
        .WithDescription("Je suis le bot Minecraft")
        .WithColor(Color.Blue)
        .WithCurrentTimestamp();
    }
  }

  /// <summary>
  /// Installe les commandes
  /// </summary>
  /// <returns></returns>
  public async Task InstallCommandsAsync()
  {
    client.MessageReceived += HandlerCommandAsync;
    await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);   
  }

  /// <summary>
  /// Fait en sorte de demander un préfix pour s'adresser au bot
  /// </summary>
  /// <param name="msg"></param>
  /// <returns></returns>
  private async Task HandlerCommandAsync(SocketMessage msg)
  {
    //Pour répondre au commande vérifier l'utilisation du préfix "!"
    var message = (SocketUserMessage)msg;
    if (message == null) return;
    int argPos = 0;
    if (!message.HasCharPrefix('!', ref argPos)) return;

    var context = new SocketCommandContext(client, message);
    var result = await commands.ExecuteAsync(context, argPos, null);

    //ERREUR
    if (!result.IsSuccess)
      await context.Channel.SendMessageAsync(result.ErrorReason);   
  }

  /// <summary>
  /// Affiche sur la console un message de Log quand le client se connecte
  /// </summary>
  /// <param name="msg">Le message à afficher sur la console</param>
  /// <returns>Retourne la tache complété</returns>
  private Task Log(LogMessage msg)

  {
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
  }
}
