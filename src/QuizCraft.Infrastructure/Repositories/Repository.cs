using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Interfaces;
using QuizCraft.Infrastructure.Data;

namespace QuizCraft.Infrastructure.Repositories;

/// <summary>
/// Repositorio generico com operacoes CRUD basicas.
/// Serve como base para repositorios especificos de cada entidade.
/// </summary>
/// <typeparam name="T">Tipo da entidade gerenciada.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly QuizCraftDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Inicializa o repositorio com o contexto e o DbSet da entidade.
    /// </summary>
    public Repository(QuizCraftDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>Busca uma entidade pelo ID (chave primaria).</summary>
    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    /// <summary>Retorna todas as entidades sem rastreamento (somente leitura).</summary>
    public virtual async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _dbSet.AsNoTracking().ToListAsync();

    /// <summary>Retorna entidades que atendem ao predicado informado.</summary>
    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    /// <summary>Adiciona uma nova entidade e persiste no banco.</summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>Atualiza uma entidade existente e persiste no banco.</summary>
    public virtual async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    /// <summary>Remove uma entidade e persiste no banco.</summary>
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>Conta entidades, opcionalmente filtradas por um predicado.</summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
}
