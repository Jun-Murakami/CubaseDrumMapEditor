using System;

namespace CubaseDrumMapEditor.Models
{
    public static class MidiUtility
    {
        private static readonly string[] NoteNames =
        {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

        public static string NoteNumberToName(int noteNumber)
        {
            var octave = noteNumber / 12 - 2;
            var noteName = NoteNames[noteNumber % 12];
            return noteName + octave;
        }

        public static int NoteNameToNumber(string noteName)
        {
            var note = noteName.Substring(0, noteName.Length - 1);
            var octave = int.Parse(noteName[^1].ToString()) + 2;
            var noteNumber = Array.IndexOf(NoteNames, note) + octave * 12;
            return noteNumber;
        }
    }
}
