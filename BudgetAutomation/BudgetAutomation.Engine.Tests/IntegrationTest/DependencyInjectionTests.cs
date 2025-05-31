using BudgetAutomation.Engine.Handler;
using BudgetAutomation.Engine.Handler.Command;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace BudgetAutomation.Engine.Tests.IntegrationTest;

public class DependencyInjectionTests(MockedHostFactory<LocalTesting.Program> factory, ITestOutputHelper output)
    : IClassFixture<MockedHostFactory<LocalTesting.Program>> // Use the new factory type
{
    [Fact]
    public void ShouldResolve_IMessageHandler_FromScopedProvider()
    {
        // Arrange: Get the root service provider from the factory
        var rootProvider = factory.Services;

        // Act & Assert:
        // Create a scope because IMessageHandler is Scoped.
        using var scope = rootProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        // Try to resolve the service *from the scoped provider*.
        var exception = Record.Exception(() => { scopedProvider.GetRequiredService<IMessageHandler>(); });

        // Assert that NO exception was thrown during resolution within the scope.
        exception.Should()
            .BeNull($"because {nameof(IMessageHandler)} should be registered correctly and resolvable within a scope.");
    }

    [Theory]
    [InlineData(typeof(ICommand), "Command")]
    [InlineData(typeof(IMessageHandler), "Handler")]
    [InlineData(typeof(IChatStateService), "Service")]
    public void ShouldResolve_AllServicesEndingWithCommand_InApplicationAssembly_FromScopedProvider(
        Type interfaceExample,
        string interfaceEnding
        )
    {
        // Arrange: Get the root provider
        var rootProvider = factory.Services;
        var serviceAssembly = interfaceExample.Assembly;

        // Find interfaces ending with interfaceEnding
        var serviceTypes = serviceAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.EndsWith(interfaceEnding) && t != typeof(IDisposable)) // Example convention
            .ToList();

        Assert.True(serviceTypes.Any(),
            $"No service interfaces found matching the convention 'I*{interfaceEnding}'. Check assembly and naming."); // Sanity check

        // Act & Assert within a single scope for efficiency
        // Create ONE scope for resolving all these services.
        using var scope = rootProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        foreach (var serviceType in serviceTypes)
        {
            var exception = Record.Exception(() =>
            {
                var resolvedService = scopedProvider.GetRequiredService(serviceType);
                Assert.NotNull(resolvedService);
            });

            exception.Should()
                .BeNull($"Failed to resolve service: {serviceType.FullName}. Check its registration and dependencies.");
        }
    }

    [Theory]
    [InlineData(typeof(LogCommand), "Command")]
    [InlineData(typeof(MessageHandler), "Handler")]
    public void ShouldEnsure_AllClassesWithEndingAreRegistered(Type classExample, string classEnding)
    {
        // Arrange: Get the root provider
        var rootProvider = factory.Services;

        var handlerAssembly = classExample.Assembly; // Or use typeof(YourActualConcreteHandler).Assembly

        // Find all concrete, non-abstract classes ending with "classEnding"
        var handlerClassTypes = handlerAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith(classEnding))
            .ToList();

        Assert.True(handlerClassTypes.Any(),
            $"No concrete classes found matching the convention '*{classEnding}'. Check assembly and naming.");

        var unregisteredHandlers = new List<string>();

        // Act: Check registration within a scope
        // Use a scope because resolving dependencies might require it, even just to check existence
        using (var scope = rootProvider.CreateScope())
        {
            var scopedProvider = scope.ServiceProvider;

            foreach (var handlerClassType in handlerClassTypes)
            {
                // Check if it's resolvable by its concrete type
                // Use GetService: returns null if not registered directly this way
                var resolvedByConcreteType = scopedProvider.GetService(handlerClassType);

                // Check if it's resolvable via any interface it implements
                // Get interfaces implemented by the concrete class
                var interfaces = handlerClassType.GetInterfaces();
                bool resolvedByInterface = false;
                foreach (var iface in interfaces)
                {
                    // Exclude common infrastructure interfaces if desired (optional)
                    if (iface == typeof(IDisposable)) // Example exclusion
                    {
                        continue;
                    }

                    // Use GetService: returns null if the *interface* isn't registered
                    // with *any* implementation (or specifically this one)
                    var servicesRegisteredForInterface = scopedProvider.GetServices(iface);

                    // If the interface IS registered, we need to check if the implementation
                    // resolved IS our handlerClassType.
                    foreach (var serviceRegistered in servicesRegisteredForInterface)
                    {
                        if (serviceRegistered?.GetType() == handlerClassType)
                        {
                            resolvedByInterface = true;
                            break; // Found registration via one interface, no need to check others
                        }

                        output.WriteLine(
                            $"  -> Interface {iface.FullName} is registered, but with a different implementation ({servicesRegisteredForInterface.GetType().Name})");
                    }
                }

                // Determine if registered in any discoverable way
                bool isRegistered = (resolvedByConcreteType != null) || resolvedByInterface;

                if (!isRegistered)
                {
                    output.WriteLine(
                        $"FAILED: Not found registered directly or via a resolved interface for class '{handlerClassType.FullName}'.");
                    unregisteredHandlers.Add(handlerClassType.FullName!);
                }
                // else if (resolvedByConcreteType != null)
                // {
                //     // output.WriteLine($"  -> OK: Registered/resolvable via concrete type.");
                // }
                // else // resolvedByInterface must be true
                // {
                //     // output.WriteLine($"  -> OK: Registered/resolvable via interface.");
                // }
            }
        }

        // Assert: Fail if any handlers were found to be unregistered
        unregisteredHandlers.Should().BeEmpty(
            "because all concrete classes ending in 'Handler' should be registered with the DI container, " +
            "either directly or via an interface for which they are the configured implementation."
        );
    }
}