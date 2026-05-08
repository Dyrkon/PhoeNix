using System.Threading.Channels;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Validation;

public enum ValidationType { System, Module }

public enum ValidationJobState { None, Queued, Running, Succeeded, Failed }

public record SystemValidationKey(ConfigurationId ConfigurationId, SystemId SystemId);

public record ModuleValidationKey(ConfigurationId ConfigurationId, ModuleTemplateId ModuleTemplateId, Architecture Architecture);

public record ValidationJob(
    ValidationType Type,
    SystemValidationKey? SystemKey,
    ModuleValidationKey? ModuleKey,
    Architecture? SystemArchitecture,
    string ConfigurationPath,
    List<(TestId Id, string Name, string CheckAttributeName)>? ModuleTests);

public record ValidationJobStatus(
    ValidationJobState State,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    string? Duration = null,
    List<ModuleTestResult>? TestResults = null);

public record ModuleTestResult(string CheckAttributeName, string TestName, bool IsSuccess, List<ModuleTestErrorResult> Errors);

public record ModuleTestErrorResult(string Expected, string Name, string Result);
