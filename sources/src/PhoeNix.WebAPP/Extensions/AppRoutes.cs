namespace PhoeNix.WebAPP.Extensions;

public static class AppRoutes
{
    public const string Home = "/";
    public const string Login = "/login";
    public const string Register = "/register";

    public const string Configurations = "/configurations";
    public static string ConfigurationDetail(Guid id) => $"/configurations/{id}";

    public const string SetupSessions = "/setup";
    public const string NewSetupSession = "/setup/new";
    public static string SetupSessionDetail(Guid id) => $"/setup/{id}";

    public const string Templates = "/templates";
    public const string TemplateCreator = "/templates/new";
    public static string TemplateDetail(Guid id) => $"/templates/{id}";
    public static string TemplateEditor(Guid id) => $"/templates/{id}/edit";

    public const string Settings = "/settings";

    public static string MachineDetail(Guid id) => $"/machines/{id}";
}
