// SnapshotTests/CssSnapshotTests.cs
using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests.SnapshotTests;

public class CssSnapshotTests
{
    [Fact]
    public Task Tokenize_CompleteStylesheet_MatchesSnapshot()
    {
        string code = """
            /* Global Styles */
            :root {
                --primary-color: #3498db;
                --secondary-color: #2ecc71;
                --danger-color: #e74c3c;
                --text-color: #333333;
                --background-color: #ffffff;
                --spacing-unit: 8px;
                --border-radius: 4px;
                --transition-duration: 200ms;
                --font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
            }

            *,
            *::before,
            *::after {
                box-sizing: border-box;
                margin: 0;
                padding: 0;
            }

            html {
                font-size: 16px;
                scroll-behavior: smooth;
            }

            body {
                font-family: var(--font-family);
                color: var(--text-color);
                background-color: var(--background-color);
                line-height: 1.6;
                -webkit-font-smoothing: antialiased;
            }

            /* Typography */
            h1, h2, h3, h4, h5, h6 {
                font-weight: 600;
                line-height: 1.2;
                margin-bottom: calc(var(--spacing-unit) * 2);
            }

            h1 { font-size: 2.5rem; }
            h2 { font-size: 2rem; }
            h3 { font-size: 1.75rem; }

            /* Layout Components */
            .container {
                width: 100%;
                max-width: 1200px;
                margin: 0 auto;
                padding: 0 calc(var(--spacing-unit) * 2);
            }

            .grid {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
                gap: calc(var(--spacing-unit) * 3);
            }

            .flex {
                display: flex;
                align-items: center;
                gap: var(--spacing-unit);
            }

            .flex-column {
                flex-direction: column;
            }

            .justify-between {
                justify-content: space-between;
            }

            /* Button Component */
            .btn {
                display: inline-flex;
                align-items: center;
                justify-content: center;
                gap: 8px;
                padding: 12px 24px;
                font-size: 1rem;
                font-weight: 500;
                text-decoration: none;
                border: none;
                border-radius: var(--border-radius);
                cursor: pointer;
                transition: all var(--transition-duration) ease-in-out;
            }

            .btn:hover {
                transform: translateY(-2px);
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            }

            .btn:active {
                transform: translateY(0);
            }

            .btn:disabled {
                opacity: 0.6;
                cursor: not-allowed;
                transform: none !important;
            }

            .btn--primary {
                background-color: var(--primary-color);
                color: #ffffff;
            }

            .btn--primary:hover {
                background-color: color-mix(in srgb, var(--primary-color) 85%, black);
            }

            .btn--secondary {
                background-color: transparent;
                color: var(--primary-color);
                border: 2px solid var(--primary-color);
            }

            .btn--danger {
                background-color: var(--danger-color);
                color: #ffffff;
            }

            /* Card Component */
            .card {
                background: #ffffff;
                border-radius: calc(var(--border-radius) * 2);
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
                overflow: hidden;
                transition: box-shadow var(--transition-duration);
            }

            .card:hover {
                box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
            }

            .card__header {
                padding: calc(var(--spacing-unit) * 2);
                border-bottom: 1px solid #eee;
            }

            .card__body {
                padding: calc(var(--spacing-unit) * 3);
            }

            .card__footer {
                padding: calc(var(--spacing-unit) * 2);
                background-color: #f8f9fa;
            }

            /* Form Elements */
            .form-group {
                margin-bottom: calc(var(--spacing-unit) * 2);
            }

            .form-label {
                display: block;
                margin-bottom: var(--spacing-unit);
                font-weight: 500;
            }

            .form-input {
                width: 100%;
                padding: 12px 16px;
                font-size: 1rem;
                border: 1px solid #ddd;
                border-radius: var(--border-radius);
                transition: border-color var(--transition-duration), box-shadow var(--transition-duration);
            }

            .form-input:focus {
                outline: none;
                border-color: var(--primary-color);
                box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.2);
            }

            .form-input::placeholder {
                color: #999;
            }

            .form-input:invalid {
                border-color: var(--danger-color);
            }

            /* Media Queries */
            @media (max-width: 768px) {
                html {
                    font-size: 14px;
                }

                .container {
                    padding: 0 var(--spacing-unit);
                }

                .grid {
                    grid-template-columns: 1fr;
                }

                .btn {
                    width: 100%;
                }
            }

            @media (prefers-color-scheme: dark) {
                :root {
                    --text-color: #f0f0f0;
                    --background-color: #1a1a1a;
                }

                .card {
                    background: #2d2d2d;
                }

                .form-input {
                    background: #333;
                    border-color: #444;
                    color: #f0f0f0;
                }
            }

            /* Animations */
            @keyframes fadeIn {
                from {
                    opacity: 0;
                    transform: translateY(10px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }

            @keyframes spin {
                to {
                    transform: rotate(360deg);
                }
            }

            .animate-fade-in {
                animation: fadeIn 300ms ease-out forwards;
            }

            .spinner {
                width: 24px;
                height: 24px;
                border: 3px solid #eee;
                border-top-color: var(--primary-color);
                border-radius: 50%;
                animation: spin 800ms linear infinite;
            }
            """;

        IReadOnlyList<Token> tokens = CssLanguage.Instance.Tokenize(code);

        return Verify(tokens.Select(t => new { t.Type, t.Value }));
    }
}