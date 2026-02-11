using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;

namespace QuizCraft.Infrastructure.Data;

/// <summary>
/// Contexto do banco de dados EF Core para o QuizCraft.
/// Define todos os DbSets e configura o modelo relacional via Fluent API.
/// </summary>
public class QuizCraftDbContext : DbContext
{
    // Tabelas principais do dominio
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<StudySet> StudySets => Set<StudySet>();
    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();
    public DbSet<QuizSessionItem> QuizSessionItems => Set<QuizSessionItem>();
    public DbSet<Mastery> Masteries => Set<Mastery>();
    public DbSet<StudyStreak> StudyStreaks => Set<StudyStreak>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    /// <summary>
    /// Construtor que recebe as opcoes de configuracao do DbContext.
    /// </summary>
    public QuizCraftDbContext(DbContextOptions<QuizCraftDbContext> options) : base(options) { }

    /// <summary>
    /// Configura o modelo relacional usando Fluent API (chaves, indices, relacionamentos).
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Disciplina (materia)
        modelBuilder.Entity<Subject>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(s => s.Name);
        });

        // Topico (suporta hierarquia via ParentTopicId)
        modelBuilder.Entity<Topic>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.HasOne(t => t.Subject).WithMany(s => s.Topics).HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.ParentTopic).WithMany(t => t.SubTopics).HasForeignKey(t => t.ParentTopicId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(t => t.SubjectId);
            e.HasIndex(t => t.Name);
        });

        // Questao (enunciado, tipo, dificuldade)
        modelBuilder.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Statement).IsRequired();
            e.Property(q => q.Type).HasConversion<int>();
            e.HasOne(q => q.Topic).WithMany(t => t.Questions).HasForeignKey(q => q.TopicId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(q => q.TopicId);
            e.HasIndex(q => q.Difficulty);
            e.HasIndex(q => q.Statement);
        });

        // Alternativa de resposta
        modelBuilder.Entity<Choice>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Text).IsRequired();
            e.HasOne(c => c.Question).WithMany(q => q.Choices).HasForeignKey(c => c.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        // Tag (etiqueta para categorizar questoes)
        modelBuilder.Entity<Tag>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(t => t.Name).IsUnique();
        });

        // Relacionamento N:N entre Questao e Tag (tabela associativa)
        modelBuilder.Entity<QuestionTag>(e =>
        {
            e.HasKey(qt => new { qt.QuestionId, qt.TagId });
            e.HasOne(qt => qt.Question).WithMany(q => q.QuestionTags).HasForeignKey(qt => qt.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qt => qt.Tag).WithMany(t => t.QuestionTags).HasForeignKey(qt => qt.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // Conjunto de estudo (agrupamento de questoes)
        modelBuilder.Entity<StudySet>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
        });

        // Sessao de quiz (modo, status, datas)
        modelBuilder.Entity<QuizSession>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Mode).HasConversion<int>();
            e.Property(s => s.Status).HasConversion<int>();
            e.HasIndex(s => s.StartedAt);
        });

        // Item da sessao (resposta individual de cada questao)
        modelBuilder.Entity<QuizSessionItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasOne(i => i.Session).WithMany(s => s.Items).HasForeignKey(i => i.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Question).WithMany().HasForeignKey(i => i.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        // Dominio (nivel de aprendizagem por questao, 1:1 com Question)
        modelBuilder.Entity<Mastery>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Question).WithOne(q => q.Mastery).HasForeignKey<Mastery>(m => m.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => m.QuestionId).IsUnique();
            e.HasIndex(m => m.NextReviewAt);
        });

        // Sequencia de estudo (registro diario de atividade)
        modelBuilder.Entity<StudyStreak>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Date).IsUnique();
        });

        // Configuracoes da aplicacao (chave-valor persistente)
        modelBuilder.Entity<AppSettings>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Key).IsUnique();
        });
    }
}
