using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Models.Validation;

namespace PhoeNix.Infrastructure.Services.Validation;

internal sealed class ValidationBackgroundService(
    IValidationJobTracker jobTracker,
    INixTestRunner nixTestRunner,
    ILogger<ValidationBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobTracker.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                ExecuteJob(job, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception executing validation job.");
                SetFailedStatus(job, "ValidationUnhandledError", ex.Message);
            }
        }
    }

    private void ExecuteJob(ValidationJob job, CancellationToken cancellationToken)
    {
        if (job.Type == ValidationType.System)
            ExecuteSystemJob(job, cancellationToken);
        else
            ExecuteModuleJob(job, cancellationToken);
    }

    private void ExecuteSystemJob(ValidationJob job, CancellationToken cancellationToken)
    {
        if (job.SystemKey is null || job.SystemArchitecture is null)
        {
            logger.LogError("System validation job is missing SystemKey or SystemArchitecture.");
            return;
        }

        jobTracker.SetSystemStatus(job.SystemKey, new ValidationJobStatus(ValidationJobState.Running));

        logger.LogInformation(
            "Running system validation for system {SystemId} in configuration {ConfigurationId}.",
            job.SystemKey.SystemId.Value,
            job.SystemKey.ConfigurationId.Value);

        var result = nixTestRunner.RunSystemTest(
            job.SystemKey.SystemId,
            job.SystemArchitecture.Value,
            job.ConfigurationPath,
            cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError(
                "System validation failed for system {SystemId}: {ErrorCode} — {ErrorMessage}",
                job.SystemKey.SystemId.Value,
                result.Error.Code,
                result.Error.Description);

            jobTracker.SetSystemStatus(job.SystemKey, new ValidationJobStatus(
                ValidationJobState.Failed,
                result.Error.Code,
                result.Error.Description));
            return;
        }

        logger.LogInformation(
            "System validation succeeded for system {SystemId} in {Duration}.",
            job.SystemKey.SystemId.Value,
            result.Value.BuildTime);

        jobTracker.SetSystemStatus(job.SystemKey, new ValidationJobStatus(
            ValidationJobState.Succeeded,
            Duration: result.Value.BuildTime));
    }

    private void ExecuteModuleJob(ValidationJob job, CancellationToken cancellationToken)
    {
        if (job.ModuleKey is null || job.ModuleTests is null)
        {
            logger.LogError("Module validation job is missing ModuleKey or ModuleTests.");
            return;
        }

        jobTracker.SetModuleStatus(job.ModuleKey, new ValidationJobStatus(ValidationJobState.Running));

        logger.LogInformation(
            "Running module validation for module {ModuleTemplateId} ({Architecture}) in configuration {ConfigurationId}.",
            job.ModuleKey.ModuleTemplateId.Value,
            job.ModuleKey.Architecture,
            job.ModuleKey.ConfigurationId.Value);

        var testResults = new List<ModuleTestResult>();

        foreach (var (testId, testName, checkAttributeName) in job.ModuleTests)
        {
            var result = nixTestRunner.RunModuleTest(
                testId,
                testName,
                checkAttributeName,
                job.ModuleKey.Architecture,
                job.ConfigurationPath,
                cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Module validation failed for module {ModuleTemplateId} test '{TestName}': {ErrorCode} — {ErrorMessage}",
                    job.ModuleKey.ModuleTemplateId.Value,
                    testName,
                    result.Error.Code,
                    result.Error.Description);

                jobTracker.SetModuleStatus(job.ModuleKey, new ValidationJobStatus(
                    ValidationJobState.Failed,
                    result.Error.Code,
                    result.Error.Description));
                return;
            }

            var testResponse = result.Value;
            testResults.Add(new ModuleTestResult(
                checkAttributeName,
                testResponse.Name,
                testResponse.IsSuccess,
                testResponse.Errors.Select(e => new ModuleTestErrorResult(e.Expected, e.Name, e.Result)).ToList()));
        }

        var anyFailed = testResults.Any(t => !t.IsSuccess);

        if (anyFailed)
        {
            logger.LogWarning(
                "Module validation completed with test failures for module {ModuleTemplateId}.",
                job.ModuleKey.ModuleTemplateId.Value);

            jobTracker.SetModuleStatus(job.ModuleKey, new ValidationJobStatus(
                ValidationJobState.Failed,
                "TestsFailed",
                "One or more tests failed.",
                TestResults: testResults));
            return;
        }

        logger.LogInformation(
            "Module validation succeeded for module {ModuleTemplateId}.",
            job.ModuleKey.ModuleTemplateId.Value);

        jobTracker.SetModuleStatus(job.ModuleKey, new ValidationJobStatus(
            ValidationJobState.Succeeded,
            TestResults: testResults));
    }

    private void SetFailedStatus(ValidationJob job, string errorCode, string errorMessage)
    {
        if (job.Type == ValidationType.System && job.SystemKey is not null)
            jobTracker.SetSystemStatus(job.SystemKey, new ValidationJobStatus(
                ValidationJobState.Failed, errorCode, errorMessage));
        else if (job.Type == ValidationType.Module && job.ModuleKey is not null)
            jobTracker.SetModuleStatus(job.ModuleKey, new ValidationJobStatus(
                ValidationJobState.Failed, errorCode, errorMessage));
    }
}
