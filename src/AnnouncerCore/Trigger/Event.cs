using System;

namespace GreyAnnouncer.AnnouncerCore;

public class EventTrigger<T>
{
    public event Action<T> OnTriggered;
    public void Raise(T value) => OnTriggered?.Invoke(value);
}
