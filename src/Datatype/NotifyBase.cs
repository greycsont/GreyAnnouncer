using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GreyAnnouncer.Base;

public abstract class NotifyBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _suspendNotify = false;

    private bool _hasPendingChange = false;

    public void BeginUpdate() => _suspendNotify = true;

    public void EndUpdate()
    {
        _suspendNotify = false;
        if (_hasPendingChange)
        {
            _hasPendingChange = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }


    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        if (!_suspendNotify)
            RaiseChanged(propertyName);
        else
            _hasPendingChange = true;

        return true;
    }

    protected void RaiseChanged(string name = null)
    {
        if (_suspendNotify)
        {
            _hasPendingChange = true;
            return;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
