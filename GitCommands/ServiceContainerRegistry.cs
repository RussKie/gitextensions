using System.ComponentModel.Design;
using GitCommands.ExternalLinks;

namespace GitCommands;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        ExternalLinksStorage externalLinksStorage = new();
        ConfiguredLinkDefinitionsProvider effectiveLinkDefinitionsProvider = new(externalLinksStorage);
        serviceContainer.AddService<IExternalLinksStorage>(externalLinksStorage);
        serviceContainer.AddService<IConfiguredLinkDefinitionsProvider>(effectiveLinkDefinitionsProvider);

    }
}
