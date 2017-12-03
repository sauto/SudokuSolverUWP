namespace SudokuSolverUWP
{
    class LogicDelegate
    {
        /// <summary>
        /// ロジック組み合わせ番号
        /// </summary>
        public int CombinationNo { set { combinationNo = value; } }
        int combinationNo = 0;
        
        /// <summary>
        /// ロジックの組み合わせ
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="candMat"></param>
        public delegate void LogicDelegates(int[][] mat, int[][] candMat);

        /// <summary>
        /// 24通りから任意のロジック組み合わせを取得
        /// </summary>
        /// <param name="solver"></param>
        /// <returns></returns>
        public LogicDelegates[] GetLogicCombination(Solver solver)
        {
            LogicDelegates[] logics = new LogicDelegates[4];
            logics[0] = solver.ShareCandLogic;
            logics[1] = solver.PairLogic;
            logics[2] = solver.CrossLogic;
            logics[3] = solver.TripleLogic;

            switch (combinationNo)
            {
                case 1:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 2:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 3:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 4:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 5:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 6:
                    logics[0] = solver.PairLogic;//双子
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 7:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 8:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 9:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 10:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.PairLogic;//双子
                    break;
                case 11:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 12:
                    logics[0] = solver.ShareCandLogic;//共有
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.PairLogic;//双子
                    break;
                case 13:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 14:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 15:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.CrossLogic;//対角
                    break;
                case 16:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.CrossLogic;//対角
                    logics[3] = solver.PairLogic;//双子
                    break;
                case 17:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 18:
                    logics[0] = solver.TripleLogic;//三つ子
                    logics[1] = solver.CrossLogic;//対角
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.PairLogic;//双子
                    break;
                case 19:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 20:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.PairLogic;//双子
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 21:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.TripleLogic;//三つ子
                    break;
                case 22:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.ShareCandLogic;//共有
                    logics[2] = solver.TripleLogic;//三つ子
                    logics[3] = solver.PairLogic;//双子
                    break;
                case 23:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.PairLogic;//双子
                    logics[3] = solver.ShareCandLogic;//共有
                    break;
                case 24:
                    logics[0] = solver.CrossLogic;//対角
                    logics[1] = solver.TripleLogic;//三つ子
                    logics[2] = solver.ShareCandLogic;//共有
                    logics[3] = solver.PairLogic;//双子
                    break;
            }

            return logics;
        }
    }
}
