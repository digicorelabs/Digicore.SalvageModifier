using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.API.Plugins;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Plugins;
using OpenMod.Unturned.Players.Connections.Events;
using Microsoft.Extensions.DependencyInjection;

[assembly: PluginMetadata("Digicore.Unturned.Plugins.SalvageModifier", DisplayName = "Digicore.Unturned.Plugins.SalvageModifier", Author = "Digicore Labs", Website = "digicorelabs.com")]
namespace SalvageModifier
{
    public class SalvageModifierPlugin : OpenModUnturnedPlugin
    {
        public readonly ILogger<SalvageModifierPlugin> m_Logger;

        public SalvageModifierPlugin(
            ILogger<SalvageModifierPlugin> logger,
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            m_Logger = logger;
        }

        protected override UniTask OnLoadAsync()
        {
            m_Logger.LogInformation("[Digicore.Unturned.Plugins.SalvageModifier] MESSAGE: SalvageModifier has loaded.");

            return UniTask.CompletedTask;
        }
    }

    [Service]
    public interface ISalvageModifierService
    {
        float getSalvageTime();
    }

    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
    public class SalvageModifierService : ISalvageModifierService
    {
        private readonly IConfiguration m_Configuration;
        private readonly ILogger<SalvageModifierService> m_Logger;

        public SalvageModifierService (
            IConfiguration configuration,
            ILogger<SalvageModifierService> logger
        )
        {
            m_Configuration = configuration;
            m_Logger = logger;
        }

        public float getSalvageTime()
        {
            var defaultSalvageTime = 3f;

            try
            {
                var salvageTimeFromConfig = m_Configuration.GetSection("salvage_time").Get<float>();

                return salvageTimeFromConfig;
            } catch (System.Exception) {
                m_Logger.LogInformation("[Digicore.Unturned.Plugins.SalvageModifier] ERROR: Unable to fetch `salvage_time` from `config.yaml` as a number.");
                m_Logger.LogInformation($"[Digicore.Unturned.Plugins.SalvageModifier] MESSAGE: Reverting to default salvage time of {(int)Math.Ceiling(defaultSalvageTime)} seconds.");

                return defaultSalvageTime;
            }
        }
    }

    public class OnPlayerConnect : IEventListener<UnturnedPlayerConnectedEvent>
    {
        private readonly ISalvageModifierService m_salvageModifierService;

        public OnPlayerConnect (
            ISalvageModifierService salvageModifierService
        )
        {
            m_salvageModifierService = salvageModifierService;
        }

        public Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
        {
            var user = @event.Player;
            var salvageTime = m_salvageModifierService.getSalvageTime();

            user.Player.interact.sendSalvageTimeOverride(salvageTime);

            return Task.CompletedTask;
        }
    }
}
