namespace PhoeNix.Application.Abstractions.Monitoring;

public interface IPrometheusTokenService
{
    string CreateToken();
    Task<bool> ValidateTokenAsync(string token);
}
