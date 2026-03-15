using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixReducer;

/// <summary>
/// 矩阵行化简算法，支持 REF（阶梯形）和 RREF（简化阶梯形）。
/// </summary>
public class MatrixOperations
{
    /// <summary>
    /// 行化简步骤记录。
    /// </summary>
    public record Step(string Description, Fraction[,] MatrixSnapshot);

    /// <summary>
    /// 行化简结果。
    /// </summary>
    public record ReductionResult(
        Fraction[,] Original,
        Fraction[,] Ref,       // 阶梯形矩阵 (Row Echelon Form)
        Fraction[,] Rref,      // 简化阶梯形矩阵 (Reduced Row Echelon Form)
        List<Step> RefSteps,   // REF 的步骤
        List<Step> RrefSteps,  // RREF 的步骤
        int Rank
    );

    /// <summary>
    /// 复制矩阵。
    /// </summary>
    private static Fraction[,] Clone(Fraction[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new Fraction[rows, cols];
        Array.Copy(matrix, result, matrix.Length);
        return result;
    }

    /// <summary>
    /// 将矩阵格式化为字符串。
    /// </summary>
    public static string FormatMatrix(Fraction[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var sb = new StringBuilder();

        // 计算每列的最大宽度
        int[] colWidths = new int[cols];
        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                int len = matrix[i, j].ToString().Length;
                if (len > colWidths[j]) colWidths[j] = len;
            }
        }

        for (int i = 0; i < rows; i++)
        {
            sb.Append("[ ");
            for (int j = 0; j < cols; j++)
            {
                string val = matrix[i, j].ToString();
                sb.Append(val.PadLeft(colWidths[j]));
                if (j < cols - 1) sb.Append("  ");
            }
            sb.Append(" ]");
            if (i < rows - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// 执行完整的行化简。
    /// </summary>
    public static ReductionResult Reduce(Fraction[,] original)
    {
        int rows = original.GetLength(0);
        int cols = original.GetLength(1);

        // 阶段 1: 前向消元 → REF
        var matrix = Clone(original);
        var refSteps = new List<Step>();
        refSteps.Add(new Step("原始矩阵", Clone(matrix)));

        int pivotRow = 0;
        var pivotCols = new List<int>(); // 记录每个主元所在列

        for (int col = 0; col < cols && pivotRow < rows; col++)
        {
            // 寻找该列中绝对值最大的行作为主元行（部分主元选取）
            int maxRow = -1;
            Fraction maxVal = Fraction.Zero;
            for (int i = pivotRow; i < rows; i++)
            {
                var absVal = matrix[i, col].Numerator < 0 ? -matrix[i, col] : matrix[i, col];
                if (!matrix[i, col].IsZero && (maxRow == -1 || absVal.CompareTo(maxVal) > 0))
                {
                    maxRow = i;
                    maxVal = absVal;
                }
            }

            if (maxRow == -1) continue; // 该列全为零，跳过

            // 交换行
            if (maxRow != pivotRow)
            {
                for (int j = 0; j < cols; j++)
                    (matrix[pivotRow, j], matrix[maxRow, j]) = (matrix[maxRow, j], matrix[pivotRow, j]);
                refSteps.Add(new Step($"R{pivotRow + 1} ↔ R{maxRow + 1}", Clone(matrix)));
            }

            // 将主元归一化
            var pivot = matrix[pivotRow, col];
            if (pivot != Fraction.One)
            {
                for (int j = 0; j < cols; j++)
                    matrix[pivotRow, j] = matrix[pivotRow, j] / pivot;
                refSteps.Add(new Step($"R{pivotRow + 1} ← R{pivotRow + 1} / {pivot}", Clone(matrix)));
            }

            // 消去下方的元素
            for (int i = pivotRow + 1; i < rows; i++)
            {
                if (!matrix[i, col].IsZero)
                {
                    var factor = matrix[i, col];
                    for (int j = 0; j < cols; j++)
                        matrix[i, j] = matrix[i, j] - factor * matrix[pivotRow, j];
                    refSteps.Add(new Step($"R{i + 1} ← R{i + 1} - ({factor}) × R{pivotRow + 1}", Clone(matrix)));
                }
            }

            pivotCols.Add(col);
            pivotRow++;
        }

        var refMatrix = Clone(matrix);
        int rank = pivotRow;

        // 阶段 2: 回代消元 → RREF
        var rrefSteps = new List<Step>();
        rrefSteps.Add(new Step("从 REF 开始回代", Clone(matrix)));

        for (int p = pivotCols.Count - 1; p >= 0; p--)
        {
            int pc = pivotCols[p];
            // 消去上方元素
            for (int i = p - 1; i >= 0; i--)
            {
                if (!matrix[i, pc].IsZero)
                {
                    var factor = matrix[i, pc];
                    for (int j = 0; j < cols; j++)
                        matrix[i, j] = matrix[i, j] - factor * matrix[p, j];
                    rrefSteps.Add(new Step($"R{i + 1} ← R{i + 1} - ({factor}) × R{p + 1}", Clone(matrix)));
                }
            }
        }

        var rrefMatrix = Clone(matrix);

        return new ReductionResult(
            Clone(original),
            refMatrix,
            rrefMatrix,
            refSteps,
            rrefSteps,
            rank
        );
    }

    /// <summary>
    /// 解析用户输入的矩阵文本。
    /// 每行用换行分隔，每个元素用空格或逗号分隔。
    /// </summary>
    public static Fraction[,] ParseMatrix(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var parsed = new List<Fraction[]>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var tokens = trimmed.Split(new[] { ' ', '\t', ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);
            var row = new Fraction[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
                row[i] = Fraction.Parse(tokens[i]);
            parsed.Add(row);
        }

        if (parsed.Count == 0)
            throw new ArgumentException("矩阵不能为空。");

        int cols = parsed[0].Length;
        foreach (var row in parsed)
        {
            if (row.Length != cols)
                throw new ArgumentException("每行的列数必须一致。");
        }

        var matrix = new Fraction[parsed.Count, cols];
        for (int i = 0; i < parsed.Count; i++)
            for (int j = 0; j < cols; j++)
                matrix[i, j] = parsed[i][j];

        return matrix;
    }
}
