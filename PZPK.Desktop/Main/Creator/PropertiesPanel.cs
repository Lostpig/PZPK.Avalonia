using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using System;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class PropertiesPanel: PZComponentBase
{
    private class TagItem : ContentControl
    {
        public string Data { get; init; }
        public TagItem(string tag, PropertiesPanel parent)
        {
            Data = tag;

            Content = new Border()
                .Padding(8, 4)
                .Margin(4)
                .CornerRadius(4)
                .Background(Brushes.LightGray)
                .Child(
                    HStackPanel().Children(
                        PzText(tag)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                        IconButton(MaterialIconKind.Close)
                            .Width(24)
                            .Height(24)
                            .Margin(8, 0, 0, 0)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .OnClick(_ =>
                            {
                                parent.RemoveTag(this);
                            })
                    )
                );
        }
    }

    protected override StyleGroup? BuildStyles()
    {
        return [
                new Style<Grid>(s => s.Class("Row"))
                    .Margin(12,4,4,0)
                    .Height(40),
                new StyleGroup(s => s.Is<Grid>().Class("Row").Child())
                {
                    new Style<ToggleSwitch>().Margin(35,0,0,0),
                    new Style<ComboBox>().Margin(-6, 0),
                    new Style<NumericUpDown>().Margin(-1, 0)
                }
            ];
    }

    private static Grid BuildRow(string label, Control control)
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
    protected override object Build()
    {
        var index = Model.Index;
        var props = Model.Properties;

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
                                        .ItemTemplate<int>(i => PzText(Utility.ComputeFileSize(i)))
                                        .SelectedItem(() => props.BlockSize, v => props.BlockSize = (int)v)
                                    ),
                                BuildRow("Description:", 
                                        PzTextBox(() => props.Description, v => props.Description = v)
                                            .VerticalContentAlignment(Avalonia.Layout.VerticalAlignment.Top)
                                            .Height(120)
                                    ).Height(120),
                                Grid("200, 15, 150, *")
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
                                        SukiButton("Add")
                                            .Col(3)
                                            .Margin(10, 0)
                                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                                            .OnClick(_ => AddTag())
                                    ),
                                TagContainer,
                                BuildRow("Resize Image:",
                                    new ToggleSwitch()
                                        .IsChecked(() => props.EnableImageResizing, v => props.EnableImageResizing = v ?? false)
                                ),
                                BuildRow("Image Format:",
                                    new ComboBox()
                                        .IsEnabled(() => props.EnableImageResizing)
                                        .ItemsSource(ImageFormats)
                                        .ItemTemplate<ImageResizerFormat>(f => PzText(f.ToString()))
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
                        SukiButton("Prev", "Accent", "Flat").Canvas_Left(0).OnClick(_ => Model.PreviousStep()),
                        SukiButton("Next", "Flat").Canvas_Right(0).OnClick(_ => Model.NextStep())
                    )
            );
    }

    public PropertiesPanel(CreatorModel model): base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Model.OnStepChanged += OnStepChanged;

        TagContainer = new WrapPanel().Margin(225, 0, 0, 0);
        Tags = new PzControls<TagItem>(TagContainer.Children);

        Initialize();
    }
    private void OnStepChanged()
    {
        if (Model.Step != 2) return;

        Tags.Clear();
        var props = Model.Properties;
        if (props != null)
        {
            foreach (var tag in props.Tags)
            {
                Tags.Add(new TagItem(tag, this));
            }
        }

        StateHasChanged();
    }

    private readonly CreatorModel Model;
    private readonly int[] BlockSizes =
    [
        Constants.Sizes.t_64KB,
        Constants.Sizes.t_256KB,
        Constants.Sizes.t_1MB,
        Constants.Sizes.t_4MB,
        Constants.Sizes.t_8MB,
        Constants.Sizes.t_16MB,
        Constants.Sizes.t_64MB
    ];
    private readonly ImageResizerFormat[] ImageFormats =
    [
        ImageResizerFormat.Jpeg,
        ImageResizerFormat.Png,
        ImageResizerFormat.Webp,
    ];

    private WrapPanel TagContainer { get; init; }
    private PzControls<TagItem> Tags { get; init; }
    private string TempTag { get; set; } = "";
    private void AddTag()
    {
        var props = Model.Properties;
        var tag = TempTag.Trim();
        if (!string.IsNullOrEmpty(tag) && !props.Tags.Contains(tag))
        {
            TempTag = "";
            props.Tags.Add(tag);
            Tags.Add(new TagItem(tag, this));

            StateHasChanged();
        }
    }
    private void RemoveTag(TagItem tag)
    {
        var props = Model.Properties;
        if (props.Tags.Contains(tag.Data))
        {
            props.Tags.Remove(tag.Data);
            Tags.Remove(tag);
        }
    }
}
