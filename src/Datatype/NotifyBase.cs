using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GreyAnnouncer.Base;

public abstract class NotifyBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _suspendNotify = false;

    public void BeginUpdate() => _suspendNotify = true;

    public void EndUpdate() => _suspendNotify = false;


    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;

        if (!_suspendNotify)
            RaiseChanged(propertyName);

        return true;
    }

    protected void RaiseChanged(string name = null)
    {
        if (_suspendNotify)
            return;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
