namespace PhoeNix.Domain.Models.Files;

public record FileBase(string Name, bool IsFolder);

public record TextFile(string Name, string Content) : FileBase(Name, false);

public record Folder(string Name, IEnumerable<FileBase> Files) : FileBase(Name, true);