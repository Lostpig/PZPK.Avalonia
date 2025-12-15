using Avalonia.Media;
using Avalonia.Styling;
using PZPK.Desktop.Common;
using PZPK.Desktop.Localization;
using SukiUI.Controls;
using SukiUI.Models;
using System.Linq;

namespace PZPK.Desktop.Main.Setting;

public class SettingPage: PZComponentBase
{
    private static void SetHeader(SettingsLayoutItem item, Func<string> func)
    {
        static void setter(SettingsLayoutItem c, string v) => c.Header = v;
        item._set(setter, func);
    }
    private static RadioButton BuildThemeRadio(ThemeVariant theme)
    {
        var fore = theme.Key.ToString() == "Light" ? Brushes.Black : Brushes.White;
        var back = theme.Key.ToString() == "Light" ? Brushes.White : Brushes.DarkGray;
        Func<string> text = theme.Key.ToString() == "Light" ? () => LOC.Base.Light : () => LOC.Base.Dark;

        return new RadioButton()
            .Width(120).Height(100)
            .Padding(0)
            .Classes("GigaChips")
            .GroupName("BaseTheme")
            .Content(
                new Border().Margin(-50)
                    .Background(back)
                    .CornerRadius(16)
                    .Child(
                        new Grid().Children(
                                PzText(text)
                                    .Margin(58, 42, 42, 42)
                                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Bottom)
                                    .FontWeight(FontWeight.DemiBold)
                                    .Foreground(fore)

                            )
                    )
            );
    }
    private SettingsLayoutItem BuildThemeItem()
    {
        var item = new SettingsLayoutItem();

        SetHeader(item, () => LOC.Base.Theme);
        item.Content = new PZRadioGroup<ThemeVariant>("BaseThemeGroup")
                            .SetItemsPanel(HStackPanel().Spacing(20))
                            .SetItemsSource(Model.BaseThemes)
                            .SetItemTemplete(t => BuildThemeRadio(t))
                            .CheckedItem(Model.BaseTheme);
        return item;
    }

    private static RadioButton BuildColorRadio(SukiColorTheme colorTheme)
    {
        return new RadioButton()
                    .Width(50).Height(50)
                    .Classes("GigaChips")
                    .CornerRadius(50)
                    .GroupName("ColorTheme")
                    .Content(
                        new Border()
                            .Margin(-30)
                            .CornerRadius(50)
                            .Background(colorTheme.PrimaryBrush)
                    );
    }
    private SettingsLayoutItem BuildColorItem()
    {
        var item = new SettingsLayoutItem();
        var radios = Model.Theme.ColorThemes.Select(c => BuildColorRadio(c));

        SetHeader(item, () => LOC.Base.Color);
        item.Content = new PZRadioGroup<SukiColorTheme>("ColorThemeGroup")
                            .SetItemsPanel(HStackPanel().Spacing(20))
                            .SetItemsSource(Model.ColorThemes)
                            .SetItemTemplete(t => BuildColorRadio(t))
                            .CheckedItem(Model.ColorTheme);

        return item;
    }

    private SettingsLayoutItem BuildLanguageItem()
    {
        var item = new SettingsLayoutItem();

        SetHeader(item, () => LOC.Base.Language);
        item.Content = VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch)
            .Children(
                new GlassCard()
                    .Margin(0, 25, 0, 0)
                    .Padding(25)
                    .Content(
                        Grid("Auto, Auto, *")
                            .Children(
                                PzText(() => LOC.Base.Language)
                                    .Col(0)
                                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center),
                                new ComboBox()
                                    .Col(1)
                                    .Margin(20, 0, 0, 0)
                                    .MinWidth(150)
                                    .ItemsSource(Model.Languages)
                                    .ItemTemplate<LanguageItem>(i => PzText(i.Name))
                                    .SelectedItemEx(Model.ActiveLanguage)
                            )
                    )
            );

        return item;
    }

    protected override Control Build()
    {
        var layout = new SettingsLayout();
        List<SettingsLayoutItem> items = [
                BuildThemeItem(),
                BuildColorItem(),
                BuildLanguageItem()
            ];

        layout.Items = items;

        return new Panel().Children(
                layout
            );
    }

    private readonly SettingModel Model = SettingModel.Instance;
}
