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
        public string? DisplayNoteName { get; set; }
        public string? Name { get; set; }
        public string? INoteName { get; set; }
        public string? ONoteName { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int Channel { get; set; }
        
        [TypeConverter(typeof(NullableFloatConverter))]
        public float Length { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int Mute { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int HeadSymbol { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int Voice { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int PortIndex { get; set; }
        
        [TypeConverter(typeof(NullableIntConverter))]
        public int QuantizeIndex { get; set; }

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
