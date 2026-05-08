namespace PhoeNix.Contracts.Validation;

public record SystemValidationStatusResponse(
    string State,
    string? ErrorCode,
    string? ErrorMessage,
    string? Duration);

public record ModuleValidationStatusResponse(
    string State,
    string? ErrorCode,
    string? ErrorMessage,
    List<ModuleTestResultResponse>? Results);

public record ModuleTestResultResponse(
    string CheckName,
    string TestName,
    bool IsSuccess,
    List<ModuleTestErrorResponse> Errors);

public record ModuleTestErrorResponse(
    string Expected,
    string Name,
    string Result);
