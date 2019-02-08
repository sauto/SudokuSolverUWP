using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Animation;

namespace SudokuSolverUWP
{
    class ViewModel : INotifyPropertyChanged
    {
        string numData = "";
        /// <summary>
        /// 入力数列
        /// </summary>
        public string NumData
        {
            get { return numData; }
            set
            {
                numData = value;
                //プロパティにセットすると更新通知が発火
                OnPropertyChanged("NumData");
                //入力時点で[解く]ボタンのEnable判定を発火
                ((DelegateCommand)SolveCommand).RaiseCanExecuteChanged();
                
                if (SolveCommand.CanExecute(null))
                {
                    List<string> li = new List<string>();
                    for (int i = 0; i < 81; i++)
                        li.Add(numData[i] == '0' ? "" : numData[i].ToString());
                    
                    DataList = li;
                }
            }
        }


        IEnumerable<int> noList = Enumerable.Range(1, 24);
        /// <summary>
        /// ロジック番号
        /// </summary>
        public IEnumerable<int> NoList { get { return noList; } set { } }

        List<string> dataList = Enumerable.Repeat("", 81).ToList();

        public List<string> DataList
        {
            get { return dataList; }
            set
            {
                dataList = value;
                OnPropertyChanged("DataList");
            }
        }

        int dscore = 0;
        /// <summary>
        /// 難易度スコア
        /// </summary>
        public int DScore
        {
            get { return dscore; }
            set
            {
                dscore = value;
                OnPropertyChanged("DScore");
            }
        }
        /// <summary>
        /// ロジック使用ログ
        /// </summary>
        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                OnPropertyChanged("Log");
            }
        }
        string log = "";

        /// <summary>
        /// バックトラック法を使うかどうか
        /// </summary>
        public bool IsUseBacktrack
        {
            get { return isUseBacktrack; }
            set
            {
                isUseBacktrack = value;
                OnPropertyChanged("IsUseBacktrack");
            }
        }
        bool isUseBacktrack = false;

        /// <summary>
        /// 使用ロジックを表示かどうか
        /// </summary>
        public bool IsDisplayUsedLogic
        {
            get { return isDisplayUsedLogic; }
            set
            {
                isDisplayUsedLogic = value;
                OnPropertyChanged("IsDisplayUsedLogic");
            }
        }
        bool isDisplayUsedLogic = false;

        private void SolveCommandExecute(object parameter)
        {
            var sm = new SolveMain();
            DataList = sm.Solve(NumData,isUseBacktrack,isDisplayUsedLogic);
            DScore = sm.DScore;
            Log = sm.Log;
            
            if(!DataList.Contains(string.Empty))
                ((Storyboard)parameter).Begin();
        }

        private bool SolveCommandCanExecute(object parameter)
        {
            string str = "";

            bool isCorrect = true;
            if (NumData.Length == 81 && NumData.ToList().TrueForAll(c => '0' <= c && c <= '9'))
            {
                int xynum = 0;
                var boardMatrix = Utility.GetInit2DimArray<int>(Utility.ROW, Utility.COL);
                for (int i = 0; i < Utility.ROW; i++)
                    for (int j = 0; j < Utility.COL; j++)
                        boardMatrix[i][j] = NumData[xynum++] - '0';

                isCorrect = !Utility.Mistake(boardMatrix, "入力盤面");
                str += isCorrect ? "" : "ありえない盤面です\n";
            }
            else
                str += NumData.Length != 0 ? "入力に過不足があります\n" : "";

            str += NumData.ToList().TrueForAll(c => '0' <= c && c <= '9') ? "" : "入力は半角数字のみです\n";
            Log = str;

            return isCorrect && NumData.Length == 81 && NumData.ToList().TrueForAll(c => '0' <= c && c <= '9');
        }

        private ICommand _solveCommand;
        public ICommand SolveCommand
        {
            get
            {
                if (_solveCommand == null)
                    _solveCommand = new DelegateCommand
                    {
                        ExecuteHandler = SolveCommandExecute,
                        CanExecuteHandler = SolveCommandCanExecute,
                    };
                return _solveCommand;
            }
        }

        private void ResetCommandExecute(object parameter)
        {

            NumData = "";
            DataList.Clear();
            DataList = Enumerable.Repeat("", 81).ToList();
            OnPropertyChanged("DataList");
            DScore = 0;
            Log = "";
            
        }

        private bool ResetCommandCanExecute(object parameter)
        {
            return true;
        }

        private ICommand _resetCommand;
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new DelegateCommand
                    {
                        ExecuteHandler = ResetCommandExecute,
                        CanExecuteHandler = ResetCommandCanExecute,
                    };
                return _resetCommand;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
            =>this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }
}
