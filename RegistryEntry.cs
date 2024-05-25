namespace OFGB;

public class RegistryEntry(string keyPath, string keyName, bool valueInverted = false)
{
    public string KeyPath { get; } = keyPath;
    public string KeyName { get; } = keyName;
    public bool ValueInverted { get; } = valueInverted;
}