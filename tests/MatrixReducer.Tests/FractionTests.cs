namespace MatrixReducer.Tests;

public class FractionTests
{
    // ==================== 构造与约分 ====================

    [Fact]
    public void Constructor_BasicFraction_Reduces()
    {
        var f = new Fraction(2, 4);
        Assert.Equal(1, f.Numerator);
        Assert.Equal(2, f.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_NormalizesToNumerator()
    {
        var f = new Fraction(3, -5);
        Assert.Equal(-3, f.Numerator);
        Assert.Equal(5, f.Denominator);
    }

    [Fact]
    public void Constructor_BothNegative_BecomesPositive()
    {
        var f = new Fraction(-6, -8);
        Assert.Equal(3, f.Numerator);
        Assert.Equal(4, f.Denominator);
    }

    [Fact]
    public void Constructor_Zero_NormalizedToZeroOverOne()
    {
        var f = new Fraction(0, 99);
        Assert.Equal(0, f.Numerator);
        Assert.Equal(1, f.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Fraction(1, 0));
    }

    [Fact]
    public void Constructor_IntegerImplicit()
    {
        Fraction f = 7;
        Assert.Equal(7, f.Numerator);
        Assert.Equal(1, f.Denominator);
    }

    // ==================== 算术运算 ====================

    [Fact]
    public void Addition_Basic()
    {
        var a = new Fraction(1, 3);
        var b = new Fraction(1, 6);
        var result = a + b;
        Assert.Equal(new Fraction(1, 2), result);
    }

    [Fact]
    public void Addition_WithNegative()
    {
        var a = new Fraction(1, 2);
        var b = new Fraction(-3, 4);
        var result = a + b;
        Assert.Equal(new Fraction(-1, 4), result);
    }

    [Fact]
    public void Subtraction_Basic()
    {
        var a = new Fraction(3, 4);
        var b = new Fraction(1, 4);
        var result = a - b;
        Assert.Equal(new Fraction(1, 2), result);
    }

    [Fact]
    public void Subtraction_ResultZero()
    {
        var a = new Fraction(5, 7);
        var result = a - a;
        Assert.True(result.IsZero);
    }

    [Fact]
    public void Multiplication_Basic()
    {
        var a = new Fraction(2, 3);
        var b = new Fraction(3, 5);
        var result = a * b;
        Assert.Equal(new Fraction(2, 5), result);
    }

    [Fact]
    public void Multiplication_ByZero()
    {
        var a = new Fraction(7, 11);
        var result = a * Fraction.Zero;
        Assert.True(result.IsZero);
    }

    [Fact]
    public void Division_Basic()
    {
        var a = new Fraction(2, 3);
        var b = new Fraction(4, 5);
        var result = a / b;
        Assert.Equal(new Fraction(5, 6), result);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        var a = new Fraction(1, 2);
        Assert.Throws<DivideByZeroException>(() => a / Fraction.Zero);
    }

    [Fact]
    public void Negation()
    {
        var a = new Fraction(3, 7);
        var neg = -a;
        Assert.Equal(new Fraction(-3, 7), neg);
    }

    [Fact]
    public void Negation_OfNegative()
    {
        var a = new Fraction(-3, 7);
        var neg = -a;
        Assert.Equal(new Fraction(3, 7), neg);
    }

    // ==================== 比较与相等 ====================

    [Fact]
    public void Equality_EquivalentFractions()
    {
        Assert.Equal(new Fraction(2, 4), new Fraction(1, 2));
    }

    [Fact]
    public void Equality_DifferentFractions()
    {
        Assert.NotEqual(new Fraction(1, 3), new Fraction(1, 4));
    }

    [Fact]
    public void CompareTo_LessThan()
    {
        var a = new Fraction(1, 3);
        var b = new Fraction(1, 2);
        Assert.True(a.CompareTo(b) < 0);
    }

    [Fact]
    public void CompareTo_Equal()
    {
        var a = new Fraction(2, 6);
        var b = new Fraction(1, 3);
        Assert.Equal(0, a.CompareTo(b));
    }

    // ==================== ToString ====================

    [Fact]
    public void ToString_Integer()
    {
        Assert.Equal("5", new Fraction(5).ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        Assert.Equal("3/7", new Fraction(3, 7).ToString());
    }

    [Fact]
    public void ToString_Negative()
    {
        Assert.Equal("-2/3", new Fraction(-2, 3).ToString());
    }

    // ==================== Parse ====================

    [Theory]
    [InlineData("5", 5, 1)]
    [InlineData("-3", -3, 1)]
    [InlineData("0", 0, 1)]
    [InlineData("1/3", 1, 3)]
    [InlineData("-5/7", -5, 7)]
    [InlineData("6/3", 2, 1)]
    [InlineData(" 2 / 4 ", 1, 2)]
    public void Parse_ValidInputs(string input, long expectedNum, long expectedDen)
    {
        var f = Fraction.Parse(input);
        Assert.Equal(expectedNum, f.Numerator);
        Assert.Equal(expectedDen, f.Denominator);
    }

    [Theory]
    [InlineData("0.5", 1, 2)]
    [InlineData("0.25", 1, 4)]
    [InlineData("1.5", 3, 2)]
    [InlineData("-2.25", -9, 4)]
    public void Parse_DecimalInputs(string input, long expectedNum, long expectedDen)
    {
        var f = Fraction.Parse(input);
        Assert.Equal(expectedNum, f.Numerator);
        Assert.Equal(expectedDen, f.Denominator);
    }

    [Fact]
    public void Parse_InvalidFormat_Throws()
    {
        Assert.Throws<FormatException>(() => Fraction.Parse("abc"));
    }

    // ==================== IsZero ====================

    [Fact]
    public void IsZero_True()
    {
        Assert.True(Fraction.Zero.IsZero);
        Assert.True(new Fraction(0, 5).IsZero);
    }

    [Fact]
    public void IsZero_False()
    {
        Assert.False(new Fraction(1, 100).IsZero);
    }
}
