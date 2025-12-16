using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class PropertiesPanel: PZComponentBase
{
    private class TagItem : ContentControl
    {
        private static readonly ConcurrentStack<TagItem> Pool = new();
        public static TagItem GetItem(string tag)
        {
            var tagItem = Pool.TryPop(out var item) ? item : new TagItem();
            tagItem.SetTag(tag);
            return tagItem;
        }

        public string Data { get; private set; } = string.Empty;
        private TextBlock Text {  get; init; }
        private void SetTag(string tag)
        {
            Data = tag;
            Text.Text = Data;
        }
        private TagItem()
        {
            var brColor = App.Instance.Suki.GetSukiColor("SukiPrimaryColor50");

            Text = new TextBlock();
            Content = new Border()
                .Padding(8, 4)
                .Margin(4)
                .CornerRadius(4)
                .BorderBrush(brColor)
                .BorderThickness(1)
                .Background(Brushes.Transparent)
                .Child(
                    HStackPanel().Children(
                        Text.VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                        IconButton(MaterialIconKind.Close)
                            .Width(24)
                            .Height(24)
                            .Margin(8, 0, 0, 0)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .OnClick(_ =>
                            {
                                Props.Tags.Remove(Data);
                            })
                    )
                );
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            Pool.Push(this);
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

    private static Grid BuildRow(Func<string> label, Control control)
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
    protected override Control Build()
    {
        return Grid(null, "*, 40").Children(
                new ScrollViewer().Row(0).Content(
                    VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch)
                        .Children(
                            BuildRow(() => LOC.Base.Name, PzTextBox(Props.Name)),
                            BuildRow(() => LOC.Base.Password, PzTextBox(Props.Password)),
                            BuildRow(() => LOC.PZPK.BlockSize,
                                new ComboBox()
                                    .SelectedItemEx(Props.BlockSize)
                                    .ItemsSource(BlockSizes)
                                    .ItemTemplate<int>(i => PzText(Utility.ComputeFileSize(i)))
                                ),
                            BuildRow(() => LOC.PZPK.Description, 
                                    PzTextBox(Props.Description)
                                        .VerticalContentAlignment(Avalonia.Layout.VerticalAlignment.Top)
                                        .Height(120)
                                ).Height(120),
                            Grid("200, 15, 150, *")
                                .Classes("Row")
                                .Children(
                                    PzText(() => LOC.PZPK.Tags)
                                        .Col(0)
                                        .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                                        .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                                        .TextAlignment(TextAlignment.Center),
                                    PzTextBox(TempTag)
                                        .Col(2)
                                        .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                                        .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                                    SukiButton(() => LOC.Base.Add)
                                        .Col(3)
                                        .Margin(10, 0)
                                        .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left)
                                        .OnClick(_ => AddTag())
                                ),
                            new ItemsControl().Margin(225, 10, 0, 0)
                                .ItemsSourceEx(Props.Tags)
                                .ItemsPanel(new WrapPanel())
                                .ItemTemplate<string, ItemsControl>(tag => TagItem.GetItem(tag)),
                            BuildRow(() => LOC.PZPK.ResizeImage,
                                new ToggleSwitch()
                                    .IsCheckedEx(Resizer.Enabled)
                            ),
                            BuildRow(() => LOC.PZPK.ImageFormat,
                                new ComboBox()
                                    .IsEnabled(Resizer.Enabled)
                                    .ItemsSource(ImageFormats)
                                    .ItemTemplate<ImageResizerFormat>(f => PzText(f.ToString()))
                                    .SelectedItemEx(Resizer.Format)
                            ),
                            BuildRow(() => LOC.PZPK.MaxSize,
                                new NumericUpDown()
                                    .IsEnabled(Resizer.Enabled)
                                    .ValueEx(Resizer.MaxSize)
                            ),
                            BuildRow(() => LOC.PZPK.Quility,
                                new NumericUpDown()
                                    .IsEnabled(Resizer.Enabled)
                                    .ValueEx(Resizer.Quality)
                            ),
                            BuildRow(() => LOC.PZPK.LossLess,
                                new ToggleSwitch()
                                    .IsEnabled(Resizer.Enabled)
                                    .IsCheckedEx(Resizer.Lossless)
                            )
                        )
                ),
                new Canvas()
                    .Row(1)
                    .Children(
                        SukiButton(() => LOC.Base.Prev, "Accent", "Flat").Canvas_Left(0).OnClick(_ => Model.PreviousStep()),
                        SukiButton(() => LOC.Base.Next, "Flat").Canvas_Right(0).OnClick(_ => Model.NextStep())
                    )
            );
    }

    private static CreatorModel Model => CreatorModel.Instance;
    private static CreateProperties Props => CreatorModel.Instance.Properties;
    private static ResizerProperties Resizer => CreatorModel.Instance.Resizer;

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

    private BehaviorSubject<string> TempTag { get; init; } = new(string.Empty);
    private void AddTag()
    {
        var tag = TempTag.Value.Trim();
        if (!string.IsNullOrEmpty(tag) && !Props.Tags.Contains(tag))
        {
            Props.Tags.Add(tag);
            TempTag.OnNext("");
        }
    }
}
