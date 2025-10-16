using System;
using System.ComponentModel;
using CsvHelper.Configuration.Attributes;

namespace CubaseDrumMapEditor.Models
{
    /// <summary>
    /// Drum map row backing model. Keeps MIDI note name/number fields in sync.
    /// </summary>
    public class MapItem : INotifyPropertyChanged
    {
        private const int MinMidiNote = 0;
        private const int MaxMidiNote = 127;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _pitch;

        [Ignore]
        public int Pitch
        {
            get => _pitch;
            set
            {
                if (_pitch == value)
                {
                    OnPropertyChanged(nameof(Pitch));
                    OnPropertyChanged(nameof(PitchName));
                    return;
                }

                _pitch = value;
                OnPropertyChanged(nameof(Pitch));
                OnPropertyChanged(nameof(PitchName));
            }
        }

        public string PitchName => MidiUtility.NoteNumberToName(_pitch);

        private int _displayNote;

        [Ignore]
        public int DisplayNote
        {
            get => _displayNote;
            set => SetDisplayNote(value);
        }

        public string? DisplayNoteName
        {
            get => MidiUtility.NoteNumberToName(_displayNote);
            set
            {
                if (!TryUpdateNoteFromName(value, SetDisplayNote))
                {
                    OnPropertyChanged(nameof(DisplayNoteName));
                }
            }
        }

        public string? DisplayNoteNumber
        {
            get => _displayNote.ToString();
            set
            {
                if (!TryUpdateNoteFromNumber(value, SetDisplayNote))
                {
                    OnPropertyChanged(nameof(DisplayNoteNumber));
                }
            }
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set => SetField(ref _name, value, nameof(Name));
        }

        private string? _snap;
        public string? Snap
        {
            get => _snap;
            set => SetField(ref _snap, value, nameof(Snap));
        }

        private int _mute;
        public int Mute
        {
            get => _mute;
            set => SetField(ref _mute, value, nameof(Mute));
        }

        private int _iNote;

        [Ignore]
        public int INote
        {
            get => _iNote;
            set => SetINote(value);
        }

        public string? INoteName
        {
            get => MidiUtility.NoteNumberToName(_iNote);
            set
            {
                if (!TryUpdateNoteFromName(value, SetINote))
                {
                    OnPropertyChanged(nameof(INoteName));
                }
            }
        }

        public string? INoteNumber
        {
            get => _iNote.ToString();
            set
            {
                if (!TryUpdateNoteFromNumber(value, SetINote))
                {
                    OnPropertyChanged(nameof(INoteNumber));
                }
            }
        }

        private int _oNote;

        [Ignore]
        public int ONote
        {
            get => _oNote;
            set => SetONote(value);
        }

        public string? ONoteName
        {
            get => MidiUtility.NoteNumberToName(_oNote);
            set
            {
                if (!TryUpdateNoteFromName(value, SetONote))
                {
                    OnPropertyChanged(nameof(ONoteName));
                }
            }
        }

        public string? ONoteNumber
        {
            get => _oNote.ToString();
            set
            {
                if (!TryUpdateNoteFromNumber(value, SetONote))
                {
                    OnPropertyChanged(nameof(ONoteNumber));
                }
            }
        }

        private int _channel;
        public int Channel
        {
            get => _channel;
            set => SetField(ref _channel, value, nameof(Channel));
        }

        private string? _output;
        public string? Output
        {
            get => _output;
            set => SetField(ref _output, value, nameof(Output));
        }

        private float _length;
        public float Length
        {
            get => _length;
            set => SetField(ref _length, value, nameof(Length));
        }

        private int _headSymbol;
        public int HeadSymbol
        {
            get => _headSymbol;
            set => SetField(ref _headSymbol, value, nameof(HeadSymbol));
        }

        private int _voice;
        public int Voice
        {
            get => _voice;
            set => SetField(ref _voice, value, nameof(Voice));
        }

        private int _portIndex;
        public int PortIndex
        {
            get => _portIndex;
            set => SetField(ref _portIndex, value, nameof(PortIndex));
        }

        private int _quantizeIndex;
        public int QuantizeIndex
        {
            get => _quantizeIndex;
            set => SetField(ref _quantizeIndex, value, nameof(QuantizeIndex));
        }

        private int _noteheadSet;
        public int NoteheadSet
        {
            get => _noteheadSet;
            set => SetField(ref _noteheadSet, value, nameof(NoteheadSet));
        }

        private string? _instrumentEntityId;
        public string? InstrumentEntityId
        {
            get => _instrumentEntityId;
            set => SetField(ref _instrumentEntityId, value, nameof(InstrumentEntityId));
        }

        private string? _techniqueEntityId;
        public string? TechniqueEntityId
        {
            get => _techniqueEntityId;
            set => SetField(ref _techniqueEntityId, value, nameof(TechniqueEntityId));
        }

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetDisplayNote(int value)
        {
            value = Math.Clamp(value, MinMidiNote, MaxMidiNote);
            if (_displayNote == value)
            {
                OnPropertyChanged(nameof(DisplayNote));
                OnPropertyChanged(nameof(DisplayNoteName));
                OnPropertyChanged(nameof(DisplayNoteNumber));
                return;
            }

            _displayNote = value;
            OnPropertyChanged(nameof(DisplayNote));
            OnPropertyChanged(nameof(DisplayNoteName));
            OnPropertyChanged(nameof(DisplayNoteNumber));
        }

        private void SetINote(int value)
        {
            value = Math.Clamp(value, MinMidiNote, MaxMidiNote);
            if (_iNote == value)
            {
                OnPropertyChanged(nameof(INote));
                OnPropertyChanged(nameof(INoteName));
                OnPropertyChanged(nameof(INoteNumber));
                return;
            }

            _iNote = value;
            OnPropertyChanged(nameof(INote));
            OnPropertyChanged(nameof(INoteName));
            OnPropertyChanged(nameof(INoteNumber));
        }

        private void SetONote(int value)
        {
            value = Math.Clamp(value, MinMidiNote, MaxMidiNote);
            if (_oNote == value)
            {
                OnPropertyChanged(nameof(ONote));
                OnPropertyChanged(nameof(ONoteName));
                OnPropertyChanged(nameof(ONoteNumber));
                return;
            }

            _oNote = value;
            OnPropertyChanged(nameof(ONote));
            OnPropertyChanged(nameof(ONoteName));
            OnPropertyChanged(nameof(ONoteNumber));
        }

        private bool TryUpdateNoteFromName(string? input, Action<int> setter)
        {
            if (!TryResolveNoteNumber(input, out int noteNumber))
            {
                return false;
            }

            setter(noteNumber);
            return true;
        }

        private bool TryUpdateNoteFromNumber(string? input, Action<int> setter)
        {
            if (!TryParseNoteNumber(input, out int noteNumber))
            {
                return false;
            }

            setter(noteNumber);
            return true;
        }

        private static bool TryResolveNoteNumber(string? input, out int noteNumber)
        {
            noteNumber = 0;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var candidate = input.Trim();
            if (MidiUtility.TryParseNoteInput(candidate, out noteNumber))
            {
                return true;
            }

            var upper = candidate.ToUpperInvariant();
            if (!string.Equals(candidate, upper, StringComparison.Ordinal) &&
                MidiUtility.TryParseNoteInput(upper, out noteNumber))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseNoteNumber(string? input, out int noteNumber)
        {
            noteNumber = 0;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (!int.TryParse(input.Trim(), out noteNumber))
            {
                return false;
            }

            return noteNumber >= MinMidiNote && noteNumber <= MaxMidiNote;
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
