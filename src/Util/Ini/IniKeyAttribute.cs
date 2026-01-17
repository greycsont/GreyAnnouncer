using System;

namespace GreyAnnouncer.Util.Ini;

[AttributeUsage(AttributeTargets.Property)]
public sealed class IniKeyAttribute : Attribute
{
    public string KeyName { get; }

    public IniKeyAttribute(string keyName)
    {
        KeyName = keyName;
    }
}
