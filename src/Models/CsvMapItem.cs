using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubaseDrumMapEditor.Models
{
    public class CsvMapItem
    {
        // CSVから直接読み込むことができるフィールドのみを含む
        public string? DisplayNoteName { get; set; }
        public string? Name { get; set; }
        public string? INoteName { get; set; }
        public string? ONoteName { get; set; }
        public int Channel { get; set; }
        public float Length { get; set; }
        public int Mute { get; set; }
        public int HeadSymbol { get; set; }
        public int Voice { get; set; }
        public int PortIndex { get; set; }
        public int QuantizeIndex { get; set; }

    }
}
