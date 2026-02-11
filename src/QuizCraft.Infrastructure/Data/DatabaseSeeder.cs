using Microsoft.EntityFrameworkCore;
using QuizCraft.Domain.Entities;
using QuizCraft.Domain.Enums;

namespace QuizCraft.Infrastructure.Data;

/// <summary>
/// Responsável por popular o banco de dados com dados de exemplo para testes.
/// Cria matérias, tópicos, questões diversificadas e tags.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Popula o banco com dados iniciais caso esteja vazio.
    /// </summary>
    public static async Task SeedAsync(QuizCraftDbContext context)
    {
        // Só popula se não houver dados
        if (await context.Subjects.AnyAsync())
            return;

        // === MATÉRIAS ===
        var matematica = new Subject { Name = "Matemática", Description = "Números, álgebra, geometria e cálculo", Color = "#2196F3" };
        var portugues = new Subject { Name = "Português", Description = "Gramática, interpretação e literatura", Color = "#4CAF50" };
        var historia = new Subject { Name = "História", Description = "História do Brasil e do mundo", Color = "#FF9800" };
        var ciencias = new Subject { Name = "Ciências", Description = "Física, química e biologia", Color = "#9C27B0" };
        var geografia = new Subject { Name = "Geografia", Description = "Geografia física e humana", Color = "#F44336" };

        context.Subjects.AddRange(matematica, portugues, historia, ciencias, geografia);
        await context.SaveChangesAsync();

        // === TÓPICOS ===
        // Matemática
        var aritmetica = new Topic { SubjectId = matematica.Id, Name = "Aritmética" };
        var algebra = new Topic { SubjectId = matematica.Id, Name = "Álgebra" };
        var geometria = new Topic { SubjectId = matematica.Id, Name = "Geometria" };
        var probabilidade = new Topic { SubjectId = matematica.Id, Name = "Probabilidade e Estatística" };

        // Português
        var gramatica = new Topic { SubjectId = portugues.Id, Name = "Gramática" };
        var interpretacao = new Topic { SubjectId = portugues.Id, Name = "Interpretação de Texto" };
        var literatura = new Topic { SubjectId = portugues.Id, Name = "Literatura Brasileira" };

        // História
        var brasilColonia = new Topic { SubjectId = historia.Id, Name = "Brasil Colônia" };
        var brasilRepublica = new Topic { SubjectId = historia.Id, Name = "Brasil República" };
        var guerraMundial = new Topic { SubjectId = historia.Id, Name = "Guerras Mundiais" };

        // Ciências
        var fisica = new Topic { SubjectId = ciencias.Id, Name = "Física" };
        var quimica = new Topic { SubjectId = ciencias.Id, Name = "Química" };
        var biologia = new Topic { SubjectId = ciencias.Id, Name = "Biologia" };

        // Geografia
        var geoFisica = new Topic { SubjectId = geografia.Id, Name = "Geografia Física" };
        var geopolitica = new Topic { SubjectId = geografia.Id, Name = "Geopolítica" };

        context.Topics.AddRange(
            aritmetica, algebra, geometria, probabilidade,
            gramatica, interpretacao, literatura,
            brasilColonia, brasilRepublica, guerraMundial,
            fisica, quimica, biologia,
            geoFisica, geopolitica);
        await context.SaveChangesAsync();

        // === TAGS ===
        var tagEnem = new Tag { Name = "ENEM" };
        var tagVestibular = new Tag { Name = "Vestibular" };
        var tagBasico = new Tag { Name = "Básico" };
        var tagAvancado = new Tag { Name = "Avançado" };
        var tagRevisao = new Tag { Name = "Revisão" };

        context.Tags.AddRange(tagEnem, tagVestibular, tagBasico, tagAvancado, tagRevisao);
        await context.SaveChangesAsync();

        // === QUESTÕES ===
        var questoes = new List<(Question q, Choice[] choices, Tag[] tags)>
        {
            // --- MATEMÁTICA - Aritmética ---
            (new Question { TopicId = aritmetica.Id, Type = QuestionType.MultipleChoice, Difficulty = 1,
                Statement = "Quanto é 15 × 8?",
                Explanation = "15 × 8 = 120. Pode-se decompor: 15 × 8 = (10 × 8) + (5 × 8) = 80 + 40 = 120.",
                Source = "Ensino Fundamental" },
            [new Choice { Text = "120", IsCorrect = true, Order = 0 },
             new Choice { Text = "115", IsCorrect = false, Order = 1 },
             new Choice { Text = "125", IsCorrect = false, Order = 2 },
             new Choice { Text = "130", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = aritmetica.Id, Type = QuestionType.TrueFalse, Difficulty = 1,
                Statement = "Todo número par é divisível por 2.",
                Explanation = "Por definição, um número par é aquele que é divisível por 2." },
            [new Choice { Text = "Verdadeiro", IsCorrect = true, Order = 0 },
             new Choice { Text = "Falso", IsCorrect = false, Order = 1 }],
            [tagBasico]),

            (new Question { TopicId = aritmetica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é o MMC (Mínimo Múltiplo Comum) de 12 e 18?",
                Explanation = "12 = 2² × 3 e 18 = 2 × 3². MMC = 2² × 3² = 4 × 9 = 36.",
                Source = "ENEM 2019" },
            [new Choice { Text = "36", IsCorrect = true, Order = 0 },
             new Choice { Text = "24", IsCorrect = false, Order = 1 },
             new Choice { Text = "48", IsCorrect = false, Order = 2 },
             new Choice { Text = "72", IsCorrect = false, Order = 3 }],
            [tagEnem, tagBasico]),

            // --- MATEMÁTICA - Álgebra ---
            (new Question { TopicId = algebra.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Resolva a equação: 2x + 5 = 17. Qual o valor de x?",
                Explanation = "2x + 5 = 17 → 2x = 12 → x = 6." },
            [new Choice { Text = "6", IsCorrect = true, Order = 0 },
             new Choice { Text = "5", IsCorrect = false, Order = 1 },
             new Choice { Text = "7", IsCorrect = false, Order = 2 },
             new Choice { Text = "8", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = algebra.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual a solução da equação quadrática x² - 5x + 6 = 0?",
                Explanation = "Fatorando: (x - 2)(x - 3) = 0, logo x = 2 ou x = 3.",
                Source = "Vestibular" },
            [new Choice { Text = "x = 2 e x = 3", IsCorrect = true, Order = 0 },
             new Choice { Text = "x = 1 e x = 6", IsCorrect = false, Order = 1 },
             new Choice { Text = "x = -2 e x = -3", IsCorrect = false, Order = 2 },
             new Choice { Text = "x = 5 e x = 1", IsCorrect = false, Order = 3 }],
            [tagVestibular]),

            (new Question { TopicId = algebra.Id, Type = QuestionType.ShortAnswer, Difficulty = 2,
                Statement = "Se f(x) = 3x + 2, quanto vale f(4)?",
                Explanation = "f(4) = 3(4) + 2 = 12 + 2 = 14." },
            [new Choice { Text = "14", IsCorrect = true, Order = 0 }],
            [tagBasico]),

            // --- MATEMÁTICA - Geometria ---
            (new Question { TopicId = geometria.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é a área de um triângulo com base 10 cm e altura 6 cm?",
                Explanation = "Área = (base × altura) / 2 = (10 × 6) / 2 = 30 cm²." },
            [new Choice { Text = "30 cm²", IsCorrect = true, Order = 0 },
             new Choice { Text = "60 cm²", IsCorrect = false, Order = 1 },
             new Choice { Text = "16 cm²", IsCorrect = false, Order = 2 },
             new Choice { Text = "36 cm²", IsCorrect = false, Order = 3 }],
            [tagBasico, tagEnem]),

            (new Question { TopicId = geometria.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Em um triângulo retângulo, os catetos medem 3 e 4. Qual é a hipotenusa?",
                Explanation = "Pelo Teorema de Pitágoras: h² = 3² + 4² = 9 + 16 = 25, logo h = 5.",
                Source = "ENEM" },
            [new Choice { Text = "5", IsCorrect = true, Order = 0 },
             new Choice { Text = "6", IsCorrect = false, Order = 1 },
             new Choice { Text = "7", IsCorrect = false, Order = 2 },
             new Choice { Text = "12", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            // --- MATEMÁTICA - Probabilidade ---
            (new Question { TopicId = probabilidade.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Ao lançar dois dados, qual a probabilidade de a soma ser 7?",
                Explanation = "Existem 6 combinações que somam 7 (1+6, 2+5, 3+4, 4+3, 5+2, 6+1) de 36 possíveis. P = 6/36 = 1/6." },
            [new Choice { Text = "1/6", IsCorrect = true, Order = 0 },
             new Choice { Text = "1/12", IsCorrect = false, Order = 1 },
             new Choice { Text = "1/4", IsCorrect = false, Order = 2 },
             new Choice { Text = "1/3", IsCorrect = false, Order = 3 }],
            [tagEnem, tagAvancado]),

            // --- PORTUGUÊS - Gramática ---
            (new Question { TopicId = gramatica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é a classe gramatical da palavra 'rapidamente'?",
                Explanation = "Palavras terminadas em '-mente' são advérbios de modo." },
            [new Choice { Text = "Advérbio", IsCorrect = true, Order = 0 },
             new Choice { Text = "Adjetivo", IsCorrect = false, Order = 1 },
             new Choice { Text = "Substantivo", IsCorrect = false, Order = 2 },
             new Choice { Text = "Verbo", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = gramatica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Identifique a frase com sujeito indeterminado:",
                Explanation = "Na frase 'Precisa-se de funcionários', o 'se' é índice de indeterminação do sujeito." },
            [new Choice { Text = "Precisa-se de funcionários.", IsCorrect = true, Order = 0 },
             new Choice { Text = "O aluno estudou muito.", IsCorrect = false, Order = 1 },
             new Choice { Text = "Choveu ontem à noite.", IsCorrect = false, Order = 2 },
             new Choice { Text = "Nós chegamos cedo.", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            (new Question { TopicId = gramatica.Id, Type = QuestionType.TrueFalse, Difficulty = 1,
                Statement = "A palavra 'porque' (junto e sem acento) é usada para dar explicações e causas.",
                Explanation = "'Porque' é uma conjunção causal/explicativa. Ex: 'Não fui porque estava chovendo.'" },
            [new Choice { Text = "Verdadeiro", IsCorrect = true, Order = 0 },
             new Choice { Text = "Falso", IsCorrect = false, Order = 1 }],
            [tagBasico, tagRevisao]),

            // --- PORTUGUÊS - Interpretação ---
            (new Question { TopicId = interpretacao.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Em uma redação dissertativa-argumentativa, qual é a função do parágrafo de desenvolvimento?",
                Explanation = "O desenvolvimento apresenta os argumentos que sustentam a tese apresentada na introdução." },
            [new Choice { Text = "Apresentar argumentos que sustentem a tese", IsCorrect = true, Order = 0 },
             new Choice { Text = "Resumir o texto inteiro", IsCorrect = false, Order = 1 },
             new Choice { Text = "Apresentar o tema pela primeira vez", IsCorrect = false, Order = 2 },
             new Choice { Text = "Propor uma solução para o problema", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            // --- PORTUGUÊS - Literatura ---
            (new Question { TopicId = literatura.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual movimento literário brasileiro é representado por Machado de Assis?",
                Explanation = "Machado de Assis é o principal representante do Realismo brasileiro, com obras como 'Dom Casmurro' e 'Memórias Póstumas de Brás Cubas'." },
            [new Choice { Text = "Realismo", IsCorrect = true, Order = 0 },
             new Choice { Text = "Romantismo", IsCorrect = false, Order = 1 },
             new Choice { Text = "Modernismo", IsCorrect = false, Order = 2 },
             new Choice { Text = "Naturalismo", IsCorrect = false, Order = 3 }],
            [tagVestibular, tagEnem]),

            (new Question { TopicId = literatura.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Quem escreveu 'Grande Sertão: Veredas'?",
                Explanation = "Grande Sertão: Veredas (1956) é a obra-prima de João Guimarães Rosa." },
            [new Choice { Text = "Guimarães Rosa", IsCorrect = true, Order = 0 },
             new Choice { Text = "Graciliano Ramos", IsCorrect = false, Order = 1 },
             new Choice { Text = "Jorge Amado", IsCorrect = false, Order = 2 },
             new Choice { Text = "Clarice Lispector", IsCorrect = false, Order = 3 }],
            [tagVestibular]),

            // --- HISTÓRIA - Brasil Colônia ---
            (new Question { TopicId = brasilColonia.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Em que ano Pedro Álvares Cabral chegou ao Brasil?",
                Explanation = "A frota de Cabral chegou ao litoral brasileiro em 22 de abril de 1500." },
            [new Choice { Text = "1500", IsCorrect = true, Order = 0 },
             new Choice { Text = "1492", IsCorrect = false, Order = 1 },
             new Choice { Text = "1510", IsCorrect = false, Order = 2 },
             new Choice { Text = "1530", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = brasilColonia.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual foi o principal objetivo das Capitanias Hereditárias?",
                Explanation = "As Capitanias Hereditárias (1534) foram criadas para colonizar e defender o território com menor custo para a Coroa Portuguesa.",
                Source = "ENEM 2020" },
            [new Choice { Text = "Colonizar e proteger o território com menor custo para Portugal", IsCorrect = true, Order = 0 },
             new Choice { Text = "Estabelecer um sistema democrático de governo", IsCorrect = false, Order = 1 },
             new Choice { Text = "Promover a industrialização da colônia", IsCorrect = false, Order = 2 },
             new Choice { Text = "Criar universidades no Brasil", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            // --- HISTÓRIA - Brasil República ---
            (new Question { TopicId = brasilRepublica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Quem proclamou a República do Brasil em 15 de novembro de 1889?",
                Explanation = "O Marechal Deodoro da Fonseca liderou o golpe que proclamou a República e se tornou o primeiro presidente." },
            [new Choice { Text = "Marechal Deodoro da Fonseca", IsCorrect = true, Order = 0 },
             new Choice { Text = "Dom Pedro II", IsCorrect = false, Order = 1 },
             new Choice { Text = "Floriano Peixoto", IsCorrect = false, Order = 2 },
             new Choice { Text = "Rui Barbosa", IsCorrect = false, Order = 3 }],
            [tagBasico, tagEnem]),

            (new Question { TopicId = brasilRepublica.Id, Type = QuestionType.TrueFalse, Difficulty = 2,
                Statement = "A Era Vargas durou de 1930 a 1945.",
                Explanation = "Getúlio Vargas governou de 1930 a 1945, incluindo o Estado Novo (1937-1945). Retornou eleito em 1951." },
            [new Choice { Text = "Verdadeiro", IsCorrect = true, Order = 0 },
             new Choice { Text = "Falso", IsCorrect = false, Order = 1 }],
            [tagEnem]),

            // --- HISTÓRIA - Guerras Mundiais ---
            (new Question { TopicId = guerraMundial.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual evento é considerado o estopim da Primeira Guerra Mundial (1914)?",
                Explanation = "O assassinato do Arquiduque Francisco Ferdinando da Áustria em Sarajevo, em 28 de junho de 1914." },
            [new Choice { Text = "Assassinato do Arquiduque Francisco Ferdinando", IsCorrect = true, Order = 0 },
             new Choice { Text = "Invasão da Polônia pela Alemanha", IsCorrect = false, Order = 1 },
             new Choice { Text = "Queda da Bolsa de Nova York", IsCorrect = false, Order = 2 },
             new Choice { Text = "Revolução Russa", IsCorrect = false, Order = 3 }],
            [tagVestibular]),

            // --- CIÊNCIAS - Física ---
            (new Question { TopicId = fisica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é a unidade de medida da força no Sistema Internacional?",
                Explanation = "A unidade de força no SI é o Newton (N), definido como 1 kg⋅m/s²." },
            [new Choice { Text = "Newton (N)", IsCorrect = true, Order = 0 },
             new Choice { Text = "Joule (J)", IsCorrect = false, Order = 1 },
             new Choice { Text = "Watt (W)", IsCorrect = false, Order = 2 },
             new Choice { Text = "Pascal (Pa)", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = fisica.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Um carro acelera de 0 a 72 km/h em 10 segundos. Qual a aceleração?",
                Explanation = "72 km/h = 20 m/s. Aceleração = ΔV/Δt = 20/10 = 2 m/s².",
                Source = "ENEM 2021" },
            [new Choice { Text = "2 m/s²", IsCorrect = true, Order = 0 },
             new Choice { Text = "7,2 m/s²", IsCorrect = false, Order = 1 },
             new Choice { Text = "20 m/s²", IsCorrect = false, Order = 2 },
             new Choice { Text = "0,2 m/s²", IsCorrect = false, Order = 3 }],
            [tagEnem, tagAvancado]),

            (new Question { TopicId = fisica.Id, Type = QuestionType.ShortAnswer, Difficulty = 1,
                Statement = "Qual é a velocidade da luz no vácuo em km/s (aproximadamente)?",
                Explanation = "A velocidade da luz no vácuo é aproximadamente 300.000 km/s (3 × 10⁸ m/s)." },
            [new Choice { Text = "300000", IsCorrect = true, Order = 0 }],
            [tagBasico]),

            // --- CIÊNCIAS - Química ---
            (new Question { TopicId = quimica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é a fórmula química da água?",
                Explanation = "A água é composta por 2 átomos de hidrogênio e 1 de oxigênio: H₂O." },
            [new Choice { Text = "H₂O", IsCorrect = true, Order = 0 },
             new Choice { Text = "CO₂", IsCorrect = false, Order = 1 },
             new Choice { Text = "NaCl", IsCorrect = false, Order = 2 },
             new Choice { Text = "O₂", IsCorrect = false, Order = 3 }],
            [tagBasico]),

            (new Question { TopicId = quimica.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual é o pH de uma solução neutra a 25°C?",
                Explanation = "Em uma solução neutra, [H⁺] = [OH⁻] = 10⁻⁷ mol/L, logo pH = 7." },
            [new Choice { Text = "7", IsCorrect = true, Order = 0 },
             new Choice { Text = "0", IsCorrect = false, Order = 1 },
             new Choice { Text = "14", IsCorrect = false, Order = 2 },
             new Choice { Text = "1", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            (new Question { TopicId = quimica.Id, Type = QuestionType.TrueFalse, Difficulty = 2,
                Statement = "Os gases nobres são altamente reativos por possuírem camada de valência completa.",
                Explanation = "Falso. Justamente por possuírem camada de valência completa, os gases nobres são extremamente estáveis e pouco reativos." },
            [new Choice { Text = "Verdadeiro", IsCorrect = false, Order = 0 },
             new Choice { Text = "Falso", IsCorrect = true, Order = 1 }],
            [tagRevisao]),

            // --- CIÊNCIAS - Biologia ---
            (new Question { TopicId = biologia.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual organela celular é responsável pela produção de energia (ATP)?",
                Explanation = "A mitocôndria realiza a respiração celular, produzindo ATP a partir de glicose." },
            [new Choice { Text = "Mitocôndria", IsCorrect = true, Order = 0 },
             new Choice { Text = "Ribossomo", IsCorrect = false, Order = 1 },
             new Choice { Text = "Lisossomo", IsCorrect = false, Order = 2 },
             new Choice { Text = "Complexo de Golgi", IsCorrect = false, Order = 3 }],
            [tagBasico, tagEnem]),

            (new Question { TopicId = biologia.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Na genética, quando um gene é dominante (A) e outro recessivo (a), qual fenótipo aparece no heterozigoto Aa?",
                Explanation = "No heterozigoto Aa, o alelo dominante (A) se expressa, então o fenótipo é o do gene dominante." },
            [new Choice { Text = "Fenótipo dominante", IsCorrect = true, Order = 0 },
             new Choice { Text = "Fenótipo recessivo", IsCorrect = false, Order = 1 },
             new Choice { Text = "Mistura dos dois fenótipos", IsCorrect = false, Order = 2 },
             new Choice { Text = "Nenhum fenótipo se expressa", IsCorrect = false, Order = 3 }],
            [tagVestibular, tagAvancado]),

            // --- GEOGRAFIA - Física ---
            (new Question { TopicId = geoFisica.Id, Type = QuestionType.MultipleChoice, Difficulty = 2,
                Statement = "Qual é o maior bioma do Brasil em extensão territorial?",
                Explanation = "A Amazônia ocupa cerca de 49% do território brasileiro, sendo o maior bioma do país." },
            [new Choice { Text = "Amazônia", IsCorrect = true, Order = 0 },
             new Choice { Text = "Cerrado", IsCorrect = false, Order = 1 },
             new Choice { Text = "Mata Atlântica", IsCorrect = false, Order = 2 },
             new Choice { Text = "Caatinga", IsCorrect = false, Order = 3 }],
            [tagEnem, tagBasico]),

            (new Question { TopicId = geoFisica.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Qual fenômeno climático é causado pelo aquecimento anormal das águas do Oceano Pacífico?",
                Explanation = "O El Niño é o aquecimento anormal das águas superficiais do Pacífico Equatorial, afetando o clima mundial.",
                Source = "ENEM 2022" },
            [new Choice { Text = "El Niño", IsCorrect = true, Order = 0 },
             new Choice { Text = "La Niña", IsCorrect = false, Order = 1 },
             new Choice { Text = "Efeito estufa", IsCorrect = false, Order = 2 },
             new Choice { Text = "Inversão térmica", IsCorrect = false, Order = 3 }],
            [tagEnem]),

            // --- GEOGRAFIA - Geopolítica ---
            (new Question { TopicId = geopolitica.Id, Type = QuestionType.MultipleChoice, Difficulty = 3,
                Statement = "Quais países fazem parte do BRICS (configuração original)?",
                Explanation = "BRICS é a sigla para Brasil, Rússia, Índia, China e África do Sul." },
            [new Choice { Text = "Brasil, Rússia, Índia, China e África do Sul", IsCorrect = true, Order = 0 },
             new Choice { Text = "Brasil, Rússia, Indonésia, Canadá e Suíça", IsCorrect = false, Order = 1 },
             new Choice { Text = "Bolívia, Rússia, Irã, China e Sudão", IsCorrect = false, Order = 2 },
             new Choice { Text = "Brasil, Reino Unido, Índia, China e Singapura", IsCorrect = false, Order = 3 }],
            [tagEnem, tagAvancado]),

            (new Question { TopicId = geopolitica.Id, Type = QuestionType.TrueFalse, Difficulty = 2,
                Statement = "O Mercosul é um bloco econômico formado exclusivamente por países da América do Sul.",
                Explanation = "Verdadeiro. Os membros plenos do Mercosul (Brasil, Argentina, Uruguai e Paraguai) são todos sul-americanos." },
            [new Choice { Text = "Verdadeiro", IsCorrect = true, Order = 0 },
             new Choice { Text = "Falso", IsCorrect = false, Order = 1 }],
            [tagBasico]),
        };

        // Salva todas as questões com alternativas e tags
        foreach (var (question, choices, tags) in questoes)
        {
            context.Questions.Add(question);
            await context.SaveChangesAsync();

            foreach (var choice in choices)
            {
                choice.QuestionId = question.Id;
                context.Choices.Add(choice);
            }

            foreach (var tag in tags)
            {
                context.QuestionTags.Add(new QuestionTag
                {
                    QuestionId = question.Id,
                    TagId = tag.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
