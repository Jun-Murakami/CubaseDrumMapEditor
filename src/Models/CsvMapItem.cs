using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace CubaseDrumMapEditor.Models
{
    public class CsvMapItem
    {
        // CSVから直接読み込むことができるフィールドのみを含む
        // LibreOffice Calc / OpenOffice Calcで空のセルが空文字列として保存される問題に対応するため、
        // 数値フィールドをnullable型に変更し、カスタムコンバーターを使用
        
        // Pitch: 絶対的なユニーク値（読み取り専用）
        public string? PitchName { get; set; }
        
        // Drum Sound: ドラムサウンド名
        public string? Name { get; set; }
        
        // Snap: クオンタイズ設定
        public string? Snap { get; set; }
        
        // Mute: ミュート設定
        [TypeConverter(typeof(NullableIntConverter))]
        public int Mute { get; set; }
        
        // I-Note: 入力音符
        public string? INoteName { get; set; }
        
        // O-Note: 出力音符
        public string? ONoteName { get; set; }
        
        // Channel: MIDIチャンネル
        [TypeConverter(typeof(NullableIntConverter))]
        public int Channel { get; set; }
        
        // Output: 出力先
        public string? Output { get; set; }
        
        // Display Note: 表示用音符（重複可能）
        public string? DisplayNoteName { get; set; }
        
        // Note Head: 符頭記号
        [TypeConverter(typeof(NullableIntConverter))]
        public int HeadSymbol { get; set; }
        
        // Voice: 声部
        [TypeConverter(typeof(NullableIntConverter))]
        public int Voice { get; set; }
        
        // Instrument Type: 楽器タイプ
        public string? InstrumentEntityId { get; set; }
        
        // Playing Technique: 演奏技法
        public string? TechniqueEntityId { get; set; }
        
        // 内部項目（非表示）
        [TypeConverter(typeof(NullableFloatConverter))]
        public float Length { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int PortIndex { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int QuantizeIndex { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int NoteheadSet { get; set; }
    }

    /// <summary>
    /// Nullable Int converter that handles empty strings by returning default value
    /// LibreOffice Calc / OpenOffice Calcで空のセルが空文字列として保存される問題に対応
    /// </summary>
    public class NullableIntConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0; // デフォルト値として0を返す
            }

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }

            return 0; // パースに失敗した場合もデフォルト値を返す
        }
    }

    /// <summary>
    /// Nullable Float converter that handles empty strings by returning default value
    /// LibreOffice Calc / OpenOffice Calcで空のセルが空文字列として保存される問題に対応
    /// </summary>
    public class NullableFloatConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0.0f; // デフォルト値として0.0fを返す
            }

            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            return 0.0f; // パースに失敗した場合もデフォルト値を返す
        }
    }
}
