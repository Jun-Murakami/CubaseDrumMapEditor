using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using FluentAvalonia.UI.Controls;
using CubaseDrumMapEditor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Diagnostics;
using Avalonia.Platform;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using System.ComponentModel;
using System.Text;
using Avalonia.Controls;

namespace CubaseDrumMapEditor.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        NewDrumMapCommand = new AsyncRelayCommand(NewDrumMapAsync);
        OpenDrumMapCommand = new AsyncRelayCommand(OpenDrumMapAsync);
        SaveDrumMapCommand = new AsyncRelayCommand(SaveDrumMapAsync);

        ImportDrumMapCommand = new AsyncRelayCommand(ImportDrumMapAsync);
        ExportDrumMapCommand = new AsyncRelayCommand(ExportDrumMapAsync);

        MoveUpCommand = new RelayCommand(MoveUp);
        MoveDownCommand = new RelayCommand(MoveDown);
    }

    public IAsyncRelayCommand NewDrumMapCommand { get; }
    public IAsyncRelayCommand OpenDrumMapCommand { get; }
    public IAsyncRelayCommand SaveDrumMapCommand { get; }
    public IAsyncRelayCommand ImportDrumMapCommand { get; }
    public IAsyncRelayCommand ExportDrumMapCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    private MapItem? _selectedMapItem;
    public MapItem? SelectedMapItem
    {
        get => _selectedMapItem;
        set => SetProperty(ref _selectedMapItem, value);
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _qGrid;
    public int QGrid
    {
        get => _qGrid;
        set => SetProperty(ref _qGrid, value);
    }

    private int _qType;
    public int QType
    {
        get => _qType;
        set => SetProperty(ref _qType, value);
    }

    private float _qSwing;
    public float QSwing
    {
        get => _qSwing;
        set => SetProperty(ref _qSwing, value);
    }

    private int _qLegato;
    public int QLegato
    {
        get => _qLegato;
        set => SetProperty(ref _qLegato, value);
    }

    private string? _deviceName;
    public string? DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    private string? _portName;
    public string? PortName
    {
        get => _portName;
        set => SetProperty(ref _portName, value);
    }

    private int _flags;
    public int Flags
    {
        get => _flags;
        set => SetProperty(ref _flags, value);
    }

    private List<int>? _order;
    public List<int>? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    private List<MapItem>? _mapList;
    public List<MapItem>? MapList
    {
        get => _mapList;
        set => SetProperty(ref _mapList, value);
    }

    private ObservableCollection<MapItem>? _sortedMapList;
    public ObservableCollection<MapItem>? SortedMapList
    {
        get => _sortedMapList;
        set => SetProperty(ref _sortedMapList, value);
    }

    private void MoveUp()
    {
        if (SelectedMapItem == null) return;
        var selectedIndex = SortedMapList!.IndexOf(SelectedMapItem);
        if (selectedIndex <= 0) return;

        var itemToMove = SortedMapList[selectedIndex];
        SortedMapList.RemoveAt(selectedIndex);
        SortedMapList.Insert(selectedIndex - 1, itemToMove);
        SelectedMapItem = itemToMove;
    }

    private void MoveDown()
    {
        if (SelectedMapItem == null) return;
        var selectedIndex = SortedMapList!.IndexOf(SelectedMapItem);
        if (selectedIndex >= SortedMapList.Count - 1) return;

        var itemToMove = SortedMapList[selectedIndex];
        SortedMapList.RemoveAt(selectedIndex);
        SortedMapList.Insert(selectedIndex + 1, itemToMove);
        SelectedMapItem = itemToMove;
    }

    private async Task NewDrumMapAsync()
    {
        try
        {
            XDocument doc = XDocument.Load(AssetLoader.Open(new Uri("avares://CubaseDrumMapEditor/Assets/EmptyMap.drm")));
            LoadDrumMap(doc);
        }
        catch (Exception ex)
        {
            var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
            await cdialog.ShowAsync();
        }
    }

    private async Task OpenDrumMapAsync()
    {
        var dialog = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select Cubase DrumMap file",
            FileTypeFilter = new List<FilePickerFileType>
                    {new("Cubase DrumMap files (*.drm)") { Patterns = new[] { "*.drm" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
        };
        var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.OpenFilePickerAsync(dialog);

        if (result.Count > 0)
        {
            try
            {
                XDocument doc = XDocument.Load(result[0].Path.LocalPath);
                LoadDrumMap(doc);
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }
    }

    private void LoadDrumMap(XDocument doc)
    {

        Name = doc.Descendants("string")
                     .Where(x => GetStringValue(x, "name") == "Name")
                     .Select(x => GetStringValue(x, "value", ""))
                     .FirstOrDefault() ?? "";

        var quantize = doc.Descendants("list")
                          .Where(x => GetStringValue(x, "name") == "Quantize")
                          .Select(x => new
                          {
                              Grid = GetIntValue(x.Element("item")?.Element("int"), "value", 4),
                              Type = GetIntValue(x.Element("item")?.Elements("int").Skip(1).FirstOrDefault(), "value", 0),
                              Swing = GetFloatValue(x.Element("item")?.Element("float"), "value", 0.0f),
                              Legato = GetIntValue(x.Element("item")?.Elements("int").Skip(2).FirstOrDefault(), "value", 50)
                          })
                          .FirstOrDefault();

        QGrid = quantize?.Grid ?? 4;
        QType = quantize?.Type ?? 0;
        QSwing = quantize?.Swing ?? 0.0f;
        QLegato = quantize?.Legato ?? 50;

        List<int> orderValues = doc.Descendants("list")
                            .Where(x => GetStringValue(x, "name") == "Order")
                            .SelectMany(x => x.Elements("item"))
                            .Select(x => GetIntValue(x, "value", 0))
                            .ToList();
        List<MapItem> mapList = doc.Descendants("list")
                                .Where(x => GetStringValue(x, "name") == "Map")
                                .SelectMany(x => x.Elements("item"))
                                .Select((x, i) =>
                                {
                                    var item = new MapItem();

                                    // Pitch: 配列のインデックス（0-127）をPitchとして設定
                                    item.Pitch = i;
                                    
                                    // DisplayNote: XMLから読み込み（安全なパース）
                                    item.DisplayNote = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "DisplayNote"), "value", 0);
                                    item.DisplayNoteName = MidiUtility.NoteNumberToName(item.DisplayNote);
                                    item.DisplayNoteNumber = item.DisplayNote.ToString();
                                    
                                    // ドラムサウンド名
                                    item.Name = GetStringValue(x.Elements("string").FirstOrDefault(e => GetStringValue(e, "name") == "Name"), "value", "");
                                    
                                    // Snap: デフォルト値を設定
                                    item.Snap = "1/16";
                                    
                                    // Mute
                                    item.Mute = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "Mute"), "value", 0);
                                    
                                    // I-Note
                                    item.INote = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "INote"), "value", 0);
                                    item.INoteName = MidiUtility.NoteNumberToName(item.INote);
                                    item.INoteNumber = item.INote.ToString();
                                    
                                    // O-Note
                                    item.ONote = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "ONote"), "value", 0);
                                    item.ONoteName = MidiUtility.NoteNumberToName(item.ONote);
                                    item.ONoteNumber = item.ONote.ToString();
                                    
                                    // Channel
                                    item.Channel = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "Channel"), "value", 0) + 1;
                                    
                                    // Output: デフォルト値を設定
                                    item.Output = "Track";
                                    
                                    // Length
                                    item.Length = GetFloatValue(x.Elements("float").FirstOrDefault(e => GetStringValue(e, "name") == "Length"), "value", 200.0f);
                                    
                                    // HeadSymbol
                                    item.HeadSymbol = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "HeadSymbol"), "value", 0);
                                    
                                    // Voice
                                    item.Voice = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "Voice"), "value", 0);
                                    
                                    // PortIndex
                                    item.PortIndex = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "PortIndex"), "value", 0);
                                    
                                    // QuantizeIndex
                                    item.QuantizeIndex = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "QuantizeIndex"), "value", 0);
                                    
                                    // NoteheadSet
                                    item.NoteheadSet = GetIntValue(x.Elements("int").FirstOrDefault(e => GetStringValue(e, "name") == "NoteheadSet"), "value", 0);
                                    
                                    // InstrumentEntityID
                                    item.InstrumentEntityId = GetStringValue(x.Elements("string").FirstOrDefault(e => GetStringValue(e, "name") == "InstrumentEntityID"), "value", "");
                                    
                                    // TechniqueEntityID
                                    item.TechniqueEntityId = GetStringValue(x.Elements("string").FirstOrDefault(e => GetStringValue(e, "name") == "TechniqueEntityID"), "value", "");

                                    return item;
                                })
                                .ToList();

        var outputDevices = doc.Descendants("list")
                               .Where(x => GetStringValue(x, "name") == "OutputDevices")
                               .SelectMany(x => x.Elements("item"))
                               .Select(x => new
                               {
                                   DeviceName = GetStringValue(x.Elements("string").FirstOrDefault(), "value", "Default Device"),
                                   PortName = GetStringValue(x.Elements("string").Skip(1).FirstOrDefault(), "value", "Default Port")
                               })
                               .FirstOrDefault();

        DeviceName = outputDevices?.DeviceName ?? "Default Device";
        PortName = outputDevices?.PortName ?? "Default Port";

        Flags = GetIntValue(doc.Descendants("int").FirstOrDefault(x => GetStringValue(x, "name") == "Flags"), "value", 0);

        // Orderリストに基づいてソート（Pitchベース）
        var sortedMapList = mapList
                            .OrderBy(item => orderValues.IndexOf(item.Pitch))
                            .ToList();

        SortedMapList = new ObservableCollection<MapItem>(sortedMapList);
    }

    private async Task SaveDrumMapAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || SortedMapList == null || SortedMapList.Count < 127)
        {
            return;
        }

        var dialog = new FilePickerSaveOptions
        {
            Title = "Export Cubase DrumMap file",
            FileTypeChoices = new List<FilePickerFileType>
                    {new("Cubase DrumMap file (*.drm)") { Patterns = new[] { "*.drm" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
        };

        var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.SaveFilePickerAsync(dialog);

        if (result != null)
        {
            var selectedFilePath = result.Path.LocalPath;
            string extension = Path.GetExtension(selectedFilePath);
            if (string.IsNullOrEmpty(extension))
            {
                selectedFilePath += ".drm";
            }

            try
            {
                var drumMap = new XElement("DrumMap");

                // Simple properties
                drumMap.Add(new XElement("string", new XAttribute("name", "Name"), new XAttribute("value", Name), new XAttribute("wide", "true")));

                // More complex properties
                var quantize = new XElement("list", new XAttribute("name", "Quantize"), new XAttribute("type", "list"));
                var quantizeItem = new XElement("item");
                quantizeItem.Add(new XElement("int", new XAttribute("name", "Grid"), new XAttribute("value", QGrid)));
                quantizeItem.Add(new XElement("int", new XAttribute("name", "Type"), new XAttribute("value", QType)));
                quantizeItem.Add(new XElement("float", new XAttribute("name", "Swing"), new XAttribute("value", QSwing)));
                quantizeItem.Add(new XElement("int", new XAttribute("name", "Legato"), new XAttribute("value", QLegato)));
                quantize.Add(quantizeItem);
                drumMap.Add(quantize);

                // MapList: Pitch順（0-127）で保存
                var sortedMapList = new XElement("list", new XAttribute("name", "Map"), new XAttribute("type", "list"));
                var sortedItems = SortedMapList.OrderBy(i => i.Pitch); // Order by Pitch
                foreach (var item in sortedItems)
                {
                    var mapItem = new XElement("item");
                    mapItem.Add(new XElement("int", new XAttribute("name", "INote"), new XAttribute("value", item.INote)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "ONote"), new XAttribute("value", item.ONote)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "Channel"), new XAttribute("value", item.Channel - 1)));
                    mapItem.Add(new XElement("float", new XAttribute("name", "Length"), new XAttribute("value", item.Length)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "Mute"), new XAttribute("value", item.Mute)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "DisplayNote"), new XAttribute("value", item.DisplayNote)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "HeadSymbol"), new XAttribute("value", item.HeadSymbol)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "Voice"), new XAttribute("value", item.Voice)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "PortIndex"), new XAttribute("value", item.PortIndex)));
                    mapItem.Add(new XElement("string", new XAttribute("name", "Name"), new XAttribute("value", item.Name ?? ""), new XAttribute("wide", "true")));
                    mapItem.Add(new XElement("int", new XAttribute("name", "QuantizeIndex"), new XAttribute("value", item.QuantizeIndex)));
                    mapItem.Add(new XElement("int", new XAttribute("name", "NoteheadSet"), new XAttribute("value", item.NoteheadSet)));
                    mapItem.Add(new XElement("string", new XAttribute("name", "InstrumentEntityID"), new XAttribute("value", item.InstrumentEntityId ?? "")));
                    mapItem.Add(new XElement("string", new XAttribute("name", "TechniqueEntityID"), new XAttribute("value", item.TechniqueEntityId ?? "")));
                    sortedMapList.Add(mapItem);
                }
                drumMap.Add(sortedMapList);

                // Order: Pitchベースで保存
                var orderList = new XElement("list", new XAttribute("name", "Order"), new XAttribute("type", "int"));
                foreach (var pitch in SortedMapList.Select(i => i.Pitch))
                {
                    orderList.Add(new XElement("item", new XAttribute("value", pitch)));
                }
                drumMap.Add(orderList);

                // outputDevices
                var outputDevices = new XElement("list", new XAttribute("name", "OutputDevices"), new XAttribute("type", "list"));
                var outputDevicesItem = new XElement("item");
                outputDevicesItem.Add(new XElement("string", new XAttribute("name", "DeviceName"), new XAttribute("value", DeviceName!)));
                outputDevicesItem.Add(new XElement("string", new XAttribute("name", "PortName"), new XAttribute("value", PortName!)));
                outputDevices.Add(outputDevicesItem);
                drumMap.Add(outputDevices);

                // Flags
                drumMap.Add(new XElement("int", new XAttribute("name", "Flags"), new XAttribute("value", Flags)));

                // Save to file
                var doc = new XDocument(drumMap);
                doc.Save(selectedFilePath);

                var cdialog = new ContentDialog() { Title = "Information", Content = $"Successfully exported DrumMap file.", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }
    }

    private async Task ImportDrumMapAsync()
    {
        var dialog = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select CSV file",
            FileTypeFilter = new List<FilePickerFileType>
                    {new("CSV files (*.csv)") { Patterns = new[] { "*.csv" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
        };

        var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.OpenFilePickerAsync(dialog);

        if (result.Count > 0)
        {
            try
            {
                ImportDrumMap(result[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }
    }

    public void ImportDrumMap(string localPath)
    {
        string fileContent;
        try
        {
            using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            fileContent = reader.ReadToEnd();
        }
        catch (IOException ex)
        {
            throw new IOException($"Failed to read CSV file. Close other applications that might be locking it and try again.\n{localPath}", ex);
        }

        var delimiter = DetectCsvDelimiter(fileContent);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            HeaderValidated = null,
            MissingFieldFound = null,
            Delimiter = delimiter
        };

        using (var reader = new StringReader(fileContent))
        using (var csv = new CsvReader(reader, config))
        {
            // 1行目のヘッダーを読み込み
            csv.Read();

            // 2行目を読み込み、各プロパティに割り当て
            csv.Read();
            Name = csv.GetField<string>(0);
            QGrid = csv.GetField<int>(1);
            QType = csv.GetField<int>(2);
            QSwing = csv.GetField<float>(3);
            QLegato = csv.GetField<int>(4);
            DeviceName = csv.GetField<string>(5);
            PortName = csv.GetField<string>(6);
            Flags = csv.GetField<int>(7);

            // 3行目のヘッダーを手動で読み込み
            csv.Read();
            var headerRow = new string[20]; // 十分なサイズを確保
            for (int i = 0; i < headerRow.Length; i++)
            {
                try
                {
                    headerRow[i] = csv.GetField<string>(i) ?? "";
                }
                catch
                {
                    break; // フィールドが存在しない場合は終了
                }
            }
            
            // ヘッダーに基づいてフィールドインデックスを取得
            var fieldIndices = new Dictionary<string, int>();
            for (int i = 0; i < headerRow.Length; i++)
            {
                if (!string.IsNullOrEmpty(headerRow[i]))
                {
                    fieldIndices[headerRow[i]] = i;
                }
            }

            // 4行目以降を読み込み、動的にMapItemリストに変換
            var mapItems = new List<MapItem>();
            int rowIndex = 0;
            
            while (csv.Read())
            {
                var mapItem = new MapItem()
                {
                    // Pitch: 配列のインデックス（0-127）をPitchとして設定
                    Pitch = rowIndex,
                    
                    // 各フィールドを動的に読み込み（存在しない場合はデフォルト値）
                    DisplayNote = GetFieldValue(csv, fieldIndices, "DisplayNoteName", s => MidiUtility.TryParseNoteInput(s ?? "", out int displayNote) ? displayNote : 0),
                    DisplayNoteName = GetFieldValue(csv, fieldIndices, "DisplayNoteName", s => s ?? MidiUtility.NoteNumberToName(0)),
                    
                    Name = GetFieldValue(csv, fieldIndices, "Name", s => s),
                    
                    Snap = GetFieldValue(csv, fieldIndices, "Snap", s => s ?? "1/16"),
                    
                    Mute = GetFieldValue(csv, fieldIndices, "Mute", s => int.TryParse(s, out int mute) ? mute : 0),
                    
                    INote = GetFieldValue(csv, fieldIndices, "INoteName", s => MidiUtility.TryParseNoteInput(s ?? "", out int iNote) ? iNote : 0),
                    INoteName = GetFieldValue(csv, fieldIndices, "INoteName", s => s ?? MidiUtility.NoteNumberToName(0)),
                    
                    ONote = GetFieldValue(csv, fieldIndices, "ONoteName", s => MidiUtility.TryParseNoteInput(s ?? "", out int oNote) ? oNote : 0),
                    ONoteName = GetFieldValue(csv, fieldIndices, "ONoteName", s => s ?? MidiUtility.NoteNumberToName(0)),
                    
                    Channel = GetFieldValue(csv, fieldIndices, "Channel", s => int.TryParse(s, out int channel) ? channel : 1),
                    
                    Output = GetFieldValue(csv, fieldIndices, "Output", s => s ?? "Track"),
                    
                    HeadSymbol = GetFieldValue(csv, fieldIndices, "HeadSymbol", s => int.TryParse(s, out int headSymbol) ? headSymbol : 0),
                    
                    Voice = GetFieldValue(csv, fieldIndices, "Voice", s => int.TryParse(s, out int voice) ? voice : 0),
                    
                    InstrumentEntityId = GetFieldValue(csv, fieldIndices, "InstrumentEntityId", s => s),
                    
                    TechniqueEntityId = GetFieldValue(csv, fieldIndices, "TechniqueEntityId", s => s),
                    
                    // 内部項目
                    Length = GetFieldValue(csv, fieldIndices, "Length", s => float.TryParse(s, out float length) ? length : 0.0f),
                    PortIndex = GetFieldValue(csv, fieldIndices, "PortIndex", s => int.TryParse(s, out int portIndex) ? portIndex : 0),
                    QuantizeIndex = GetFieldValue(csv, fieldIndices, "QuantizeIndex", s => int.TryParse(s, out int quantizeIndex) ? quantizeIndex : 0),
                    NoteheadSet = GetFieldValue(csv, fieldIndices, "NoteheadSet", s => int.TryParse(s, out int noteheadSet) ? noteheadSet : 0)
                };

                mapItems.Add(mapItem);
                rowIndex++;
            }
            SortedMapList = new ObservableCollection<MapItem>(mapItems);
        }
    }

    // ヘルパーメソッド：フィールド値を安全に取得
    private static T GetFieldValue<T>(CsvReader csv, Dictionary<string, int> fieldIndices, string fieldName, Func<string?, T> converter, T defaultValue = default(T))
    {
        if (fieldIndices.TryGetValue(fieldName, out int index))
        {
            try
            {
                var value = csv.GetField<string>(index);
                return converter(value);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    // 安全なXMLパース用ヘルパーメソッド
    private static string GetStringValue(XElement? element, string attributeName, string defaultValue = "")
    {
        if (element?.Attribute(attributeName)?.Value != null)
        {
            return element.Attribute(attributeName)!.Value;
        }
        return defaultValue;
    }

    private static int GetIntValue(XElement? element, string attributeName, int defaultValue = 0)
    {
        if (element?.Attribute(attributeName)?.Value != null)
        {
            if (int.TryParse(element.Attribute(attributeName)!.Value, out int result))
            {
                return result;
            }
        }
        return defaultValue;
    }

    private static float GetFloatValue(XElement? element, string attributeName, float defaultValue = 0.0f)
    {
        if (element?.Attribute(attributeName)?.Value != null)
        {
            if (float.TryParse(element.Attribute(attributeName)!.Value, out float result))
            {
                return result;
            }
        }
        return defaultValue;
    }

    private static string DetectCsvDelimiter(string content)
    {
        var candidates = new[] { ',', ';', '\t' };
        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        var bestDelimiter = ',';
        var bestScore = -1;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            foreach (var candidate in candidates)
            {
                var score = line.Count(ch => ch == candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDelimiter = candidate;
                }
            }

            if (bestScore > 0)
            {
                break; // Stop after finding the first obvious delimiter
            }
        }

        return bestScore > 0 ? bestDelimiter.ToString() : ",";
    }
    private async Task ExportDrumMapAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || SortedMapList == null || SortedMapList.Count < 127)
        {
            return;
        }

        var dialog = new FilePickerSaveOptions
        {
            Title = "Export CSV file",
            FileTypeChoices = new List<FilePickerFileType>
                    {new("CSV file (*.csv)") { Patterns = new[] { "*.csv" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
        };

        var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.SaveFilePickerAsync(dialog);

        if (result != null)
        {
            var selectedFilePath = result.Path.LocalPath;
            string extension = Path.GetExtension(selectedFilePath);
            if (string.IsNullOrEmpty(extension))
            {
                selectedFilePath += ".csv";
            }

            try
            {
                using (var writer = new StreamWriter(selectedFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // 1行目: グローバルヘッダー
                    csv.WriteField("Name");
                    csv.WriteField("QGrid");
                    csv.WriteField("QType");
                    csv.WriteField("QSwing");
                    csv.WriteField("QLegato");
                    csv.WriteField("DeviceName");
                    csv.WriteField("PortName");
                    csv.WriteField("Flags");
                    csv.NextRecord();

                    // 2行目: グローバル値
                    csv.WriteField(Name ?? "");
                    csv.WriteField(QGrid);
                    csv.WriteField(QType);
                    csv.WriteField(QSwing);
                    csv.WriteField(QLegato);
                    csv.WriteField(DeviceName ?? "");
                    csv.WriteField(PortName ?? "");
                    csv.WriteField(Flags);
                    csv.NextRecord();

                    // 3行目: MapItemヘッダー
                    csv.WriteHeader<CsvMapItem>();
                    csv.NextRecord();

                    // 4行目以降: MapItemデータをCsvMapItemに変換して書き出し
                    foreach (var mapItem in SortedMapList)
                    {
                        var csvItem = new CsvMapItem
                        {
                            PitchName = mapItem.PitchName,
                            Name = mapItem.Name,
                            Snap = mapItem.Snap,
                            Mute = mapItem.Mute,
                            INoteName = mapItem.INoteName,
                            ONoteName = mapItem.ONoteName,
                            Channel = mapItem.Channel,
                            Output = mapItem.Output,
                            DisplayNoteName = mapItem.DisplayNoteName,
                            HeadSymbol = mapItem.HeadSymbol,
                            Voice = mapItem.Voice,
                            InstrumentEntityId = mapItem.InstrumentEntityId,
                            TechniqueEntityId = mapItem.TechniqueEntityId,
                            Length = mapItem.Length,
                            PortIndex = mapItem.PortIndex,
                            QuantizeIndex = mapItem.QuantizeIndex,
                            NoteheadSet = mapItem.NoteheadSet
                        };
                        csv.WriteRecord(csvItem);
                        csv.NextRecord();
                    }
                }

                var cdialog = new ContentDialog() { Title = "Information", Content = $"Successfully exported CSV file.\n\n*Note:\n1.Do not edit the Pitch column (First column).\n2.Do not rearrange the order of columns.\n3.Do not delete or add rows.", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }
    }

    public void ProcessDroppedFile(string filePath)
    {
        if (Path.GetExtension(filePath) == ".drm")
        {
            XDocument doc = XDocument.Load(filePath);
            LoadDrumMap(doc);
        }
        else if (Path.GetExtension(filePath) == ".csv")
        {
            ImportDrumMap(filePath);
        }
    }
}








