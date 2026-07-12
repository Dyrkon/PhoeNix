using PhoeNix.Application.Abstractions.Nix;

namespace PhoeNix.Application.Abstractions.Git;

/// <summary>
/// Marker interface for the friendly-name configuration files builder used by GitOps.
/// Produces Nix files with human-readable slugified names instead of GUID-based paths.
/// </summary>
public interface IGitOpsConfigurationFilesBuilder : IConfigurationFilesBuilder;

/// <summary>
/// Marker interface for the friendly-name module files builder used by GitOps.
/// Produces Nix files with human-readable slugified names instead of GUID-based paths.
/// </summary>
public interface IGitOpsModuleFilesBuilder : IModuleFilesBuilder;
