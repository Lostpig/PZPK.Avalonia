using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using PZPK.Core;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class PropertiesPanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
    protected override StyleGroup? BuildStyles()
    {
        return [
                new Style<TextBlock>(s => s.Class("Label"))
                    .Col(0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                    .TextAlignment(Avalonia.Media.TextAlignment.Center),
                new Style<TextBox>()
                    .Col(2)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                    .MinWidth(120),
                new Style<Grid>(s => s.Class("Row"))
                    .Margin(12,4,4,0)
                    .Height(40)
            ];
    }

    private Grid BuildRow(string label, Control control)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition().Width(new GridLength(1, GridUnitType.Auto)).SharedSizeGroup("LabelColumn"),
                new ColumnDefinition().Width(new GridLength(4, GridUnitType.Pixel)),
                new ColumnDefinition().Width(new GridLength(1, GridUnitType.Star))
            ]
        };

        return grid
                .Classes("Row")
                .Children(
                    PzText(label).Classes("Label"),
                    control
                );
    }
    protected override object Build(CreatorModel vm)
    {
        var index = vm.Index;
        var props = vm.Properties;

        return new ScrollViewer()
            .Content(
                VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch)
                    .Children(
                        BuildRow("Files:", PzTextBox(() => index.FilesCount.ToString()).IsReadOnly(true)),
                        BuildRow("Size:", PzTextBox(() => Utility.ComputeFileSize(index.SumFilesSize())).IsReadOnly(true)),
                        PzSeparatorV(),
                        BuildRow("Name:", PzTextBox(() => props.Name, v => props.Name = v)),
                        BuildRow("Password:", PzTextBox(() => props.Password, v => props.Password = v)),
                        BuildRow("BlockSize:",
                            new ComboBox()
                                .ItemsSource(BlockSizes)
                                .ItemTemplate<int>(i => 
                                        new FuncView<int>(i, s => PzText(() => Utility.ComputeFileSize(i)))
                                    )
                                .SelectedItem(() => props.BlockSize, v => props.BlockSize = (int)v)
                            ),
                        BuildRow("Resize Image:", 
                            new ToggleSwitch()
                            .IsChecked(() => props.EnableImageResizing, v => props.EnableImageResizing = v ?? false)
                        ),
                        BuildRow("Image Format:",
                            new ComboBox()
                                .ItemsSource(ImageFormats)
                                .ItemTemplate<ImageResizerFormat>(f =>
                                        new FuncView<ImageResizerFormat>(f, s => PzText(() => f.ToString()))
                                    )
                                .SelectedItem(() => props.ImageOptions.Format, v => props.ImageOptions.Format = (ImageResizerFormat)v)
                        ),
                        BuildRow("Quility:",
                            new NumericUpDown().Value(() => props.ImageOptions.Quality, v => props.ImageOptions.Quality = (int)(v ?? 100))
                        )
                    )
            );
    }

    protected override void OnAfterInitialized()
    {
        base.OnAfterInitialized();
        ViewModel?.OnStepChanged += StateHasChanged;
    }

    private int[] BlockSizes = new int[]
    {
        Constants.Sizes.t_4KB,
        Constants.Sizes.t_64KB,
        Constants.Sizes.t_256KB,
        Constants.Sizes.t_1MB,
        Constants.Sizes.t_4MB,
        Constants.Sizes.t_8MB,
        Constants.Sizes.t_16MB,
        Constants.Sizes.t_64MB
    };
    private ImageResizerFormat[] ImageFormats = new ImageResizerFormat[]
    {
        ImageResizerFormat.Jpeg,
        ImageResizerFormat.Png,
        ImageResizerFormat.Webp,
    };
}
