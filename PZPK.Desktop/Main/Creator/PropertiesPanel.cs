using System.Reflection.Emit;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;

using PZPK.Core;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class PropertiesPanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
    protected override StyleGroup? BuildStyles()
    {
        return [
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
                new ColumnDefinition().Width(200),
                new ColumnDefinition().Width(15),
                new ColumnDefinition().Width(GridLength.Star)
            ]
        };

        return grid
                .Classes("Row")
                .Children(
                    PzText(label)
                        .Col(0)
                        .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                        .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                        .TextAlignment(Avalonia.Media.TextAlignment.Center),
                    control
                        .Col(2)
                        .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                        .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                );
    }
    protected override object Build(CreatorModel vm)
    {
        var index = vm.Index;
        var props = vm.Properties;

        return Grid(null, "*, 40")
            .Children(
                new ScrollViewer()
                    .Row(0)
                    .Content(
                        VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch)
                            .Children(
                                BuildRow("Files:", PzText(() => index.FilesCount.ToString())),
                                BuildRow("Size:", PzText(() => Utility.ComputeFileSize(index.SumFilesSize()))),
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
                                BuildRow("Description:", 
                                        PzTextBox(() => props.Description, v => props.Description = v)
                                            .VerticalContentAlignment(Avalonia.Layout.VerticalAlignment.Top)
                                            .Height(120)
                                    ).Height(120),
                                Grid("200, 15, 150, 80")
                                    .Classes("Row")
                                    .Children(
                                        PzText("Tags")
                                            .Col(0)
                                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                                            .TextAlignment(Avalonia.Media.TextAlignment.Center),
                                        PzTextBox(() => TempTag, v => TempTag = v)
                                            .Col(2)
                                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                                        SukiButton("Add").Col(3).OnClick(_ => AddTag())
                                    ),
                                new WrapPanel()
                                    .Margin(215, 0, 0, 0)
                                    .Ref(out TagContainer),
                                BuildRow("Resize Image:",
                                    new ToggleSwitch()
                                        .IsChecked(() => props.EnableImageResizing, v => props.EnableImageResizing = v ?? false)
                                ),
                                BuildRow("Image Format:",
                                    new ComboBox()
                                        .IsEnabled(() => props.EnableImageResizing)
                                        .ItemsSource(ImageFormats)
                                        .ItemTemplate<ImageResizerFormat>(f =>
                                                new FuncView<ImageResizerFormat>(f, s => PzText(() => f.ToString()))
                                            )
                                        .SelectedItem(() => props.ImageOptions.Format, v => props.ImageOptions.Format = (ImageResizerFormat)v)
                                ),
                                BuildRow(" - Max size:",
                                    new NumericUpDown()
                                        .IsEnabled(() => props.EnableImageResizing)
                                        .Value(() => props.ImageOptions.MaxSize, v => props.ImageOptions.MaxSize = (int)(v ?? 2160))
                                ),
                                BuildRow(" - Quility:",
                                    new NumericUpDown()
                                        .IsEnabled(() => props.EnableImageResizing)
                                        .Value(() => props.ImageOptions.Quality, v => props.ImageOptions.Quality = (int)(v ?? 75))
                                ),
                                BuildRow(" - LossLess:",
                                    new ToggleSwitch()
                                        .IsEnabled(() => props.EnableImageResizing)
                                        .IsChecked(() => props.ImageOptions.Lossless, v => props.ImageOptions.Lossless = v ?? false)
                                )
                            )
                ),
                new Canvas()
                    .Row(1)
                    .Children(
                        SukiButton("Prev", "", "Flat").Canvas_Left(0).OnClick(_ => vm.PreviousStep()),
                        SukiButton("Next", "Flat").Canvas_Right(0).OnClick(_ => vm.NextStep())
                    )
            );
    }

    protected override void OnCreated()
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

    private WrapPanel TagContainer;
    private string TempTag { get; set; } = "";
    private void UpdateTags() {         
        TagContainer.Children.Clear();
        var props = ViewModel?.Properties;
        if (props == null) return;
        foreach (var tag in props.Tags)
        {
            var border = new Border()
                .Padding(8, 4)
                .Margin(4)
                .CornerRadius(4)
                .Background(Avalonia.Media.Brushes.LightGray)
                .Child(
                    HStackPanel().Children(
                        PzText(tag)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                        SukiButton("x", "Flat")
                            .Width(16)
                            .Height(16)
                            .Margin(8, 0, 0, 0)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .OnClick(_ =>
                            {
                                props.Tags.Remove(tag);
                                UpdateTags();
                            })
                    )
                );
            TagContainer.Children.Add(border);
        }
    }
    private void AddTag()
    {
        var props = ViewModel?.Properties;
        if (props == null) return;
        var tag = TempTag.Trim();
        if (!string.IsNullOrEmpty(tag) && !props.Tags.Contains(tag))
        {
            props.Tags.Add(tag);
            TempTag = "";

            UpdateTags();
            StateHasChanged();
        }
    }
}
