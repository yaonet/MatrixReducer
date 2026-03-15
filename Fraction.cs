using System;

namespace MatrixReducer;

/// <summary>
/// 用分数表示精确的有理数运算，避免浮点误差。
/// </summary>
public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
{
    public long Numerator { get; }
    public long Denominator { get; }

    public static readonly Fraction Zero = new(0, 1);
    public static readonly Fraction One = new(1, 1);

    public Fraction(long numerator, long denominator = 1)
    {
        if (denominator == 0)
            throw new DivideByZeroException("分母不能为零。");

        if (numerator == 0)
        {
            Numerator = 0;
            Denominator = 1;
            return;
        }

        // 保证分母为正
        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        long gcd = Gcd(Math.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    public bool IsZero => Numerator == 0;

    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }

    public static Fraction operator +(Fraction a, Fraction b) =>
        new(a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator);

    public static Fraction operator -(Fraction a, Fraction b) =>
        new(a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator);

    public static Fraction operator *(Fraction a, Fraction b) =>
        new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    public static Fraction operator /(Fraction a, Fraction b)
    {
        if (b.IsZero) throw new DivideByZeroException();
        return new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    public static Fraction operator -(Fraction a) =>
        new(-a.Numerator, a.Denominator);

    public static implicit operator Fraction(long value) => new(value);
    public static implicit operator Fraction(int value) => new(value);

    public bool Equals(Fraction other) =>
        Numerator == other.Numerator && Denominator == other.Denominator;

    public override bool Equals(object? obj) => obj is Fraction f && Equals(f);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    public static bool operator ==(Fraction a, Fraction b) => a.Equals(b);
    public static bool operator !=(Fraction a, Fraction b) => !a.Equals(b);

    public int CompareTo(Fraction other) =>
        (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

    public override string ToString()
    {
        if (Denominator == 1) return Numerator.ToString();
        return $"{Numerator}/{Denominator}";
    }

    /// <summary>
    /// 从字符串解析分数，支持 "3", "-2", "1/3", "-5/7" 等格式。
    /// </summary>
    public static Fraction Parse(string s)
    {
        s = s.Trim();
        if (s.Contains('/'))
        {
            var parts = s.Split('/');
            if (parts.Length != 2)
                throw new FormatException($"无法解析分数: {s}");
            return new Fraction(long.Parse(parts[0].Trim()), long.Parse(parts[1].Trim()));
        }
        // 支持小数输入
        if (s.Contains('.'))
        {
            var parts = s.Split('.');
            string decimalPart = parts.Length > 1 ? parts[1] : "";
            long pow = 1;
            for (int i = 0; i < decimalPart.Length; i++) pow *= 10;
            long whole = long.Parse(parts[0]);
            long frac = decimalPart.Length > 0 ? long.Parse(decimalPart) : 0;
            if (whole < 0 || s.StartsWith("-"))
                return new Fraction(whole * pow - frac, pow);
            return new Fraction(whole * pow + frac, pow);
        }
        return new Fraction(long.Parse(s));
    }
}
