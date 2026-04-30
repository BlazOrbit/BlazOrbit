using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlazOrbit.Tools.MaterialIconsScrapper;

/// <summary>
/// A static class for generating icons classes.
/// </summary>
public static class IconsClassGenerator
{
    private const string _classDirectory = "./Icons";
    private const string _className = "UIIcons";
    private const string _classNamespace = "BlazOrbit.Media";
    private const string _iconNamePrefix = "i_";
    private static readonly HttpClient _httpClient = new();
    private static readonly string _iconsMetaUrl = "https://fonts.google.com/metadata/icons";

    /// <summary>
    /// Builds an icons file by fetching icons metadata from HTTP, generating icons code, and
    /// writing it to a file.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public static async Task BuildIconsFile()
    {
        string finalPath = $"{Path.Combine(_classDirectory, _className)}.cs";
        if (File.Exists(finalPath))
        {
            return;
        }

        string iconsMetaJson = FetchIconsMetaFromHttp();

        string iconsCode = GenerateIcons(iconsMetaJson);

        Directory.CreateDirectory(_classDirectory);
        File.WriteAllText(finalPath, iconsCode);
    }

    private static string FetchIconsMetaFromHttp()
    {
        string iconsMetaJson = _httpClient.GetStringAsync(_iconsMetaUrl).Result;
        iconsMetaJson = iconsMetaJson[5..];

        return iconsMetaJson;
    }

    private static string FetchIconSvg(string iconUrl)
    {
        string iconString = _httpClient.GetStringAsync(iconUrl).Result;
        return iconString;
    }

    private static string GenerateIcons(string iconsMetaJson)
    {
        IconsMeta? iconsMeta = JsonSerializer.Deserialize<IconsMeta>(iconsMetaJson) ?? throw new ArgumentException($"Unable to parse json: {iconsMetaJson}");

        FileScopedNamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(_classNamespace));

        ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration(_className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword));

        IEnumerable<ClassDeclarationSyntax> members = iconsMeta.families.Select(family =>
        {
            ClassDeclarationSyntax nestedClass = SyntaxFactory.ClassDeclaration(family.Replace(" ", ""))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            IEnumerable<PropertyDeclarationSyntax> iconProperties = iconsMeta.icons.Select(icon =>
            {
                string iconPath = iconsMeta.asset_url_pattern
                    .Replace("{family}", family.Replace(" ", "").ToLower())
                    .Replace("{icon}", icon.name)
                    .Replace("{version}", $"{icon.version}")
                    .Replace("{asset}", "24px.svg");

                string iconString = FetchIconSvg($"https://{iconsMeta.host}{iconPath}");

                iconString = Regex.Replace(iconString, "<title>.+?</title>", "");
                iconString = Regex.Match(iconString, "<svg[^>]*>(.*?)</svg>").Groups[1].Value;

                PropertyDeclarationSyntax propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), $"{_iconNamePrefix}{icon.name}")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(iconString)
                            )
                        )
                    ).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                return propertyDeclaration;
            });

            nestedClass = nestedClass.AddMembers(iconProperties.ToArray<MemberDeclarationSyntax>());

            return nestedClass;
        });

        SyntaxList<MemberDeclarationSyntax> membersList = SyntaxFactory.List<MemberDeclarationSyntax>(members);

        ClassDeclarationSyntax materialsClass = SyntaxFactory.ClassDeclaration("Materials")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        materialsClass = materialsClass.WithMembers(membersList);
        classDeclaration = classDeclaration.WithMembers([materialsClass]);

        namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit().AddMembers(namespaceDeclaration);

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }

    /// <summary>
    /// Represents an icon.
    /// </summary>
    public class Icon
    {
        /// <summary>
        /// Gets or sets the list of categories.
        /// </summary>
        /// <value>
        /// The list of categories.
        /// </value>
        public List<string> categories { get; set; } = [];

        /// <summary>
        /// Gets or sets the Unicode code point of a character.
        /// </summary>
        /// <value>
        /// The Unicode code point of a character.
        /// </value>
        public int codepoint { get; set; }

        /// <summary>
        /// Gets or sets the name property.
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the popularity value.
        /// </summary>
        /// <value>
        /// The popularity value.
        /// </value>
        public int popularity { get; set; }

        /// <summary>
        /// Gets or sets the list of sizes in pixels.
        /// </summary>
        /// <value>
        /// The list of sizes in pixels.
        /// </value>
        public List<int> sizes_px { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of tags associated with an item.
        /// </summary>
        /// <value>
        /// The list of tags.
        /// </value>
        public List<string> tags { get; set; } = [];

        /// <summary>
        /// Gets or sets a list of unsupported font families.
        /// </summary>
        /// <value>
        /// The list of unsupported font families.
        /// </value>
        public List<object> unsupported_families { get; set; } = [];

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public int version { get; set; }
    }

    /// <summary>
    /// Represents metadata related to icons.
    /// </summary>
    public class IconsMeta
    {
        /// <summary>
        /// Gets or sets the pattern for asset URLs.
        /// </summary>
        public string asset_url_pattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of font families.
        /// </summary>
        public List<string> families { get; set; } = [];

        /// <summary>
        /// Gets or sets the host value.
        /// </summary>
        public string host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of icons.
        /// </summary>
        public List<Icon> icons { get; set; } = [];
    }
}