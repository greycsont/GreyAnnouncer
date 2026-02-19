using System;
namespace GreyAnnouncer.Config;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigInfoAttribute : Attribute
{
    public string Section { get; }
    public string Name { get; }
    public string Description { get; }

    public ConfigInfoAttribute(string section, string name, string description)
    {
        Section = section;
        Name = name;
        Description = description;
    }
}