using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PZPKRecorderGenerator.Helpers;
using System.Text;
using System.Xml.Linq;

namespace PZPKRecorderGenerator;

[Generator(LanguageNames.CSharp)]
public class I18NGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Debugger.Launch();
        IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith("languages.json"));

        context.RegisterSourceOutput(textFiles, (spc, tfs) =>
        {
            if (Path.GetFileNameWithoutExtension(tfs.Path) != "languages") return;

            var jsonText = tfs.GetText()?.ToString();
            if (jsonText == null)
            {
                throw new Exception("cannot read languages.json file.");
            }
            var languagesJson = Helpers.I18NHelper.DeserializeLanguage(jsonText);
            if (languagesJson == null)
            {
                throw new ArgumentException("Language json cannot load!");
            }

            var sourceText = GetLocalizeDictSource(languagesJson);
            spc.AddSource("I18N.g.cs", sourceText);
        });
    }

    private static SourceText GetLocalizeDictSource(LanguageJson languagesJson)
    {
        var sourceText = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PZPK.I18N"))
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
            )
            .AddMembers(
                SyntaxFactory.ClassDeclaration("Localization")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddMembers(GetFieldProterties(languagesJson))
                    .AddMembers(GetUpdateMethod(languagesJson))
            )
            .NormalizeWhitespace()
            .GetText(Encoding.UTF8);

        return sourceText;
    }
    private static MemberDeclarationSyntax[] GetFieldProterties(Helpers.LanguageJson language)
    {
        return language.Fields.Select(field =>
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), field)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List<AccessorDeclarationSyntax>(
                            new[]
                            {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }
                        )
                    )
                )
        ).ToArray();
    }
    private static MemberDeclarationSyntax GetUpdateMethod(Helpers.LanguageJson language)
    {
        var fields = language.Fields.Select(f => $"{f} = getText(\"{f}\");");

        var code = $@"
            public static void Update(Dictionary<string, string> dict)
            {{
                {string.Join("\r\n", fields)}
                
                string getText(string key)
                {{
                    if (dict != null && dict.ContainsKey(key))
                    {{
                        return dict[key];
                    }}
                    return key;
                }}
            }}
        ";

        return SyntaxFactory.ParseMemberDeclaration(code);
    }
}
