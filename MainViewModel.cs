using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MatrixReducer;

public class MainViewModel : INotifyPropertyChanged
{
    private string _inputText = "1 2 -1 3\n2 4 1 0\n-1 -2 3 -6";
    private string _refResult = "";
    private string _rrefResult = "";
    private string _stepsText = "";
    private string _statusText = "请输入矩阵，然后点击「行化简」。";
    private int _rows = 3;
    private int _cols = 4;

    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); }
    }

    public string RefResult
    {
        get => _refResult;
        set { _refResult = value; OnPropertyChanged(); }
    }

    public string RrefResult
    {
        get => _rrefResult;
        set { _rrefResult = value; OnPropertyChanged(); }
    }

    public string StepsText
    {
        get => _stepsText;
        set { _stepsText = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public int Rows
    {
        get => _rows;
        set
        {
            if (value < 1) value = 1;
            if (value > 10) value = 10;
            _rows = value;
            OnPropertyChanged();
            GenerateEmptyMatrix();
        }
    }

    public int Cols
    {
        get => _cols;
        set
        {
            if (value < 1) value = 1;
            if (value > 10) value = 10;
            _cols = value;
            OnPropertyChanged();
            GenerateEmptyMatrix();
        }
    }

    private void GenerateEmptyMatrix()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                sb.Append('0');
                if (j < _cols - 1) sb.Append(' ');
            }
            if (i < _rows - 1) sb.AppendLine();
        }
        InputText = sb.ToString();
    }

    public void ExecuteReduce()
    {
        try
        {
            var matrix = MatrixOperations.ParseMatrix(InputText);
            var result = MatrixOperations.Reduce(matrix);

            RefResult = MatrixOperations.FormatMatrix(result.Ref);
            RrefResult = MatrixOperations.FormatMatrix(result.Rref);

            // 构建步骤文本
            var sb = new StringBuilder();
            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine("  前向消元 → 阶梯形矩阵 (REF)");
            sb.AppendLine("══════════════════════════════════");
            foreach (var step in result.RefSteps)
            {
                sb.AppendLine();
                sb.AppendLine($"▶ {step.Description}");
                sb.AppendLine(MatrixOperations.FormatMatrix(step.MatrixSnapshot));
            }

            sb.AppendLine();
            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine("  回代消元 → 简化阶梯形矩阵 (RREF)");
            sb.AppendLine("══════════════════════════════════");
            foreach (var step in result.RrefSteps)
            {
                sb.AppendLine();
                sb.AppendLine($"▶ {step.Description}");
                sb.AppendLine(MatrixOperations.FormatMatrix(step.MatrixSnapshot));
            }

            sb.AppendLine();
            sb.AppendLine($"矩阵的秩 (Rank) = {result.Rank}");

            StepsText = sb.ToString();
            StatusText = $"化简完成。矩阵大小: {matrix.GetLength(0)}×{matrix.GetLength(1)}，秩 = {result.Rank}";
        }
        catch (Exception ex)
        {
            StatusText = $"错误: {ex.Message}";
            RefResult = "";
            RrefResult = "";
            StepsText = "";
        }
    }

    public void LoadExample(int index)
    {
        InputText = index switch
        {
            0 => "1 2 -1 3\n2 4 1 0\n-1 -2 3 -6",
            1 => "1 3 5 7\n3 5 7 9\n5 7 9 1",
            2 => "2 1 -1 8\n-3 -1 2 -11\n-2 1 2 -3",
            3 => "1 0 0\n0 1 0\n0 0 1",
            4 => "1 2 3\n4 5 6\n7 8 9",
            _ => InputText
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
