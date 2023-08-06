using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper.Configuration.Attributes;

namespace CubaseDrumMapEditor.Models
{
    public class MapItem : ObservableObject
    {
        public MapItem(int iNote, int oNote, int displayNote)
        {
            _updatingINote = true;
            INoteName = MidiUtility.NoteNumberToName(iNote);
            _updatingINote = false;

            _updatingONote = true;
            ONoteName = MidiUtility.NoteNumberToName(oNote);
            _updatingONote = false;

            _updatingDisplayNote = true;
            DisplayNoteName = MidiUtility.NoteNumberToName(displayNote);
            _updatingDisplayNote = false;
        }

        private int _displayNote;
        private string? _name;
        private int _iNote;
        private int _oNote;
        private int _channel;
        private float _length;
        private int _mute;
        private int _headSymbol;
        private int _voice;
        private int _portIndex;
        private int _quantizeIndex;

        private string? _displayNoteName;
        public string? DisplayNoteName
        {
            get => _displayNoteName;
            set
            {
                if (SetProperty(ref _displayNoteName, value))
                {
                    if (!_updatingDisplayNote)
                    {
                        _updatingDisplayNote = true;
                        DisplayNote = MidiUtility.NoteNameToNumber(value!);
                        _updatingDisplayNote = false;
                    }
                }
            }
        }

        private bool _updatingDisplayNote;
        [Ignore]
        public int DisplayNote
        {
            get => _displayNote;
            set
            {
                if (_updatingDisplayNote) return;
                if (SetProperty(ref _displayNote, value))
                {
                    _updatingDisplayNote = true;
                    DisplayNoteName = MidiUtility.NoteNumberToName(value);
                    _updatingDisplayNote = false;
                }
            }
        }

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string? _iNoteName;
        public string? INoteName
        {
            get => _iNoteName;
            set
            {
                if (SetProperty(ref _iNoteName, value))
                {
                    if (!_updatingINote)
                    {
                        _updatingINote = true;
                        INote = MidiUtility.NoteNameToNumber(value!);
                        _updatingINote = false;
                    }
                }
            }
        }

        private bool _updatingINote;
        [Ignore]
        public int INote
        {
            get => _iNote;
            set
            {
                if (_updatingINote) return;
                if (SetProperty(ref _iNote, value))
                {
                    _updatingINote = true;
                    INoteName = MidiUtility.NoteNumberToName(value);
                    _updatingINote = false;
                }
            }
        }

        private string? _oNoteName;
        public string? ONoteName
        {
            get => _oNoteName;
            set
            {
                if (SetProperty(ref _oNoteName, value))
                {
                    if (!_updatingONote)
                    {
                        _updatingONote = true;
                        ONote = MidiUtility.NoteNameToNumber(value!);
                        _updatingONote = false;
                    }
                }
            }
        }

        private bool _updatingONote;
        [Ignore]
        public int ONote
        {
            get => _oNote;
            set
            {
                if (_updatingONote) return;
                if (SetProperty(ref _oNote, value))
                {
                    _updatingONote = true;
                    ONoteName = MidiUtility.NoteNumberToName(value);
                    _updatingONote = false;
                }
            }
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

        public int QuantizeIndex
        {
            get => _quantizeIndex;
            set => SetProperty(ref _quantizeIndex, value);
        }
    }
}

