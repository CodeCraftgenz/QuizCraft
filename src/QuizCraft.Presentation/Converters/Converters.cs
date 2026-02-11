using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using QuizCraft.Domain.Enums;

namespace QuizCraft.Presentation.Converters;

/// <summary>
/// Converte booleano para Visibility WPF. Suporta parâmetro "Invert" para inverter a lógica.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>Converte bool para Visible/Collapsed. Inverte com parâmetro "Invert".</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        // Inverte o resultado se o parâmetro for "Invert"
        if (parameter?.ToString() == "Invert") boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Inverte um valor booleano (true vira false e vice-versa).</summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>
/// Converte percentual (0-100) para cor indicativa: verde (>=80), amarelo (>=60), laranja (>=40), vermelho (<40).
/// </summary>
public class PercentageToColorConverter : IValueConverter
{
    /// <summary>Retorna um SolidColorBrush baseado na faixa do percentual.</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double pct) return Brushes.Gray;
        return pct switch
        {
            >= 80 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Verde
            >= 60 => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Amarelo
            >= 40 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Laranja
            _ => new SolidColorBrush(Color.FromRgb(244, 67, 54))        // Vermelho
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte booleano para texto "Correto" ou "Incorreto".</summary>
public class BoolToCorrectTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "Correto" : "Incorreto";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte booleano para cor: verde (correto) ou vermelho (incorreto).</summary>
public class BoolToCorrectColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte nível de domínio (0-5) para texto descritivo em português.</summary>
public class MasteryLevelToTextConverter : IValueConverter
{
    /// <summary>Retorna o nome do nível: Novo, Iniciante, Aprendiz, Intermediário, Avançado ou Dominado.</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int level) return "?";
        return level switch
        {
            0 => "Novo",
            1 => "Iniciante",
            2 => "Aprendiz",
            3 => "Intermediário",
            4 => "Avançado",
            5 => "Dominado",
            _ => "?"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte null para Collapsed e não-null para Visible. Suporta parâmetro "Invert".</summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>Oculta o elemento se o valor for null (ou inverte com parâmetro "Invert").</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isNull = value == null;
        if (parameter?.ToString() == "Invert") isNull = !isNull;
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte um double para texto percentual formatado (ex: "85.3%").</summary>
public class DoubleToPercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? d.ToString("F1", culture) + "%" : "0%";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converte percentual (0-100) para altura em pixels para gráficos de barras. Altura máxima = 160px.
/// </summary>
public class PercentageToHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return Math.Max(4, d / 100.0 * 160);
        return 4.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converte percentual (0-100) para largura em pixels para barras horizontais. Largura máxima = 200px.
/// </summary>
public class PercentageToBarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return Math.Max(2, d / 100.0 * 200);
        return 2.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Compara dois valores e retorna true se forem iguais. Usado em MultiBinding.</summary>
public class EqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.Length == 2 && Equals(values[0], values[1]);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converte QuizMode para texto em português.
/// </summary>
public class QuizModeToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not QuizMode mode) return "?";
        return mode switch
        {
            QuizMode.Training => "Treino",
            QuizMode.Exam => "Prova",
            QuizMode.ErrorReview => "Revisão de Erros",
            QuizMode.SpacedReview => "Revisão Espaçada",
            _ => mode.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converte QuestionType para texto em português.
/// </summary>
public class QuestionTypeToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not QuestionType type) return "?";
        return type switch
        {
            QuestionType.MultipleChoice => "Múltipla Escolha",
            QuestionType.TrueFalse => "Verdadeiro/Falso",
            QuestionType.ShortAnswer => "Resposta Curta",
            QuestionType.MultipleSelection => "Múltipla Seleção",
            _ => type.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converte null ou string vazia para Collapsed. Suporta parâmetro "Invert".</summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>Oculta o elemento se o valor for null ou string vazia/espaços.</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEmpty = value == null || (value is string s && string.IsNullOrWhiteSpace(s));
        if (parameter?.ToString() == "Invert") isEmpty = !isEmpty;
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
