using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GenshinLib;
using PropertyChanged;


namespace GenshinNotifier {
    public class WidgetViewModel : INotifyPropertyChanged {

        public UserGameRole User { get; set; }
        public DailyNote Note { get; set; }

        [DoNotNotify]
        public bool TestValue { get; set; }


        public WidgetViewModel() {
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
