using NeUWP.Views;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeUWP.Frameworks
{
    public class ViewModelBase : IBindable, ICleanup
    {
        /// <summary>
        /// Creates an instance of <see cref = "ViewModelBase" />.
        /// </summary>
        public ViewModelBase()
        {
            IsNotifying = true;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Occurs when a property value changing.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

        /// <summary>
        /// Enables/Disables property change notification.
        /// </summary>
        public bool IsNotifying { get; set; }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public virtual void Refresh()
        {
            NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        public virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            if (IsNotifying)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;

                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            var propertyName = ViewModelExtensions.GetPropertyName(property);
            if (propertyName != null)
            {
                NotifyOfPropertyChange(propertyName);
            }
        }

        /// <summary>
        /// Notifies subscribers of the property changing.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        public virtual void NotifyOfPropertyChanging([CallerMemberName] string propertyName = null)
        {
            if (IsNotifying)
            {
                OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Notifies subscribers of the property changing.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        public void NotifyOfPropertyChanging<TProperty>(Expression<Func<TProperty>> property)
        {
            var propertyName = ViewModelExtensions.GetPropertyName(property);
            if (propertyName != null)
            {
                NotifyOfPropertyChanging(propertyName);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged" /> event directly.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanging" /> event directly.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            var handler = PropertyChanging;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Cleans up the instance, for example by saving its state,
        /// removing resources, etc...
        /// </summary>
        public virtual void Cleanup()
        {
            IsNotifying = false;
        }
    }
}
