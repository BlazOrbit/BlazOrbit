using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Infrastructure;

[ExcludeFromCodeCoverage]
public class BuildTemplates
{
    [BuildTemplate("CssBundle/entry.js")]
    public static string GetEntryJsTemplate() => """import "./main.css";""";

    [BuildTemplate("CssBundle/main.css")]
    public static string GetMainCssTemplate() => """
/* === GLOBAL CSS BUNDLE === */

/* 1. Reset & Base */
@import './_reset.css';
@import './_typography.css';

/* 2. Theme Variables */
@import './_themes.css';
@import './_initialize-themes.css';

/* 3. Universal Component Styles */
@import './_tokens.css';
@import './_base.css';
@import './_scrollbar.css';
@import './_transition-classes.css';

/* 4. Family-based Shared Styles */
@import './_input-family.css';
@import './_picker-family.css';
@import './_data-collection-family.css';

""";

    [BuildTemplate(".npmrc")]
    public static string GetNpmRcTemplate() => """
        fund=false
        audit=false
        """;

    [BuildTemplate("package.json")]
    public static string GetPackageJsonTemplate() => """
{
  "name": "cdcsharp-blazorbit",
  "version": "1.0.0",
  "description": "BlazOrbit Component Library",
  "private": true,
  "type": "module",
  "scripts": {
    "build": "echo 'Use MSBuild to build BlazOrbit'"
  },
  "devDependencies": {
    "typescript": "latest",
    "vite": "latest",
    "terser": "latest",
    "glob": "latest"
  }
}
""";

    [BuildTemplate("tsconfig.json")]
    public static string GetTsConfigTemplate() => """
{
  "compilerOptions": {
    "baseUrl": "./",
    "module": "ES2022",
    "target": "ES2022",
    "outDir": "./wwwroot/js",
    "moduleResolution": "node",
    "sourceMap": true,
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "paths": {
      "@types/*": ["./Types/*"]
    }
  },
  "exclude": [
    "node_modules",
    "wwwroot"
  ],
  "include": [
    "./Types/**/*.ts"
  ]
}
""";

    [BuildTemplate("vite.config.css.js")]
    public static string GetViteCssConfigTemplate() => """
// Vite CSS bundle config for BlazOrbit.
//
//   - Do NOT add PurgeCSS, postcss-purge, or any tree-shaking plugin.
//     Many BlazOrbit selectors are applied dynamically through variants,
//     `IHas*` interfaces, and consumer-supplied RenderFragments. A static
//     analyzer would flag those as "unused" and silently delete them, with
//     no signal until visual QA notices the gap.
//   - Vite's default CSS handling (cssnano `default` preset under the hood)
//     only minifies — whitespace, comment, longhand collapse — and is
//     safe for our custom properties (`--bob-*`, `--palette-*`) and
//     attribute selectors. Keep it.
//   - If a future version of Vite changes the default to a more aggressive
//     preset, pin it here explicitly to `cssnano({ preset: 'default' })`
//     instead of `'advanced'`.
import { defineConfig } from "vite";

export default defineConfig({
    build: {
        outDir: "wwwroot/css",
        emptyOutDir: false,
        cssCodeSplit: false,
        rollupOptions: {
            input: "./CssBundle/entry.js",
            output: {
                assetFileNames: (assetInfo) => {
                    const name = assetInfo.name ?? (assetInfo.names && assetInfo.names[0]) ?? "";
                    if (name.endsWith(".css")) {
                        return "blazorbit.css";
                    }
                    return "[name][extname]";
                }
            },
            plugins: [
                {
                    name: "remove-js-output",
                    generateBundle(_, bundle) {
                        Object.keys(bundle).forEach((file) => {
                            if (file.endsWith(".js")) {
                                delete bundle[file];
                            }
                        });
                    }
                }
            ]
        }
    }
});
""";

    [BuildTemplate("vite.config.js")]
    public static string GetViteJsConfigTemplate() => """
// Vite JS bundle config for BlazOrbit.
//
// Debug/Release toggle:
//   - BLAZORBIT_BUILD_CONFIG is propagated by BlazOrbit.Dev.targets from
//     MSBuild $(Configuration) ('Debug' | 'Release').
//   - Release  ⇒ sourcemap: false, minify: 'terser' (small payload, no .map shipped).
//   - Debug    ⇒ sourcemap: true,  minify: false    (DevTools-friendly inspection).
//   - Default (env unset) is Debug — local `npm run build` outside MSBuild is dev-mode.
import { defineConfig } from 'vite';
import path from 'path';
import { glob } from 'glob';

const isRelease = process.env.BLAZORBIT_BUILD_CONFIG === 'Release';

const inputFiles = glob.sync('./Types/**/[A-Z]*.ts').reduce((entries, filePath) => {
    const entryName = filePath.replace(/\\/g, '/').replace(/^\.\//, '').replace('.ts', '');
    entries[entryName] = filePath;
    return entries;
}, {});

export default defineConfig({
    resolve: {
        alias: {
            '@types': path.resolve(__dirname, 'Types')
        }
    },
    build: {
        outDir: path.resolve(__dirname, 'wwwroot/js'),
        emptyOutDir: false,
        rollupOptions: {
            input: inputFiles,
            preserveEntrySignatures: 'strict',
            output: {
                entryFileNames: '[name].min.js',
                format: 'es',
                manualChunks: undefined
            }
        },
        target: 'esnext',
        sourcemap: !isRelease,
        minify: isRelease ? 'terser' : false,
        terserOptions: isRelease ? {
            compress: {
                drop_console: true,
                drop_debugger: true
            },
            format: {
                comments: false
            }
        } : undefined
    }
});
""";
}