using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Lottery
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 定义存储的变量
        List<List<int>> gotPrized;                          // 已获奖人
        List<int> allSignedNum;                             // 阳光普照奖候选人
        List<string> allSignedName;                         // 阳光普照奖候选人名字
        List<int> allSignedStudents;                        // 特一二三等奖候选人
        List<string> allSignedStudentsName;                 // 大奖候选人名字
        HashSet<int> alreadyGetPrize;                       // 已经获取了奖的
       

        int state = 0;                                      // 目前状态
        int speed = 100;                                    // 显示帧率
        private readonly Stopwatch _sw = new Stopwatch();   // 用于计时
        public List<string[]> AllData;                      // 所有数据
        public string recordFileName;                       // 记录文件名
        public string configFileName;                       // 配置文件名
        public string readFileName;                         // 读取文件名
        public static System.DateTime startTime;            // 系统启动时间
        public string recordFolderPath;                    // 存储文件目录
        public string readFolderPath;                       // 读取文件目录
        public string picFolderPath;                        // 图片存储目录

        public string[] picFiles;                           // 图片文件地址
        //System.IO.StreamWriter sw;                          // 写入的文件流
        //System.IO.StreamReader sr;                          // 读取的文件流

        Random rd = new Random();                           // 随机数
        bool stophere = false;                              // 抽奖停止

        private double offset_x = 0;
        private double offset_y = 50;
        public MainWindow()
        {
            InitializeComponent();
            // 更新窗体大小
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            Console.WriteLine("this.ActualWidth=" + this.ActualWidth);
            Console.WriteLine("this.ActualHeight=" + this.ActualHeight);

            picFolderPath = "C:\\BBL\\年会相片处理代码";
            readFolderPath = "C:\\BBL\\readcsv";
            readFileName = "年会参与名单.csv";
            recordFolderPath = "C:\\BBL\\recordcsv";
            configFileName = "config.txt";
            //InitPictures("C:\\BBL\\年会相片处理代码");

            gotPrized = new List<List<int>>();
            alreadyGetPrize = new HashSet<int>();


            readConfig();

            InitAndPreProcessData();

            // 初始化布局
            InitLayout(offset_x, offset_y);

            // 重置界面到最开始阶段
            ResetLayoutBnt.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }
        public void readConfig()
        {
            String state_str = "";
            String csv_name = "";
            try
            {
                FileStream fs = new FileStream(readFolderPath+"\\"+ configFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(fs);
                state_str = reader.ReadLine();
                csv_name = reader.ReadLine();
                reader.Close();
                fs.Close();
            }
            catch (Exception)
            {
                state_str = "";
                csv_name = "";
            }
            if (state_str != null && state_str!="")
            {
                int state_num = int.Parse(state_str);
                if (state_num < 9)
                {
                    recordFileName = csv_name;
                    state = state_num;
                    FileStream fs = new FileStream(recordFolderPath + "\\" + recordFileName , System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    System.IO.StreamReader sr = new StreamReader(fs, UnicodeEncoding.GetEncoding("GB2312"));
                    //记录每次读取的一行记录
                    string strLine = "";
                   
                    bool is_first=true;
                    int flag_num=-1;
                    string[] aryLine = null;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        aryLine = strLine.Split(',');
                        if (is_first)
                        {
                            is_first = false;
                            flag_num = int.Parse(aryLine[3]);
                            List<int> temp_list = new List<int>();
                            temp_list.Add(int.Parse(aryLine[0]));
                            gotPrized.Add(temp_list);
                        }
                        else
                        {
                            int temp = int.Parse(aryLine[3]);
                            if (temp != flag_num)
                            {
                                gotPrized[gotPrized.Count-1].Add(flag_num);
                                List<int> temp_list = new List<int>();
                                temp_list.Add(int.Parse(aryLine[0]));
                                gotPrized.Add(temp_list);
                                flag_num = temp;
                            }
                            else
                            {
                                gotPrized[gotPrized.Count - 1].Add(flag_num);
                            }
                        }
                    }
                    gotPrized[gotPrized.Count - 1].Add(flag_num);
                    sr.Close();
                    fs.Close();
                }
                else InitRecordFile();
            }
            else InitRecordFile();   
        }

        /// <summary>
        /// 初始化布局
        /// </summary>
        public void InitLayout(double offset_x, double offset_y)
        {
            HideAllImg();
            clearAllImgText();

            // 更新特等奖的位置
            Grid_Special.Width = this.Width * 500 / 1920;
            Grid_Special.Height = this.Height * 450 / 1080;
            Canvas.SetLeft(Grid_Special, (this.Width - Grid_Special.Width)/2);
            Canvas.SetTop(Grid_Special, (this.Height - Grid_Special.Height) / 2 + offset_y);

            // 更新一等奖的位置
            Grid_First.Width = this.Width * 900 / 1920;
            Grid_First.Height = this.Height * 400 / 1080;
            Canvas.SetLeft(Grid_First, (this.Width - Grid_First.Width) / 2);
            Canvas.SetTop(Grid_First, (this.Height - Grid_First.Height) / 2 + offset_y);

            // 更新二等奖的位置
            Grid_Second.Width = this.Width * 1400 / 1920;
            Grid_Second.Height = this.Height * 350 / 1080;
            Canvas.SetLeft(Grid_Second, (this.Width - Grid_Second.Width) / 2);
            Canvas.SetTop(Grid_Second, (this.Height - Grid_Second.Height) / 2 + offset_y);

            // 更新三等奖的位置
            Grid_Third.Width = this.Width * 1600 / 1920;
            Grid_Third.Height = this.Height * 500 / 1080;
            Canvas.SetLeft(Grid_Third, (this.Width - Grid_Third.Width) / 2);
            Canvas.SetTop(Grid_Third, (this.Height - Grid_Third.Height) / 2 + offset_y);

            // 更新阳光普照奖的位置
            Grid_Sunshine.Width = this.Width * 1600 / 1920;
            Grid_Sunshine.Height = this.Height * 400 / 1080;
            Canvas.SetLeft(Grid_Sunshine, (this.Width - Grid_Sunshine.Width) / 2);
            Canvas.SetTop(Grid_Sunshine, (this.Height - Grid_Sunshine.Height) / 2 + offset_y);

            // 更新用于点开始抽奖的人的位置
            Grid_Executor.Width = this.Width * 500 / 1920;
            Grid_Executor.Height = this.Height * 450 / 1080;
            Canvas.SetLeft(Grid_Executor, (this.Width - Grid_Executor.Width) / 2);
            Canvas.SetTop(Grid_Executor, (this.Height - Grid_Executor.Height) / 2 + offset_y);

            // 更新抽奖按钮的位置
            Grid_SysBnt.Width = this.Width * 800 / 1920;
            Canvas.SetLeft(Grid_SysBnt, (this.Width - Grid_SysBnt.Width) / 2);
            Canvas.SetTop(Grid_SysBnt, this.Height - Grid_SysBnt.Height - 100 + offset_y);
            WithdrawBnt.Height = Grid_SysBnt.Height/2;
            Canvas.SetLeft(WithdrawBnt, this.Width - WithdrawBnt.Width);
            Canvas.SetTop(WithdrawBnt, Canvas.GetTop(Grid_SysBnt) + WithdrawBnt.Height/2);

            // 更新抽奖状态信息的位置
            Grid_Status.Width = this.Width;
            Canvas.SetLeft(Grid_Status, (this.Width - Grid_Status.Width) / 2);
            Canvas.SetTop(Grid_Status, Canvas.GetTop(Grid_SysBnt) - Grid_Status.Height*1.2 + offset_y);

            // 更新抽奖啦GIF的位置
            Gif_lottery.Width = this.Width * 500 / 1920;
            Gif_lottery.Height = this.Height * 300 / 1080;
            Canvas.SetLeft(Gif_lottery, (this.Width - Gif_lottery.Width) / 2);
            Canvas.SetTop(Gif_lottery, (this.Height - Gif_lottery.Height) / 2 + offset_y);
        }
        private void InitLayoutBnt_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("初始化布局");
            this.Dispatcher.Invoke(new Action((() =>
            {
                InitLayout(offset_x, offset_y);
            })));
        }
        public void HideAllImg()
        {
            Grid_Special.Visibility = Visibility.Hidden;
            Grid_First.Visibility = Visibility.Hidden;
            Grid_Second.Visibility = Visibility.Hidden;
            Grid_Third.Visibility = Visibility.Hidden;
            Grid_Sunshine.Visibility = Visibility.Hidden;
            Grid_Executor.Visibility = Visibility.Hidden;
            Gif_lottery.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 初始化人员图片目录
        /// </summary>
        //public void InitPictures(String PicFolder)
        //{
        //    picFolderPath = PicFolder;
        //// picFiles = Directory.GetFiles(picFolderPath+"\\processed", "*.jp*");
        
        //    var path = GetBitImage(picFolderPath+"\\processed\\00.jpg");
        //}

        /// <summary>
        /// 初始化人员名单及编号
        /// </summary>
        private void InitRecordFile()
        {
            startTime = System.DateTime.Now;
            recordFileName = startTime.ToString("MM-dd-HH-mm-ss") + ".csv";
            //sw = new StreamWriter(recordFolderPath + "\\" + recordFileName, true, UnicodeEncoding.GetEncoding("GB2312"));
            //sw.Flush();
        }

        /// <summary>
        /// 初始化及预处理数据
        /// </summary>
        private void InitAndPreProcessData()
        {
            AllData = new List<string[]>();
            
            allSignedStudents = new List<int>();
            allSignedNum = new List<int>();
            allSignedName = new List<string>();
            allSignedStudentsName = new List<string>();

            FileStream fs = new FileStream(readFolderPath + "\\" + readFileName , System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs, UnicodeEncoding.GetEncoding("GB2312"));
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                if (IsFirst == true)
                {
                    tableHead = strLine.Split(',');
                    IsFirst = false;
                }
                else
                {
                    // 0编号 1name 2类型 3是否签到
                    aryLine = strLine.Split(',');
                    AllData.Add(aryLine);
                }
            }
            sr.Close();
            // removeRepeateData();
            removeUnsignedPeople();
            foreach (var people in AllData)
            {
                if (people[2] == "学生")
                {
                    allSignedStudents.Add(int.Parse(people[0]));
                    allSignedStudentsName.Add(people[1]);
                }
                if(people[2] == "学生" || people[2] == "往年校友")
                {
                    allSignedNum.Add(int.Parse(people[0]));
                    allSignedName.Add(people[1]);
                }
            }
            
        }

        private void removeUnsignedPeople()
        {
            for (int i = 0; i < AllData.Count; i++)
            {
                if (AllData[i][3] == "0")
                {
                    Console.WriteLine(AllData[i][1]);
                    AllData.Remove(AllData[i]);
                }
            }
        }
        private void removeRepeateData()
        {
            List<int[]> repeatindex = new List<int[]>();
            int length = AllData.Count;
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (AllData[j][1] == AllData[i][1] && AllData[j][4] == AllData[i][4])
                    {
                        int[] buffer = { 0, 0 };
                        buffer[0] = i;
                        buffer[1] = j;
                        repeatindex.Add(buffer);
                    }
                }
            }
            int removeCount = 0;
            List<int> deletindex = new List<int>();
            foreach (var data in repeatindex)
            {
                if (data[0] > data[1])
                {
                    deletindex.Add(data[0]);
                }
            }
            for (int i = 0; i < deletindex.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (deletindex[j] == deletindex[i])
                    {
                        deletindex.RemoveAt(j);
                        i--;
                    }
                }
            }
            foreach (var index in deletindex)
            {
                AllData.RemoveAt(index - removeCount);
                removeCount++;
            }
        }
        private string getPicPath(String name)
        {
            foreach (var picpath in picFiles)
            {
                String matchexp = name + "+";
                if (Regex.IsMatch(picpath, matchexp))
                {
                    return picpath;
                }
            }
            return "";
        }
        
        /// <summary>
        /// 窗体拖动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBnt_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 最小化按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiniBnt_OnClick(object sender, RoutedEventArgs e)
        {
            ResetLayoutBnt.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 开始抽奖按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBnt_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("开始抽奖！");
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { clearAllImgText(); })));
            stophere = false;
            switch (state)
            {
                case 0:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Executor.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取一人，来抽选阳光普照奖");
                    // TODO: 确定编号
                    Console.WriteLine("开始确定人选抽奖");
                    randomDispOneperson(24);
                    break;
                case 1:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Sunshine.Visibility = Visibility.Visible; })));
                    Console.WriteLine(Grid_Sunshine.Visibility);
                    setStatusMsg("正在抽取阳光普照奖50人……");
                    Console.WriteLine(Text_Status.Text);
                    startLottery(5);
                    break;
                case 2:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Third.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取三等奖……");
                    startLottery(3);
                    break;
                case 3:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Executor.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取一人，来抽选阳光普照奖");
                    //TODO: 确定编号
                    randomDispOneperson(240);
                    break;
                case 4:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Sunshine.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取阳光普照奖50人……");
                    startLottery(4);
                    break;
                case 5:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Second.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取二等奖……");
                    startLottery(2);
                    break;
                case 6:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Sunshine.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取阳光普照奖50人……");
                    startLottery(4);
                    break;
                case 7:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_First.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取一等奖……");
                    startLottery(1);
                    break;
                case 8:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() => { Grid_Special.Visibility = Visibility.Visible; })));
                    setStatusMsg("正在抽取特等奖……");
                    startLottery(0);
                    break;
                default:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() =>
                    {
                        Grid_Special.Visibility = Visibility.Visible;
                        Img_Special.Source = new BitmapImage(new Uri("pack://application:,,,/Lottery;component/Resources/end.png"));
                    })));
                    setStatusMsg("抽奖已经结束");
                    break;
            }
        }

        /// <summary>
        /// 结束抽奖按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EndBnt_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("结束抽奖！");
            stophere = true;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action((() =>
            {
                switch (state)
                {
                    case 0:
                        setStatusMsg("一人抽取完成");
                        break;
                    case 1:
                        setStatusMsg("阳光普照奖50人抽取完成");
                        break;
                    case 2:
                        setStatusMsg("三等奖抽取完成");
                        break;
                    case 3:
                        setStatusMsg("一人抽取完成");
                        break;
                    case 4:
                        setStatusMsg("阳光普照奖50人抽取完成");
                        break;
                    case 5:
                        setStatusMsg("二等奖抽取完成");
                        break;
                    case 6:
                        setStatusMsg("阳光普照奖50人抽取完成");
                        break;
                    case 7:
                        setStatusMsg("一等奖抽取完成");
                        break;
                    case 8:
                        setStatusMsg("特等奖抽取完成");
                        break;
                    default:
                        setStatusMsg("抽奖已经结束啦");
                        break;
                }
            })));
            state++;
            if (state > 8) state = 9;
        }

        /// <summary>
        /// 撤回抽奖按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WithdrawBnt_OnClick(object sender, RoutedEventArgs e)
        {
            clearAllImgText();
            Console.WriteLine("撤回抽奖！");
            ResetLayoutBnt.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            if (state > 0)
                state--;
            else
                state = 0;
            if (gotPrized.Count > 0)
            {
                gotPrized.RemoveAt(gotPrized.Count - 1);
            }
            writeFile();
        }

        private BitmapImage GetBitImage(string path)
        { 
            if (File.Exists(path))
            {
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            else
            {
                return new BitmapImage(new Uri(picFolderPath + "\\processed\\00.jpg", UriKind.Absolute));
            }
        }

        private void setStatusMsg(String text)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal,new Action(() =>
            {
                Text_Status.Text = text;
            }));
        }

        /// <summary>
        /// 重置布局按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetLayoutBnt_OnClick(object sender, RoutedEventArgs e)
        {
            clearAllImgText();
            setStatusMsg("机器人所 2 0 2 1 年会抽奖");
            Gif_lottery.Visibility = Visibility.Visible;
            //Grid_Special.Visibility = Visibility.Visible;
            //string[] _path1 = { @"C:\BBL\1.jpg" };
            //LoadPicture(0, _path1);
        }

        public void clearAllImgText()
        {
            HideAllImg();

            //clear Img
            Img_Special.Source = null;
            Img_First01.Source = null;
            Img_First02.Source = null;
            Img_Second01.Source = null;
            Img_Second02.Source = null;
            Img_Second03.Source = null;
            Img_Second04.Source = null;
            Img_Second05.Source = null;
            Img_Third01.Source = null;
            Img_Third02.Source = null;
            Img_Third03.Source = null;
            Img_Third04.Source = null;
            Img_Third05.Source = null;
            Img_Third06.Source = null;
            Img_Third07.Source = null;
            Img_Third08.Source = null;
            Img_Third09.Source = null;
            Img_Third10.Source = null;
            Img_Executor.Source = null;

            //clear Name
            Text_Special.Text = null;
            Text_First01.Text = null;
            Text_First02.Text = null;
            Text_Second01.Text = null;
            Text_Second02.Text = null;
            Text_Second03.Text = null;
            Text_Second04.Text = null;
            Text_Second05.Text = null;
            Text_Third01.Text = null;
            Text_Third02.Text = null;
            Text_Third03.Text = null;
            Text_Third04.Text = null;
            Text_Third05.Text = null;
            Text_Third06.Text = null;
            Text_Third07.Text = null;
            Text_Third08.Text = null;
            Text_Third09.Text = null;
            Text_Third10.Text = null;
            Text_Sunshine01.Text = null;
            Text_Sunshine02.Text = null;
            Text_Sunshine03.Text = null;
            Text_Executor.Text = null;
        }
        private bool LoadPicture(int id, string[] path)
        {
            switch (id)
            {
                case 0:
                    Img_Special.Source = GetBitImage(path[0]);
                    break;

                case 1:
                    Img_First01.Source = GetBitImage(path[0]);
                    Img_First02.Source = GetBitImage(path[1]);
                    break;

                case 2:
                    Img_Second01.Source = GetBitImage(path[0]);
                    Img_Second02.Source = GetBitImage(path[1]);
                    Img_Second03.Source = GetBitImage(path[2]);
                    Img_Second04.Source = GetBitImage(path[3]);
                    Img_Second05.Source = GetBitImage(path[4]);
                    break;

                case 3:
                    Img_Third01.Source = GetBitImage(path[0]);
                    Img_Third02.Source = GetBitImage(path[1]);
                    Img_Third03.Source = GetBitImage(path[2]);
                    Img_Third04.Source = GetBitImage(path[3]);
                    Img_Third05.Source = GetBitImage(path[4]);
                    Img_Third06.Source = GetBitImage(path[5]);
                    Img_Third07.Source = GetBitImage(path[6]);
                    Img_Third08.Source = GetBitImage(path[7]);
                    Img_Third09.Source = GetBitImage(path[8]);
                    Img_Third10.Source = GetBitImage(path[9]);
                    break;

                case 4:
                    return true;
                case 5:
                    return true;
                case 6:
                    Img_Executor.Source = GetBitImage(path[0]);
                    break;
                default:
                    return false;
            }
            return true;
        }
        public bool LoadText(int id, string[] name)
        {
            switch (id)
            {
                case 0:
                    setStatusMsg("正在抽取特等奖。。。");
                    Text_Special.Text = name[0];
                    break;
                case 1:
                    setStatusMsg("正在抽取一等奖。。。");
                    Text_First01.Text = name[0];
                    Text_First02.Text = name[1];
                    break;
                case 2:
                    setStatusMsg("正在抽取二等奖、、、");
                    Text_Second01.Text = name[0];
                    Text_Second02.Text = name[1];
                    Text_Second03.Text = name[2];
                    Text_Second04.Text = name[3];
                    Text_Second05.Text = name[4];
                    break;
                case 3:
                    setStatusMsg("正在抽取三等奖、、、");
                    Text_Third01.Text = name[0];
                    Text_Third02.Text = name[1];
                    Text_Third03.Text = name[2];
                    Text_Third04.Text = name[3];
                    Text_Third05.Text = name[4];
                    Text_Third06.Text = name[5];
                    Text_Third07.Text = name[6];
                    Text_Third08.Text = name[7];
                    Text_Third09.Text = name[8];
                    Text_Third10.Text = name[9];
                    break;
                case 4:
                    setStatusMsg("正在抽取阳光普照奖、、、");
                    String name41 = "";
                    String name42 = "";
                    String name43 = "";
                    String name44 = "";
                    String name45 = "";
                    for (int i = 0; i < 10; i++)
                    {
                        name41 += name[i] += "  ";
                        name42 += name[10 + i] += "  ";
                        name43 += name[20 + i] += "  ";
                        name44 += name[30 + i] += "  ";
                        name45 += name[40 + i] += "  ";
                    }
                    Text_Sunshine01.Text = name41;
                    Text_Sunshine02.Text = name42;
                    Text_Sunshine03.Text = name43;
                    Text_Sunshine04.Text = name44;
                    Text_Sunshine05.Text = name45;
                    break;
                case 5:
                    return true;
                case 6:
                    Text_Executor.Text = name[0];
                    break;
                default:
                    return false;
            }
            return true;
        }

        // 前面随机显示，停止后显示编号为Id的人
        public void randomDispOneperson(int id)
        {
            int index = 0;
            string[] name = new string[1];
            string[] _path = new string[1];
    
            while (!stophere)
            {
                index = (int)Math.Floor(rd.NextDouble() * allSignedName.Count);
                _path[0] = picFolderPath + "\\processed\\" + allSignedNum[index].ToString() + ".jpg";
                name[0] = allSignedName[index];
                if (LoadPicture(6, _path) && LoadText(6, name))
                {
                    App.DoEvents(1000 / speed);
                }
            }
           
            _path[0] = picFolderPath + "\\processed\\" + id.ToString() + ".jpg";
            for (int i = 0; i < allSignedNum.Count; i++)
            {
                if (allSignedNum[i] == id)
                {
                    index = i;
                }
            }
            name[0] = allSignedName[index];
            if (LoadPicture(6, _path) && LoadText(6, name))
            {
                App.DoEvents(1000 / speed);
            }
        }

        public void startLottery(int id)
        {
            int randomCount = 0;
            switch (id)
            {
                case 0:
                    randomCount = 1;
                    break;
                case 1:
                    randomCount = 2;
                    break;
                case 2:
                    randomCount = 5;
                    break;
                case 3:
                    randomCount = 10;
                    break;
                case 4:
                    randomCount = 50;
                    break;
                case 5:
                    randomCount = 50;
                    break;
                default:
                    break;
            }
            generateAlreadyPrize();
            HashSet<int> num = new HashSet<int>();
            int[] randomNum = new int[randomCount];
            int[] _bufferrandomNum = new int[randomCount];
            string[] _path = new string[randomCount];
            string[] name = new string[randomCount];
            int newRandomdata = 0;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (id <= 4)
                {
                    //clearAllImgText();
                    // 生成随机数
                    for (int i = 0; num.Count < randomCount; i++)
                    {
                        newRandomdata = (int)Math.Floor(rd.NextDouble() * allSignedStudents.Count);
                        // 去重复
                        if (!alreadyGetPrize.Contains(newRandomdata))
                        {
                            num.Add(newRandomdata);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    randomNum = num.ToArray();
                    for (int i = 0; i < randomCount; i++)
                    {
                        int index = randomNum[i];
                        _path[i] = picFolderPath + "\\processed\\" + allSignedStudents[index].ToString() + ".jpg";
                        name[i] = allSignedStudentsName[index];
                    }
                    if (LoadPicture(id, _path) && LoadText(id, name))
                    {
                        App.DoEvents(50);
                    }
                    while (!stophere)
                    {
                        num.Remove(randomNum[0]);
                        // 不重复添加
                        for (int i = 0; num.Count < randomCount; i++)
                        {
                            newRandomdata = (int)Math.Floor(rd.NextDouble() * allSignedStudents.Count);
                            // 去重复
                            if (!alreadyGetPrize.Contains(newRandomdata))
                            {
                                
                                num.Add(newRandomdata);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        _bufferrandomNum = randomNum;
                        for (int i = 0; i < randomCount - 1; i++)
                        {
                            randomNum[i] = _bufferrandomNum[i + 1];
                        }
                        randomNum[randomCount - 1] = newRandomdata;
                        for (int i = 0; i < randomCount; i++)
                        {
                            int index = randomNum[i];
                            _path[i] = picFolderPath + "\\processed\\" + allSignedStudents[index].ToString() + ".jpg";
                            name[i] = allSignedStudentsName[index];
                        }
                        if (LoadPicture(id, _path) && LoadText(id, name))
                        {
                            App.DoEvents(1000 / speed);
                        }
                    }
                   
                    gotPrized.Add(randomNum.ToList());
                    gotPrized[gotPrized.Count - 1].Add(id);

                    writeFile();           
                }
                else
                {
                    //clearAllImgText();
                    // 生成随机数
                    id = 4;
                    Console.WriteLine("开始校友阳光普照奖抽取");
                    for (int i = 0; num.Count < randomCount; i++)
                    {
                        newRandomdata = (int)Math.Floor(rd.NextDouble() * allSignedNum.Count);
                        // 去重复
                        if (!alreadyGetPrize.Contains(newRandomdata))
                        {
                            //Console.WriteLine(newRandomdata);
                            num.Add(newRandomdata);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    randomNum = num.ToArray();
                    for (int i = 0; i < randomCount; i++)
                    {
                        int index = randomNum[i];
                        _path[i] = picFolderPath + "\\processed\\" + allSignedNum[index].ToString() + ".jpg";
                        name[i] = allSignedName[index];
                    }
                    if (LoadPicture(id, _path) && LoadText(id, name))
                    {
                        App.DoEvents(50);
                    }
                    while (!stophere)
                    {
                        num.Remove(randomNum[0]);
                        // 不重复添加
                        for (int i = 0; num.Count < randomCount; i++)
                        {
                            newRandomdata = (int)Math.Floor(rd.NextDouble() * allSignedNum.Count);
                            // 去重复
                            if (!alreadyGetPrize.Contains(newRandomdata))
                            {
                                

                                num.Add(newRandomdata);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        _bufferrandomNum = randomNum;
                        for (int i = 0; i < randomCount - 1; i++)
                        {
                            randomNum[i] = _bufferrandomNum[i + 1];
                        }
                        randomNum[randomCount - 1] = newRandomdata;
                        for (int i = 0; i < randomCount; i++)
                        {
                            int index = randomNum[i];
                            _path[i] = picFolderPath + "\\processed\\" + allSignedNum[index].ToString() + ".jpg";
                            name[i] = allSignedName[index];
                        }
                        if (LoadPicture(id, _path) && LoadText(id, name))
                        {
                            App.DoEvents(2000 / speed);
                        }
                    }

                    gotPrized.Add(randomNum.ToList());
                    gotPrized[gotPrized.Count - 1].Add(id);

                    writeFile();
                }

            }));
        }


        public void generateAlreadyPrize()
        {
            alreadyGetPrize.Clear();
            for(int i = 0; i < gotPrized.Count; i++)
            {
                for(int j = 0; j < gotPrized[i].Count-1; j++)
                {
                    alreadyGetPrize.Add(gotPrized[i][j]);
                }
            }
        }

        public void writeFile()
        {

            System.IO.StreamWriter sw = new StreamWriter(recordFolderPath + "\\" + recordFileName, false, UnicodeEncoding.GetEncoding("GB2312"));
            sw.Flush();
            for (int i = 0; i < gotPrized.Count; i++)
            {
                int count = gotPrized[i].Count;
                for (int j = 0; j < count - 1; j++)
                {
                    sw.WriteLine(gotPrized[i][j].ToString() + "," + allSignedNum[gotPrized[i][j]].ToString() + "," + allSignedName[gotPrized[i][j]] + "," + gotPrized[i][count - 1].ToString());
                }
            }
            sw.Flush();
            sw.Close();


            String path = "C:\\BBL\\readcsv\\setup.txt";

            try
            {
                FileStream fs_ = new FileStream(readFolderPath+"\\"+configFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw_ = new StreamWriter(fs_);
                sw_.WriteLine(state.ToString());
                sw_.WriteLine(recordFileName);                       // 记录文件名);
                sw_.Flush();
                sw_.Close();
                fs_.Close();
            }
            catch (Exception)
            {

            }
        }
    }


    
    // 用于刷新，并控制延时
    public partial class App : System.Windows.Application
    {
        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);
        //用于刷新，并控制延时
        public static void DoEvents(int delay)
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status !=
                DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
            Thread.Sleep(delay);
        }

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;
            frame.Continue = false;
            return null;
        }
    }
}
