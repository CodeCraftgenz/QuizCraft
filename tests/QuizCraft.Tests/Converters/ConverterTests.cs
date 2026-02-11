using System.Globalization;
using System.Windows;
using QuizCraft.Presentation.Converters;

namespace QuizCraft.Tests.Converters;

public class ConverterTests
{
    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    public void BoolToVisibility_ConvertsCorrectly(bool input, Visibility expected)
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(input, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, Visibility.Collapsed)]
    [InlineData(false, Visibility.Visible)]
    public void BoolToVisibility_Inverted_ConvertsCorrectly(bool input, Visibility expected)
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(input, typeof(Visibility), "Invert", CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InverseBool_Inverts(bool input, bool expected)
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(90.0)]
    [InlineData(70.0)]
    [InlineData(50.0)]
    [InlineData(20.0)]
    public void PercentageToColor_ReturnsBrush(double pct)
    {
        var converter = new PercentageToColorConverter();
        var result = converter.Convert(pct, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.IsType<System.Windows.Media.SolidColorBrush>(result);
    }

    [Fact]
    public void BoolToCorrectText_ReturnsCorrectStrings()
    {
        var converter = new BoolToCorrectTextConverter();
        Assert.Equal("Correto", converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("Incorreto", converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(0, "Novo")]
    [InlineData(1, "Iniciante")]
    [InlineData(2, "Aprendiz")]
    [InlineData(3, "Intermediário")]
    [InlineData(4, "Avançado")]
    [InlineData(5, "Dominado")]
    [InlineData(99, "?")]
    public void MasteryLevelToText_ReturnsCorrectLabel(int level, string expected)
    {
        var converter = new MasteryLevelToTextConverter();
        var result = converter.Convert(level, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NullToVisibility_NullCollapses()
    {
        var converter = new NullToVisibilityConverter();
        Assert.Equal(Visibility.Collapsed, converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("something", typeof(Visibility), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullToVisibility_Inverted()
    {
        var converter = new NullToVisibilityConverter();
        Assert.Equal(Visibility.Visible, converter.Convert(null, typeof(Visibility), "Invert", CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(75.5, "75.5%")]
    [InlineData(0.0, "0.0%")]
    [InlineData(100.0, "100.0%")]
    public void DoubleToPercentage_FormatsCorrectly(double input, string expected)
    {
        var converter = new DoubleToPercentageConverter();
        var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.0, 160.0)]
    [InlineData(50.0, 80.0)]
    [InlineData(0.0, 4.0)]
    public void PercentageToHeight_ConvertsCorrectly(double input, double expected)
    {
        var converter = new PercentageToHeightConverter();
        var result = (double)converter.Convert(input, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result, 1);
    }

    [Theory]
    [InlineData(100.0, 200.0)]
    [InlineData(50.0, 100.0)]
    [InlineData(0.0, 2.0)]
    public void PercentageToBarWidth_ConvertsCorrectly(double input, double expected)
    {
        var converter = new PercentageToBarWidthConverter();
        var result = (double)converter.Convert(input, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result, 1);
    }
}
