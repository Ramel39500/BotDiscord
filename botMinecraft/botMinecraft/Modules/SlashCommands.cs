using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace botMinecraft.Modules
{
  /// <summary>
  /// Classe de gestion des Slash Commands
  /// </summary>
  public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
  {
    /// <summary>
    /// Réponds Bonjour
    /// </summary>
    /// <returns></returns>
    [SlashCommand("Hello", "Dire Bonjour")]
    public async Task Hello()
    {
      await RespondAsync($"Bonjour {Context.User.Mention} !");
    }

    /// <summary>
    /// Configure le framework d'interaction
    /// </summary>
    /// <returns></returns>
    public async Task SetupInteractionframework()
    {
      MinecraftServer mcServ = new MinecraftServer();

      var services = new ServiceCollection()
        .AddSingleton(mcServ.Client)
        .BuildServiceProvider();

      InteractionService interactionService = new InteractionService(mcServ.Client.Rest);
      await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

      mcServ.Client.InteractionCreated += async (interaction) =>
      {
        var context = new SocketInteractionContext(mcServ.Client, interaction);
        await interactionService.ExecuteCommandAsync(context, services);
      };
    }
  }
}
