using Avalonia.Media;
using PZPK.Desktop.Common;
using PZPK.Desktop.Localization;
using SukiUI.Controls;
using SukiUI.Models;
using System.Linq;

namespace PZPK.Desktop.Main.Setting;

public class SettingPage: PZComponentBase
{
    private void SetHeader(SettingsLayoutItem item, Func<string> func)
    {
        void setter(string v) => item.Header = v;
        item._set(setter, func, null, null);
    }
    private static RadioButton BuildThemeRadio(IBrush background, IBrush foreground, Func<string> textFunc)
    {
        return new RadioButton()
            .Width(120).Height(100)
            .Padding(0)
            .Classes("GigaChips")
            .GroupName("BaseTheme")
            .Content(
                new Border().Margin(-50)
                    .Background(background)
                    .CornerRadius(16)
                    .Child(
                        new Grid().Children(
                                PzText(textFunc)
                                    .Margin(58, 42, 42, 42)
                                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Bottom)
                                    .FontWeight(FontWeight.DemiBold)
                                    .Foreground(foreground)

                            )
                    )
            );
    }
    private SettingsLayoutItem BuildThemeItem()
    {
        var item = new SettingsLayoutItem();

        SetHeader(item, () => LOC.Base.Theme);
        item.Content = HStackPanel()
                .Spacing(20)
                .Children(
                    BuildThemeRadio(Brushes.White, Brushes.DarkGray, () => LOC.Base.Light)
                        .IsChecked(() => Model.IsLightTheme, v => Model.IsLightTheme = v ?? false),
                    BuildThemeRadio(Brushes.Black, Brushes.White, () => LOC.Base.Dark)
                        .IsChecked(() => !Model.IsLightTheme)
                );

        return item;
    }

    private StackPanel BuildColorRadio(SukiColorTheme colorTheme)
    {
        return HStackPanel()
            .Children(
                new RadioButton()
                    .Width(50).Height(50)
                    .Classes("GigaChips")
                    .CornerRadius(50)
                    .GroupName("ColorTheme")
                    .OnIsCheckedChanged(e =>
                    {
                        if (e.Source is RadioButton r)
                        {
                            if (r.IsChecked == true)
                            {
                                Model.ChangeColorTheme(colorTheme);
                            }
                        }
                    })
                    .IsChecked(() => Model.ColorTheme == colorTheme)
                    .Content(
                        new Border()
                            .Margin(-30)
                            .CornerRadius(50)
                            .Background(colorTheme.PrimaryBrush)
                    )
            );
    }
    private SettingsLayoutItem BuildColorItem()
    {
        var item = new SettingsLayoutItem();
        var radios = Model.Theme.ColorThemes.Select(c => BuildColorRadio(c));

        SetHeader(item, () => LOC.Base.Color);
        item.Content = HStackPanel()
                .Spacing(20)
                .Children(
                    children: [.. radios]
                );

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
                                    .SelectedItem(() => Model.ActiveLanguage!, v => Model.ActiveLanguage = (LanguageItem)v)
                            )
                    )
            );

        return item;
    }

    protected override object Build()
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
