using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carrot.UI.Controls.ViewModels {


    // https://stackoverflow.com/questions/47723876/
    // public delegate void PropertyChangedEnhancedEventHandler(object sender, PropertyChangedEnhancedEventArgs e);

    public interface INotifyPropertyChangedEnhanced {
        //
        // Summary:
        //     Occurs when a property value changes.
        event EventHandler<PropertyChangedEnhancedEventArgs> PropertyChangedEnhanced;
    }

    public class PropertyChangedEnhancedEventArgs : PropertyChangedEventArgs {
        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public PropertyChangedEnhancedEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName) {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public abstract class Enhanced : INotifyPropertyChangedEnhanced {
        public event EventHandler<PropertyChangedEnhancedEventArgs> PropertyChangedEnhanced;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]
    string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            var oldValue = field;
            field = value;
            this.OnPropertyChangedEnhanced(oldValue, value, propertyName);
            return true;
        }
        protected void OnPropertyChangedEnhanced<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = null) {
            this.PropertyChangedEnhanced?.Invoke(this, new PropertyChangedEnhancedEventArgs(propertyName, oldValue, newValue));
        }
    }
}
