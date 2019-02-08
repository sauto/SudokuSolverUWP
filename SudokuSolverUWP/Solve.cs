using System.Collections.Generic;

namespace SudokuSolverUWP
{
    public class SolveMain
    {
        public int DScore { get { return dscore; } }
        int dscore = 0;

        public string Log { get { return log; } set { log = value; } }
        string log = "";

        public List<string> Solve(string data,bool isUseBacktrack,bool isDisplayUsedLogic)
        {
            int[] dataList = new int[Utility.ROW * Utility.COL];
            
            int t = 0;
            foreach (char c in data)
            {
                var n = c - '0';
                if (n >= 0 && n <= 9 && t < Utility.ROW * Utility.COL)
                    dataList[t++] = n;
            }

            //2次元行列に変換
            int xynum = 0;
            var boardMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            for (int i = 0; i < Utility.ROW; i++)
                for (int j = 0; j < Utility.COL; j++)
                    boardMatrix[i][j] = dataList[xynum++];

            //Utility.BoardPrint(boardMatrix);

            Solver solver = new Solver(isDisplayUsedLogic);

            bool wholeLoopFlag = true;
            //CRBE法
            solver.Crbe(boardMatrix);
            if (Utility.IsCompleteBoard(boardMatrix))
                wholeLoopFlag = false;

            //候補数字書き込み
            var candidateMat = solver.WriteCandidate(boardMatrix);
            //Utility.CandidateOutput(candidateMat);

            //初回単一法
            solver.DoSingleLogics(boardMatrix, candidateMat);
            if (Utility.IsCompleteBoard(boardMatrix))
                wholeLoopFlag = false;

            if(Utility.Mistake(boardMatrix,"初回単一まで"))
            {
                List<string> l = new List<string>();
                foreach (int i in dataList)
                    l.Add(i == 0 ? string.Empty : i.ToString());
                log = "ありえない盤面です";
                return l;
            }


            var bufMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            Utility.CopyToBufferMatrix(boardMatrix, bufMatrix);

            //ロジック登録
            var logics = new LogicDelegate() { CombinationNo = 8 }.GetLogicCombination(solver);
            
            var bufferMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            //処理前盤面記憶
            Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);
            bool loopFlag = true;

            while (wholeLoopFlag)
            {
                //定石Aのみ盤面変化ならループ
                while (loopFlag)
                {
                    //Utility.Mistake(boardMatrix,"共有");

                    //処理前盤面記憶
                    Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                    //●●●●定石A●●●●//
                    logics[0](boardMatrix, candidateMat);

                    loopFlag = Utility.IsChangeBoard(candidateMat, bufferMatrix);

                    //完成時強制終了
                    if (Utility.IsCompleteBoard(boardMatrix))
                        goto endLabel;

                    //if(loopFlag) Utility.CandidateOutput(candidateMat);
                }
                loopFlag = true;
                //処理前盤面記憶
                Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                //●●●●定石B●●●●//
                logics[1](boardMatrix, candidateMat);
                
                //完成時強制終了
                if (Utility.IsCompleteBoard(boardMatrix))
                    break;

                //盤面変化したら最初に戻って定石Aから
                if (Utility.IsChangeBoard(candidateMat, bufferMatrix))
                {
                    //Utility.CandidateOutput(candidateMat);
                    continue;
                }
                //処理前盤面記憶
                Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                //●●●●定石C●●●●//
                logics[2](boardMatrix, candidateMat);
                
                //完成時強制終了
                if (Utility.IsCompleteBoard(boardMatrix))
                    break;

                //盤面変化したら最初に戻って定石Aから
                if (Utility.IsChangeBoard(candidateMat, bufferMatrix))
                {
                    //Utility.Mistake(boardMatrix,"対角");
                    continue;
                }

                //処理前盤面記憶
                Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                //●●●●定石D●●●●//
                logics[3](boardMatrix, candidateMat);
                
                //完成時強制終了
                if (Utility.IsCompleteBoard(boardMatrix))
                    break;

                //盤面変化したら最初に戻って定石Aから
                if (Utility.IsChangeBoard(candidateMat, bufferMatrix))
                {
                    //Utility.Mistake(boardMatrix,"三つ子");
                    continue;
                }

                //どれも適用できなかったら処理終了
                break;
            }
            endLabel:;

            //Utility.CandidateOutput(candidateMat);
            //Utility.BoardPrint(boardMatrix);

            string useBrute = "";
            if (Utility.IsCompleteBoard(boardMatrix))
            {
                xynum = 0;
                for (int i = 0; i < Utility.ROW; i++)
                    for (int j = 0; j < Utility.COL; j++)
                        dataList[xynum++] = boardMatrix[i][j];
            }
            else
            {
                //Console.WriteLine("未完成・・・");

                if (isUseBacktrack)
                {
                    //処理後の盤面に総当たり　袋小路に入ってると総当たりでも完成できない
                    //xynum = 0;
                    //for (int i = 0; i < Utility.ROW; i++)
                    //    for (int j = 0; j < Utility.COL; j++)
                    //        dataList[xynum++] = boardMatrix[i][j];

                    //CRBE単一使用後に総当たり　確実　CRBE単一で誤入力はしない
                    xynum = 0;
                    for (int i = 0; i < Utility.ROW; i++)
                        for (int j = 0; j < Utility.COL; j++)
                            dataList[xynum++] = bufMatrix[i][j];


                    solver.BruteForce(dataList, 0, boardMatrix);
                    useBrute = solver.DifficultScore <= 10000000 ? "\n総当たり法使用" : "\n解けません";

                    //Utility.BoardPrint(boardMatrix);

                    //if (!Utility.IsCompleteBoard(boardMatrix))
                        //Console.WriteLine("失敗・・・");
                }
                xynum = 0;
                for (int i = 0; i < Utility.ROW; i++)
                    for (int j = 0; j < Utility.COL; j++)
                        dataList[xynum++] = boardMatrix[i][j];
            }

            string result = Utility.IsCompleteBoard(boardMatrix) ? "完成" : "未完成";

            log = result + solver.Log + useBrute;

            dscore = solver.DifficultScore;

            List<string> strData = new List<string>();
            foreach(int i in dataList)
                strData.Add(i == 0 ? string.Empty : i.ToString());
            

            return strData;
        }

    }
}
