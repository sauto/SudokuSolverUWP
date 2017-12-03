using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;


namespace SudokuSolverUWP
{
    public struct Point
    {
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }

    public static class Utility
    {
        public const int ROW = 9;
        public const int COL = 9;
        public const int MAX = 82;

        public static List<string> outList = new List<string>();

        /// <summary>
        /// コンソールに盤面行列を表示
        /// </summary>
        /// <param name="mat"></param>
        public static void BoardPrint(int[][] mat)
        {
            string board = string.Empty;
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    board += string.Format("|{0}|", mat[i][j]);
                    if (j == 8)
                        board += Environment.NewLine;
                }
            }
            Debug.WriteLine(board);
        }

        /// <summary>
        /// 全てのマスが正しく埋まっているかを判定
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static bool IsCompleteBoard(int[][] mat)
        {
            List<int> numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            /*行の判定*/
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    numAppearCheck[mat[i][j]]++;
                //空白マス（index=0）を除外
                numAppearCheck.RemoveAt(0);
                //1～9が1回ずつ出ていなければ終了
                if (!numAppearCheck.TrueForAll(s => s == 1))
                    return false;

                numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            }

            /*列の判定*/
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    numAppearCheck[mat[j][i]]++;
                //空白マス（index=0）を除外
                numAppearCheck.RemoveAt(0);
                //1～9が1回ずつ出ていなければ終了
                if (!numAppearCheck.TrueForAll(s => s == 1))
                    return false;

                numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            }

            /*ブロックの判定*/
            for (int r = 0; r < ROW; r++)
            {
                for (int c = 0; c < COL; c++)
                {
                    var blockPoint = GetBlockPoint(r, c);
                    for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                        for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                            numAppearCheck[mat[j][i]]++;
                    //空白マス（index=0）を除外
                    numAppearCheck.RemoveAt(0);
                    //1～9が1回ずつ出ていなければ終了
                    if (!numAppearCheck.TrueForAll(s => s == 1))
                        return false;

                    numAppearCheck = Enumerable.Repeat(0, 10).ToList();
                }
            }
            return true;
        }
        
        /// <summary>
        /// 座標からブロック座標(n,m)を返す関数
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Point GetBlockPoint(int row, int col)
        {
            //(0,0)(0,3)(0,6)
            //(3,0)(3,3)(3,6)
            //(6,0)(6,3)(6,6)

            //ブロックの走査
            for (int p = 0; p < 9; p = p + 3)
                for (int q = 0; q < 9; q = q + 3)
                    //3*3の走査
                    for (int r = p; r <= p + 2; r++)
                        for (int s = q; s <= q + 2; s++)
                            if ((row == r) && (col == s))
                                return new Point(p, q);
            return new Point(-1, -1);
        }

        /// <summary>
        /// 処理を行って盤面が変化したかどうかを判定
        /// </summary>
        /// <param name="mat">変化後の盤面</param>
        /// <param name="prevMat">変化前の盤面</param>
        /// <returns></returns>
        public static bool IsChangeBoard(int[][] mat,int[][] prevMat)
        {
            for (int i = 0; i < ROW; i++)
                for (int j = 0; j < COL; j++) 
                    if (mat[i][j] != prevMat[i][j])
                        return true;

            return false;
        }

        /// <summary>
        /// 処理前の盤面をバッファ盤面にコピーする
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="bufferMatrix"></param>
        public static void CopyToBufferMatrix(int[][] mat, int[][] bufferMatrix)
        {
            for (int i = 0; i < ROW; i++)
                for (int j = 0; j < COL; j++)
                    bufferMatrix[i][j] = mat[i][j];
        }

        /// <summary>
        /// 埋まったマスを含む行列ブロックから検索対象の数字を除外
        /// </summary>
        /// <param name="isBlankPoint">その数字が座標に入っているか</param>
        /// <param name="inputablePoint">座標</param>
        public static void ExcludeNumberInRowColBlock(bool[][] isBlankPoint,Point inputablePoint)
        {
            for (int j = 0; j < COL; j++) isBlankPoint[inputablePoint.X][j] = false;
            for (int i = 0; i < ROW; i++) isBlankPoint[i][inputablePoint.Y] = false;

            var blockPoint = GetBlockPoint(inputablePoint.X, inputablePoint.Y);
            for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                    isBlankPoint[i][j] = false;
        }

        /// <summary>
        /// 埋まったマスを含む行列ブロックから対象の候補数字を除外
        /// </summary>
        /// <param name="candMat">候補数字配列</param>
        /// <param name="targetPoint">基点座標。この座標と同業同列同ブロックのoffBitを消去</param>
        ///　<param name="offbit">候補数字から消す数字</param>
        public static void ExcludeCandidateInRowColBlock(int[][] candMat, Point targetPoint,int offBit)
        {
            for (int q = 0; q < COL; q++)
                candMat[targetPoint.X][q] = candMat[targetPoint.X][q] & ~offBit;

            for (int p = 0; p < ROW; p++)
                candMat[p][targetPoint.Y] = candMat[p][targetPoint.Y] & ~offBit;

            var blockPoint = GetBlockPoint(targetPoint.X, targetPoint.Y);
            for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                    candMat[i][j] = candMat[i][j] & ~offBit;
        }
        /// <summary>
        /// 候補数字を表示
        /// </summary>
        /// <param name="candmat"></param>
        public static void CandidateOutput(int[][] candmat)
        {
            string output = "";

            string outputCandidate = string.Empty;

            //各マスの候補数列入力
            for (int i = 0; i < ROW; i++)
                for (int j = 0; j < COL; j++)
                    for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                    {
                        if ((candmat[i][j] & bit) == bit)
                            outputCandidate += Math.Round(Math.Log(bit, 2));
                        else
                            outputCandidate += ' ';
                    }
            //各マスの候補数字表示 わかんないなら一番単純なものを作って一般化

            for (int n = 0; n < 9; n++)
            {
                for (int i = n * 81; i < (n + 1) * 81; i = i + 9)
                {
                    for (int k = i; k < i + 3; k++)
                    {//3個ずつぶち抜く
                        output += (outputCandidate[k]);
                        output += ("|");
                        if (k == i + 2) { output += ("|"); }
                    }
                }
                outList.Add(output);
                output = "";
                for (int i = n * 81; i < (n + 1) * 81; i = i + 9)
                {
                    for (int k = i + 3; k < i + 6; k++)
                    {//3個ずつぶち抜く
                        output += (outputCandidate[k]);
                        output += ("|");
                        if (k == i + 5) { output += ("|"); }
                    }
                }
                outList.Add(output);
                output = "";
                for (int i = n * 81; i < (n + 1) * 81; i = i + 9)
                {
                    for (int k = i + 6; k < i + 9; k++)
                    {//3個ずつぶち抜く
                        output += (outputCandidate[k]);
                        output += ("|");
                        if (k == i + 8) { output += ("|"); }
                    }
                }
                
                outList.Add(output);
                output = "";
                output += ("---------------------------------------------------------------");
                outList.Add(output);
                output = "";
                
            }
            outList.Add("");
        }

        /// <summary>
        /// 候補数字の数を数える
        /// </summary>
        /// <param name="candidate">候補を格納したビット数列</param>
        /// <param name="numExistCheck">各ビットの出現数チェック用配列 [bit]=出現数</param>
        public static void CandidateNumCheck(int candidate,int[] numExistCheck)
        {
            for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                //候補が引っ掛かったら
                if ((candidate & bit) == bit)
                    numExistCheck[(int)Math.Round(Math.Log(bit, 2))]++;
        }

        /// <summary>
        /// 指定の数字の、ビットが立っている桁-1を返す
        /// </summary>
        /// <param name="targetNum">調べる数字</param>
        /// <param name="maxDigit">調べる最大桁数</param>
        /// <returns>ONだった桁数のリスト</returns>
        public static List<int> GetOnBitList(int targetNum, int maxDigit)
        {
            var list = new List<int>();
            for (int bit = 1; bit < (int)Math.Pow(2, maxDigit); bit <<= 1)
                if ((targetNum & bit) == bit)
                    list.Add((int)Math.Round(Math.Log(targetNum & bit, 2)));

            return list;
        }

        /// <summary>
        /// 埋まったマスの候補数字を全消去
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candmat"></param>
        public static void FilledCellCandidateDel(int[][] mat, int[][] candmat)
        {
            for (int i = 0; i < ROW; i++)
                for (int j = 0; j < COL; j++)
                    if (mat[i][j] > 0)
                        candmat[i][j] = 0;
        }

        /// <summary>
        /// 値型の2次元配列を初期化して返すメソッド
        /// </summary>
        /// <example>GetInit2DimArray<int>(9,9)</example>
        /// <typeparam name="T">配列の型。値型のみ</typeparam>
        /// <param name="maxRow">生成する2次元配列の最大行数</param>
        /// <param name="maxCol">生成する2次元配列の最大列数</param>
        /// <returns>値型の2次元配列</returns>
        public static T[][] GetInit2DimArray<T>(int maxRow,int maxCol) where T : struct
        {
            T[][] initMat = new T[maxRow][];
            //それぞれの型に定義された初期値(default(T))で初期化
            for (int i = 0; i < maxRow; i++)
                initMat[i] = Enumerable.Repeat(default(T), maxCol).ToArray();
            
            return initMat;
        }
        /// <summary>
        /// 回答中のダブリ検知
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="logic"></param>
        public static bool Mistake(int[][] mat, string logic)
        {
            List<int> numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            /*行の判定*/
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    numAppearCheck[mat[i][j]]++;
                //空白マス（index=0）を除外
                numAppearCheck.RemoveAt(0);
                //ダブってたら終了
                if (numAppearCheck.Where(s => s >= 2).Count() > 0)
                {
                    string str = "";
                    foreach (var k in numAppearCheck.Where(s => s >= 2))
                    {
                        str += k.ToString();
                    }

                    Debug.WriteLine(i + "行" + str + "失敗:" + logic);
                    return true;
                }
                numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            }

            /*列の判定*/
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    numAppearCheck[mat[j][i]]++;
                //空白マス（index=0）を除外
                numAppearCheck.RemoveAt(0);
                //ダブってたら終了
                if (numAppearCheck.Where(s => s >= 2).Count() > 0)
                {
                    string str = "";
                    foreach (var k in numAppearCheck.Where(s => s >= 2))
                    {
                        str += k.ToString();
                    }

                    Debug.WriteLine(i + "列" + str + "失敗:" + logic);
                    return true;
                }

                numAppearCheck = Enumerable.Repeat(0, 10).ToList();
            }

            /*ブロックの判定*/
            for (int r = 0; r < ROW; r++)
            {
                for (int c = 0; c < COL; c++)
                {
                    var blockPoint = GetBlockPoint(r, c);
                    for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                        for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                            numAppearCheck[mat[j][i]]++;
                    //空白マス（index=0）を除外
                    numAppearCheck.RemoveAt(0);
                    //ダブってたら終了
                    if (numAppearCheck.Where(s => s >= 2).Count() > 0)
                    {
                        string str = "";
                        foreach (var k in numAppearCheck.Where(s => s >= 2))
                        {
                            str += k.ToString();
                        }

                        Debug.WriteLine(r + c + "ブロック" + str + "失敗:" + logic);
                        return true;
                    }

                    numAppearCheck = Enumerable.Repeat(0, 10).ToList();
                }
            }
            return false;

        }

        public static int CandExistRowCellCount(int[][] candMat,int row,int initValue,int maxRange,int bit)
        {
            int candHit = 0;
            for (int col = initValue; col <= maxRange; col++)
                if ((candMat[row][col] & bit) == bit)
                    candHit++;

            return candHit;
        }

        public static int CandExistColCellCount(int[][] candMat, int col, int initValue, int maxRange, int bit)
        {
            int candHit = 0;
            for (int row = initValue; row <= maxRange; row++)
                if ((candMat[row][col] & bit) == bit)
                    candHit++;

            return candHit;
        }


    }
}

