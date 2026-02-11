using System.Linq.Expressions;

namespace QuizCraft.Domain.Interfaces;

/// <summary>
/// Repositório genérico com operações CRUD básicas.
/// </summary>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Obtém uma entidade pelo seu identificador.</summary>
    /// <param name="id">Identificador único da entidade.</param>
    Task<T?> GetByIdAsync(int id);
    /// <summary>Retorna todas as entidades.</summary>
    Task<IReadOnlyList<T>> GetAllAsync();
    /// <summary>Retorna entidades que atendem ao predicado informado.</summary>
    /// <param name="predicate">Expressão de filtro.</param>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    /// <summary>Adiciona uma nova entidade e a retorna com o Id gerado.</summary>
    Task<T> AddAsync(T entity);
    /// <summary>Atualiza uma entidade existente.</summary>
    Task UpdateAsync(T entity);
    /// <summary>Remove uma entidade.</summary>
    Task DeleteAsync(T entity);
    /// <summary>Conta entidades que atendem ao predicado (todas, se nulo).</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
