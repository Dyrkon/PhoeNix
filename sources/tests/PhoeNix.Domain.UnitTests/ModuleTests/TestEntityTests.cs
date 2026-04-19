using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class TestEntityTests
{
    private readonly TestId _testId = new(Guid.NewGuid());
    private readonly ModuleTemplateId _templateId = new(Guid.NewGuid());

    [Fact]
    public void Test_Should_Create_Successfully()
    {
        var result = Test.Create(_testId, _templateId, "my-test");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_testId);
        result.Value.ModuleTemplateId.Should().Be(_templateId);
        result.Value.Name.Should().Be("my-test");
        result.Value.Content.Should().BeEmpty();
        result.Value.VariableNames.Should().BeEmpty();
    }

    [Fact]
    public void Test_Should_Trim_Name_On_Create()
    {
        var result = Test.Create(_testId, _templateId, "  my-test  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("my-test");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Test_Should_Fail_Create_When_Name_Empty(string name)
    {
        var result = Test.Create(_testId, _templateId, name);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module test name can't be empty.");
    }

    [Fact]
    public void Test_Should_Rename()
    {
        var test = Test.Create(_testId, _templateId, "old-name").Value;

        var result = test.Rename("new-name");

        result.IsSuccess.Should().BeTrue();
        test.Name.Should().Be("new-name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Test_Should_Fail_Rename_When_Empty(string name)
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.Rename(name);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module test name can't be empty.");
        test.Name.Should().Be("my-test");
    }

    [Fact]
    public void Test_Should_ChangeContent()
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.ChangeContent("echo ${VAR1} and ${VAR2}", new List<string> { "${VAR1}", "${VAR2}" });

        result.IsSuccess.Should().BeTrue();
        test.Content.Should().Be("echo ${VAR1} and ${VAR2}");
        test.VariableNames.Should().BeEquivalentTo("${VAR1}", "${VAR2}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Test_Should_Fail_ChangeContent_When_Content_Empty(string content)
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.ChangeContent(content, new List<string>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("can't be empty");
    }

    [Fact]
    public void Test_Should_Fail_ChangeContent_When_VariableName_Empty()
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.ChangeContent("some content", new List<string> { "VAR1", "" });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("can't be empty");
    }

    [Fact]
    public void Test_Should_Fail_ChangeContent_When_Variable_Not_In_Content()
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.ChangeContent("content without var", new List<string> { "MISSING_VAR" });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("All variables must be present");
    }

    [Fact]
    public void Test_Should_ChangeContent_With_No_Variables()
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;

        var result = test.ChangeContent("static content with no vars", new List<string>());

        result.IsSuccess.Should().BeTrue();
        test.Content.Should().Be("static content with no vars");
        test.VariableNames.Should().BeEmpty();
    }

    [Fact]
    public void Test_Should_Replace_Variables_On_Subsequent_ChangeContent()
    {
        var test = Test.Create(_testId, _templateId, "my-test").Value;
        test.ChangeContent("content VAR1", new List<string> { "VAR1" });

        var result = test.ChangeContent("content VAR2", new List<string> { "VAR2" });

        result.IsSuccess.Should().BeTrue();
        test.VariableNames.Should().ContainSingle("VAR2");
        test.VariableNames.Should().NotContain("VAR1");
    }
}
