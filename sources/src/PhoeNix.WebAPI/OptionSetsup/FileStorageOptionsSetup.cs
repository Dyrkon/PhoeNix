using Microsoft.Extensions.Options;
using PhoeNix.Domain.Options;

namespace PhoeNix.WebAPI.OptionSetsup;

public class FileStorageOptionsSetup : IConfigureOptions<FileStorageOptions>
{
    private const string SectionName = "FileStorage";
    private readonly  IConfiguration _configuration;
    
    public FileStorageOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(FileStorageOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}