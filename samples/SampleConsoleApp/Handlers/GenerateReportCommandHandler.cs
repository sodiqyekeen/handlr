using System;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;
using SampleConsoleApp.Commands;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Handler for GenerateReportCommand - no partial class required!
/// </summary>
public class GenerateReportCommandHandler : ICommandHandler<GenerateReportCommand, Result<string>>
{
    /// <summary>
    /// Handles the GenerateReportCommand
    /// </summary>
    public async Task<Result<string>> Handle(GenerateReportCommand command, CancellationToken cancellationToken)
    {
        // Simulate report generation
        await Task.Delay(500, cancellationToken);

        var reportContent = $"Report Type: {command.ReportType}\n" +
                           $"Period: {command.StartDate:yyyy-MM-dd} to {command.EndDate:yyyy-MM-dd}\n" +
                           $"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        return Result<string>.Success(reportContent);
    }
}
