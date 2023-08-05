using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using FluentAvalonia.UI.Controls;
using CubaseDrumMapEditor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Xml;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace CubaseDrumMapEditor.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        OpenDrumMapCommand = new AsyncRelayCommand(OpenDrumMapAsync);
        MoveUpCommand = new RelayCommand(MoveUp);
        MoveDownCommand = new RelayCommand(MoveDown);
    }

    public IAsyncRelayCommand OpenDrumMapCommand { get; }
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
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;

                using (XmlReader reader = XmlReader.Create(result[0].Path.LocalPath, settings))
                {
                    reader.MoveToContent();
                    if (reader.Name == "DrumMap")
                    {
                        await ParseDrumMapAsync(reader);
                    }
                }

                SortedMapList = new ObservableCollection<MapItem>(MapList!.OrderBy(item => Order!.IndexOf(item.DisplayNote)).ToList());
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = $"Error: {ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
                throw;
            }
        }
    }

    private async Task ParseDrumMapAsync(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("DrumMap");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "string":
                            if (reader.GetAttribute("name") == "Name")
                            {
                                Name = reader.GetAttribute("value");
                            }
                            break;
                        case "list":
                            if (reader.GetAttribute("name") == "Map")
                            {
                                MapList = await ParseMapAsync(reader);
                            }
                            else if (reader.GetAttribute("name") == "Quantize")
                            {
                                await ParseQuantizeAsync(reader);
                            }
                            else if (reader.GetAttribute("name") == "Order")
                            {
                                Order = await ParseOrderAsync(reader);
                            }
                            else if (reader.GetAttribute("name") == "OutputDevices")
                            {
                                await ParseOutputDevicesAsync(reader);
                            }
                            break;
                        case "int":
                            if (reader.GetAttribute("name") == "Flags")
                            {
                                //Flags = reader.GetAttribute("value");
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "DrumMap")
                {
                    return;  // DrumMapエレメントの終了
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<List<MapItem>> ParseMapAsync(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("list");
            List<MapItem> items = new List<MapItem>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                {
                    MapItem item = new MapItem();
                    reader.ReadStartElement("item");
                    while (await reader.ReadAsync())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "int":
                                    if (reader.GetAttribute("name") == "INote")
                                    {
                                        item.INote = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "ONote")
                                    {
                                        item.ONote = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "Channel")
                                    {
                                        item.Channel = int.Parse(reader.GetAttribute("value")!)+1;
                                    }
                                    else if (reader.GetAttribute("name") == "Mute")
                                    {
                                        item.Mute = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "DisplayNote")
                                    {
                                        item.DisplayNote = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "HeadSymbol")
                                    {
                                        item.HeadSymbol = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "Voice")
                                    {
                                        item.Voice = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "PortIndex")
                                    {
                                        item.PortIndex = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "QuantizeIndex")
                                    {
                                        item.QuantizeIndex = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    break;
                                case "float":
                                    if (reader.GetAttribute("name") == "Length")
                                    {
                                        item.Length = float.Parse(reader.GetAttribute("value")!);
                                    }
                                    break;
                                case "string":
                                    if (reader.GetAttribute("name") == "Name")
                                    {
                                        item.Name = reader.GetAttribute("value");
                                    }
                                    break;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "item")
                        {
                            break;  // Itemエレメントの終了
                        }
                    }
                    items.Add(item);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "list")
                {
                    return items;  // Listエレメントの終了
                }
            }
            return items;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<List<int>> ParseOrderAsync(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("list");
            List<int> items = new List<int>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                {
                    items.Add(int.Parse(reader.GetAttribute("value")!));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "list")
                {
                    return items;  // Listエレメントの終了
                }
            }
            return items;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task ParseQuantizeAsync(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("list");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                {
                    reader.ReadStartElement("item");
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "int":
                                    if (reader.GetAttribute("name") == "Grid")
                                    {
                                        QGrid = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "Type")
                                    {
                                        QType = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    else if (reader.GetAttribute("name") == "Legato")
                                    {
                                        QLegato = int.Parse(reader.GetAttribute("value")!);
                                    }
                                    break;
                                case "float":
                                    if (reader.GetAttribute("name") == "Swing")
                                    {
                                        QSwing = float.Parse(reader.GetAttribute("value")!);
                                    }
                                    break;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "item")
                        {
                            return;  // Itemエレメントの終了
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "list")
                {
                    return;  // Listエレメントの終了
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task ParseOutputDevicesAsync(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("list");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                {
                    reader.ReadStartElement("item");
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "string":
                                    if (reader.GetAttribute("name") == "DeviceName")
                                    {
                                        DeviceName = reader.GetAttribute("value")!;
                                    }
                                    else if (reader.GetAttribute("name") == "PortName")
                                    {
                                        PortName = reader.GetAttribute("value")!;
                                    }
                                    break;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "item")
                        {
                            return;  // Itemエレメントの終了
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "list")
                {
                    return;  // Listエレメントの終了
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
