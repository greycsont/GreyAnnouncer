using System.Collections.Generic;
using UnityEngine;

namespace greycsont.GreyAnnouncer;

internal class LoadingStatus
{
    public string          Category      { get; set;         }
    public int             ExpectedFiles { get; set;         }
    public int             LoadedFiles   { get; set;         }
    public bool            HasError      { get; set;         }
    public List<AudioClip> LoadedClips   { get; private set; } = new List<AudioClip>();
}
