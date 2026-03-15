namespace MatrixReducer.Tests;

public class MatrixOperationsTests
{
    /// <summary>
    /// 辅助方法：比较两个矩阵是否相等。
    /// </summary>
    private static void AssertMatrixEqual(Fraction[,] expected, Fraction[,] actual)
    {
        Assert.Equal(expected.GetLength(0), actual.GetLength(0));
        Assert.Equal(expected.GetLength(1), actual.GetLength(1));
        for (int i = 0; i < expected.GetLength(0); i++)
            for (int j = 0; j < expected.GetLength(1); j++)
                Assert.Equal(expected[i, j], actual[i, j]);
    }

    /// <summary>
    /// 辅助方法：快速构造矩阵。
    /// </summary>
    private static Fraction[,] MakeMatrix(Fraction[][] rows)
    {
        int r = rows.Length;
        int c = rows[0].Length;
        var m = new Fraction[r, c];
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                m[i, j] = rows[i][j];
        return m;
    }

    // ==================== ParseMatrix ====================

    [Fact]
    public void ParseMatrix_SpaceSeparated()
    {
        var m = MatrixOperations.ParseMatrix("1 2 3\n4 5 6");
        Assert.Equal(2, m.GetLength(0));
        Assert.Equal(3, m.GetLength(1));
        Assert.Equal(new Fraction(1), m[0, 0]);
        Assert.Equal(new Fraction(6), m[1, 2]);
    }

    [Fact]
    public void ParseMatrix_CommaSeparated()
    {
        var m = MatrixOperations.ParseMatrix("1,2,3\n4,5,6");
        Assert.Equal(2, m.GetLength(0));
        Assert.Equal(3, m.GetLength(1));
        Assert.Equal(new Fraction(2), m[0, 1]);
    }

    [Fact]
    public void ParseMatrix_WithFractions()
    {
        var m = MatrixOperations.ParseMatrix("1/2 1/3\n1/4 1/5");
        Assert.Equal(new Fraction(1, 2), m[0, 0]);
        Assert.Equal(new Fraction(1, 3), m[0, 1]);
        Assert.Equal(new Fraction(1, 4), m[1, 0]);
        Assert.Equal(new Fraction(1, 5), m[1, 1]);
    }

