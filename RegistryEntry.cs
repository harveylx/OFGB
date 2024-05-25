using System.Collections.Generic;

namespace OFGB;

public class RegistryEntry
{
    public string KeyPath { get; set; }
    public string KeyName { get; set; }
    public bool ValueInverted { get; set; } = false;
    public bool RequiresAdminPermissions { get; set; } = false;
}

public class RegistryEntriesConfig
{
    public Dictionary<string, List<RegistryEntry>> RegistryEntries { get; set; }
}