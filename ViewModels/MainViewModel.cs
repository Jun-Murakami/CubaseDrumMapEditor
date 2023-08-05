using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using FluentAvalonia.UI.Controls;
using CubaseDrumMapEditor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Xml.Linq;
using Avalonia.Controls;

namespace CubaseDrumMapEditor.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        OpenDrumMapCommand = new AsyncRelayCommand(OpenDrumMapAsync);
    }

    public IAsyncRelayCommand OpenDrumMapCommand { get; }


    private List<MapItem>? _sortedMapList;

    public List<MapItem>? SortedMapList
    {
        get => _sortedMapList;
        set => SetProperty(ref _sortedMapList, value);
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

                var drumMapName = doc.Descendants("string")
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

                var mapList = doc.Descendants("list")
                                 .Where(x => (string)x.Attribute("name")! == "Map")
                                 .SelectMany(x => x.Elements("item"))
                                 .Select(x => new MapItem
                                 {
                                     INote = (int)x.Elements("int").First().Attribute("value")!,
                                     ONote = (int)x.Elements("int").Skip(1).First().Attribute("value")!,
                                     Channel = (int)x.Elements("int").Skip(2).First().Attribute("value")!,
                                     Length = (float)x.Element("float")!.Attribute("value")!,
                                     Mute = (int)x.Elements("int").Skip(3).First().Attribute("value")!,
                                     DisplayNote = (int)x.Elements("int").Skip(4).First().Attribute("value")!,
                                     HeadSymbol = (int)x.Elements("int").Skip(5).First().Attribute("value")!,
                                     Voice = (int)x.Elements("int").Skip(6).First().Attribute("value")!,
                                     PortIndex = (int)x.Elements("int").Skip(7).First().Attribute("value")!,
                                     Name = (string)x.Element("string")!.Attribute("value")!,
                                     QuantizeIndex = (int)x.Elements("int").Skip(8).First().Attribute("value")!
                                 })
                                 .ToList();

                var orderValues = doc.Descendants("list")
                                     .Where(x => (string)x.Attribute("name")! == "Order")
                                     .SelectMany(x => x.Elements("item"))
                                     .Select(x => (int)x.Attribute("value")!)
                                     .ToList();

                var outputDevices = doc.Descendants("list")
                                       .Where(x => (string)x.Attribute("name")! == "OutputDevices")
                                       .SelectMany(x => x.Elements("item"))
                                       .Select(x => new
                                       {
                                           DeviceName = (string)x.Elements("string").First().Attribute("value")!,
                                           PortName = (string)x.Elements("string").Skip(1).First().Attribute("value")!
                                       })
                                       .ToList();

                var flags = doc.Descendants("int")
                                .Where(x => (string)x.Attribute("Flags")! == "Name")
                                .Select(x => (int)x.Attribute("value")!)
                                .FirstOrDefault();

                // mapListをINoteの値をキーとする辞書に変換
                var mapDict = mapList.ToDictionary(item => item.DisplayNote, item => item);

                // orderValuesの順番を保持したまま、辞書から順に要素を取り出す
                var sortedMapList = orderValues.Select(value => mapDict[value]).Cast<MapItem>().ToList();

                SortedMapList = sortedMapList;
            }
            catch (Exception ex)
            {
                var cdialog = new ContentDialog() { Title = $"Error: {ex.Message}", PrimaryButtonText = "OK" };
                await cdialog.ShowAsync();
                throw;
            }
        }
    }
}
