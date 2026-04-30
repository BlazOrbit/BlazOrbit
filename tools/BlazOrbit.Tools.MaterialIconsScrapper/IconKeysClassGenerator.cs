using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazOrbit.Tools.MaterialIconsScrapper;

/// <summary>
/// Generates <c>BOBIconKeys</c> partial classes by reflecting over existing <c>BOBIcons</c> constants.
/// </summary>
public static class IconKeysClassGenerator
{
    /// <summary>
    /// Reads <paramref name="sourcePath"/> (a <c>BOBIcons*.cs</c> file), finds every nested class
    /// that contains <c>public const string</c> fields/properties, and emits a matching
    /// <c>BOBIconKeys*.cs</c> file where each constant becomes a <c>static readonly IconKey</c>.
    /// </summary>
    public static void GenerateFromIconsFile(string sourcePath, string outputDirectory)
    {
        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        string outputFileName = fileName.StartsWith("BOBIcons")
            ? fileName.Replace("BOBIcons", "BOBIconKeys") + ".cs"
            : $"BOBIconKeys.{fileName}.cs";
        string outputPath = Path.Combine(outputDirectory, outputFileName);

        Directory.CreateDirectory(outputDirectory);

        string sourceText = File.ReadAllText(sourcePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        // Extract namespace and class declarations
        BaseNamespaceDeclarationSyntax? namespaceDecl = root.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

        if (namespaceDecl == null)
        {
            throw new InvalidOperationException($"No namespace found in {sourcePath}");
        }

        ClassDeclarationSyntax? iconsClass = namespaceDecl.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (iconsClass == null)
        {
            throw new InvalidOperationException($"No class found in {sourcePath}");
        }

        string targetClassName = iconsClass.Identifier.Text.Replace("Icons", "IconKeys");

        // Collect nested classes that have string constants
        List<ClassDeclarationSyntax> nestedClasses = [];
        foreach (ClassDeclarationSyntax nested in iconsClass.Members.OfType<ClassDeclarationSyntax>())
        {
            bool hasStringConsts = nested.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(f => f.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                          f.Modifiers.Any(SyntaxKind.ConstKeyword) &&
                          f.Declaration.Type.ToString() == "string");

            bool hasStringConstProps = nested.Members
                .OfType<PropertyDeclarationSyntax>()
                .Any(p => p.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                          p.Modifiers.Any(SyntaxKind.ConstKeyword) &&
                          p.Type.ToString() == "string");

            if (hasStringConsts || hasStringConstProps)
            {
                nestedClasses.Add(nested);
            }
        }

        if (nestedClasses.Count == 0)
        {
            Console.WriteLine($"  Skipped (no string constants): {sourcePath}");
            return;
        }

        // Build output class
        ClassDeclarationSyntax outputClass = SyntaxFactory.ClassDeclaration(targetClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        foreach (ClassDeclarationSyntax nested in nestedClasses)
        {
            ClassDeclarationSyntax keysNested = SyntaxFactory.ClassDeclaration(nested.Identifier.Text)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Fields: public const string Name = "...";
            foreach (FieldDeclarationSyntax field in nested.Members.OfType<FieldDeclarationSyntax>())
            {
                if (!field.Modifiers.Any(SyntaxKind.PublicKeyword) || !field.Modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    continue;
                }

                if (field.Declaration.Type.ToString() != "string")
                {
                    continue;
                }

                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    string memberName = variable.Identifier.Text;
                    PropertyDeclarationSyntax prop = BuildIconKeyProperty(memberName, nested.Identifier.Text, iconsClass.Identifier.Text);
                    keysNested = keysNested.AddMembers(prop);
                }
            }

            // Properties: public const string Name { get; } = "...";
            foreach (PropertyDeclarationSyntax prop in nested.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (!prop.Modifiers.Any(SyntaxKind.PublicKeyword) || !prop.Modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    continue;
                }

                if (prop.Type.ToString() != "string")
                {
                    continue;
                }

                string memberName = prop.Identifier.Text;
                PropertyDeclarationSyntax keyProp = BuildIconKeyProperty(memberName, nested.Identifier.Text, iconsClass.Identifier.Text);
                keysNested = keysNested.AddMembers(keyProp);
            }

            outputClass = outputClass.AddMembers(keysNested);
        }

        NamespaceDeclarationSyntax outputNamespace = SyntaxFactory.NamespaceDeclaration(
                SyntaxFactory.ParseName(namespaceDecl.Name.ToString()))
            .AddMembers(outputClass);

        CompilationUnitSyntax outputCompilation = SyntaxFactory.CompilationUnit()
            .AddMembers(outputNamespace);

        File.WriteAllText(outputPath, outputCompilation.NormalizeWhitespace().ToFullString());
        Console.WriteLine($"  Generated: {outputPath}");
    }

    private static PropertyDeclarationSyntax BuildIconKeyProperty(string memberName, string nestedClassName, string iconsClassName)
    {
        // new IconKey("Name") { SvgContent = BOBIcons.NestedClass.Name }
        ObjectCreationExpressionSyntax creation = SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.IdentifierName("IconKey"))
            .WithArgumentList(SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(memberName))))))
            .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("SvgContent"),
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(iconsClassName),
                                SyntaxFactory.IdentifierName(nestedClassName)),
                            SyntaxFactory.IdentifierName(memberName))))));

        return SyntaxFactory.PropertyDeclaration(SyntaxFactory.IdentifierName("IconKey"), memberName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
            .WithInitializer(SyntaxFactory.EqualsValueClause(creation))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}
