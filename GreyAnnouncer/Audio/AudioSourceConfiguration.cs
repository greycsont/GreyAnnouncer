
namespace greycsont.GreyAnnouncer;

public struct AudioSourceConfiguration
{
    public float SpatialBlend { get; set; }
    public int   Priority     { get; set; }
    public float Volume       { get; set; }
    public float Pitch        { get; set; }

    public static AudioSourceConfiguration Default => new()
    {
        SpatialBlend = 0f,
        Priority     = 0,
        Volume       = 1f,
        Pitch        = 1f,
    };
}
   