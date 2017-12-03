using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace SudokuSolver
{
    class MainProgram
    {
        static readonly string filePath = @"C:\Users\yuuta\Desktop\プログラミング\ソフト\卒研プログラムと資料\smp1\spa.txt";
        const int NUM_PROBLEM = 1;
        
        static void Main(string[] args)
        {
            int[] dataList = new int[Utility.ROW * Utility.COL * NUM_PROBLEM];

            using (var sr = new StreamReader(filePath))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    var n = sr.Read() - '0';
                    if (n >= 0 && n <= 9 && i < Utility.ROW * Utility.COL * NUM_PROBLEM)
                        dataList[i++] = n;
                }
            }
            //dataList.ToList().ForEach(s => Console.Write(s));

            
            



            //2次元行列に変換
            int xynum = 0;
            var boardMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            for (int i = 0; i < Utility.ROW; i++)
                for (int j = 0; j < Utility.COL; j++)
                    boardMatrix[i][j] = dataList[xynum++];
            //if (dataList.Count() - xynum < 81)
            //{
            //    Console.WriteLine("データが足りません");
            //    Console.ReadLine();
            //    return;
            //}

            Utility.BoardPrint(boardMatrix);

            Solver solver = new Solver();

            bool wholeloopflag = true;
            //CRBE法
            solver.Crbe(boardMatrix);
            if (Utility.IsCompleteBoard(boardMatrix))
            {
                wholeloopflag = false;
                Console.WriteLine("完成！");
            }
            //候補数字書き込み
            var candidateMat = solver.WriteCandidate(boardMatrix);
            Utility.CandidateOutput(candidateMat);
            Console.WriteLine("実行");

            //初回単一法
            solver.DoSingleLogics(boardMatrix, candidateMat);
            if (Utility.IsCompleteBoard(boardMatrix))
            {
                wholeloopflag = false;
                Console.WriteLine("完成！");
            }

            var bufMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            Utility.CopyToBufferMatrix(boardMatrix, bufMatrix);

            //ロジック登録
            var logics = new LogicDelegate() { CombinationNo = 8 }.GetLogicCombination(solver);
            
            var bufferMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
            //処理前盤面記憶
            Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);
            bool loopFlag = true;

            while (wholeloopflag)
            {
                //定石Aのみ盤面変化ならループ
                while (loopFlag)
                {
                    //処理前盤面記憶
                    Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                    //●●●●定石A●●●●//
                    logics[0](boardMatrix, candidateMat);

                    loopFlag = Utility.IsChangeBoard(candidateMat, bufferMatrix);

                    //完成時強制終了
                    if (Utility.IsCompleteBoard(boardMatrix))
                        goto endLabel;

                    //if (loopFlag) Utility.CandidateOutput(candidateMat);
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
                    continue;

                //処理前盤面記憶
                Utility.CopyToBufferMatrix(candidateMat, bufferMatrix);

                //●●●●定石D●●●●//
                logics[3](boardMatrix, candidateMat);

                //完成時強制終了
                if (Utility.IsCompleteBoard(boardMatrix))
                    break;

                //盤面変化したら最初に戻って定石Aから
                if (Utility.IsChangeBoard(candidateMat, bufferMatrix))
                    continue;

                //どれも適用できなかったら処理終了
                break;
            }
            endLabel:;

            Utility.CandidateOutput(candidateMat);


            Console.WriteLine();
            Utility.BoardPrint(boardMatrix);

            if (Utility.IsCompleteBoard(boardMatrix))
                Console.WriteLine("完成！");
            else
            {
                Console.WriteLine("未完成・・・");


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

                Utility.BoardPrint(boardMatrix);

                if(!Utility.IsCompleteBoard(boardMatrix))
                    Console.WriteLine("失敗・・・");

            }
            Console.WriteLine("スコア："+solver.DifficultScore);

            Console.ReadLine();

        }


        

    }
}
