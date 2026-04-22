using System.Text.Json;
using System.Text.Json.Serialization;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public sealed class ConfigurationRevision : AggregateRoot<ConfigurationId>
{
    public ConfigurationId ConfigurationId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime TimeStamp { get; private set; }
    public int Revision { get; private set; }

    public string SnapshotJson { get; private set; }

    private ConfigurationRevision(ConfigurationId id) : base(id)
    {
    }

    internal ConfigurationRevision(ConfigurationId revisionId, Configuration configuration) : base(revisionId)
    {
        ConfigurationId = configuration.Id;
        Title = configuration.Title;
        Description = configuration.Description;
        TimeStamp = configuration.TimeStamp;
        Revision = configuration.Revision;

        var snapshotData = new
        {
            configuration.Inputs,
            configuration.Modules,
            configuration.SystemSpecifications
        };

        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };

        SnapshotJson = JsonSerializer.Serialize(snapshotData, jsonOptions);
    }
}