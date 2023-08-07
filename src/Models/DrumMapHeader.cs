namespace CubaseDrumMapEditor.Models
{
    public class DrumMapHeader
    {
        public string? Name { get; set; }
        public int QGrid { get; set; }
        public int QType { get; set; }
        public float QSwing { get; set; }
        public int QLegato { get; set; }
        public string? DeviceName { get; set; }
        public string? PortName { get; set; }
        public int Flags { get; set; }
    }

    public class DrumMapItemsHeader
    {
        public string? Display { get; set; }
        public string? Name { get; set; }
        public string? In { get; set; }
        public string? Out { get; set; }
        public int Channel { get; set; }
        public int Length { get; set; }
        public int Mute { get; set; }
        public int HeadSymbol { get; set; }
        public int Voice { get; set; }
        public int PortIndex { get; set; }
        public int QuantizeIndex { get; set; }
    }
}
