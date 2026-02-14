// =============================================================================
// SubjectCatalog.cs - Catalogo de materias e lotes para geracao de questoes
// Define as 10 materias com 4 lotes de topicos cada (total: 40 lotes, 2000 questoes)
// =============================================================================

namespace QuizCraft.QuestionGenerator;

/// <summary>
/// Representa um lote de topicos para geracao de questoes.
/// Cada lote gera 50 questoes distribuidas entre os topicos.
/// </summary>
public record Lote(
    int Numero,
    string Nivel,
    string[] Topicos
);

/// <summary>
/// Representa uma materia com seus 4 lotes de topicos.
/// </summary>
public record Materia(
    string Nome,
    Lote[] Lotes
);

/// <summary>
/// Catalogo completo das 10 materias e seus lotes.
/// 10 materias x 4 lotes x 50 questoes = 2.000 questoes totais.
/// </summary>
public static class SubjectCatalog
{
    /// <summary>
    /// Retorna a lista completa das 10 materias com seus lotes.
    /// </summary>
    public static Materia[] ObterMaterias() =>
    [
        // =====================================================================
        // 1) Matematica (Ensino Fundamental)
        // =====================================================================
        new("Matemática", [
            new(1, "EF", ["Aritmética", "Frações", "Porcentagem", "Razão e Proporção"]),
            new(2, "EF", ["Álgebra básica", "Equações do 1º grau", "Sistemas simples", "Expressões algébricas"]),
            new(3, "EF", ["Funções do 1º grau", "PA e PG (introdução)", "Probabilidade básica", "Leitura de gráficos"]),
            new(4, "EF", ["Geometria plana", "Geometria espacial (introdução)", "Trigonometria básica", "Estatística (média/mediana/moda)"])
        ]),

        // =====================================================================
        // 2) Lingua Portuguesa (Ensino Fundamental)
        // =====================================================================
        new("Língua Portuguesa", [
            new(1, "EF", ["Interpretação de texto", "Coesão e coerência", "Tipos textuais", "Gêneros textuais"]),
            new(2, "EF", ["Classes gramaticais", "Concordância", "Regência", "Crase"]),
            new(3, "EF", ["Pontuação", "Ortografia", "Semântica", "Figuras de linguagem (básico)"]),
            new(4, "EF", ["Variação linguística", "Funções da linguagem", "Literatura (introdução)", "Recursos expressivos"])
        ]),

        // =====================================================================
        // 3) Ciencias (Ensino Fundamental)
        // =====================================================================
        new("Ciências", [
            new(1, "EF", ["Corpo humano", "Sistemas do corpo", "Saúde e higiene", "Doenças e prevenção"]),
            new(2, "EF", ["Ecologia", "Cadeias alimentares", "Ciclos biogeoquímicos (introdução)", "Sustentabilidade"]),
            new(3, "EF", ["Matéria e energia", "Estados físicos", "Misturas e separação", "Transformações"]),
            new(4, "EF", ["Terra e universo", "Clima e tempo", "Astronomia básica", "Recursos naturais"])
        ]),

        // =====================================================================
        // 4) Historia (Ensino Medio)
        // =====================================================================
        new("História", [
            new(1, "EM", ["Antiguidade", "Idade Média", "Feudalismo", "Renascimento"]),
            new(2, "EM", ["Iluminismo", "Revolução Francesa", "Revolução Industrial", "Imperialismo"]),
            new(3, "EM", ["Brasil Colônia", "Independência", "Império", "Abolição/Escravidão"]),
            new(4, "EM", ["República", "Era Vargas", "Ditadura", "Nova República"])
        ]),

        // =====================================================================
        // 5) Geografia (Ensino Medio)
        // =====================================================================
        new("Geografia", [
            new(1, "EM", ["Cartografia", "Escalas", "Coordenadas", "Fusos horários"]),
            new(2, "EM", ["Climas", "Vegetação", "Biomas do Brasil", "Hidrografia"]),
            new(3, "EM", ["População", "Urbanização", "Migrações", "Indicadores (IDH/PIB)"]),
            new(4, "EM", ["Globalização", "Geopolítica", "Economia (setores)", "Meio ambiente"])
        ]),

        // =====================================================================
        // 6) Fisica (Ensino Medio)
        // =====================================================================
        new("Física", [
            new(1, "EM", ["Cinemática", "MU/MUV", "Gráficos", "Queda livre (intro)"]),
            new(2, "EM", ["Dinâmica", "Leis de Newton", "Trabalho/Energia", "Impulso/QdM"]),
            new(3, "EM", ["Hidrostática", "Termologia", "Calorimetria", "Mudanças de estado"]),
            new(4, "EM", ["Óptica", "Ondulatória", "Eletrostática (básico)", "Eletrodinâmica (básico)"])
        ]),

        // =====================================================================
        // 7) Quimica (Ensino Medio)
        // =====================================================================
        new("Química", [
            new(1, "EM", ["Estrutura atômica", "Tabela periódica", "Ligações", "Geometria molecular (básico)"]),
            new(2, "EM", ["Funções inorgânicas", "Reações", "Balanceamento", "Nomenclatura (básico)"]),
            new(3, "EM", ["Estequiometria", "Soluções/concentração", "Gases (básico)", "pH/pOH (intro)"]),
            new(4, "EM", ["Orgânica (intro)", "Hidrocarbonetos", "Funções orgânicas (básico)", "Química ambiental"])
        ]),

        // =====================================================================
        // 8) Biologia (Ensino Medio)
        // =====================================================================
        new("Biologia", [
            new(1, "EM", ["Citologia", "Organelas", "Membrana", "Metabolismo celular (intro)"]),
            new(2, "EM", ["Genética", "DNA/RNA", "Mendel", "Hereditariedade"]),
            new(3, "EM", ["Ecologia", "Evolução", "Seleção natural", "Relações ecológicas"]),
            new(4, "EM", ["Fisiologia humana", "Sistemas", "Imunologia (básico)", "Saúde"])
        ]),

        // =====================================================================
        // 9) Ingles (Ensino Medio)
        // =====================================================================
        new("Inglês", [
            new(1, "EM", ["Reading", "Cognatos/false friends", "Vocabulário por contexto", "Compreensão geral"]),
            new(2, "EM", ["Simple present", "Simple past", "Questions", "Modal verbs"]),
            new(3, "EM", ["Conditionals 0/1", "Passive (básico)", "Reported (básico)", "Linking words"]),
            new(4, "EM", ["Vocabulary por temas", "Phrasal verbs", "Inference", "Text genres"])
        ]),

        // =====================================================================
        // 10) Filosofia e Sociologia (Ensino Medio)
        // =====================================================================
        new("Filosofia e Sociologia", [
            new(1, "EM", ["Ética", "Moral", "Cidadania", "Direitos humanos"]),
            new(2, "EM", ["Estado e poder", "Democracia", "Ideologias (intro)", "Contrato social (intro)"]),
            new(3, "EM", ["Cultura e sociedade", "Identidade", "Socialização", "Preconceito/discriminação"]),
            new(4, "EM", ["Trabalho e capitalismo", "Indústria cultural (intro)", "Globalização e sociedade", "Movimentos sociais"])
        ])
    ];
}
