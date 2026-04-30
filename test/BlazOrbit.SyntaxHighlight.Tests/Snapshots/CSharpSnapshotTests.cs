// SnapshotTests/CSharpSnapshotTests.cs
using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests.SnapshotTests;

public class CSharpSnapshotTests
{
    [Fact]
    public Task Tokenize_CompleteClass_MatchesSnapshot()
    {
        string code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            namespace MyApplication.Services;

            /// <summary>
            /// Represents a generic repository for data access.
            /// </summary>
            public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
            {
                private readonly DbContext _context;
                private readonly ILogger<Repository<TEntity>> _logger;

                public Repository(DbContext context, ILogger<Repository<TEntity>> logger)
                {
                    _context = context ?? throw new ArgumentNullException(nameof(context));
                    _logger = logger;
                }

                public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
                {
                    if (id <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(id), "Id must be positive");
                    }

                    try
                    {
                        return await _context.Set<TEntity>()
                            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving entity with id {Id}", id);
                        throw;
                    }
                }

                public async Task<IReadOnlyList<TEntity>> GetAllAsync(
                    Expression<Func<TEntity, bool>>? predicate = null,
                    int? skip = null,
                    int? take = null)
                {
                    IQueryable<TEntity> query = _context.Set<TEntity>();

                    if (predicate is not null)
                    {
                        query = query.Where(predicate);
                    }

                    if (skip.HasValue)
                    {
                        query = query.Skip(skip.Value);
                    }

                    if (take.HasValue)
                    {
                        query = query.Take(take.Value);
                    }

                    return await query.ToListAsync();
                }

                public virtual async Task<TEntity> CreateAsync(TEntity entity)
                {
                    ArgumentNullException.ThrowIfNull(entity);

                    _context.Set<TEntity>().Add(entity);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Created entity of type {Type} with id {Id}",
                        typeof(TEntity).Name, entity.Id);

                    return entity;
                }

                #region Private Methods

                private static bool IsValid(TEntity entity) => entity?.Id > 0;

                #endregion Private Methods
            }
            """;

        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        return Verify(tokens.Select(t => new { t.Type, t.Value }));
    }

    [Fact]
    public Task Tokenize_ModernCSharpFeatures_MatchesSnapshot()
    {
        string code = """
            #nullable enable

            using System.Text.Json;

            namespace Demo;

            public record Person(string FirstName, string LastName)
            {
                public string FullName => $"{FirstName} {LastName}";
            }

            public static class Extensions
            {
                public static async IAsyncEnumerable<T> WhereAsync<T>(
                    this IAsyncEnumerable<T> source,
                    Func<T, bool> predicate,
                    [EnumeratorCancellation] CancellationToken ct = default)
                {
                    await foreach (var item in source.WithCancellation(ct))
                    {
                        if (predicate(item))
                        {
                            yield return item;
                        }
                    }
                }

                public static T? ParseOrDefault<T>(this string? input) where T : struct
                {
                    return input switch
                    {
                        null or "" => null,
                        _ when typeof(T) == typeof(int) => (T?)(object?)int.Parse(input),
                        _ when typeof(T) == typeof(double) => (T?)(object?)double.Parse(input),
                        _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
                    };
                }
            }

            public class DataProcessor
            {
                private readonly Dictionary<string, object> _cache = [];

                public required string Name { get; init; }

                public async Task ProcessAsync(ReadOnlyMemory<byte> data)
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(data.Span);

                    var result = json.ValueKind switch
                    {
                        JsonValueKind.Object => ProcessObject(json),
                        JsonValueKind.Array => ProcessArray(json),
                        _ => throw new InvalidOperationException()
                    };

                    await Task.CompletedTask;
                }

                private static int ProcessObject(JsonElement element) => element.GetPropertyCount();
                private static int ProcessArray(JsonElement element) => element.GetArrayLength();
            }
            """;

        IReadOnlyList<Token> tokens = CSharpLanguage.Instance.Tokenize(code);

        return Verify(tokens.Select(t => new { t.Type, t.Value }));
    }
}