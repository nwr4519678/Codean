using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Judge.Abstractions;
using Platform.Judge.Engines.Browser;
using Platform.Judge.Engines.Judge0;
using Platform.Judge.Languages;
using Platform.Judge.Services;
using Platform.Judge.Worker;

namespace Platform.Judge;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformJudge(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurations
        services.Configure<Judge0Options>(configuration.GetSection(Judge0Options.SectionName));

        // Judge0 Client
        services.AddHttpClient<Judge0Client>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Judge0Options>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Abstractions & Implementations
        services.AddSingleton<ILanguageResolver, LanguageResolver>();
        services.AddSingleton<ITestCaseEvaluator, TestCaseEvaluator>();

        // Engines
        services.AddSingleton<IExecutionEngine, Judge0ExecutionEngine>();
        services.AddSingleton<BrowserTestRunner>();
        services.AddSingleton<IExecutionEngine, BrowserExecutionEngine>();

        // Orchestrator
        services.AddSingleton<IExecutionOrchestrator, JudgeOrchestrator>();

        // Execution Storage & Worker Queue
        services.AddSingleton<ExecutionStore>();
        services.AddSingleton<ExecutionQueue>();
        services.AddHostedService<JudgeWorkerHostedService>();

        return services;
    }
}
