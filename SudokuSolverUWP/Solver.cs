using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolverUWP
{
    public class Solver// : ISolver
    {
        public Solver(bool isDisplayUsedLogic)
        {
            DisplayUsedLogic = isDisplayUsedLogic;
        }

        //難易度スコア
        public int DifficultScore { get; private set; } = 0;

        /// <summary>
        /// 使ったロジックの表示可否
        /// </summary>
        public bool DisplayUsedLogic { get; set; } = true;

        /// <summary>
        /// 使ったロジックなどのログ
        /// </summary>
        public string Log { get { return _log; } set { _log = value; } }
        string _log = "";

        /// <summary>
        /// 対角線法
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public void CrossLogic(int[][] mat, int[][] candMat)
        {
            int[][] bufMat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            int n = 1, sameCandCheck = 0;
            
            if (Utility.IsCompleteBoard(mat)) return;

            int targetCol1 = 0, targetCol2 = 0;
            //行固定して列走査
            for (int i = 0; i < Utility.ROW; i++)
            {
                for (int r = n; r < Utility.ROW; r++)
                {
                    if (i != r)
                    {
                        for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                        {
                            DifficultScore++;

                            for (int m = 0; m < Utility.COL; m++)
                            {
                                //同列に同じ候補があったら
                                if ((candMat[i][m] & bit) == bit && (candMat[r][m] & bit) == bit)
                                {
                                    sameCandCheck++;
                                    //2列まで記憶
                                    if (sameCandCheck == 1) targetCol1 = m;
                                    if (sameCandCheck == 2) targetCol2 = m;
                                }
                            }
                            //クロスになってたら
                            if (sameCandCheck == 2)
                            {
                                int col1CandCount = 0, col2CandCount = 0;

                                //候補が行に2つのみか判定
                                for (int m = 0; m < Utility.COL; m++)
                                {
                                    if ((candMat[i][m] & bit) == bit)
                                        col1CandCount++;

                                    if ((candMat[r][m] & bit) == bit)
                                        col2CandCount++;
                                }
                                //2行とも候補が2つのみだったら
                                if ((col1CandCount == 2) && (col2CandCount == 2))
                                {
                                    //処理前盤面記憶
                                    Utility.CopyToBufferMatrix(candMat, bufMat);

                                    for (int w = 0; w < Utility.ROW; w++)
                                    {
                                        //列走査して交差点以外の列の候補消去
                                        if ((w != targetCol1) && (w != targetCol2))
                                        {
                                            candMat[i][w] &= ~bit;
                                            candMat[r][w] &= ~bit;
                                        }
                                        //行走査して交差点以外の行の候補消去
                                        if ((w != i) && (w != r))
                                        {
                                            candMat[w][targetCol1] &= ~bit;
                                            candMat[w][targetCol2] &= ~bit;
                                        }
                                    }

                                    //処理後盤面変化なら候補使用して脱出
                                    if (Utility.IsChangeBoard(candMat, bufMat))
                                    {
                                        if (DisplayUsedLogic)
                                             _log+="\n"+string.Format("対角行{0}行,{1}行", i, r);

                                        DoSingleLogics(mat, candMat);
                                        return;
                                    }
                                }
                            }
                            sameCandCheck = 0;
                        }
                        sameCandCheck = 0;
                        //確定数字版
                        targetCol1 = targetCol2 = 0;
                        //i行の空白マスカウント
                        for (int q = 0; q < Utility.COL; q++)
                        {
                            if ((mat[i][q] == 0) && (mat[r][q] == 0))
                            {
                                sameCandCheck++;
                                //2列まで記憶
                                if (sameCandCheck == 1) targetCol1 = q;
                                if (sameCandCheck == 2) targetCol2 = q;
                            }
                        }
                        int spacecountA = 0, spacecountB = 0;
                        for (int q = 0; q < Utility.COL; q++)
                        {
                            if (mat[i][q] == 0) spacecountA++;
                            if (mat[r][q] == 0) spacecountB++;
                        }
                        //空白クロスになってたら
                        if ((sameCandCheck == 2) && (spacecountA == 2) && (spacecountB == 2))
                        {
                            //共通候補の記憶
                            int candmemory = candMat[i][targetCol1] & candMat[i][targetCol2] & candMat[r][targetCol1] & candMat[r][targetCol2];
                            //残ったのが1つか2つのとき　3つ以上は仕様上あり得ない
                            int c = Utility.GetOnBitList(candmemory, 10).Count;
                            if (c == 1 || c == 2)
                            {
                                //処理前盤面記憶
                                Utility.CopyToBufferMatrix(candMat, bufMat);

                                //記憶した2列の交差点のマスを含む行から交差点以外のマスの候補を除外
                                for (int p = 0; p < Utility.ROW; p++)
                                {
                                    if ((p != i) && (p != r))
                                    {
                                        candMat[p][targetCol1] &= ~candmemory;
                                        candMat[p][targetCol2] &= ~candmemory;
                                    }
                                }
                                //処理後盤面変化なら候補使用して脱出
                                if (Utility.IsChangeBoard(candMat, bufMat))
                                {
                                    if (DisplayUsedLogic)
                                        _log+="\n"+string.Format("対角行確定数字版{0}行,{1}行", i, r);

                                    DoSingleLogics(mat, candMat);
                                    return;
                                }
                            }
                        }
                    }
                }
                n++;
            }
            n = 1;
            int targetRow1 = 0, targetRow2 = 0;
            //列固定して行走査
            for (int j = 0; j < Utility.COL; j++)
            {
                for (int c = n; c < Utility.COL; c++)
                {
                    if (j != c)
                    {
                        for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                        {
                            DifficultScore++;
                            for (int m = 0; m < Utility.ROW; m++)
                            {
                                //同列に同じ候補があったら
                                if ((candMat[m][j] & bit) == bit && (candMat[m][c] & bit) == bit)
                                {
                                    sameCandCheck++;
                                    //2列まで記憶
                                    if (sameCandCheck == 1) targetRow1 = m;
                                    if (sameCandCheck == 2) targetRow2 = m;
                                }
                            }
                            //クロスになってたら
                            if (sameCandCheck == 2)
                            {
                                int row1check = 0, row2check = 0;
                                //候補が行に2つのみか判定
                                for (int m = 0; m < Utility.COL; m++)
                                {
                                    if ((candMat[m][j] & bit) == bit)
                                        row1check++;
                                    if ((candMat[m][c] & bit) == bit)
                                        row2check++;
                                }
                                if ((row1check == 2) && (row2check == 2))
                                {
                                    //処理前盤面記憶
                                    Utility.CopyToBufferMatrix(candMat, bufMat);

                                    for (int w = 0; w < Utility.COL; w++)
                                    {
                                        //列走査して交差点以外の列の候補消去
                                        if ((w != targetRow1) && (w != targetRow2))
                                        {
                                            candMat[w][j] = (candMat[w][j] | bit) - bit;
                                            candMat[w][c] = (candMat[w][c] | bit) - bit;
                                        }
                                        //行走査して交差点以外の行の候補消去
                                        if ((w != j) && (w != c))
                                        {
                                            candMat[targetRow1][w] &= ~bit;
                                            candMat[targetRow2][w] &= ~bit;
                                        }
                                    }

                                    //処理後盤面変化なら候補使用して脱出
                                    if (Utility.IsChangeBoard(candMat, bufMat))
                                    {
                                        if (DisplayUsedLogic)
                                            _log+="\n"+string.Format("対角列{0}行,{1}行", j, c);

                                        DoSingleLogics(mat, candMat);
                                        return;
                                    }
                                }
                            }
                            sameCandCheck = 0;
                        }
                        sameCandCheck = 0;
                        //確定数字版
                        //i行の空白マスカウント
                        for (int p = 0; p < Utility.ROW; p++)
                        {
                            if ((mat[p][j] == 0) && (mat[p][c] == 0))
                            {
                                sameCandCheck++; 
                                //2列まで記憶
                                if (sameCandCheck == 1) { targetRow1 = p; }
                                if (sameCandCheck == 2) { targetRow2 = p; }
                            }
                        }
                        int spacecountA = 0, spacecountB = 0;
                        for (int p = 0; p < Utility.ROW; p++)
                        {
                            if (mat[p][j] == 0) { spacecountA++; }
                            if (mat[p][c] == 0) { spacecountB++; }
                        }
                        //空白クロスになってたら
                        if ((sameCandCheck == 2) && (spacecountA == 2) && (spacecountB == 2))
                        {
                            //共通候補の記憶
                            int candmemory = candMat[targetRow1][j] & candMat[targetRow2][j] & candMat[targetRow1][c] & candMat[targetRow2][c];
                            //残ったのが1つか2つのとき　3つ以上は仕様上あり得ない
                            int count = Utility.GetOnBitList(candmemory, 10).Count;
                            if (count == 1 || count == 2)
                            {
                                //処理前盤面記憶
                                Utility.CopyToBufferMatrix(candMat, bufMat);

                                //記憶した2行の交差点のマスを含む行から交差点以外のマスの候補を除外
                                for (int q = 0; q < Utility.COL; q++)
                                {
                                    if ((q != j) && (q != c))
                                    {
                                        candMat[targetRow1][q] &= ~candmemory;
                                        candMat[targetRow2][q] &= ~candmemory;
                                    }
                                }
                                //処理後盤面変化なら候補使用して脱出
                                if (Utility.IsChangeBoard(candMat, bufMat))
                                {
                                    
                                    if (DisplayUsedLogic)
                                        _log+="\n"+string.Format("対角列確定数字版{0}行,{1}行\n", j, c);

                                    DoSingleLogics(mat, candMat);
                                    return;
                                }
                            }
                        }
                    }
                }
                n++;
            }
        }

        /// <summary>
        /// 三つ子法
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public void TripleLogic(int[][] mat, int[][] candMat)
        {
            int candMemory = 0,
                tripleCandMatchCount = 0, tripleCandMatchRowA = 0, tripleCandMatchRowB = 0, tripleCandMatchColA = 0, tripleCandMatchColB = 0,
                candMemoryBak = 0, candMemorySame = 0, candMemoryDif = 0, candMemoryTri = 0;
            int[][] bufmat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);

            if (Utility.IsCompleteBoard(mat))
                return;
            
            #region 独立三つ子法
            //行内で候補が3つしかない
            for (int i = 0; i < Utility.ROW; i++)
            {
                DifficultScore++;
                for (int j = 0; j < Utility.COL; j++)
                {
                    //その1マスで3個しか候補数字がなかったら
                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 3)
                    {
                        //その行検索して
                        for (int m = 0; m < Utility.COL; m++)
                        {
                            //3個の候補数字のうち2つ以上が、現在の行のうち空白である2マスで一致したら　332　322
                            //candMat[i][j]の候補が1,2,3=1110のとき、candMat[i][m]の候補が1,3=1010のときは
                            //candMat[i][m] & candMat[i][j]は1,3=1010でcandMat[i][m]に等しくなるので候補が2個あれば通る
                            //候補1つのときも通るが事前にDoSingleLogicで単一候補法を適用済みだから無視できる
                            if ((m != j) && ((candMat[i][m] & candMat[i][j]) == candMat[i][m]) && (candMat[i][m] > 0))
                            {
                                tripleCandMatchCount++;
                                //一致したマス記憶
                                tripleCandMatchColA = m;
                                if (tripleCandMatchCount == 1)
                                    tripleCandMatchColB = m;
                            }
                        }
                        //現在マスの3個の候補数字のうち2つ以上が、現在の行のうち空白である2マスで一致したら
                        if (tripleCandMatchCount == 2)
                        {
                            //処理前盤面記憶
                            Utility.CopyToBufferMatrix(candMat, bufmat);

                            //さらにその行検索して
                            for (int n = 0; n < Utility.COL; n++)
                                //3つ子法の対象3マス以外の
                                if ((n != j) && (n != tripleCandMatchColA) && (n != tripleCandMatchColB))
                                    //他のマスのその3つの候補数字を候補から除外
                                    candMat[i][n] &= ~candMat[i][j];

                            //処理後盤面変化なら候補使用して脱出
                            if (Utility.IsChangeBoard(candMat, bufmat))
                            {
                                if (DisplayUsedLogic)
                                    _log+="\n"+string.Format("独立三つ子行333({0},{1}),({2},{3}),({4},{5})", i, j, i, tripleCandMatchColA, i, tripleCandMatchColB);

                                DoSingleLogics(mat, candMat);
                                return;
                            }
                            
                        }
                    }
                    tripleCandMatchColA = 0;
                    tripleCandMatchColB = 0;
                    tripleCandMatchCount = 0;
                    //その1マスで2個しか候補数字がなかったら 222
                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 2)
                    {
                        //その行検索して
                        for (int m = 0; m < Utility.COL; m++)
                        {
                            //一個も記憶していないなら
                            if (tripleCandMatchCount == 0)
                            {
                                //空白マスで
                                if ((m != j) && (candMat[i][m] > 0))
                                {
                                    //2個目のマスの候補個数が2個ならば
                                    if (Utility.GetOnBitList(candMat[i][m], 10).Count == 2)
                                    {
                                        //2個の候補数字の一致部分を記憶し
                                        candMemory = candMat[i][m] & candMat[i][j];
                                        //一致した個数をカウントし候補が1つだけダブっていたら
                                        if (Utility.GetOnBitList(candMemory, 10).Count == 1)
                                        {
                                            //printf("%d",m);
                                            //両者の候補数字を合わせ
                                            candMemory = candMat[i][m] | candMat[i][j];
                                            tripleCandMatchCount++;
                                            //条件を満たした列記憶
                                            tripleCandMatchColA = m;
                                        }
                                    }
                                }
                            }
                            //一個記憶済みなら
                            if (tripleCandMatchCount == 1)
                            {
                                if ((m != j) && (m != tripleCandMatchColA))
                                {
                                    //既出の2マスの候補数字のうち2つが一致していたら
                                    //
                                    //2,4と2,6だったら4,6になればいい
                                    //2,4と2,6だったら2,4,6のいずれかに一致していればいいので2,4か2,6も条件を満たしてしまうが、
                                    //最適順序の場合、双子法で既に消えているハズなので問題ないし、
                                    //2,4 2,4 2,6となった場合でも双子法で2,6の2が消えて6が確定するから3マス以外のマスから6を消しても問題ないので特に問題はない
                                    //この場合は事実上双子法と単一候補法をやっている
                                    //まあスコアの付け方は同じなので特に影響はないと思われる                                    
                                    if (((candMemory & candMat[i][m]) == candMat[i][m]) && (candMat[i][m] > 0))
                                    {
                                        //条件を満たした列記憶
                                        tripleCandMatchColB = m;
                                        tripleCandMatchCount++;
                                    }
                                }
                            }
                        }
                        //222を満たしたら
                        if (tripleCandMatchCount == 2)
                        {
                            //処理前盤面記憶
                            Utility.CopyToBufferMatrix(candMat, bufmat);

                            for (int n = 0; n < Utility.COL; n++)
                                //さらにその行検索して
                                if ((n != j) && (n != tripleCandMatchColA) && (n != tripleCandMatchColB))
                                    //他のマスのその3つの候補数字を候補から除外
                                    candMat[i][n] &= ~candMemory;

                            //処理後盤面変化なら候補使用して脱出
                            if (Utility.IsChangeBoard(candMat, bufmat))
                            {
                                if (DisplayUsedLogic)
                                    _log+="\n"+string.Format("独立三つ子行222({0},{1}),({2},{3}),({4},{5})", i, j, i, tripleCandMatchColA, i, tripleCandMatchColB);

                                DoSingleLogics(mat, candMat);
                                return;
                            }
                        }
                    }
                    tripleCandMatchColA = 0;
                    tripleCandMatchColB = 0;
                    tripleCandMatchCount = 0;
                }

            }
            //列内で候補が3つしかない
            for (int j = 0; j < Utility.COL; j++)
            {
                DifficultScore++;
                for (int i = 0; i < Utility.ROW; i++)
                {
                    //その1マスで3個しか候補数字がなかったら
                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 3)
                    {
                        for (int m = 0; m < Utility.ROW; m++)
                        {//その列検索して
                         //3個の候補数字のうち2つ以上が空白である2マスで一致したら　332　322
                            if ((m != i) && ((candMat[m][j] & candMat[i][j]) == candMat[m][j]) && (candMat[m][j] > 0))
                            {
                                tripleCandMatchCount++;
                                //一致したマス記憶
                                tripleCandMatchRowA = m;
                                if (tripleCandMatchCount == 1)
                                {
                                    tripleCandMatchRowB = m;
                                }
                            }
                        }
                        if (tripleCandMatchCount == 2)
                        {//3個の候補数字が3マスで一致したら
                         //処理前盤面記憶
                            Utility.CopyToBufferMatrix(candMat, bufmat);

                            for (int n = 0; n < Utility.ROW; n++)
                            {//さらにその列検索して
                                if ((n != i) && (n != tripleCandMatchRowA) && (n != tripleCandMatchRowB))
                                {
                                    //他のマスのその3つの候補数字を候補から除外
                                    candMat[n][j] = (candMat[n][j] | candMat[i][j]) - candMat[i][j];
                                }
                            }

                            //処理後盤面変化なら候補使用して脱出
                            if (Utility.IsChangeBoard(candMat, bufmat))
                            {
                                if (DisplayUsedLogic)
                                    _log+="\n"+string.Format("独立三つ子列333({0},{1}),({2},{3}),({4},{5})", i, j, i, tripleCandMatchRowA, i, tripleCandMatchRowB);

                                DoSingleLogics(mat, candMat);
                                return;
                            }
                        }
                    }
                    tripleCandMatchRowA = 0;
                    tripleCandMatchRowB = 0;
                    tripleCandMatchCount = 0;

                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 2)
                    {//その1マスで2個しか候補数字がなかったら 222
                        for (int m = 0; m < Utility.ROW; m++)
                        {//その列検索して
                         //一個も記憶していないなら
                            if (tripleCandMatchCount == 0)
                            {
                                if ((m != i) && (candMat[m][j] > 0))
                                {
                                    //2個目のマスの候補個数をカウントし2個ならば
                                    if (Utility.GetOnBitList(candMat[m][j], 10).Count == 2)
                                    {
                                        //2個の候補数字の一致部分を記憶し
                                        candMemory = candMat[m][j] & candMat[i][j];
                                        //一致した個数をカウントし候補が1つだけダブっていたら
                                        if (Utility.GetOnBitList(candMemory, 10).Count == 1)
                                        {
                                            //両者の候補数字を合わせ
                                            candMemory = candMat[m][j] | candMat[i][j];
                                            tripleCandMatchCount++;
                                            //条件を満たした行記憶
                                            tripleCandMatchRowA = m;
                                        }
                                    }
                                }
                            }
                            //一個記憶済みなら
                            if (tripleCandMatchCount == 1)
                            {
                                if ((m != i) && (m != tripleCandMatchRowA))
                                {
                                    //既出の2マスの数字のうち2つが一致していたら
                                    if (((candMemory & candMat[m][j]) == candMat[m][j]) && (candMat[m][j] > 0))
                                    {
                                        tripleCandMatchRowB = m;
                                        tripleCandMatchCount++;
                                    }
                                }
                            }
                        }
                        if (tripleCandMatchCount == 2)
                        {//222を満たしたら
                         //処理前盤面記憶
                            Utility.CopyToBufferMatrix(candMat, bufmat);

                            for (int n = 0; n < Utility.ROW; n++)
                            {//さらにその列検索して
                                if ((n != i) && (n != tripleCandMatchRowA) && (n != tripleCandMatchRowB))
                                {
                                    //他のマスのその3つの候補数字を候補から除外
                                    candMat[n][j] &= ~candMemory;
                                }
                            }

                            //処理後盤面変化なら候補使用して脱出
                            if (Utility.IsChangeBoard(candMat, bufmat))
                            {
                                if (DisplayUsedLogic)
                                    _log+="\n"+string.Format("独立三つ子列222({0},{1}),({2},{3}),({4},{5})", i, j, i, tripleCandMatchRowA, i, tripleCandMatchRowB);

                                DoSingleLogics(mat, candMat);
                                return;
                            }
                        }
                        candMemory = 0;
                    }
                    tripleCandMatchRowA = 0;
                    tripleCandMatchRowB = 0;
                    tripleCandMatchCount = 0;
                }

            }
            //ブロック内で候補が3つしかない
            for (int i = 0; i < Utility.ROW; i = i + 3)
            {
                for (int j = 0; j < Utility.COL; j = j + 3)
                {
                    DifficultScore++;
                    for (int p = i; p <= i + 2; p++)
                    {
                        for (int q = j; q <= j + 2; q++)
                        {
                            //その1マスで3個しか候補数字がなかったら
                            if (Utility.GetOnBitList(candMat[p][q], 10).Count == 3)
                            {
                                //そのブロック検索して
                                for (int s = i; s <= i + 2; s++)
                                {
                                    for (int t = j; t <= j + 2; t++)
                                    {
                                        //3個の候補数字のうち2つ以上が空白である2マスで一致したら　332　322
                                        if (!((s == p) && (t == q)) && ((candMat[s][t] & candMat[p][q]) == candMat[s][t]) && (candMat[s][t] > 0))
                                        {
                                            tripleCandMatchCount++;
                                            //一致したマス記憶
                                            tripleCandMatchRowA = s;//0もありや
                                            tripleCandMatchColA = t;
                                            if (tripleCandMatchCount == 1)
                                            {
                                                tripleCandMatchRowB = s;
                                                tripleCandMatchColB = t;
                                            }
                                        }
                                    }
                                }
                                if (tripleCandMatchCount == 2)
                                {//3個の候補数字が3マスで一致したら
                                 //処理前盤面記憶
                                    Utility.CopyToBufferMatrix(candMat, bufmat);

                                    for (int s = i; s <= i + 2; s++)
                                    {
                                        for (int t = j; t <= j + 2; t++)
                                        {//さらにそのブロック検索して
                                            if (!((s == p) && (t == q)) && !((s == tripleCandMatchRowA) && (t == tripleCandMatchColA)) && !((s == tripleCandMatchRowB) && (t == tripleCandMatchColB)))
                                            {
                                                //printf("(%d,%d),(%d,%d)",i,j,s,t);
                                                //他のマスのその3つの候補数字を候補から除外
                                                candMat[s][t] &= ~candMat[p][q];
                                            }
                                        }
                                    }

                                    //処理後盤面変化なら候補使用して脱出
                                    if (Utility.IsChangeBoard(candMat, bufmat))
                                    {
                                        if (DisplayUsedLogic)
                                            _log+="\n"+string.Format("独立三つ子ブロック333({0},{1}),({2},{3}),({4},{5})",
                                                p, q, tripleCandMatchRowA, tripleCandMatchColA, tripleCandMatchRowB, tripleCandMatchColB);

                                        DoSingleLogics(mat, candMat);
                                        return;
                                    }
                                }
                            }
                            tripleCandMatchRowA = 0;
                            tripleCandMatchColA = 0;
                            tripleCandMatchRowB = 0;
                            tripleCandMatchColB = 0;

                            //その1マスで2個しか候補数字がなかったら 222
                            if (Utility.GetOnBitList(candMat[p][q], 10).Count == 2)
                            {
                                //そのブロック検索して
                                for (int s = i; s <= i + 2; s++)
                                {
                                    for (int t = j; t <= j + 2; t++)
                                    {
                                        //一個も記憶していないなら
                                        if (tripleCandMatchCount == 0)
                                        {
                                            if (!((s == p) && (t == q)) && (candMat[s][t] > 0))
                                            {
                                                //2個目のマスの候補個数をカウントし2個ならば
                                                if (Utility.GetOnBitList(candMat[s][t], 10).Count == 2)
                                                {
                                                    //2個の候補数字の一致部分を記憶し
                                                    candMemory = candMat[s][t] & candMat[p][q];
                                                    //一致した個数をカウントし候補が1つだけダブっていたら
                                                    if (Utility.GetOnBitList(candMemory, 10).Count == 1)
                                                    {
                                                        //両者の候補数字を合わせ
                                                        candMemory = candMat[s][t] | candMat[p][q];
                                                        tripleCandMatchCount++;
                                                        //条件を満たしたマス記憶
                                                        tripleCandMatchRowA = s;
                                                        tripleCandMatchColA = t;
                                                    }
                                                }
                                            }
                                        }
                                        //一個記憶済みなら
                                        if (tripleCandMatchCount == 1)
                                        {
                                            if (!((s == p) && (t == q)) && !((s == tripleCandMatchRowA) && (t == tripleCandMatchColA)))
                                            {
                                                //既出の2マスの数字のうち2つが一致していたら
                                                if (((candMemory & candMat[s][t]) == candMat[s][t]) && (candMat[s][t] > 0))
                                                {
                                                    tripleCandMatchRowB = s;
                                                    tripleCandMatchColB = t;
                                                    tripleCandMatchCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                                //222を満たしたら
                                if (tripleCandMatchCount == 2)
                                {
                                    //処理前盤面記憶
                                    Utility.CopyToBufferMatrix(candMat, bufmat);

                                    for (int s = i; s <= i + 2; s++)
                                    {
                                        for (int t = j; t <= j + 2; t++)
                                        {//さらにそのブロック検索して
                                            if (!((s == p) && (t == q)) && !((s == tripleCandMatchRowA) && (t == tripleCandMatchColA)) && !((s == tripleCandMatchRowB) && (t == tripleCandMatchColB)))
                                            {
                                                //他のマスのその3つの候補数字を候補から除外
                                                candMat[s][t] &= ~candMemory;
                                            }
                                        }
                                    }

                                    //処理後盤面変化なら候補使用して脱出
                                    if (Utility.IsChangeBoard(candMat, bufmat))
                                    {
                                        if (DisplayUsedLogic)
                                            _log+="\n"+string.Format("独立三つ子ブロック333({0},{1}),({2},{3}),({4},{5})",
                                                p, q, tripleCandMatchRowA, tripleCandMatchColA, tripleCandMatchRowB, tripleCandMatchColB);

                                        DoSingleLogics(mat, candMat);
                                        return;
                                    }
                                }
                                candMemory = 0;
                            }
                            tripleCandMatchRowA = 0;
                            tripleCandMatchColA = 0;
                            tripleCandMatchRowB = 0;
                            tripleCandMatchColB = 0;
                            tripleCandMatchCount = 0;
                        }
                    }

                }
            }
            #endregion

            candMemory = 0;
            if (Utility.IsCompleteBoard(mat))
                return;

            #region 居候三つ子法
            //行内で候補が3つしかない判定
            for (int i = 0; i < Utility.ROW; i++)
            {
                DifficultScore++;
                for (int j = 0; j < Utility.COL; j++)
                {
                    //マス(i,j)が埋まってなかったら
                    if (candMat[i][j] > 0)
                    {
                        for (int k = 0; k < Utility.COL; k++)
                        {
                            //333 332
                            if (k != j)
                                //他のマスの候補数字と一致する候補数字を記憶
                                candMemory = candMat[i][j] & candMat[i][k];

                            //一致した候補数字の数カウントして一致したのが1つ以下の場合333 332判定飛ばして222 223判定へ
                            if (Utility.GetOnBitList(candMemory, 10).Count < 2)
                                candMemory = 0;

                            if (candMemory > 0)
                            {
                                candMemoryBak = candMemory;
                                for (int m = 0; m < Utility.COL; m++)
                                {
                                    //j列k列以外の
                                    if ((m != j) && (m != k))
                                    {
                                        //他の7マスにおいて、j列k列の2マスで共通する候補数字とダブリが2つ以上なら
                                        if (Utility.GetOnBitList((candMemory & candMat[i][m]), 10).Count >= 2)
                                        {
                                            for (int n = 0; n < Utility.COL; n++)
                                            {
                                                //6マスのうち
                                                if ((n != j) && (n != k) && (n != m))
                                                {
                                                    //他のマスで出ている候補を「3マスで共通する候補数字」から除外
                                                    //つまり3マスで共通する候補数字を作る
                                                    candMemory &= ~candMat[i][n];
                                                }
                                            }

                                            //3マスで共通する候補数字の数をカウントして候補が3つしか残らなかったら 
                                            if (Utility.GetOnBitList(candMemory, 10).Count == 3)
                                            {
                                                //3マス目(i,m)に1・2マス目の共通候補のうち2つ以上一致する数字が存在するとき
                                                //例えば2,4,6 2,4,6ときて3マス目が2,4か2,6か4,6か2,4,6のとき
                                                if (Utility.GetOnBitList((candMat[i][m] & candMemory), 10).Count >= 2)
                                                {
                                                    //処理前盤面記憶
                                                    Utility.CopyToBufferMatrix(candMat, bufmat);

                                                    //3マスに適用
                                                    candMat[i][j] &= candMemory;
                                                    candMat[i][k] &= candMemory;
                                                    candMat[i][m] &= candMemory;

                                                    //処理後盤面変化なら候補使用して脱出
                                                    if (Utility.IsChangeBoard(candMat, bufmat))
                                                    {
                                                        if (DisplayUsedLogic)
                                                            _log+="\n"+string.Format("居候三つ子行333({0},{1}),({2},{3}),({4},{5})", i, j, i, k, i, m);

                                                        DoSingleLogics(mat, candMat);
                                                        return;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    //1・2マス目の共通候補に戻す
                                    candMemory = candMemoryBak;
                                }
                            }

                            candMemory = 0;
                            candMemoryBak = 0;
                            candMemorySame = 0;
                            candMemoryDif = 0;
                            candMemoryTri = 0;
                            //222 223
                            if (k != j)
                            {
                                if (candMat[i][k] > 0)
                                {
                                    //少なくともどっちかで出ている候補数字を記憶
                                    candMemory = candMat[i][j] | candMat[i][k];
                                    //両方で一致する候補数字を記憶
                                    candMemorySame = candMat[i][j] & candMat[i][k];
                                    //両方で一致しない候補数字を記憶
                                    candMemoryDif = (candMat[i][j] | candMat[i][k]) - (candMat[i][j] & candMat[i][k]);
                                }
                            }

                            //一致しない候補数字の数カウントして一致しないのが2つ以外の場合今回ループ終了
                            if (Utility.GetOnBitList(candMemoryDif, 10).Count != 2)
                                candMemory = 0;

                            if (candMemory > 0)
                            {
                                candMemoryBak = candMemory;
                                //候補数字を検索
                                for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                                {
                                    //2マスで一致する候補数字のうち1つをとって
                                    if ((candMemorySame & bit) == bit)
                                    {
                                        //それと2マスで一致しない候補数字を混ぜ合わせて三つ子法を適用する3つの数字を作って
                                        candMemoryTri = bit | candMemoryDif;
                                        //三つ子法の対象になる3マス目探索
                                        for (int m = 0; m < Utility.COL; m++)
                                        {
                                            if ((m != j) && (m != k))
                                            {
                                                //探索マスが、三つ子法を適用する3数字のうち2つ以上を含んでいるとき
                                                if (Utility.GetOnBitList((candMemoryTri & candMat[i][m]), 10).Count >= 2)
                                                {
                                                    for (int n = 0; n < Utility.COL; n++)
                                                    {
                                                        //三つ子法を適用しない6マスを探索し、
                                                        if ((n != j) && (n != k) && (n != m))
                                                        {
                                                            //三つ子法を適用する3マス以外にある候補を3マスの候補数字から除外
                                                            candMemory &= ~candMat[i][n];
                                                        }
                                                    }
                                                    //残った候補数字の数カウントして候補が3つしか残らず、その3つが対象の3候補数字と同じだったら
                                                    if ((Utility.GetOnBitList(candMemory, 10).Count == 3) && (candMemory == candMemoryTri))
                                                    {
                                                        //処理前盤面記憶
                                                        Utility.CopyToBufferMatrix(candMat, bufmat);

                                                        //3マスに適用
                                                        candMat[i][j] &= candMemory;
                                                        candMat[i][k] &= candMemory;
                                                        candMat[i][m] &= candMemory;

                                                        //処理後盤面変化なら候補使用して脱出
                                                        if (Utility.IsChangeBoard(candMat, bufmat))
                                                        {
                                                            if (DisplayUsedLogic)
                                                                _log+="\n"+string.Format("居候三つ子行222({0},{1}),({2},{3}),({4},{5})", i, j, i, k, i, m);

                                                            DoSingleLogics(mat, candMat);
                                                            return;
                                                        }
                                                    }

                                                }
                                            }
                                            candMemory = candMemoryBak;
                                        }
                                    }
                                }
                            }
                            candMemory = 0;

                        }
                    }
                }

            }
            candMemory = 0;
            candMemoryBak = 0;
            candMemorySame = 0;
            candMemoryDif = 0;
            candMemoryTri = 0;
            //列内で候補が3つしかない判定
            for (int j = 0; j < Utility.COL; j++)
            {
                DifficultScore++;
                for (int i = 0; i < Utility.ROW; i++)
                {
                    if (candMat[i][j] > 0)
                    {//マス(i,j)が埋まってなかったら
                        for (int k = 0; k < Utility.ROW; k++)
                        {
                            //333 332
                            if (k != i)
                            {
                                //他のマスの候補数字と一致する候補数字を記憶
                                candMemory = candMat[i][j] & candMat[k][j];
                            }

                            //一致した候補数字の数カウントして一致したのが1つ以下の場合強制終了
                            if (Utility.GetOnBitList(candMemory, 10).Count < 2)
                                candMemory = 0;

                            if (candMemory > 0)
                            {
                                candMemoryBak = candMemory;
                                for (int m = 0; m < Utility.ROW; m++)
                                {
                                    if ((m != i) && (m != k))
                                    {
                                        //7マスに対し2マスで共通する数字とダブリが2つ以上なら
                                        if (Utility.GetOnBitList((candMemory & candMat[m][j]), 10).Count >= 2)
                                        {
                                            for (int n = 0; n < Utility.ROW; n++)
                                            {
                                                //6マスのうち
                                                if ((n != i) && (n != k) && (n != m))
                                                {
                                                    //他のマスで出ている候補を「3マスで共通する候補数字」から除外
                                                    candMemory &= ~candMat[n][j];
                                                }
                                            }

                                            //一致した候補数字の数カウントして候補が3つしか残らなかったら かつ 3マス目にその候補のうち2つ以上一致する数字が存在するとき
                                            if (Utility.GetOnBitList(candMemory, 10).Count == 3)
                                            {
                                                if (Utility.GetOnBitList((candMat[m][j] & candMemory), 10).Count >= 2)
                                                {
                                                    //処理前盤面記憶
                                                    Utility.CopyToBufferMatrix(candMat, bufmat);

                                                    //3マスに適用
                                                    candMat[i][j] &= candMemory;
                                                    candMat[k][j] &= candMemory;
                                                    candMat[m][j] &= candMemory;

                                                    //処理後盤面変化なら候補使用して脱出
                                                    if (Utility.IsChangeBoard(candMat, bufmat))
                                                    {
                                                        if (DisplayUsedLogic)
                                                            _log+="\n"+string.Format("居候三つ子列333({0},{1}),({2},{3}),({4},{5})", i, j, k, j, m, j);

                                                        DoSingleLogics(mat, candMat);
                                                        return;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    candMemory = candMemoryBak;
                                }
                            }
                            candMemory = 0;

                            candMemory = 0;
                            candMemoryBak = 0;
                            candMemorySame = 0;
                            candMemoryDif = 0;
                            candMemoryTri = 0;
                            //222 223
                            if (k != i)
                            {
                                if (candMat[k][j] > 0)
                                {
                                    //少なくともどっちかで出ている候補数字を記憶
                                    candMemory = candMat[i][j] | candMat[k][j];
                                    //両方で一致する候補数字を記憶
                                    candMemorySame = candMat[i][j] & candMat[k][j];
                                    //両方で一致しない候補数字を記憶
                                    candMemoryDif = (candMat[i][j] | candMat[k][j]) - (candMat[i][j] & candMat[k][j]);
                                }
                            }

                            //一致しない候補数字の数カウントして一致しないのが2つ以外の場合強制終了
                            if (Utility.GetOnBitList(candMemoryDif, 10).Count != 2)
                                candMemory = 0;

                            if (candMemory > 0)
                            {
                                candMemoryBak = candMemory;
                                //2マスで一致する数字のうち1つを取って
                                for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                                {
                                    if ((candMemorySame & bit) == bit)
                                    {
                                        //2マスで一致する数字のうち1つと2マスで一致しない数字のORを取り
                                        candMemoryTri = bit | candMemoryDif;
                                        //3マス目探索
                                        for (int m = 0; m < Utility.ROW; m++)
                                        {
                                            if ((m != i) && (m != k))
                                            {
                                                //7マスに対し、2マスで共通する数字1つと一致しない数字2つのうち2つ以上を含んでいるとき
                                                if (Utility.GetOnBitList((candMemoryTri & candMat[m][j]), 10).Count >= 2)
                                                {
                                                    for (int n = 0; n < Utility.COL; n++)
                                                    {
                                                        if ((n != i) && (n != k) && (n != m))
                                                        {//6マスのうち
                                                         //3マス以外で出ている候補を「3マスのどれかにある候補数字」から除外
                                                            candMemory = (candMemory | candMat[n][j]) - candMat[n][j];
                                                        }
                                                    }

                                                    //一致した候補数字の数カウントして候補が3つしか残らず、残った数字が対象の3候補数字と同じだったら
                                                    if ((Utility.GetOnBitList(candMemory, 10).Count == 3) && (candMemory == candMemoryTri))
                                                    {
                                                        //処理前盤面記憶
                                                        Utility.CopyToBufferMatrix(candMat, bufmat);

                                                        //3マスに適用
                                                        candMat[i][j] &= candMemory;
                                                        candMat[k][j] &= candMemory;
                                                        candMat[m][j] &= candMemory;

                                                        //処理後盤面変化なら候補使用して脱出
                                                        if (Utility.IsChangeBoard(candMat, bufmat))
                                                        {
                                                            if (DisplayUsedLogic)
                                                                _log+="\n"+string.Format("居候三つ子列222({0},{1}),({2},{3}),({4},{5})", i, j, k, j, m, j);

                                                            DoSingleLogics(mat, candMat);
                                                            return;
                                                        }
                                                        
                                                    }
                                                }
                                            }
                                            candMemory = candMemoryBak;
                                        }
                                    }
                                }
                            }
                            candMemory = 0;

                        }
                    }
                }

            }
            candMemory = 0;
            candMemoryBak = 0;
            candMemorySame = 0;
            candMemoryDif = 0;
            candMemoryTri = 0;
            //ブロック内で候補が3つしかない判定
            for (int i = 0; i < Utility.ROW; i = i + 3)
            {
                for (int j = 0; j < Utility.COL; j = j + 3)
                {
                    DifficultScore++;
                    for (int p = i; p <= i + 2; p++)
                    {
                        for (int q = j; q <= j + 2; q++)
                        {
                            if (candMat[p][q] > 0)
                            {//マス(i,j)が埋まってなかったら
                                for (int s = i; s <= i + 2; s++)
                                {
                                    for (int t = j; t <= j + 2; t++)
                                    {//そのブロック検索して
                                     //333 332
                                        if (!((s == p) && (t == q)))
                                            //他のマスの候補数字と一致する候補数字を記憶
                                            candMemory = candMat[p][q] & candMat[s][t];

                                        //一致した候補数字の数カウントして一致したのが2つ以下の場合強制終了
                                        if (Utility.GetOnBitList(candMemory, 10).Count < 3)
                                            candMemory = 0;

                                        if (candMemory > 0)
                                        {
                                            candMemoryBak = candMemory;
                                            for (int u = i; u <= i + 2; u++)
                                            {
                                                for (int w = j; w <= j + 2; w++)
                                                {
                                                    //7マスに対し
                                                    if (!((u == p) && (w == q)) && !((u == s) && (w == t)))
                                                    {
                                                        //2マスで共通する数字とダブリが2つ以上なら
                                                        if (Utility.GetOnBitList((candMat[u][w] & candMemory), 10).Count >= 2)
                                                        {
                                                            for (int x = i; x <= i + 2; x++)
                                                                for (int y = j; y <= j + 2; y++)
                                                                    //さらにブロックを探索し
                                                                    if (!((x == p) && (y == q)) && !((x == s) && (y == t)) && !((x == u) && (y == w)))
                                                                        //3マス以外の
                                                                        //他のマスで出てる候補を「3マスで共通する候補数字」から除外
                                                                        candMemory &= ~candMat[x][y];

                                                            //一致した候補数字の数カウントして候補が3つしか残らなかったら 
                                                            if (Utility.GetOnBitList(candMemory, 10).Count == 3)
                                                            {
                                                                //3マス目にその候補のうち2つ以上一致する数字が存在するとき
                                                                if (Utility.GetOnBitList((candMat[u][w] & candMemory), 10).Count >= 2)
                                                                {
                                                                    //処理前盤面記憶
                                                                    Utility.CopyToBufferMatrix(candMat, bufmat);

                                                                    //3マスに適用
                                                                    candMat[p][q] &= candMemory;
                                                                    candMat[s][t] &= candMemory;
                                                                    candMat[u][w] &= candMemory;

                                                                    //処理後盤面変化なら候補使用して脱出
                                                                    if (Utility.IsChangeBoard(candMat, bufmat))
                                                                    {
                                                                        if (DisplayUsedLogic)
                                                                            _log+="\n"+string.Format("居候三つ子ブロック333({0},{1}),({2},{3}),({4},{5})", p, q, s, t, u, w);
                                                                        DoSingleLogics(mat, candMat);
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //2マスで共通する候補数字復活
                                                    candMemory = candMemoryBak;
                                                }
                                            }
                                        }
                                        candMemory = 0;

                                        candMemory = 0;
                                        candMemoryBak = 0;
                                        candMemorySame = 0;
                                        candMemoryDif = 0;
                                        candMemoryTri = 0;
                                        //222 223
                                        if (!((s == p) && (t == q)))
                                        {
                                            if (candMat[s][t] > 0)
                                            {
                                                //少なくともどっちかで出ている候補数字を記憶
                                                candMemory = candMat[p][q] | candMat[s][t];
                                                //両方で一致する候補数字を記憶
                                                candMemorySame = candMat[p][q] & candMat[s][t];
                                                //両方で一致しない候補数字を記憶
                                                candMemoryDif = (candMat[p][q] | candMat[s][t]) - (candMat[p][q] & candMat[s][t]);
                                            }
                                        }
                                        //一致しない候補数字の数カウントして一致しないのが2つ以外の場合強制終了
                                        if (Utility.GetOnBitList(candMemoryDif, 10).Count != 2)
                                            candMemory = 0;

                                        if (candMemory > 0)
                                        {
                                            candMemoryBak = candMemory;
                                            //2マスで一致する数字のうち1つを取って
                                            for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                                            {
                                                if ((candMemorySame & bit) == bit)
                                                {
                                                    //2マスで一致する数字のうち1つと2マスで一致しない数字のORを取り
                                                    candMemoryTri = bit | candMemoryDif;

                                                    //3マス目探索
                                                    for (int u = i; u <= i + 2; u++)
                                                    {
                                                        for (int w = j; w <= j + 2; w++)
                                                        {
                                                            if (!((u == p) && (w == q)) && !((u == s) && (w == t)))
                                                            {
                                                                //7マスに対し、2マスで共通する数字1つと一致しない数字2つのうち2つ以上を含んでいるとき
                                                                if (Utility.GetOnBitList((candMemoryTri & candMat[u][w]), 10).Count >= 2)
                                                                {
                                                                    for (int x = i; x <= i + 2; x++)
                                                                        for (int y = j; y <= j + 2; y++)
                                                                            //さらにブロックを探索し
                                                                            if (!((x == p) && (y == q)) && !((x == s) && (y == t)) && !((x == u) && (y == w)))
                                                                                //3マス以外の
                                                                                //6マスで出ている候補を「3マスのどれかにある候補数字」から除外
                                                                                candMemory &= ~candMat[x][y];

                                                                    //一致した候補数字の数カウントして候補が3つしか残らず、残った数字が対象の3候補数字と同じだったら
                                                                    if ((Utility.GetOnBitList(candMemory, 10).Count == 3) && (candMemory == candMemoryTri))
                                                                    {
                                                                        //処理前盤面記憶
                                                                        Utility.CopyToBufferMatrix(candMat, bufmat);

                                                                        //3マスに適用
                                                                        candMat[p][q] &= candMemory;
                                                                        candMat[s][t] &= candMemory;
                                                                        candMat[u][w] &= candMemory;

                                                                        //処理後盤面変化なら候補使用して脱出
                                                                        if (Utility.IsChangeBoard(candMat, bufmat))
                                                                        {
                                                                            if (DisplayUsedLogic)
                                                                                _log+="\n"+string.Format("居候三つ子ブロック222({0},{1}),({2},{3}),({4},{5})", p, q, s, t, u, w);
                                                                            DoSingleLogics(mat, candMat);
                                                                            return;
                                                                        }																				
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        candMemory = candMemoryBak;
                                                    }
                                                }
                                            }
                                        }
                                        candMemory = 0;

                                    }
                                }
                            }
                        }
                    }

                }
            }
            #endregion
        }

        /// <summary>
        /// 共有候補法
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public void ShareCandLogic(int[][] mat, int[][] candMat)
        {
            int blankCheck = 0, candHit3Cell = 0, candHit9Cell = 0;
            int[][] bufMat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);


            if (Utility.IsCompleteBoard(mat))
                return;

            //行→ブロック
            for (int i = 0; i < Utility.ROW; i++)
            {
                //ビットごとの判定
                for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                {
                    //ブロック座標での1行の各ブロックごとに判定
                    for (int m = 0; m < 9; m = m + 3)
                    {
                        //1ブロックの1行分の3マスで
                        for (int j = m; j <= m + 2; j++)
                            //空白マスかチェック
                            if (candMat[i][j] > 0)
                                blankCheck++;
                        //空白が2マス以上のとき
                        if (blankCheck > 1)
                        {
                            //1ブロックの1行分の3マスで候補aの数カウント
                            for (int j = m; j <= m + 2; j++)
                                if ((candMat[i][j] & bit) == bit)
                                    candHit3Cell++;
                            //1ブロックの1行分の3マス内で候補aが２個以上あるとき
                            if (candHit3Cell > 1)
                            {
                                //printf("%d,%d\n",i,bit);

                                //ブロックの9マスを探索して候補aの数をカウント
                                for (int j = 0; j < Utility.COL; j++)
                                    if ((candMat[i][j] & bit) == bit)
                                        candHit9Cell++;

                                /*if(bit == 256){
                                    printf("%d,%d,%d\n",i,candhit9,candhit3);
                                }*/

                                //現在ブロックのi行の３マス以外の６マスに候補aが存在しないとき
                                //つまり現在ブロックのi行の3マスにしか候補aが存在しないとき
                                //if (Utility.CandExistRowCellCount(candMat, i, 0, Utility.COL, bit)
                                //    == Utility.CandExistRowCellCount(candMat, i, m, m + 2, bit))

                                if (candHit9Cell == candHit3Cell)
                                {
                                    //その３マスを含むブロックの
                                    var blockPoint = Utility.GetBlockPoint(i, m);

                                    //その３マス以外の６マスから候補aを除外
                                    for (int u = blockPoint.X; u <= blockPoint.X + 2; u++)
                                    {
                                        for (int w = blockPoint.Y; w <= blockPoint.Y + 2; w++)
                                        {
                                            //処理前盤面記憶
                                            Utility.CopyToBufferMatrix(candMat, bufMat);
                                            //現在の行は除いて
                                            if (u != i)
                                                candMat[u][w] &= ~bit;
                                            //処理後盤面変化なら候補使用して脱出
                                            if (Utility.IsChangeBoard(candMat, bufMat))
                                            {
                                                if (DisplayUsedLogic)
                                                    _log+="\n"+string.Format("共有行→ブロック({0},{1})", i, m);

                                                //Utility.Mistake(mat, string.Format("共有行→ブロック({0},{1})", i, m));
                                                DoSingleLogics(mat, candMat);
                                                //Utility.Mistake(mat, string.Format("共有行→ブロック({0},{1})", i, m));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            candHit3Cell = 0;
                            candHit9Cell = 0;
                        }
                        blankCheck = 0;
                    }
                    DifficultScore++;

                }
            }

            blankCheck = 0;
            candHit3Cell = 0;
            candHit9Cell = 0;
            //列→ブロック
            for (int j = 0; j < Utility.COL; j++)
            {
                for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                {
                    for (int m = 0; m < 9; m = m + 3)
                    {
                        for (int i = m; i <= m + 2; i++)
                        {
                            //空白マスかチェック
                            if (candMat[i][j] > 0) { blankCheck++; }
                        }
                        //空白が2マス以上のとき
                        if (blankCheck > 1)
                        {
                            //３マスで候補aの数カウント
                            for (int i = m; i <= m + 2; i++)
                                if ((candMat[i][j] & bit) == bit)
                                    candHit3Cell++;
                            //３マス内で候補aが２個以上あるとき
                            if (candHit3Cell > 1)
                            {
                                for (int i = 0; i < Utility.ROW; i++)
                                    if ((candMat[i][j] & bit) == bit)
                                        candHit9Cell++;
                                //この行の３マス以外の６マスに候補aが存在しないとき
                                if (candHit9Cell == candHit3Cell)
                                {
                                    //その３マスを含むブロックの
                                    var blockPoint = Utility.GetBlockPoint(m, j);
                                    //その３マス以外の６マスから候補aを除外
                                    for (int u = blockPoint.X; u <= blockPoint.X + 2; u++)
                                    {
                                        for (int w = blockPoint.Y; w <= blockPoint.Y + 2; w++)
                                        {
                                            //処理前盤面記憶
                                            Utility.CopyToBufferMatrix(candMat, bufMat);
                                            //現在の列は除いて
                                            if (w != j)
                                                candMat[u][w] &= ~bit;
                                            //処理後盤面変化なら候補使用して脱出
                                            if (Utility.IsChangeBoard(candMat, bufMat))
                                            {
                                                if (DisplayUsedLogic)
                                                    _log+="\n"+string.Format("共有列→ブロック({0},{1})", m, j);

                                                //Utility.Mistake(mat, string.Format("共有列→ブロック({0},{1})", m, j));
                                                DoSingleLogics(mat, candMat);
                                                //Utility.Mistake(mat, string.Format("共有列→ブロック({0},{1})", m, j));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            candHit3Cell = 0;
                            candHit9Cell = 0;
                        }
                        blankCheck = 0;
                    }
                    DifficultScore++;

                }
            }
            blankCheck = 0;
            candHit3Cell = 0;
            candHit9Cell = 0;
            //ブロック→行、列
            for (int i = 0; i < 9; i = i + 3)
            {
                for (int j = 0; j < 9; j = j + 3)
                {
                    for (int bit = 2; bit < (int)Math.Pow(2, 10); bit <<= 1)
                    {

                        //行
                        for (int p = i; p <= i + 2; p++)
                        {
                            for (int q = j; q <= j + 2; q++)
                            {
                                //空白マスかチェック
                                if (candMat[p][q] > 0) { blankCheck++; }
                            }

                            //空白が2マス以上のとき
                            if (blankCheck > 1)
                            {
                                //３マスで候補aの数カウント
                                for (int q = j; q <= j + 2; q++)
                                {
                                    if ((candMat[p][q] & bit) == bit) { candHit3Cell++; }
                                }
                                //３マス内で候補aが２個以上あるとき
                                if (candHit3Cell > 1)
                                {
                                    for (int o = i; o <= i + 2; o++)
                                    {
                                        for (int q = j; q <= j + 2; q++)
                                        {
                                            if ((candMat[o][q] & bit) == bit) { candHit9Cell++; }
                                        }
                                    }
                                    //この行の３マス以外の６マスに候補aが存在しないとき
                                    if (candHit9Cell == candHit3Cell)
                                    {
                                        //処理前盤面記憶
                                        Utility.CopyToBufferMatrix(candMat, bufMat);
                                        //その３マスを含む行のその３マス以外の６マスから候補aを除外
                                        for (int w = 0; w < Utility.COL; w++)
                                        {
                                            //現在のブロックは除いて
                                            if ((w != j) && (w != j + 1) && (w != j + 2))
                                            {
                                                candMat[p][w] = (candMat[p][w] | bit) - bit;
                                            }
                                        }
                                        //処理後盤面変化なら候補使用して脱出
                                        if (Utility.IsChangeBoard(candMat, bufMat))
                                        {
                                            if (DisplayUsedLogic)
                                                _log+="\n"+string.Format("共有ブロック→行({0},{1})", i, j);

                                            //Utility.Mistake(mat, string.Format("共有ブロック→行({0},{1})", i, j));
                                            DoSingleLogics(mat, candMat);
                                            //Utility.Mistake(mat, string.Format("共有ブロック→行({0},{1})", i, j));
                                            return;
                                        }
                                        
                                    }
                                }
                                candHit3Cell = 0;
                                candHit9Cell = 0;
                            }
                            blankCheck = 0;

                        }
                        blankCheck = 0;
                        candHit3Cell = 0;
                        candHit9Cell = 0;
                        //列
                        for (int q = j; q <= j + 2; q++)
                        {
                            for (int p = i; p <= i + 2; p++)
                            {
                                //空白マスかチェック
                                if (candMat[p][q] > 0) { blankCheck++; }
                            }

                            //空白が2マス以上のとき
                            if (blankCheck > 1)
                            {
                                //３マスで候補aの数カウント
                                for (int p = i; p <= i + 2; p++)
                                {
                                    if ((candMat[p][q] & bit) == bit) { candHit3Cell++; }
                                }
                                //３マス内で候補aが２個以上あるとき
                                if (candHit3Cell > 1)
                                {
                                    for (int o = j; o <= j + 2; o++)
                                    {
                                        for (int p = i; p <= i + 2; p++)
                                        {
                                            if ((candMat[p][o] & bit) == bit) { candHit9Cell++; }
                                        }
                                    }
                                    //この列の３マス以外の６マスに候補aが存在しないとき
                                    if (candHit9Cell == candHit3Cell)
                                    {
                                        //処理前盤面記憶
                                        Utility.CopyToBufferMatrix(candMat, bufMat);
                                        //その３マスを含む列のその３マス以外の６マスから候補aを除外
                                        for (int w = 0; w < Utility.ROW; w++)
                                        {
                                            //現在のブロックは除いて
                                            if ((w != i) && (w != i + 1) && (w != i + 2))
                                            {
                                                candMat[w][q] = (candMat[w][q] | bit) - bit;

                                            }
                                        }
                                        //処理後盤面変化なら候補使用して脱出
                                        if (Utility.IsChangeBoard(candMat, bufMat))
                                        {
                                            if (DisplayUsedLogic)
                                                _log+="\n"+string.Format("共有ブロック→列({0},{1})", i, j);

                                            //Utility.Mistake(mat, string.Format("共有ブロック→列({0},{1})", i, j));
                                            DoSingleLogics(mat, candMat);
                                            //Utility.Mistake(mat, string.Format("共有ブロック→列({0},{1})", i, j));
                                            return;
                                        }
                                    }
                                }
                                candHit3Cell = 0;
                                candHit9Cell = 0;
                            }
                            blankCheck = 0;

                        }
                        DifficultScore++;

                    }
                }
            }

        }

        /// <summary>
        /// 双子法
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public void PairLogic(int[][] mat, int[][] candMat)
        {
            int[][] bufmat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);

            if (Utility.IsCompleteBoard(mat)) return;

            //行内で候補が２つしかない
            for (int i = 0; i < Utility.ROW; i++)
            {
                DifficultScore++;
                for (int j = 0; j < Utility.COL; j++)
                {
                    //まとめられねーかなーと思ったが、2次元配列のどちらかの次元数を指定してforループなんてできないので無理

                    //その1マスで2個しか候補数字がなかったら
                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 2)
                    {
                        //その行検索して
                        for (int m = 0; m < Utility.COL; m++)
                        {
                            //2個の候補数字が別々の2マスで一致したら
                            if ((m != j) && (candMat[i][m] == candMat[i][j]))
                            {
                                //処理前盤面記憶
                                Utility.CopyToBufferMatrix(candMat, bufmat);

                                for (int n = 0; n < Utility.COL; n++)
                                    //他のマスのその2つの候補数字を候補から除外
                                    if ((n != m) && (n != j))
                                        candMat[i][n] &= ~candMat[i][j];

                                //処理後盤面変化なら候補使用して脱出
                                if (Utility.IsChangeBoard(candMat, bufmat))
                                {
                                    if (DisplayUsedLogic)
                                        _log+="\n"+string.Format("独立双子行({0},{1}),({2},{3})", i, j, i, m);

                                    //Utility.Mistake(mat, string.Format("独立双子行({0},{1}),({2},{3})", i, j, i, m));
                                    DoSingleLogics(mat, candMat);
                                    //Utility.Mistake(mat, string.Format("独立双子行後({0},{1}),({2},{3})", i, j, i, m));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            //列内で候補が２つしかない
            for (int j = 0; j < Utility.COL; j++)
            {
                DifficultScore++;
                for (int i = 0; i < Utility.ROW; i++)
                {
                    //その1マスで2個しか候補数字がなかったら
                    if (Utility.GetOnBitList(candMat[i][j], 10).Count == 2)
                    {
                        //その列検索して
                        for (int m = 0; m < Utility.ROW; m++)
                        {
                            //2個の候補数字が異なる2マスで一致したら
                            if ((m != i) && (candMat[m][j] == candMat[i][j]))
                            {
                                //処理前盤面記憶
                                Utility.CopyToBufferMatrix(candMat, bufmat);

                                for (int n = 0; n < Utility.ROW; n++)
                                    //他のマスのその2つの候補数字を候補から除外
                                    if ((n != m) && (n != i))
                                        candMat[n][j] &= ~candMat[i][j];

                                //処理後盤面変化なら候補使用して脱出
                                if (Utility.IsChangeBoard(candMat, bufmat))
                                {
                                    if (DisplayUsedLogic)
                                        _log+="\n"+string.Format("独立双子列({0},{1}),({2},{3})", i, j, m, j);

                                    //Utility.Mistake(mat, string.Format("独立双子列({0},{1}),({2},{3})", i, j, m, j));
                                    DoSingleLogics(mat, candMat);
                                    //Utility.Mistake(mat, string.Format("独立双子列後({0},{1}),({2},{3})", i, j, m, j));
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //ブロック内で候補が２つしかない
            //ブロック座標(i,j)移動のために最初の2重ループ
            //さらに最初の検索対象マス(p,q)とそれと比較する1マス(s,t)と
            //前記2マス以外のマス(u,w)の3種類のマスを総ざらいするため二重forループが4つ必要
            for (int i = 0; i < Utility.ROW; i = i + 3)
            {
                for (int j = 0; j < Utility.COL; j = j + 3)
                {
                    DifficultScore++;
                    for (int p = i; p <= i + 2; p++)
                        for (int q = j; q <= j + 2; q++)
                            //その1マスで2個しか候補数字がなかったら
                            if (Utility.GetOnBitList(candMat[p][q], 10).Count == 2)
                                //そのブロック検索して
                                for (int s = i; s <= i + 2; s++)
                                    for (int t = j; t <= j + 2; t++)
                                        //2個の候補数字が異なる2マスで一致したら
                                        if (!((s == p) && (t == q)) && (candMat[s][t] == candMat[p][q]))
                                        {
                                            //処理前盤面記憶
                                            Utility.CopyToBufferMatrix(candMat, bufmat);

                                            for (int u = i; u <= i + 2; u++)
                                                for (int w = j; w <= j + 2; w++)
                                                    //他のマスのその2つの候補数字を候補から除外
                                                    if (!((u == p) && (w == q)) && !((u == s) && (w == t)))
                                                        candMat[u][w] &= ~candMat[p][q];

                                            //処理後盤面変化なら候補使用して脱出
                                            if (Utility.IsChangeBoard(candMat, bufmat))
                                            {
                                                if (DisplayUsedLogic)
                                                    _log+="\n"+string.Format("独立双子ブロック({0},{1}),({2},{3})", p, q, s, t);

                                                //Utility.Mistake(mat, string.Format("独立双子ブロック({0},{1}),({2},{3})", p, q, s, t));
                                                DoSingleLogics(mat, candMat);
                                                //Utility.Mistake(mat, string.Format("独立双子ブロック({0},{1}),({2},{3})", p, q, s, t));
                                                return;
                                            }
                                        }


                }
            }
            
            //居候双子法
            //行内で候補が2つしかない判定
            for (int i = 0; i < Utility.ROW; i++)
            {
                DifficultScore++;
                for (int j = 0; j < Utility.COL; j++)
                {
                    //埋まってなかったら
                    if (candMat[i][j] > 0)
                    {
                        //一致する候補数字を記憶
                        for (int k = 0; k < Utility.COL; k++)
                        {
                            //独立双子の場合は除き 別にやんなくて良かったかも 何の意味が？
                            if ((Utility.GetOnBitList(candMat[i][j], 10).Count > 2)
                            && (Utility.GetOnBitList(candMat[i][k], 10).Count > 2))
                            {
                                if (j != k)
                                {
                                    //他のマスと比較
                                    int candmemory = candMat[i][j] & candMat[i][k];
                                    //一致したのが一つの場合強制終了
                                    if (Utility.GetOnBitList(candmemory, 10).Count < 2)
                                        continue;

                                    if (candmemory > 0)
                                    {
                                        for (int m = 0; m < Utility.COL; m++)
                                            if ((m != j) && (m != k))
                                                //他のマスで出てる候補を「一致する候補数字」から除外
                                                candmemory &= ~candMat[i][m];

                                        //候補が2つしか残らなかったら
                                        if (Utility.GetOnBitList(candmemory, 10).Count == 2)
                                        {
                                            //処理前盤面記憶
                                            Utility.CopyToBufferMatrix(candMat, bufmat);

                                            //2マスに適用
                                            candMat[i][j] &= candmemory;
                                            candMat[i][k] &= candmemory;

                                            //処理後盤面変化なら候補使用して脱出
                                            if (Utility.IsChangeBoard(candMat, bufmat))
                                            {
                                                if (DisplayUsedLogic)
                                                    _log+="\n"+string.Format("居候双子行({0},{1}),({2},{3})", i, j, i, k);

                                                //Utility.Mistake(mat, string.Format("居候双子行({0},{1}),({2},{3})", i, j, i, k));
                                                DoSingleLogics(mat, candMat);
                                                //Utility.Mistake(mat, string.Format("居候双子行({0},{1}),({2},{3})", i, j, i, k));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //列内で候補が2つしかない判定
            for (int j = 0; j < Utility.COL; j++)
            {
                DifficultScore++;
                for (int i = 0; i < Utility.ROW; i++)
                {
                    if (candMat[i][j] > 0)
                    {//埋まってなかったら
                     //一致する候補数字を記憶
                        for (int k = 0; k < Utility.ROW; k++)
                        {
                            //独立双子の場合は除き
                            if ((Utility.GetOnBitList(candMat[i][j], 10).Count > 2)
                            && (Utility.GetOnBitList(candMat[i][k], 10).Count > 2))
                            {
                                if (i != k)
                                {
                                    //他のマスと比較
                                    int candmemory = candMat[i][j] & candMat[k][j];

                                    //一致したのが2つ未満の場合強制終了
                                    if (Utility.GetOnBitList(candmemory, 10).Count < 2)
                                        continue;

                                    if (candmemory > 0)
                                    {
                                        for (int m = 0; m < Utility.ROW; m++)
                                            if ((m != i) && (m != k))
                                                //他のマスで出てる候補を「2マスで一致する候補数字」から除外
                                                candmemory &= ~candMat[m][j];

                                        //候補が2つしか残らなかったら
                                        if (Utility.GetOnBitList(candmemory, 10).Count == 2)
                                        {
                                            //処理前盤面記憶
                                            Utility.CopyToBufferMatrix(candMat, bufmat);

                                            //2マスに適用
                                            candMat[i][j] &= candmemory;
                                            candMat[k][j] &= candmemory;

                                            //処理後盤面変化なら候補使用して脱出
                                            if (Utility.IsChangeBoard(candMat, bufmat))
                                            {
                                                if (DisplayUsedLogic)
                                                    _log+="\n"+string.Format("居候双子列({0},{1}),({2},{3})", i, j, k,j);

                                                //Utility.Mistake(mat, string.Format("居候双子列({0},{1}),({2},{3})", i, j, k, j));
                                                DoSingleLogics(mat, candMat);
                                                //Utility.Mistake(mat, string.Format("居候双子列({0},{1}),({2},{3})", i, j, k, j));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //ブロック内で候補が2つしかない判定
            for (int i = 0; i < Utility.ROW; i = i + 3)
            {
                for (int j = 0; j < Utility.COL; j = j + 3)
                {
                    DifficultScore++;
                    //ブロック内を検索　計３回やるからややこしいことこの上ない 最大8ネスト
                    for (int p = i; p <= i + 2; p++)
                    {
                        for (int q = j; q <= j + 2; q++)
                        {
                            //埋まってなかったら
                            if (candMat[p][q] > 0)
                            {
                                 //ブロック内を検索し一致する候補数字を記憶
                                for (int s = i; s <= i + 2; s++)
                                {
                                    for (int t = j; t <= j + 2; t++)
                                    {
                                        //独立双子の場合は除き
                                        if ((Utility.GetOnBitList(candMat[s][t], 10).Count > 2)
                                            && (Utility.GetOnBitList(candMat[p][q], 10).Count > 2))
                                        {
                                            //同じマスは除いて
                                            if (!((s == p) && (t == q)))
                                            {
                                                //他のマスと一致する候補数字を取得
                                                int candmemory = candMat[p][q] & candMat[s][t];

                                                //一致したのが2つ未満の場合強制終了
                                                if (Utility.GetOnBitList(candmemory, 10).Count < 2)
                                                    continue;

                                                if (candmemory > 0)
                                                {
                                                    for (int u = i; u <= i + 2; u++)
                                                        for (int v = j; v <= j + 2; v++)
                                                            //ブロック内を検索し
                                                            if (!((u == p) && (v == q)) && !((u == s) && (v == t)))
                                                                //他のマスで出てる候補を「一致する候補数字」から除外
                                                                candmemory &= ~candMat[u][v];

                                                    //候補が2つしか残らなかったら
                                                    if (Utility.GetOnBitList(candmemory, 10).Count == 2)
                                                    {
                                                        //処理前盤面記憶
                                                        Utility.CopyToBufferMatrix(candMat, bufmat);

                                                        //2マスに適用
                                                        candMat[p][q] &= candmemory;
                                                        candMat[s][t] &= candmemory;

                                                        //処理後盤面変化なら候補使用して脱出
                                                        if (Utility.IsChangeBoard(candMat, bufmat))
                                                        {
                                                            if (DisplayUsedLogic)
                                                                _log+="\n"+string.Format("居候双子ブロック({0},{1}),({2},{3})", p, q, s, t);

                                                            //Utility.Mistake(mat, string.Format("居候双子ブロック({0},{1}),({2},{3})", p, q, s, t));
                                                            DoSingleLogics(mat, candMat);
                                                            //Utility.Mistake(mat, string.Format("居候双子ブロック({0},{1}),({2},{3})", p, q, s, t));
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 単一候補法と単一マス法を使用できなくなるまで使用する
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public void DoSingleLogics(int[][] mat, int[][] candMat)
        {
            bool loopFlag = true;
            int[][] bufMat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);

            //盤面変化ならループ
            while (loopFlag)
            {
                //処理前盤面記憶
                Utility.CopyToBufferMatrix(mat, bufMat);

                //埋まってるマスの候補を全部消す
                Utility.FilledCellCandidateDel(mat, candMat);
                //単一候補法
                SinglizeCandidateLogic(mat, candMat);
                //単一マス法
                SingleCellFillLogic(candMat);

                //処理後盤面変化ならループ
                loopFlag = Utility.IsChangeBoard(mat, bufMat);
                //完成時終了
                if (Utility.IsCompleteBoard(mat))
                    break;
            }
        }

        /// <summary>
        /// 単一候補法
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candmat"></param>
        public void SinglizeCandidateLogic(int[][] mat, int[][] candmat)
        {
            List<int> candList = new List<int>();

            for (int i = 0; i < Utility.ROW; i++)
                for (int j = 0; j < Utility.COL; j++)
                    if (candmat[i][j] > 0)
                    {
                        //候補の取得
                        candList = Utility.GetOnBitList(candmat[i][j], 10);

                        if (candList.Count == 1)
                        {//1個しか候補なかったら記憶したビットに対応する数字を確定
                            mat[i][j] = candList[0];
                            Utility.ExcludeCandidateInRowColBlock(candmat, new Point(i, j), (int)Math.Pow(2, candList[0]));
                        }
                    }

        }

        /// <summary>
        /// 単一マス法
        /// </summary>
        /// <param name="candmat"></param>
        public void SingleCellFillLogic(int[][] candmat)
        {
            int[] existCheckRow = Enumerable.Repeat(0, 10).ToArray();//要素の数字が使用済みか判定用
            int[] existCheckCol = Enumerable.Repeat(0, 10).ToArray();
            int[] existCheckBlock = Enumerable.Repeat(0, 10).ToArray();

            for (int i = 0; i < Utility.ROW; i++)
                for (int j = 0; j < Utility.COL; j++)
                    if (candmat[i][j] > 0)
                    {
                        /*行内で候補が1つしかない判定*/
                        for (int q = 0; q < Utility.COL; q++)
                            Utility.CandidateNumCheck(candmat[i][q], existCheckRow);

                        //1回しか出てこなかった候補を検索
                        for (int m = 1; m < 10; m++)
                            if (existCheckRow[m] == 1)
                                for (int q = 0; q < Utility.COL; q++)
                                    //その候補が入るマスのその候補以外の候補数字を消去
                                    if ((candmat[i][q] & (int)Math.Pow(2, m)) == (int)Math.Pow(2, m))
                                        candmat[i][q] &= (int)Math.Pow(2, m);

                        existCheckRow = Enumerable.Repeat(0, 10).ToArray();

                        /*列内で候補が1つしかない判定*/
                        for (int p = 0; p < Utility.ROW; p++)
                            Utility.CandidateNumCheck(candmat[p][j], existCheckCol);

                        //1回しか出てこなかった候補を検索
                        for (int m = 1; m < 10; m++)
                            if (existCheckCol[m] == 1)
                                for (int p = 0; p < Utility.ROW; p++)
                                    //その候補が入るマスのその候補以外の候補数字を消去
                                    if ((candmat[p][j] & (int)Math.Pow(2, m)) == (int)Math.Pow(2, m))
                                        candmat[p][j] = candmat[p][j] & (int)Math.Pow(2, m);

                        existCheckCol = Enumerable.Repeat(0, 10).ToArray();

                        /*ブロック内で候補が1つしかない判定*/
                        //マス(i,j)があるブロックを探索　厳密にいえばマスに着目しているわけではない
                        var blockPoint = Utility.GetBlockPoint(i, j);
                        for (int p = blockPoint.X; p <= blockPoint.X + 2; p++)
                            for (int q = blockPoint.Y; q <= blockPoint.Y + 2; q++)
                                Utility.CandidateNumCheck(candmat[p][q], existCheckRow);
                        //1回しか出てこなかった候補を検索
                        for (int m = 1; m < 10; m++)
                            if (existCheckBlock[m] == 1)
                                for (int p = blockPoint.X; p <= blockPoint.X + 2; p++)
                                    for (int q = blockPoint.Y; q <= blockPoint.Y + 2; q++)
                                        //その候補が入るマスのその候補以外の候補数字を消去
                                        //要するに1回しか出てこなかった数字のあるマスに対し適用
                                        if ((candmat[p][q] & (int)Math.Pow(2, m)) == (int)Math.Pow(2, m))
                                            candmat[p][q] = candmat[p][q] & (int)Math.Pow(2, m);

                        existCheckBlock = Enumerable.Repeat(0, 10).ToArray();
                    }
        }

        /// <summary>
        /// 候補数字の書き込み
        /// </summary>
        /// <param name="mat"></param>
        public int[][] WriteCandidate(int[][] matrix)
        {
            int[][] candidateMat = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);

            //埋まってないマスの候補を全部書く
            for (int i = 0; i < Utility.ROW; i++)
                for (int j = 0; j < Utility.COL; j++)
                    if (matrix[i][j] == 0)
                    {
                        candidateMat[i][j] = 1022;//1111111110
                        DifficultScore++;
                    }
            //候補数字書き込み(というか絞り込み)　書き込み時にスコア++だけど
            //これ入らないのを消去してるだけだから厳密に言えば書き込んでない
            for (int i = 0; i < Utility.ROW; i++)
            {
                for (int j = 0; j < Utility.COL; j++)
                {
                    for (int n = 1; n < 10; n++)
                    {
                        //そのマスに入っている数字を同列同行同ブロックの候補から除外
                        if (matrix[i][j] == n)
                        {
                            for (int k = 0; k < Utility.ROW; k++)
                                if ((candidateMat[k][j] & (int)Math.Pow(2,n)) == (int)Math.Pow(2, n))
                                    candidateMat[k][j] = candidateMat[k][j] & ~(int)Math.Pow(2, n);

                            for (int l = 0; l < Utility.COL; l++)
                                if ((candidateMat[i][l] & (int)Math.Pow(2, n)) == (int)Math.Pow(2, n))
                                    candidateMat[i][l] = candidateMat[i][l] & ~(int)Math.Pow(2, n);

                            Point blockPoint = Utility.GetBlockPoint(i, j);
                            for (int r = blockPoint.X; r <= blockPoint.X + 2; r++)
                                for (int s = blockPoint.Y; s <= blockPoint.Y + 2; s++)
                                    if ((candidateMat[r][s] & (int)Math.Pow(2, n)) == (int)Math.Pow(2, n))
                                        candidateMat[r][s] = candidateMat[r][s] & ~(int)Math.Pow(2, n);
                        }
                    }
                }
            }

            return candidateMat;
        }


        public void Crbe(int[][] mat)
        {
            int prevSearchNum = -1,//0-9だとアカン
                loopCount = 9;
            //盤面変化比較用のバッファ
            int[][] bufferMatCrbe = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL), 
                bufferMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            //座標のマスが空いているか否か
            bool[][] isBlankPoint = new bool[Utility.ROW][];
            //探索したがCRBEで埋められなかった数字とその個数
            int[] cannotFillMaxExistNums = Enumerable.Repeat(0, 10).ToArray();
            //埋める数字それぞれが座標4マス分を保持　[埋める数字][デッドロックマスのID(1～4)]=デッドロックマスの座標
            Point[][] rest4CellPointsForNums = Utility.GetInit2DimArray<Point>(10, 4);
            
            for(int i=0;i< Utility.ROW;i++)
                isBlankPoint[i] = Enumerable.Repeat(true, Utility.COL).ToArray();

            bool[] isIgnoreNumber = Enumerable.Repeat(false, 10).ToArray();

            var blockPoint = new Point();

            //9ループしても（＝cannotFillMaxExistNumsが埋まった）盤面に変化が無いなら終了
            while (loopCount > 0)
            {
                //処理前盤面記憶
                Utility.CopyToBufferMatrix(mat, bufferMatrix);

                //残り1マスなら埋める
                CellFill(mat);

                if (Utility.IsCompleteBoard(mat))
                    break;


                //「残り２つ」ルール処理
                for (int fillNum = 1; fillNum <= 9; fillNum++)
                    for (int indexOfRest4Cell = 0; indexOfRest4Cell < 4; indexOfRest4Cell++)
                        for (int r = 0; r < Utility.ROW; r++)
                            for (int c = 0; c < Utility.COL; c++)
                                if (mat[r][c] > 0)
                                    //埋める数字pが残り2つで入り得るマスが残り4マスだけの時の
                                    //その4マスのうちどれかが埋まっていたら（「残り2つ」ルールと呼ぶ）
                                    if ((rest4CellPointsForNums[fillNum][indexOfRest4Cell].X == r)
                                        && (rest4CellPointsForNums[fillNum][indexOfRest4Cell].Y == c))
                                        //該当数字を探索除外から復帰
                                        isIgnoreNumber[fillNum] = false;

                #region 探索数字決定

                //各数字の盤上に出ている個数と空白マスのカウント
                //nが各数字　countOfFilledNum[n]は数字nが出ている個数 n=0のときは空白マスのことになる
                int[] countOfFilledNum = new int[10];
                for (int i = 0; i < Utility.ROW; i++)
                    for (int j = 0; j < Utility.COL; j++)
                        countOfFilledNum[mat[i][j]]++;

                /*for(n=0;n<10;n++){
                    if(n == 0){printf("空白数:%d\n",numappear[n]);}
                    else if(n > 0){printf("%dの個数:%d\n",n,numappear[n]);}
                }*/

                //探索したがCRBEで埋められなかった数字の場合、個数消滅させて探索から一時除外
                for (int n = 1; n < 10; n++)
                    if (countOfFilledNum[n] == cannotFillMaxExistNums[n])
                        countOfFilledNum[n] = 0;

                var maxExistNumAndItsCount = new Dictionary<int, int>();
                maxExistNumAndItsCount.Add(1, countOfFilledNum[1]);
                var tempMaxExistNumCount = countOfFilledNum[1];

                //盤上に出ている数字の中で個数が最大の数字を求める(探索対象の数字決定)
                for (int n = 1; n < 9; n++)
                {
                    //探索候補の数字n+1の存在個数が暫定最大存在個数を上回っており、
                    //かつ暫定最大存在個数が9個でなく
                    //かつ探索候補の数字の存在個数が9個でなく、
                    //かつ前回探索した数字でなく、
                    //かつ「残り2つ」ルールで探索候補の数字が対象外になっていないならば
                    if ((tempMaxExistNumCount < countOfFilledNum[n + 1]) && (tempMaxExistNumCount != 9)
                        && (countOfFilledNum[n + 1] != 9) && ((n + 1) != prevSearchNum) && !isIgnoreNumber[n + 1])
                    {
                        maxExistNumAndItsCount.Clear();
                        //最大個数の数字とその個数を更新
                        maxExistNumAndItsCount.Add(n+1, countOfFilledNum[n+1]);
                        tempMaxExistNumCount = countOfFilledNum[n + 1];
                    }
                }

                //printf("一番多い数字は%dで%d個prev=%d\n",maxexistnum,maxnum,prevnum);
                prevSearchNum = maxExistNumAndItsCount.Keys.First();

                #endregion

                int maxExistNumCount = 0;
                bool isChangeBoard = true;
                //
                for (int i = 0; i < Utility.ROW; i++)
                    for (int j = 0; j < Utility.COL; j++)
                        isBlankPoint[i][j] = true;

                //スコアカウント
                DifficultScore++;

                #region CRBE
                //決定した探索数字でCRBE法を適用。9個埋まるまでor盤面が変化しなくなる＝埋まらなくなるまでループ
                while ((maxExistNumCount < 9) && isChangeBoard)
                {
                    //処理前盤面記憶
                    Utility.CopyToBufferMatrix(mat, bufferMatCrbe);

                    //マスが埋まってるか判定
                    for (int i = 0; i < Utility.ROW; i++)
                        for (int j = 0; j < Utility.COL; j++)
                            if (mat[i][j] > 0) isBlankPoint[i][j] = false;

                    //crbeoccprint(crbeoccupy);
                    //printf("最大数字:%d\n",maxexistnum);

                    //探索数字が入るマスを含む行列ブロックのマスを探索数字が入り得るマスの検索対象から除外
                    for (int r = 0; r < Utility.ROW; r++)
                    {
                        for (int c = 0; c < Utility.COL; c++)
                        {
                            //探索数字がひっかかったら
                            if (mat[r][c] == maxExistNumAndItsCount.Keys.First())
                            {
                                for (int j = 0; j < 9; j++) isBlankPoint[r][j] = false;//そこの行を入らない状態にする
                                for (int i = 0; i < 9; i++) isBlankPoint[i][c] = false;//そこの列を入らない状態にする
                                //そこのブロックを入らない状態にする
                                blockPoint = Utility.GetBlockPoint(r, c);
                                for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                                    for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                                        isBlankPoint[i][j] = false;
                            }
                        }
                    }

                    //探索数字が残り2個のときisBlankPoint探索で
                    //4つしか入り得るマスがないとき探索対象から一時除外（確かデッドロックになったため）
                    //（「残り2つ」ルールと呼ぶ）
                    int blankCellCount = 0;
                    if (maxExistNumAndItsCount.Values.First() == 7)
                    {
                        for (int r = 0; r < Utility.ROW; r++)
                        {
                            for (int c = 0; c < Utility.COL; c++)
                            {
                                if (isBlankPoint[r][c])
                                {
                                    //4マスを記憶
                                    if (blankCellCount < 4)
                                        rest4CellPointsForNums[maxExistNumAndItsCount.Keys.First()][blankCellCount] = new Point(r, c);
                                    
                                    blankCellCount++;
                                }
                            }
                        }
                        //探索数字を対象から除外
                        if (blankCellCount == 4)
                            isIgnoreNumber[maxExistNumAndItsCount.Keys.First()] = true;
                    }

                    //探索数字が入り得るマスの座標群
                    var inputablePoints = Enumerable.Repeat(new Point(0,0), Utility.MAX).ToArray();

                    //探索数字が入り得るマスを全て記憶
                    int memIndex = 0;
                    for (int r = 0; r < Utility.ROW; r++)
                        for (int c = 0; c < Utility.COL; c++)
                            if (isBlankPoint[r][c])
                                inputablePoints[memIndex++] = new Point(r, c);

                    //記憶したマスが行列ブロック内でそこにしか入らなければそこに入れる
                    for (int index = 0; index < inputablePoints.Count(); index++)
                    {
                        blankCellCount = 0;
                        //行

                        //現在の行の空白マスをカウントし、1つしか空白マスがない場合CRBE法実行
                        for (int j = 0; j < Utility.COL; j++)
                            if (isBlankPoint[inputablePoints[index].X][j])
                                blankCellCount++;

                        //入り得るのが1マスだけでそのマスに入る可能性を残している場合
                        if (blankCellCount == 1 && isBlankPoint[inputablePoints[index].X][inputablePoints[index].Y])
                        {
                            //そこに検索対象の数字を入れる
                            mat[inputablePoints[index].X][inputablePoints[index].Y] = maxExistNumAndItsCount.Keys.First();

                            Utility.ExcludeNumberInRowColBlock(isBlankPoint, inputablePoints[index]);
                        }
                        //列
                        blankCellCount = 0;
                        //現在の列の空白マスをカウントし、1つしか空白マスがない場合CRBE法実行
                        for (int i = 0; i < Utility.COL; i++)
                            if (isBlankPoint[i][inputablePoints[index].Y])
                                blankCellCount++;

                        //入り得るのが1マスだけでそのマスに入る可能性を残している場合
                        if (blankCellCount == 1 && isBlankPoint[inputablePoints[index].X][inputablePoints[index].Y])
                        {
                            //そこに検索対象の数字を入れる
                            mat[inputablePoints[index].X][inputablePoints[index].Y] = maxExistNumAndItsCount.Keys.First();

                            Utility.ExcludeNumberInRowColBlock(isBlankPoint, inputablePoints[index]);
                        }

                        //ブロック
                        blankCellCount = 0;
                        blockPoint = Utility.GetBlockPoint(inputablePoints[index].X, inputablePoints[index].Y);
                        //現在のブロックの空白マスをカウントし、1つしか空白マスがない場合CRBE法実行
                        for (int i = blockPoint.X; i <= blockPoint.X + 2; i++)
                            for (int j = blockPoint.Y; j <= blockPoint.Y + 2; j++)
                                if (isBlankPoint[i][j]) blankCellCount++;

                        //入り得るのが1マスだけでそのマスに入る可能性を残している場合
                        if (blankCellCount == 1 && (isBlankPoint[inputablePoints[index].X][inputablePoints[index].Y]))
                        {
                            //そこに検索対象の数字を入れる
                            mat[inputablePoints[index].X][inputablePoints[index].Y] = maxExistNumAndItsCount.Keys.First();

                            Utility.ExcludeNumberInRowColBlock(isBlankPoint, inputablePoints[index]);
                        }
                    }


                    //盤上に探索数字がいくつ出ているか　9個なら探索終了
                    maxExistNumCount = 0;
                    for (int i = 0; i < Utility.ROW; i++)
                        for (int j = 0; j < Utility.COL; j++)
                            if (mat[i][j] == maxExistNumAndItsCount.Keys.First()) maxExistNumCount++;

                    //処理後盤面比較　無変化なら探索終了
                    isChangeBoard = Utility.IsChangeBoard(mat, bufferMatCrbe);
                    //printf("max=%d,not=%d",maxnumcount,notboardmatch);
                }
                #endregion

                //盤面変化時ループカウンタと個数最大数字保存バッファをリセット 
                if (Utility.IsChangeBoard(mat, bufferMatrix))
                {
                    loopCount = 10;
                    cannotFillMaxExistNums = Enumerable.Repeat(0, 10).ToArray();
                }
                //無変化時、次に盤面が変化するまで個数最大の数字を一時記憶
                else
                    cannotFillMaxExistNums[maxExistNumAndItsCount.Keys.First()] = maxExistNumAndItsCount.Values.First();

                loopCount--;

                //完成時強制終了
                if (Utility.IsCompleteBoard(mat))
                    break;
            }
        }

        /// <summary>
        /// 行・列・ブロックのいずれかで1マスしか埋める場所がない時にそのマスを埋める
        /// </summary>
        /// <param name="mat">盤面データ</param>
        public void CellFill(int[][] mat)
        {
            var blankPoint = new Point();

            bool[] usedNumCheckRow = Enumerable.Repeat(false, 10).ToArray();//要素の数字が使用済みか判定用
            int[] rowBlankCheck = Enumerable.Repeat(0, 10).ToArray();//現在の行の空白マス数判定用

            bool[] usedNumCheckCol = Enumerable.Repeat(false, 10).ToArray();
            int[] colBlankCheck = Enumerable.Repeat(0, 10).ToArray();

            bool[] usedNumCheckBlock = Enumerable.Repeat(false, 10).ToArray();
            int[] blockBlankCheck = Enumerable.Repeat(0, 10).ToArray();
            //行で残り1マスなら埋める
            for (int r = 0; r < Utility.ROW; r++)
            {
                for (int c = 0; c < Utility.COL; c++)
                {
                    //各行の空白マス数チェック
                    if (mat[r][c] == 0)
                    {
                        rowBlankCheck[r]++;
                        //空白マス記憶
                        blankPoint.X = r;
                        blankPoint.Y = c;
                    }
                    int usedNumber = mat[r][c];
                    usedNumCheckRow[usedNumber] = true;//行内既出数字チェック
                    //最終列
                    if (c == Utility.COL - 1)
                    {
                        //printf("%d行の空白マス数:%d\n",r,rspacecheck[r]);
                        for (int fillNum = 1; fillNum <= 9; fillNum++)
                        {
                            if (usedNumCheckRow[fillNum])
                            {
                                //printf("既出数字:%d,",m);
                            }
                            //if(m == 9){printf("\n");}

                            if ((rowBlankCheck[r] == 1) && (!usedNumCheckRow[fillNum]))
                            {
                                //この行の空白マス数が1つしかなく、fillNum(=1～9)が未使用なら
                                //記憶した空白マスに未使用の数字fillNumを入れる
                                mat[blankPoint.X][blankPoint.Y] = fillNum;
                            }

                            usedNumCheckRow[fillNum] = false;
                        }
                    }
                }
            }

            //列で残り1マスなら埋める
            for (int c = 0; c < Utility.COL; c++)
            {
                for (int r = 0; r < Utility.ROW; r++)
                {
                    //各列の空白マス数チェック
                    if (mat[r][c] == 0)
                    {
                        colBlankCheck[c]++;
                        //空白マス記憶
                        blankPoint.X = r;
                        blankPoint.Y = c;
                    }
                    int usedNumber = mat[r][c];
                    usedNumCheckCol[usedNumber] = true;//行内既出数字チェック
                    //最終行
                    if (r == Utility.ROW - 1)
                    {
                        //printf("%d列の空白マス数:%d\n",c,cspacecheck[c]);
                        for (int fillNum = 1; fillNum <= 9; fillNum++)
                        {
                            if (usedNumCheckCol[fillNum])
                            {
                                //printf("既出数字:%d,",m);
                            }
                            //if(m == 9){printf("\n");}

                            if ((colBlankCheck[c] == 1) && (!usedNumCheckCol[fillNum]))
                            {
                                //この列の空白マス数が1で、m(=1～9)が未使用なら
                                //記憶した空白マスに未使用の数字を入れる
                                mat[blankPoint.X][blankPoint.Y] = fillNum;
                            }

                            usedNumCheckCol[fillNum] = false;
                        }
                    }
                }
            }

            int blockNum = 0;
            //ブロックで残り1マスなら埋める
            for (int blockRow = 0; blockRow < 9; blockRow = blockRow + 3)
            {
                for (int blockCol = 0; blockCol < 9; blockCol = blockCol + 3)
                {
                    //現ブロックの3x3マスだけ走査
                    for (int r = blockRow; r <= blockRow + 2; r++)
                    {
                        for (int c = blockCol; c <= blockCol + 2; c++)
                        {
                            //各マスの空白マス数チェック
                            if (mat[r][c] == 0)
                            {
                                blockBlankCheck[blockNum]++;
                                //空白マス記憶
                                blankPoint.X = r;
                                blankPoint.Y = c;
                            }
                            int usedNumber = mat[r][c];
                            usedNumCheckBlock[usedNumber] = true;//行内既出数字チェック
                            //最終ブロック
                            if ((r == blockRow + 2) && (c == blockCol + 2))
                            {
                                for (int fillNum = 1; fillNum <= 9; fillNum++)
                                {
                                    if (usedNumCheckBlock[fillNum])
                                    {
                                        //printf("既出数字:%d,",m);
                                    }
                                    //if(m == 9){printf("\n");}

                                    if ((blockBlankCheck[blockNum] == 1) && (!usedNumCheckBlock[fillNum]))
                                    {
                                        //この列の空白マス数が1で、m(=1～9)が未使用なら
                                        mat[blankPoint.X][blankPoint.Y] = fillNum;//記憶した空白マスに未使用の数字を入れる
                                    }

                                    usedNumCheckBlock[fillNum] = false;
                                }
                            }
                        }
                    }
                    blockNum++;//ブロック番号(ストラックアウト順)カウントアップ
                }
            }
        }


        
        /// <summary>
        /// 同行同列同ブロックで入れようとしている数字がダブっておらず入れられるか
        /// </summary>
        /// <param name="board"></param>
        /// <param name="pos">現在のマス番号</param>
        /// <param name="inputNum">入力予定の数字</param>
        /// <returns></returns>
        bool CanInput(int[] board, int pos, int inputNum)
        {
            int N = 9, B = 3;
            int row = pos / N;
            int col = pos % N;
            //行と列に入力数字が出ているか
            for (int i = 0; i < N; ++i)
            {
                if (board[row * N + i] == inputNum) return false;
                if (board[col + i * N] == inputNum) return false;
            }
            //ブロックの左上の位置を取得
            int topLeft = N * (row / B) * B + (col / B) * B;
            //ブロック内に入力数字が出ているか
            for (int i = 0; i < B; ++i)
                for (int j = 0; j < B; ++j)
                    if (board[topLeft + i * N + j] == inputNum)
                        return false;

            return true;
        }

        /// <summary>
        /// 総当たり回答　http://newtgecko.blog.fc2.com/blog-entry-174.html
        /// </summary>
        /// <param name="board">入力盤面</param>
        /// <param name="pos">一行での現在マス番号</param>
        /// <param name="retBoard">回答盤面</param>
        public void BruteForce(int[] board, int pos, int[][] retBoard)
        {
            //探索が終わらないなら強制終了　完成時も終了
            if (DifficultScore > 10000000 || Utility.IsCompleteBoard(retBoard))
                return;

            int emptyPos = 0;
            //現在位置から最も近い空白マスの位置を取得
            emptyPos = 
                board.Select((p, i) => new { Content = p, Index = i })//匿名クラスのリストに要素とそのインデックスを射影
                .Where(p => p.Content == 0 && p.Index >= pos)//上記リストをフィルタリング
                .Select(p => p.Index)//匿名クラスからインデックスのリストを取得
                .FirstOrDefault();
            //1マス目以外で空白マス位置が0になるようなら
            //Firstが見つからずDefaultが返ってきたということなので全部埋めたことになる
            //そのときは終了
            if (pos>0 && emptyPos==default(int))
            {
                int xynum = 0;
                for (int k = 0; k < Utility.ROW; k++)
                    for (int j = 0; j < Utility.COL; j++)
                        retBoard[k][j] = board[xynum++];

                //Utility.BoardPrint(retBoard);
                return;
            }
            //1から9のなかで現在のマスに入れられる数字があったなら埋めて次のマスへ
            for (int n = 1; n <= 9; n++)
            {
                DifficultScore++;
                if (CanInput(board, emptyPos, n))
                {
                    board[emptyPos] = n;
                    BruteForce(board, emptyPos + 1, retBoard);
                    //ここにきた時点で入れた数字が間違っていたことになるので
                    //それをなかったことにする
                    board[emptyPos] = 0;//backtracking
                }
            }
        }
    }
}

