using System.Collections.Generic;

using PluginConfig.API.Fields;

namespace GreyAnnouncer.FrontEnd;

public class PackConfigFields
{
    public BoolField RandomizeAudioField;
    public Dictionary<string, CategoryFields> CategoryFields;

}