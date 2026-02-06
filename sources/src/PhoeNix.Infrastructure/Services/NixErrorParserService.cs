using System.Text.Json;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class NixErrorParserService : INixErrorParserService
{
    public Result<ModuleTestResponse> ParseModelTestResult(TestId id, string testName, string testOutput,
        string errorOutput, int exitCode)
    {
        if (exitCode == 0 && testOutput == string.Empty)
            return new ModuleTestResponse(id, testName, true, []);

        try
        {
            var text = errorOutput.Split('\n').First().Replace($"{testName}> ", "");
            var errors =
                ModuleTestParser.ParseFailures(text);

            if (errors is null)
                throw new JsonException();
            return new ModuleTestResponse(id, testName, false, errors);
        }
        catch (Exception e)
        {
            return Result.Failure<ModuleTestResponse>(new Error("NixErrorParseError",
                $"Unable to parser result of test {testName}: {e.Message}"));
        }
    }
}