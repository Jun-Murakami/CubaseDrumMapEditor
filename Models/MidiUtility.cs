using System;
using System.Diagnostics;

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
            if (noteNumber == 0)
            {
                return "C-2";
            }

            var octave = noteNumber / 12 - 2;
            Debug.WriteLine(noteNumber);
            var noteName = NoteNames[noteNumber % 12];
            return noteName + octave;
        }

        public static int NoteNameToNumber(string noteName)
        {
            if (noteName == "C-2")
            {
                return 0;
            }

            var noteAndOctave = noteName.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var note = noteAndOctave[0];

            // Octave can be negative, so we should consider this case.
            int octave;
            if (noteName.Contains("-"))
            {
                octave = int.Parse(noteAndOctave[1]) - 2;
            }
            else
            {
                octave = int.Parse(noteName[^1..]) + 2;
            }

            var noteNumber = Array.IndexOf(NoteNames, note) + octave * 12;
            return noteNumber;
        }
    }
}
