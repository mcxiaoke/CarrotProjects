using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using PropertyChanged;

namespace CMDPanel {

    public class TabHeader : INotifyPropertyChanged {
        public string Title { get; set; } = "Tab";
        public CommandExecutor? Command { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabHeader(string title, CommandExecutor cmd) {
            Title = title;
            Command = cmd;
        }

        public TabHeader(string title) : this(title, new CommandExecutor()) { }

        public TabHeader() {
        }
    }

    public class TabContent {
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Level { get; set; }
        public string Prefix => CreatedAt.ToString("[HH:mm:ss.fff]");

        public TabContent(string text, int level) {
            Text = text;
            CreatedAt = DateTime.Now;
            Level = level;
        }
    }

    public class TabModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabHeader Header { get; set; }
        public ObservableCollection<TabContent> Content { get; set; }

        public TabModel(TabHeader header) {
            Header = header;
            Content = new ObservableCollection<TabContent>();
        }

        public TabModel() : this(new TabHeader()) {
        }

        public ICommand ToggleCommand => new RelayCommand(x => {
            Debug.WriteLine($"{Header.Title} ToggleCommand {x}");
        });

        public bool EditEnabled => !Header.Command?.IsRunning ?? true;
    }
}
