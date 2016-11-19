using System.ComponentModel;

namespace NeUWP.Frameworks
{
    /// <summary>
    /// Extends <see cref = "INotifyPropertyChanged" and cref = "INotifyPropertyChanging" /> such that the change event can be raised by external parties.
    /// </summary>
    public interface IBindable : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Enables/Disables property change notification.
        /// </summary>
        bool IsNotifying { get; set; }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        void NotifyOfPropertyChange(string propertyName);

        /// <summary>
        /// Notifies subscribers of the property changing.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        void NotifyOfPropertyChanging(string propertyName);

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        void Refresh();
    }
}
