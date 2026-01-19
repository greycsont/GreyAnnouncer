using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GreyAnnouncer.Base;

public abstract class NotifyBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        LogManager.LogDebug($"Set Field Called by: {propertyName}");
        field = value;
        RaiseChanged(propertyName);
        return true;
    }

    protected void RaiseChanged(string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