    [Fact]
    public void ParseMatrix_EmptyInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => MatrixOperations.ParseMatrix(""));
    }

    [Fact]
    public void ParseMatrix_InconsistentColumns_Throws()
    {
        Assert.Throws<ArgumentException>(() => MatrixOperations.ParseMatrix("1 2 3\n4 5"));
    }

    // ==================== 单位矩阵 ====================

    [Fact]
    public void Reduce_IdentityMatrix_RemainsIdentity()
    {
        var input = MakeMatrix([
            [1, 0, 0],
            [0, 1, 0],
            [0, 0, 1],
        ]);
        var result = MatrixOperations.Reduce(input);

        AssertMatrixEqual(input, result.Ref);
        AssertMatrixEqual(input, result.Rref);
        Assert.Equal(3, result.Rank);
    }

    // ==================== 零矩阵 ====================

    [Fact]
    public void Reduce_ZeroMatrix_RemainsZero()
    {
        var input = MakeMatrix([
            [0, 0, 0],
            [0, 0, 0],
        ]);
        var result = MatrixOperations.Reduce(input);

        AssertMatrixEqual(input, result.Ref);
        AssertMatrixEqual(input, result.Rref);
        Assert.Equal(0, result.Rank);
    }

    // ==================== 3×4 标准矩阵 ====================

    [Fact]
    public void Reduce_3x4Matrix_CorrectRREF()
    {
        // 输入:
        // 1  2 -1  3
        // 2  4  1  0
        // -1 -2  3 -6
        var input = MatrixOperations.ParseMatrix("1 2 -1 3\n2 4 1 0\n-1 -2 3 -6");
        var result = MatrixOperations.Reduce(input);

        // RREF 验证: 每个主元列只有主元为1，其余为0
        var rref = result.Rref;
        int rows = rref.GetLength(0);
        int cols = rref.GetLength(1);

        // 验证主元为 1
        VerifyRrefProperties(rref, rows, cols);
        Assert.Equal(3, result.Rank);
    }

    // ==================== 满秩方阵 (3×3) ====================

    [Fact]
    public void Reduce_FullRank3x3_ReducesToIdentity()
    {
        // 增广矩阵: 2x + y - z = 8; -3x - y + 2z = -11; -2x + y + 2z = -3
        var input = MatrixOperations.ParseMatrix("2 1 -1 8\n-3 -1 2 -11\n-2 1 2 -3");
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(3, result.Rank);

        // RREF 的前3列应为单位矩阵
        var rref = result.Rref;
        Assert.Equal(Fraction.One, rref[0, 0]);
        Assert.Equal(Fraction.Zero, rref[0, 1]);
        Assert.Equal(Fraction.Zero, rref[0, 2]);
        Assert.Equal(Fraction.Zero, rref[1, 0]);
        Assert.Equal(Fraction.One, rref[1, 1]);
        Assert.Equal(Fraction.Zero, rref[1, 2]);
        Assert.Equal(Fraction.Zero, rref[2, 0]);
        Assert.Equal(Fraction.Zero, rref[2, 1]);
        Assert.Equal(Fraction.One, rref[2, 2]);

        // 解为 x=2, y=3, z=-1
        Assert.Equal(new Fraction(2), rref[0, 3]);
        Assert.Equal(new Fraction(3), rref[1, 3]);
        Assert.Equal(new Fraction(-1), rref[2, 3]);
    }

    // ==================== 奇异矩阵 (秩不满) ====================

    [Fact]
    public void Reduce_SingularMatrix_CorrectRank()
    {
        // 1 2 3
        // 4 5 6
        // 7 8 9
        var input = MatrixOperations.ParseMatrix("1 2 3\n4 5 6\n7 8 9");
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(2, result.Rank);

        // 最后一行应为全零
        var rref = result.Rref;
        Assert.True(rref[2, 0].IsZero);
        Assert.True(rref[2, 1].IsZero);
        Assert.True(rref[2, 2].IsZero);
    }

    // ==================== 1×1 矩阵 ====================

    [Fact]
    public void Reduce_1x1_NonZero()
    {
        var input = MakeMatrix([[5]]);
        var result = MatrixOperations.Reduce(input);
        Assert.Equal(Fraction.One, result.Rref[0, 0]);
        Assert.Equal(1, result.Rank);
    }

    [Fact]
    public void Reduce_1x1_Zero()
    {
        var input = MakeMatrix([[0]]);
        var result = MatrixOperations.Reduce(input);
        Assert.True(result.Rref[0, 0].IsZero);
        Assert.Equal(0, result.Rank);
    }

    // ==================== 行向量 ====================

    [Fact]
    public void Reduce_SingleRow()
    {
        var input = MakeMatrix([[2, 4, 6]]);
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(Fraction.One, result.Rref[0, 0]);
        Assert.Equal(new Fraction(2), result.Rref[0, 1]);
        Assert.Equal(new Fraction(3), result.Rref[0, 2]);
        Assert.Equal(1, result.Rank);
    }

    // ==================== 列向量 ====================

    [Fact]
    public void Reduce_SingleColumn()
    {
        var input = MakeMatrix([
            [3],
            [6],
            [9],
        ]);
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(Fraction.One, result.Rref[0, 0]);
        Assert.True(result.Rref[1, 0].IsZero);
        Assert.True(result.Rref[2, 0].IsZero);
        Assert.Equal(1, result.Rank);
    }

    // ==================== 需要行交换的矩阵 ====================

    [Fact]
    public void Reduce_RequiresRowSwap()
    {
        // 第一个元素为0，需要行交换
        var input = MakeMatrix([
            [0, 1, 2],
            [1, 0, 3],
            [0, 0, 1],
        ]);
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(3, result.Rank);
        VerifyRrefProperties(result.Rref, 3, 3);
    }

    // ==================== 分数输入 ====================

    [Fact]
    public void Reduce_FractionInput()
    {
        var input = MatrixOperations.ParseMatrix("1/2 1/3\n1/4 1/5");
        var result = MatrixOperations.Reduce(input);

        Assert.Equal(2, result.Rank);

        // RREF 应为单位矩阵
        Assert.Equal(Fraction.One, result.Rref[0, 0]);
        Assert.Equal(Fraction.Zero, result.Rref[0, 1]);
        Assert.Equal(Fraction.Zero, result.Rref[1, 0]);
        Assert.Equal(Fraction.One, result.Rref[1, 1]);
    }

    // ==================== 宽矩阵 (列 > 行) ====================

    [Fact]
    public void Reduce_WideMatrix()
    {
        // 2×4 矩阵
        var input = MakeMatrix([
            [1, 2, 3, 4],
            [2, 4, 7, 8],
        ]);
        var result = MatrixOperations.Reduce(input);
        Assert.Equal(2, result.Rank);
        VerifyRrefProperties(result.Rref, 2, 4);
    }

    // ==================== 高矩阵 (行 > 列) ====================

    [Fact]
    public void Reduce_TallMatrix()
    {
        // 4×2 矩阵，秩为2
        var input = MakeMatrix([
            [1, 2],
            [3, 4],
            [5, 6],
            [7, 8],
        ]);
        var result = MatrixOperations.Reduce(input);
        Assert.Equal(2, result.Rank);

        // 后两行应为全零
        Assert.True(result.Rref[2, 0].IsZero);
        Assert.True(result.Rref[2, 1].IsZero);
        Assert.True(result.Rref[3, 0].IsZero);
        Assert.True(result.Rref[3, 1].IsZero);
    }

    // ==================== REF 性质验证 ====================

    [Fact]
    public void Reduce_RefProperties()
    {
        var input = MatrixOperations.ParseMatrix("1 3 5 7\n3 5 7 9\n5 7 9 1");
        var result = MatrixOperations.Reduce(input);
        var refM = result.Ref;
        int rows = refM.GetLength(0);
        int cols = refM.GetLength(1);

        // REF 性质: 每个主元在前一个主元的右边，主元下方全为零
        int lastPivotCol = -1;
        for (int i = 0; i < rows; i++)
        {
            // 找到该行第一个非零元素
            int pivotCol = -1;
            for (int j = 0; j < cols; j++)
            {
                if (!refM[i, j].IsZero)
                {
                    pivotCol = j;
                    break;
                }
            }

            if (pivotCol == -1) // 全零行
            {
                // 后续行也应全为零
                for (int ii = i; ii < rows; ii++)
                    for (int jj = 0; jj < cols; jj++)
                        Assert.True(refM[ii, jj].IsZero);
                break;
            }

            Assert.True(pivotCol > lastPivotCol, "主元列必须严格递增");
            Assert.Equal(Fraction.One, refM[i, pivotCol]); // 主元为 1
            lastPivotCol = pivotCol;
        }
    }

    // ==================== 步骤记录验证 ====================

    [Fact]
    public void Reduce_StepsAreRecorded()
    {
        var input = MatrixOperations.ParseMatrix("1 2\n3 4");
        var result = MatrixOperations.Reduce(input);

        Assert.True(result.RefSteps.Count >= 1, "REF 步骤不应为空");
        Assert.True(result.RrefSteps.Count >= 1, "RREF 步骤不应为空");

        // 每个步骤应有描述和矩阵快照
        foreach (var step in result.RefSteps)
        {
            Assert.False(string.IsNullOrEmpty(step.Description));
            Assert.NotNull(step.MatrixSnapshot);
        }
    }

    // ==================== FormatMatrix ====================

    [Fact]
    public void FormatMatrix_BasicOutput()
    {
        var m = MakeMatrix([
            [1, 2],
            [3, 4],
        ]);
        var text = MatrixOperations.FormatMatrix(m);

        Assert.Contains("1", text);
        Assert.Contains("4", text);
        Assert.Contains("[", text);
        Assert.Contains("]", text);
    }

    // ==================== 辅助方法 ====================

    /// <summary>
    /// 验证 RREF 的基本性质:
    /// 1. 每行第一个非零元素（主元）为 1
    /// 2. 主元列其他元素为 0
    /// 3. 主元列严格递增
    /// 4. 全零行在最下方
    /// </summary>
    private static void VerifyRrefProperties(Fraction[,] rref, int rows, int cols)
    {
        int lastPivotCol = -1;
        bool hitZeroRow = false;

        for (int i = 0; i < rows; i++)
        {
            int pivotCol = -1;
            for (int j = 0; j < cols; j++)
            {
                if (!rref[i, j].IsZero)
                {
                    pivotCol = j;
                    break;
                }
            }

            if (pivotCol == -1)
            {
                hitZeroRow = true;
                continue;
            }

            Assert.False(hitZeroRow, "全零行之后不应有非零行");
            Assert.Equal(Fraction.One, rref[i, pivotCol]);
            Assert.True(pivotCol > lastPivotCol, "主元列必须严格递增");

            // 主元列其他行应为 0
            for (int ii = 0; ii < rows; ii++)
            {
                if (ii != i)
                    Assert.True(rref[ii, pivotCol].IsZero,
                        $"RREF[{ii},{pivotCol}] 应为 0，实际为 {rref[ii, pivotCol]}");
            }

            lastPivotCol = pivotCol;
        }
    }
}
