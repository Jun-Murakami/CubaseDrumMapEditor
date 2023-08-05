using CommunityToolkit.Mvvm.ComponentModel;

namespace CubaseDrumMapEditor.Models
{
    public class MapItem : ObservableObject
    {
        private int _iNote;
        private int _oNote;
        private int _channel;
        private float _length;
        private int _mute;
        private int _displayNote;
        private int _headSymbol;
        private int _voice;
        private int _portIndex;
        private string? _name;
        private int _quantizeIndex;

        public int INote
        {
            get => _iNote;
            set => SetProperty(ref _iNote, value);
        }

        public int ONote
        {
            get => _oNote;
            set => SetProperty(ref _oNote, value);
        }

        public int Channel
        {
            get => _channel;
            set => SetProperty(ref _channel, value);
        }

        public float Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        public int Mute
        {
            get => _mute;
            set => SetProperty(ref _mute, value);
        }

        public int DisplayNote
        {
            get => _displayNote;
            set => SetProperty(ref _displayNote, value);
        }

        public int HeadSymbol
        {
            get => _headSymbol;
            set => SetProperty(ref _headSymbol, value);
        }

        public int Voice
        {
            get => _voice;
            set => SetProperty(ref _voice, value);
        }

        public int PortIndex
        {
            get => _portIndex;
            set => SetProperty(ref _portIndex, value);
        }

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int QuantizeIndex
        {
            get => _quantizeIndex;
            set => SetProperty(ref _quantizeIndex, value);
        }
    }
}

