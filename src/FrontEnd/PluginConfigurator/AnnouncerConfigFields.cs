using System.Collections.Generic;

using PluginConfig.API.Fields;

namespace GreyAnnouncer.FrontEnd;

public class AnnouncerConfigFields
{
    public BoolField RandomizeAudioField;
    public Dictionary<string, CategoryFields> CategoryFields;

}