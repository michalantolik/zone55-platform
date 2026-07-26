namespace Zone55.Management.Models;

public sealed record SeedFileDownload(
    string FileName,
    byte[] Content);
