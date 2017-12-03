using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    interface ISolver
    {
        void CellFill(int[][] mat);
        void Crbe(int[][] mat);
        void Pair(int[][] mat, int[][] candMat);
    }
}
