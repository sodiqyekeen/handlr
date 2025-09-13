using Handlr.Abstractions.Common;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Results;
using SampleConsoleApp;
using SampleConsoleApp.Commands;
using SampleConsoleApp.Examples;
using SampleConsoleApp.Queries;

// First check debug info to see what's being discovered
Console.WriteLine("=== Source Generator Debug Info ===");
TestDebug.PrintDebugInfo();
Console.WriteLine();

Console.WriteLine("=== CQRS Framework with Pipeline Behaviors ===");
Console.WriteLine("This framework provides the foundation for CQRS with pipeline behaviors.");
Console.WriteLine("The source generator automatically discovers commands, queries, and handlers.");
Console.WriteLine();

Console.WriteLine("=== Available Pipeline Behavior Examples ===");
Console.WriteLine("✓ ValidationBehaviorExample - Shows how to implement request validation");
Console.WriteLine("✓ LoggingBehaviorExample - Shows how to add logging with correlation IDs");
Console.WriteLine("✓ CachingBehaviorExample - Shows how to implement query result caching");
Console.WriteLine("✓ AuthorizationBehaviorExample - Shows how to implement permission checking");
Console.WriteLine("✓ RetryBehaviorExample - Shows how to handle transient failures");
Console.WriteLine("✓ MetricsBehaviorExample - Shows how to collect performance metrics");
Console.WriteLine();

Console.WriteLine("=== Example Commands and Queries ===");
Console.WriteLine("✓ ExampleCommand - Demonstrates IValidatable interface");
Console.WriteLine("✓ ExampleQuery - Demonstrates ICacheable interface");
Console.WriteLine("✓ SecureCommand/AdminCommand - Demonstrate IRequireAuthorization interface");
Console.WriteLine("✓ RetryableCommand/RetryableQuery - Demonstrate IRetryable interface");
Console.WriteLine("✓ MetricsEnabledCommand/MetricsEnabledQuery - Demonstrate IMetricsEnabled interface");
Console.WriteLine("✓ Example Handlers - Show partial class pattern for source generator");
Console.WriteLine();

Console.WriteLine("=== How to Create Your Own Pipeline Behaviors ===");
Console.WriteLine(@"
1. Implement IPipelineBehavior<TRequest, TResponse>:

   public class MyCustomBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
   {
       public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
       {
           // Your custom logic before
           var response = await next();
           // Your custom logic after
           return response;
       }
   }

2. Register in DI container:

   services.AddScoped<IPipelineBehavior<MyCommand, Result>, MyCustomBehavior<MyCommand, Result>>();

3. Common behavior patterns:
   - Validation: Check request validity before processing
   - Authorization: Verify user permissions
   - Logging: Track request execution and performance
   - Caching: Cache query results for better performance
   - Retry: Handle transient failures with retry logic
   - Metrics: Collect performance and usage metrics

4. Make your commands/queries implement behavior interfaces:
   - IValidatable: For validation behaviors
   - ICacheable: For caching behaviors
   - Custom interfaces: For your specific needs
");

Console.WriteLine("The framework handles the pipeline execution automatically!");
Console.WriteLine();
Console.WriteLine("Build and run this sample to see the source generator in action!");

// Only wait for key press in interactive environments
if (Environment.UserInteractive && !Console.IsInputRedirected)
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
else
{
    Console.WriteLine("Sample completed successfully.");
}
