namespace SmartQr.Codes.Rendering.Matrix;

/// <summary>Represents a square grid of code modules — <c>true</c> is dark, <c>false</c> is light — with no quiet zone, the framework-agnostic bit grid the emitter consumes independent of the matrix source.</summary>
/// <remarks>The quiet zone is not part of the matrix; the emitter adds it from the <c>StyleSpec</c>.</remarks>
public sealed class ModuleMatrix
{
    private readonly bool[,] _modules;

    /// <summary>Creates a matrix from a row-major <c>[row, col]</c> grid, taking the array as-is.</summary>
    /// <param name="modules">The square module grid; ownership transfers to the matrix.</param>
    /// <exception cref="ArgumentException">The grid is not square.</exception>
    public ModuleMatrix(bool[,] modules)
    {
        var rows = modules.GetLength(0);
        var cols = modules.GetLength(1);
        if (rows != cols)
            throw new ArgumentException($"Module matrix must be square; got {rows}x{cols}.", nameof(modules));

        _modules = modules;
        Size = rows;
    }

    /// <summary>Gets the side length of the square matrix in modules.</summary>
    public int Size { get; }

    /// <summary>Gets whether the module at <paramref name="row"/>, <paramref name="col"/> is dark.</summary>
    public bool this[int row, int col] => _modules[row, col];
}
