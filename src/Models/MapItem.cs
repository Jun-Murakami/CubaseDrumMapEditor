using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper.Configuration.Attributes;
using System.ComponentModel;

namespace CubaseDrumMapEditor.Models
{
    public class MapItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _displayNote;
        [Ignore]
        public int DisplayNote
        {
            get => _displayNote;
            set
            {
                _displayNote = value;
                OnPropertyChanged(nameof(DisplayNote));
            }
        }

        private string? _displaeNoteName;
        public string? DisplayNoteName
        {
            get => _displaeNoteName;
            set
            {
                _displaeNoteName = value;
                OnPropertyChanged(nameof(DisplayNoteName));
            }
        }

        public string? Name { get; set; }

        private int _iNote;
        [Ignore]
        public int INote
        {
            get => _iNote;
            set
            {
                _iNote = value;
                OnPropertyChanged(nameof(INote));
            }
        }

        private string? _iNoteName;
        public string? INoteName
        {
            get => _iNoteName;
            set
            {
                _iNoteName = value;
                OnPropertyChanged(nameof(INoteName));
            }
        }

        private int _oNote;
        [Ignore]
        public int ONote
        {
            get => _oNote;
            set
            {
                _oNote = value;
                OnPropertyChanged(nameof(ONote));
            }
        }
        private string? _oNoteName;
        public string? ONoteName
        {
            get => _oNoteName;
            set
            {
                _oNoteName = value;
                OnPropertyChanged(nameof(ONoteName));
            }
        }

        public int Channel { get; set; }
        public float Length { get; set; }
        public int Mute { get; set; }
        public int HeadSymbol { get; set; }
        public int Voice { get; set; }
        public int PortIndex { get; set; }
        public int QuantizeIndex { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

