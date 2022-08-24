using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using DotNetOutdated;
using DotNetOutdated.Core.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("DotNetOutdated.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

var builder = new CommandLineBuilder(new DotnetOutdatedCommand());

builder.AddMiddleware((context, next) =>
{
    context.BindingContext.AddService(s => PhysicalConsole.Singleton);
    context.BindingContext.AddService<IReporter>(s => new ConsoleReporter(s.GetService<IConsole>()!));

    context.BindingContext.AddService<IFileSystem>(s => new FileSystem());

    context.BindingContext.AddService<IProjectDiscoveryService>(s => new ProjectDiscoveryService(s.GetService<IFileSystem>()!));

    context.BindingContext.AddService<IDotNetRunner>(s => new DotNetRunner());

    context.BindingContext.AddService<IDependencyGraphService>(s => new DependencyGraphService(s.GetService<IDotNetRunner>()!, s.GetService<IFileSystem>()!));

    context.BindingContext.AddService<IProjectAnalysisService>(s => new ProjectAnalysisService(s.GetService<IDependencyGraphService>()!, s.GetService<IDotNetRestoreService>()!, s.GetService<IFileSystem>()!));

    context.BindingContext.AddService<IDotNetRestoreService>(s => new DotNetRestoreService(s.GetService<IDotNetRunner>()!, s.GetService<IFileSystem>()!));

    context.BindingContext.AddService<IDotNetAddPackageService>(s => new DotNetAddPackageService(s.GetService<IDotNetRunner>()!, s.GetService<IFileSystem>()!));

    context.BindingContext.AddService<INuGetPackageInfoService>(s => new NuGetPackageInfoService());

    context.BindingContext.AddService<INuGetPackageResolutionService>(s => new NuGetPackageResolutionService(s.GetService<INuGetPackageInfoService>()!));

    context.BindingContext.AddService<ICentralPackageVersionManagementService>(s => new CentralPackageVersionManagementService(s.GetService<IFileSystem>()!));

    return next(context);
});

var result = await builder.UseDefaults().Build().InvokeAsync(args).ConfigureAwait(false);

return result;
