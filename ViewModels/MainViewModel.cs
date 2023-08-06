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

    private bool _isUpdating;

    private async void HandleMapItemPropertyChangedAsync(object? sender, PropertyChangedEventArgs e)
    {
        if (_isUpdating) return; // 無限ループを防ぐためのフラグチェック

        try
        {
            if (sender is MapItem changedItem) // sender を MapItem としてキャスト
            {
                _isUpdating = true; // フラグを設定

                if (e.PropertyName == nameof(MapItem.DisplayNote))
                {
                    changedItem.DisplayNoteName = MidiUtility.NoteNumberToName(changedItem.DisplayNote);
                }
                else if (e.PropertyName == nameof(MapItem.DisplayNoteName))
                {
                    changedItem.DisplayNote = MidiUtility.NoteNameToNumber(changedItem.DisplayNoteName!);
                }
                else if (e.PropertyName == nameof(MapItem.INote))
                {
                    changedItem.INoteName = MidiUtility.NoteNumberToName(changedItem.INote);
                }
                else if (e.PropertyName == nameof(MapItem.INoteName))
                {
                    changedItem.INote = MidiUtility.NoteNameToNumber(changedItem.INoteName!);
                }
                else if (e.PropertyName == nameof(MapItem.ONote))
                {
                    changedItem.ONoteName = MidiUtility.NoteNumberToName(changedItem.ONote);
                }
                else if (e.PropertyName == nameof(MapItem.ONoteName))
                {
                    changedItem.ONote = MidiUtility.NoteNameToNumber(changedItem.ONoteName!);
                }

                _isUpdating = false; // フラグをリセット
            }
        }
        catch (Exception ex)
        {
            var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
            await cdialog.ShowAsync();
        }
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
        _isUpdating = true;

        Name = doc.Descendants("string")
                     .Where(x => (string)x.Attribute("name")! == "Name")
                     .Select(x => (string)x.Attribute("value")!)
                     .FirstOrDefault();

        var quantize = doc.Descendants("list")
                          .Where(x => (string)x.Attribute("name")! == "Quantize")
                          .Select(x => new
                          {
                              Grid = (int)x.Element("item")!.Element("int")!.Attribute("value")!,
                              Type = (int)x.Element("item")!.Elements("int").Skip(1).First().Attribute("value")!,
                              Swing = (float)x.Element("item")!.Element("float")!.Attribute("value")!,
                              Legato = (int)x.Element("item")!.Elements("int").Skip(2).First().Attribute("value")!
                          })
                          .FirstOrDefault();

        QGrid = quantize!.Grid;
        QType = quantize!.Type;
        QSwing = quantize!.Swing;
        QLegato = quantize!.Legato;

        List<int> orderValues = doc.Descendants("list")
                            .Where(x => (string)x.Attribute("name")! == "Order")
                            .SelectMany(x => x.Elements("item"))
                            .Select(x => (int)x.Attribute("value")!)
                            .ToList();
        List<MapItem> mapList = doc.Descendants("list")
                                .Where(x => (string)x.Attribute("name")! == "Map")
                                .SelectMany(x => x.Elements("item"))
                                .Select((x, i) =>
                                {
                                    var item = new MapItem();
                                    item.PropertyChanged += HandleMapItemPropertyChangedAsync;

                                    item.DisplayNote = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "DisplayNote")?.Attribute("value")!;
                                    item.DisplayNoteName = MidiUtility.NoteNumberToName((int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "DisplayNote")?.Attribute("value")!);
                                    item.INote = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "INote")?.Attribute("value")!;
                                    item.INoteName = MidiUtility.NoteNumberToName((int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "INote")?.Attribute("value")!);
                                    item.ONote = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "ONote")?.Attribute("value")!;
                                    item.ONoteName = MidiUtility.NoteNumberToName((int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "ONote")?.Attribute("value")!);
                                    item.Channel = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "Channel")?.Attribute("value")! + 1;
                                    item.Length = (float)x.Elements("float").FirstOrDefault(e => e.Attribute("name")?.Value == "Length")?.Attribute("value")!;
                                    item.Mute = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "Mute")?.Attribute("value")!;
                                    item.HeadSymbol = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "HeadSymbol")?.Attribute("value")!;
                                    item.Voice = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "Voice")?.Attribute("value")!;
                                    item.PortIndex = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "PortIndex")?.Attribute("value")!;
                                    item.Name = (string)x.Elements("string").FirstOrDefault(e => e.Attribute("name")?.Value == "Name")?.Attribute("value")!;
                                    item.QuantizeIndex = (int)x.Elements("int").FirstOrDefault(e => e.Attribute("name")?.Value == "QuantizeIndex")?.Attribute("value")!;

                                    return item;
                                })
                                .ToList();

        var outputDevices = doc.Descendants("list")
                               .Where(x => (string)x.Attribute("name")! == "OutputDevices")
                               .SelectMany(x => x.Elements("item"))
                               .Select(x => new
                               {
                                   DeviceName = (string)x.Elements("string").First().Attribute("value")!,
                                   PortName = (string)x.Elements("string").Skip(1).First().Attribute("value")!
                               })
                               .FirstOrDefault();

        DeviceName = outputDevices!.DeviceName;
        PortName = outputDevices!.PortName;

        Flags = doc.Descendants("int")
                        .Where(x => (string)x.Attribute("Flags")! == "Name")
                        .Select(x => (int)x.Attribute("value")!)
                        .FirstOrDefault();

        var sortedMapList = mapList
                            .OrderBy(item => orderValues.IndexOf(item.DisplayNote))
                            .ToList();

        SortedMapList = new ObservableCollection<MapItem>(sortedMapList);

        _isUpdating = false;
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

                // MapList
                var sortedMapList = new XElement("list", new XAttribute("name", "Map"), new XAttribute("type", "list"));
                var sortedItems = SortedMapList.OrderBy(i => i.DisplayNote); // Order by DisplayNote
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
                    sortedMapList.Add(mapItem);
                }
                drumMap.Add(sortedMapList);

                // Order
                var orderList = new XElement("list", new XAttribute("name", "Order"), new XAttribute("type", "int"));
                foreach (var displayNote in SortedMapList.Select(i => i.DisplayNote))
                {
                    orderList.Add(new XElement("item", new XAttribute("value", displayNote)));
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

        _isUpdating = true;

        if (result.Count > 0)
        {
            try
            {
                ImportDrumMap(result[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                _isUpdating = false;
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }

        _isUpdating = false;
    }

    public void ImportDrumMap(string localPath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,  // ヘッダー行を読み飛ばす
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var reader = new StreamReader(localPath))
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

            // 3行目のヘッダーを読み込み
            csv.Read();

            // 4行目以降を読み込み、MapItemリストに割り当て
            var csvMapItems = csv.GetRecords<CsvMapItem>().ToList();

            // CsvMapItemのリストをループして、各CsvMapItemをMapItemに変換
            var mapItems = new List<MapItem>();
            foreach (var csvMapItem in csvMapItems)
            {
                var mapItem = new MapItem()
                {
                    DisplayNote = MidiUtility.NoteNameToNumber(csvMapItem.DisplayNoteName!),
                    DisplayNoteName = csvMapItem.DisplayNoteName!,
                    Name = csvMapItem.Name,
                    INote = MidiUtility.NoteNameToNumber(csvMapItem.INoteName!),
                    INoteName = csvMapItem.INoteName,
                    ONote = MidiUtility.NoteNameToNumber(csvMapItem.ONoteName!),
                    ONoteName = csvMapItem.ONoteName!,
                    Channel = csvMapItem.Channel,
                    Length = csvMapItem.Length,
                    Mute = csvMapItem.Mute,
                    HeadSymbol = csvMapItem.HeadSymbol,
                    Voice = csvMapItem.Voice,
                    PortIndex = csvMapItem.PortIndex,
                    QuantizeIndex = csvMapItem.QuantizeIndex
                };

                mapItem.PropertyChanged += HandleMapItemPropertyChangedAsync;

                mapItems.Add(mapItem);
            }
            SortedMapList = new ObservableCollection<MapItem>(mapItems);
        }
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
                    // Write header
                    csv.WriteHeader<DrumMapHeader>();
                    csv.NextRecord();

                    // Write values
                    var header = new DrumMapHeader
                    {
                        Name = Name,
                        QGrid = QGrid,
                        QType = QType,
                        QSwing = QSwing,
                        QLegato = QLegato,
                        DeviceName = DeviceName,
                        PortName = PortName,
                        Flags = Flags
                    };
                    csv.WriteRecord(header);
                    csv.NextRecord();

                    csv.WriteHeader<DrumMapItemsHeader>();
                    csv.NextRecord();

                    // Write MapItem
                    csv.WriteRecords(SortedMapList);
                }

                var cdialog = new ContentDialog() { Title = "Information", Content = $"Successfully exported CSV file.\n\n*Note:\n1.Do not edit the DisplayNote column (First column).\n2.Do not rearrange the order of columns.\n3.Do not delete or add rows.", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = "Error", Content = $"{ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
            }
        }
    }
}
