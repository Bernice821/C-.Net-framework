using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;//匯入網路通訊協定相關函數
using System.Net.Sockets;//匯入網路插座功能函數
using System.Threading;//匯入多執行緒功能函數


namespace CardGame_Client
{
    public partial class elementary : Form
    {
        public intermediate F3 = new intermediate();
        
        public elementary()
        {
            InitializeComponent();
        }
        //全域變數
        //Random location = new Random(); //隨機更改位置 
        //List<Point> points = new List<Point>();
        int FlippedCount ; //已被翻過來圖像(一對)
        int count; //翻了幾張牌
        int myGrade = 0;
        int youGrade = 0;
        Socket T;//通訊物件
        Thread Th;//網路監聽執行緒
        string User;//使用者
        bool play = false;
        char[] click;
        char[] click1;
 
        
        public bool turn //輪到你
        {
            get { return play; }
            set { play = value; }
        }
        public string test5//Form1 to Form2  //接受邀請的玩家名字
        {
            get { return label18.Text; }
            set { label18.Text = value; }
        }
        public string test4//Form1 to Form2  //玩家名稱
        {
            get { return label7.Text; }
            set { label7.Text = value; }
        }
        private string MyIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList; //取得本機IP陣列
            foreach (IPAddress it in ip)
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString();//如果是IPv4回傳此IP字串
                }
            }
            return ""; //找不到合格IP回傳空字串
        }
        private void Listen()
        {
            EndPoint ServerEP = (EndPoint)T.RemoteEndPoint; //Server的EndPoint
            byte[] B = new byte[1023]; //接收用的Byte陣列
            int inLen = 0; //接收的位元組數目
            string Msg; //接收到的完整訊息
            string St; //命令碼
            string Str; //訊息內容(不含命令碼)
            while (true)//無限次監聽迴圈
            {
                try
                {
                    inLen = T.ReceiveFrom(B, ref ServerEP);//收聽資訊並取得位元組數
                }
                catch (Exception)
                {
                    T.Close();//關閉通訊器
                    listBox1.Items.Clear();//清除線上名單
                    listBox2.Items.Add("伺服器斷線了！");//顯示斷線
                    button1.Enabled = true;//連線按鍵恢復可用
                    Th.Abort();//刪除執行緒
                }
                Msg = Encoding.Default.GetString(B, 0, inLen); //解讀完整訊息
                St = Msg.Substring(0, 1); //取出命令碼 (第一個字)
                Str = Msg.Substring(1); //取出命令碼之後的訊息   
                switch (St)//依命令碼執行功能
                {
                    case "L":
                        listBox1.Items.Clear();
                        string[] M = Str.Split(',');
                        for (int i = 0; i < M.Length; i++) listBox1.Items.Add(M[i]);
                        continue;
                    case "R":
                        MessageBox.Show(Str);
                        button4.Enabled = false;
                        button5.Enabled = false;
                        button6.Enabled = false;
                        button3.Enabled = true;
                        continue;
                    case "X":
                        MessageBox.Show(Str);
                        button4.Enabled = false;
                        button5.Enabled = false;
                        button1.Enabled = false;
                        button3.Enabled = false;

                        continue;
                    case "K":
                        youGrade = int.Parse(Str);
                        youScore.Text = "對手分數: ";
                        label22.Text = youGrade.ToString();
                        continue;
                    case "G":
                        listBox2.Items.Add(Str);
                        continue;
                        //遊戲結束
                    case "E":
                        MessageBox.Show("遊戲結束!");
                        TimeRemaining.Stop();
                        button1.Enabled = true;
                        button6.Enabled = true;
                        F3.grade1 = myGrade;
                        F3.grade2 = youGrade;
                        play = false;
                        for (int i = 0; i < 8; i++)
                        {
                            Button D = (Button)this.Controls["img" + i.ToString()];
                            D.BackgroundImage = Properties.Resources.cover;
                        }
                        button3.Enabled = true;
                        button6.Enabled = true;
                        F3.grade1 = myGrade;
                        F3.grade2 = youGrade;
                        F3.turn = true;
                        continue;

                }
                St = Msg.Substring(1, 1);
                Str = Msg.Substring(2);
                switch (St)
                {
                //讀取對手傳來的牌
                    case "c":
                        char[] C = Str.ToCharArray();
                        for (int i = 0; i < 8; i++)
                        {
                            Button D = (Button)this.Controls["img" + i.ToString()];
                            switch (C[i])
                            {
                                case '_':
                                    D.BackgroundImage = Properties.Resources.cover;
                                    D.Enabled = true;
                                    break;
                                case 'O':
                                    D.Tag = "O";
                                    if(D==img3 || D == img5) { D.BackgroundImage = Properties.Resources.img2; }
                                    if (D == img2 || D == img4) { D.BackgroundImage = Properties.Resources.img3; }
                                    if (D == img1|| D == img0) { D.BackgroundImage = Properties.Resources.img4; }
                                    if (D == img6 || D == img7) { D.BackgroundImage = Properties.Resources.img1; }
                                    D.Enabled = false;
                                    break;
                            }

                        }
                        string A = "";
                        for (int i = 0; i <8 ; i++)
                        {
                            A += this.Controls["img" + i.ToString()].Tag;

                        }
                        //判斷是否已經配對完成
                        if (chk(A) >= 4)
                        {
                             TimeRemaining.Stop();
                             button3.Enabled = true;
                             play = false;
                             Send("E" + "結束" + "|" + label18.Text);
                             MessageBox.Show("遊戲結束");
                            button1.Enabled = true;
                            button6.Enabled = true;
                            F3.grade1 = myGrade;
                            F3.grade2 = youGrade;
                            for (int i = 0; i < 8; i++)
                            {
                                Button D = (Button)this.Controls["img" + i.ToString()];
                                D.BackgroundImage = Properties.Resources.cover;
                            }
                            F3.grade1 = myGrade;
                                F3.grade2 = youGrade;
                                //判斷遊戲是否繼續
                            if (myGrade > youGrade)
                                {
                                    listBox2.Items.Add("你贏了");
                                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                                    MessageBox.Show("你贏了");
                                    result.Text = "你贏了";
                                    Send("2" + "ILose" + "|" + label18.Text);
                                }
                                else if (myGrade < youGrade)
                                {
                                    listBox2.Items.Add("你輸了");
                                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                                    MessageBox.Show("你輸了");
                                    result.Text = "你輸了";
                                    Send("2" + "IWin" + "|" + label18.Text);
                                }
                                else if (myGrade == youGrade)
                                {
                                    listBox2.Items.Add("平手了");
                                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                                    MessageBox.Show("平手了");
                                    result.Text = "平手";
                                    Send("2" + "ITie" + "|" + label18.Text);
                            }
                        }
                        else {
                            FlippedCount = 0;
                            listBox2.Items.Add("輪到你");
                            MessageBox.Show("輪到你");
                            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                            play = true;
                        }
                        break;
                    case "I":
                     switch (Str)
                     {
                        case "Replay":
                            listBox2.Items.Add("你邀請玩家重玩初級遊戲");
                            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                            Send("2"+"IV" + "|" + label18.Text);
                            break;
                        case "Win":
                            listBox2.Items.Add("你贏了！");
                            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                            result.Text = "你贏了";
                            play = false;
                            button3.Enabled = true;
                            break;
                            //對手贏
                        case "Lose":
                            listBox2.Items.Add("你輸了！");
                            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                            play = false;
                            result.Text = "你輸了";
                            button3.Enabled = true;
                            break;
                            //平局訊息
                        case "Tie":
                            listBox2.Items.Add("平局！");
                            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                            play = false;
                            result.Text = "平局";
                            button3.Enabled = true;
                            break;
                            //對手邀請重玩遊戲
                        case "V":
                                string[] M = Str.Split('|');
                                if (MessageBox.Show(M[0] + "邀請你重玩初級遊戲，是否接受?", "邀請訊息", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    Send("2" +"IA" + "|" + label18.Text);
                                    label2.Text = "3";
                                    listBox2.Items.Clear();
                                    button4.Enabled = true;
                                    play = true;
                                    myGrade = 0; //歸零我的分數
                                    youGrade = 0; //歸零對方分數
                                    GameReload();
                                    Send("9" + User); //傳送自己的離線訊息給伺服器
                                    listBox1.Items.Clear();//清除線上名單
                                    Th.Abort(); //關閉監聽執行緒
                                    T.Close(); //關閉監聽器
                                    SoundPlayer player = new SoundPlayer(); //開始撥放背景音樂
                                    player.SoundLocation = (Application.StartupPath + "\\source\\one.wav"); //音樂路徑
                                    player.Load(); //同步載入聲音
                                    player.Play(); //啟用新執行緒播放
                                }
                                else
                                {
                                    Send("2"+"IU"  + "|" + M[0]);
                                }
                                break;
                            //同意重玩邀請
                         case "A":
                                MessageBox.Show(label18.Text + "同意你的邀請，可以開始遊戲!");
                                button4.Enabled = true;
                                label2.Text = "3";
                                listBox2.Items.Clear();
                                myGrade = 0; //歸零我的分數
                                youGrade = 0; //歸零對方分數
                                GameReload();
                                Send("9" + User); //傳送自己的離線訊息給伺服器
                                listBox1.Items.Clear();//清除線上名單
                                Th.Abort(); //關閉監聽執行緒
                                T.Close(); //關閉監聽器
                                SoundPlayer player1 = new SoundPlayer(); //開始撥放背景音樂
                                player1.SoundLocation = (Application.StartupPath + "\\source\\one.wav"); //音樂路徑
                                player1.Load(); //同步載入聲音
                                player1.Play(); //啟用新執行緒播放
                                break;
                            //不同意重玩邀請
                            case "U":
                                MessageBox.Show(label18.Text + "不同意你的邀請!");
                                break;
                            case "R":
                                string[] V = Str.Split('|');
                                if (MessageBox.Show(V[0] + "邀請你進入中級遊戲，是否接受?", "邀請訊息", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    Send("2" + "IJ" + "|" + label18.Text);
                                    listBox1.Enabled = false;
                                    button5.Enabled = false;
                                    button1.Enabled = false;
                                    button6.Enabled = false;
                                    button3.Enabled = true;
                                }
                                else
                                {
                                    Send("2" + "IU" + "|" + V[0]);
                                }
                                break;
                            //同意下一級邀請
                            case "J":
                                MessageBox.Show(label18.Text + "同意你的邀請，進入中級!");
                                break;
                            //不同意下一級邀請
                            case "N":
                                MessageBox.Show(label18.Text + "不同意你的邀請!");
                                break;
                        }
                    break;
                }
            }

        }//載入表單
        private void elementary_Load(object sender, EventArgs e)
        {
            label7.Enabled = false;
            label18.Enabled = false;
            button1.Enabled = false;
            button6.Enabled = false;
            button5.Enabled = false;
            label2.Text = "3"; //標籤顯示將卡牌翻轉到封面模式之前的時間3秒鐘
            F3.test4 = label7.Text;
            F3.test5 = label18.Text;
            listBox1.Enabled = false;
            button3.Enabled = false;
            //圖像隨機變動
            /*foreach (PictureBox picture in panel1.Controls)
            {
                next = location.Next(points.Count);
                p = points[next];
                picture.Location = p;
                points.Remove(p);
            }*/


            for (int i = 0; i < 8; i++)
            {
                Button D = (Button)this.Controls["img" + i.ToString()];
                D.BackgroundImage = Properties.Resources.cover;
            }
            myGrade = 0;
            youGrade = 0;
            myScore.Text = "我的分數: ";
            youScore.Text = "對手分數: " ;
            label21.Text = myGrade.ToString();
            label22.Text = youGrade.ToString();
            timeLeft.Text = "40";
            SoundPlayer player = new SoundPlayer(); //開始撥放背景音樂
            player.SoundLocation = (Application.StartupPath + "\\source\\one.wav") ; //音樂路徑
            player.Load(); //同步載入聲音
            player.Play(); //啟用新執行緒播放
            GameReload();
        }
        public void GameReload()
        {
            count = 0;
            for (int i = 0; i < 8; i++)
            {
                Button D = (Button)this.Controls["img" + i.ToString()];
                D.Tag = "_";
                D.Enabled = true;
                D.BackgroundImage = Properties.Resources.cover; 

            }
            myGrade = 0;
            youGrade = 0;
            FlippedCount = 0;
            myScore.Text = "我的分數:";
            youScore.Text = "對手分數:";
            label21.Text = myGrade.ToString();
            label22.Text = youGrade.ToString();
            timeLeft.Text = "40";
            listBox2.Items.Clear();
            listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
        }
        //傳送訊息給 Server
        private void Send(string Str)
        {
            byte[] B = Encoding.Default.GetBytes(Str); //翻譯文字成Byte陣列
            T.Send(B, 0, B.Length, SocketFlags.None); //傳送訊息給伺服器
        }

       
        //重玩按鈕
        private void button1_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            Send("2" + "IReplay" + "|" + User);
          
        }

        //ScoreTimer
        private void ScoreTimer_Tick(object sender, EventArgs e)
        {
            ScoreTimer.Stop();

            //將所有卡切換回背面
            for (int i = 0; i < 8; i++)
            {
                Button D = (Button)this.Controls["img" + i.ToString()];
                D.Enabled = true;
                D.BackgroundImage = Properties.Resources.cover;

            }
        }

        //CountdownTimer
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            int timer = Convert.ToInt32(label2.Text);
            timer -= 1;
            label2.Text = Convert.ToString(timer);
            if (timer == 0)
            {
                CountdownTimer.Stop();
                TimeRemaining.Start();
            }
        }

        //計時器顯示剩餘多少時間才能完成關卡
        private void TimeRemaining_Tick_1(object sender, EventArgs e)
        {
            int timer = Convert.ToInt32(timeLeft.Text);
            timer -= 1;
            timeLeft.Text = Convert.ToString(timer);
            if (timer == 0)
            {
                TimeRemaining.Stop();
                button3.Enabled = true;
                play = false;
                Send("E" + "結束" + "|" + label18.Text);
                F3.grade1 = myGrade;
                F3.grade2 = youGrade;
                for (int i = 0; i < 8; i++)
                {
                    Button D = (Button)this.Controls["img" + i.ToString()];
                    D.BackgroundImage = Properties.Resources.cover;
                }
                if (myGrade > youGrade)
                {
                    listBox2.Items.Add("你贏了");
                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                    MessageBox.Show("你贏了");
                    result.Text = "你贏了";
                    Send("2" + "ILose" + "|" + label18.Text);
                }
                else if (myGrade < youGrade)
                {
                    listBox2.Items.Add("你輸了");
                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                    MessageBox.Show("你輸了");
                    result.Text = "你輸了";
                    Send("2" + "IWin" + "|" + label18.Text);
                }
                else if (myGrade == youGrade)
                {
                    listBox2.Items.Add("平手了");
                    listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
                    MessageBox.Show("平手了");
                    result.Text = "平手";
                    Send("2" + "ITie" + "|" + label18.Text);
                }
                button1.Enabled = true;
                button6.Enabled = true;
                //判斷勝利
            }
        }

        public int chk(string A)
        {
            char[] C = A.ToCharArray();
            if (C[0] == 'O' && C[1] == 'O') { FlippedCount ++; }
            if (C[4] == 'O' && C[2] == 'O') { FlippedCount ++; }
            if (C[3] == 'O' && C[5] == 'O' ) { FlippedCount++; }
            if (C[6] == 'O' && C[7] == 'O' ) { FlippedCount++; }
            return FlippedCount;
        }


        #region Cards
        private void img1_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img0.BackgroundImage = Properties.Resources.img4;
            if (count == 1)
            {
                click1 = "0".ToCharArray();
            }
            else
            {
                click = "0".ToCharArray();
            }
            count++;
            if (count ==2)
            {
                checkImages();
            }

        }
        
        #endregion

        
        //檢查兩個PictureBox中的圖像是否相同
        private void checkImages()
        {
            if ((click[0] == '0' && click1[0] == '1') || (click[0] == '1' && click1[0] == '0'))
            {
                img0.Tag = "O";
                img1.Tag = "O";
                myGrade += 10; //如果匹配正確，則得分增加
                myScore.Text = "我的分數: ";
                label21.Text = myGrade.ToString();
                Send("K" + myGrade + "|" + label18.Text);
                string A = "";
                for (int i = 0; i < 8; i++)
                {
                    A += this.Controls["img" + i.ToString()].Tag;
                }
                Send("2" + "c" + A + "|" + label18.Text);
                count = 0;
                play = false;
                listBox2.Items.Add("換對方!");
                listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
            }
            else if ((click[0] == '6' && click1[0] == '7') || (click[0] == '7' && click1[0] == '6'))
            {
                img6.Tag = "O";
                img7.Tag = "O";
                myGrade += 10; //如果匹配正確，則得分增加
                myScore.Text = "我的分數: ";
                label21.Text = myGrade.ToString();
                Send("K" + myGrade + "|" + label18.Text);
                string A = "";
                for (int i = 0; i < 8; i++)
                {
                    A += this.Controls["img" + i.ToString()].Tag;
                }
                Send("2" + "c" + A + "|" + label18.Text);
                count = 0;
                play = false;
                listBox2.Items.Add("換對方!");
                listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
            }
            else if ((click[0] == '3' && click1[0] == '5') || (click[0] == '5' && click1[0] == '3'))
            {
                img3.Tag = "O";
                img5.Tag = "O";
                myGrade += 10; //如果匹配正確，則得分增加
                myScore.Text = "我的分數: ";
                label21.Text = myGrade.ToString();
                Send("K" + myGrade + "|" + label18.Text);
                string A = "";
                for (int i = 0; i < 8; i++)
                {
                    A += this.Controls["img" + i.ToString()].Tag;
                }
                Send("2" + "c" + A + "|" + label18.Text);
                count = 0;
                play = false;
                listBox2.Items.Add("換對方!");
                listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
            }
            else if ((click[0] == '2' && click1[0] == '4') || (click[0] == '4' && click1[0] == '2'))
            {
                img2.Tag = "O";
                img4.Tag = "O";
                myGrade += 10; //如果匹配正確，則得分增加
                myScore.Text = "我的分數: ";
                label21.Text = myGrade.ToString();
                Send("K" + myGrade + "|" + label18.Text);
                string A = "";
                for (int i = 0; i < 8; i++)
                {
                    A += this.Controls["img" + i.ToString()].Tag;
                }
                Send("2" + "c" + A + "|" + label18.Text);
                count = 0;
                play = false;
                listBox2.Items.Add("換對方!");
                listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
            }
            else
            {
                string A = "";
                for (int i = 0; i < 8; i++)
                {
                    A += this.Controls["img" + i.ToString()].Tag;
                }
                errorChange();
                Send("2" + "c" + A + "|" + label18.Text);
                count = 0;
                play = false;
                listBox2.Items.Add("換對方!");
                listBox2.TopIndex = listBox2.Items.Count - 1; //ListBox捲到底部
            }
        }
        //結束按鈕
        private void button2_Click(object sender, EventArgs e)
        {
            SoundPlayer player = new SoundPlayer();
            player.Stop(); //關閉音樂
            try
            {
                Send("9" + User); //傳送自己的離線訊息給伺服器
                listBox1.Items.Clear();//清除線上名單
                Th.Abort(); //關閉監聽執行緒
                T.Close(); //關閉監聽器
                this.Close();
            }
            catch
            {
                //如果監聽執行緒沒開會出現錯誤，程式跳到此處執行，
                //此處不寫程式就是忽略錯誤，程式繼續執行的意思！
            }
        }
        //判斷牌組配對是否正確
        public void errorChange()
        {
            string g = click[0].ToString();
            string t = click1[0].ToString();
            switch (g)
            {
                case "0":
                    img0.Tag = "_";
                    img0.BackgroundImage = Properties.Resources.cover;
                    img0.Enabled = true;
                    break;
                case "1":
                    img1.Tag = "_";
                    img1.BackgroundImage = Properties.Resources.cover;
                    img1.Enabled = true;
                    break;
                case "2":
                    img2.Tag = "_";
                    img2.BackgroundImage = Properties.Resources.cover;
                    img2.Enabled = true;
                    break;
                case "3":
                    img3.Tag = "_";
                    img3.BackgroundImage = Properties.Resources.cover;
                    img3.Enabled = true;
                    break;
                case "4":
                    img4.Tag = "_";
                    img4.BackgroundImage = Properties.Resources.cover;
                    img4.Enabled = true;
                    break;
                case "5":
                    img5.Tag = "_";
                    img5.BackgroundImage = Properties.Resources.cover;
                    img5.Enabled = true;
                    break;
                case "6":
                    img6.Tag = "_";
                    img6.BackgroundImage = Properties.Resources.cover;
                    img6.Enabled = true;
                    break;
                case "7":
                    img7.Tag = "_";
                    img7.BackgroundImage = Properties.Resources.cover;
                    img7.Enabled = true;
                    break;
            }
            switch (t)
            {
                case "0":
                    img0.Tag = "_";
                    img0.BackgroundImage = Properties.Resources.cover;
                    img0.Enabled = true;
                    break;
                case "1":
                    img1.Tag = "_";
                    img1.BackgroundImage = Properties.Resources.cover;
                    img1.Enabled = true;
                    break;
                case "2":
                    img2.Tag = "_";
                    img2.BackgroundImage = Properties.Resources.cover;
                    img2.Enabled = true;
                    break;
                case "3":
                    img3.Tag = "_";
                    img3.BackgroundImage = Properties.Resources.cover;
                    img3.Enabled = true;
                    break;
                case "4":
                    img4.Tag = "_";
                    img4.BackgroundImage = Properties.Resources.cover;
                    img4.Enabled = true;
                    break;
                case "5":
                    img5.Tag = "_";
                    img5.BackgroundImage = Properties.Resources.cover;
                    img5.Enabled = true;
                    break;
                case "6":
                    img6.Tag = "_";
                    img6.BackgroundImage = Properties.Resources.cover;
                    img6.Enabled = true;
                    break;
                case "7":
                    img7.Tag = "_";
                    img7.BackgroundImage = Properties.Resources.cover;
                    img7.Enabled = true;
                    break;

            }
                myGrade -= 1; //如果匹配錯誤，則得分減少
                if (myGrade < 0) { myGrade = 0; }
                myScore.Text = "我的分數: ";
                label21.Text = myGrade.ToString();
            Send("K" + myGrade + "|" + label18.Text);
        }
        //Closing
        private void elementary_FormClosing(object sender, FormClosingEventArgs e)
        {
            SoundPlayer player = new SoundPlayer();
            player.Stop(); //關閉音樂
            try
            {
                Send("9" + User); //傳送自己的離線訊息給伺服器
                listBox1.Items.Clear();//清除線上名單
                Th.Abort(); //關閉監聽執行緒
                T.Close(); //關閉監聽器
                this.Close();
            }
            catch
            {
                //如果監聽執行緒沒開會出現錯誤，程式跳到此處執行，
                //此處不寫程式就是忽略錯誤，程式繼續執行的意思！
            }
        }

        //返回按鈕，返回form1
        private void button6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("返回主頁面");
            Send("X" + User + "回到主畫面" + "|" + label18.Text);
            SoundPlayer player = new SoundPlayer();
            player.Stop(); //關閉音樂
            Send("9" + User); //傳送自己的離線訊息給伺服器
            listBox1.Items.Clear();//清除線上名單
            Th.Abort(); //關閉監聽執行緒
            T.Close(); //關閉監聽器
            this.Visible = false;
            Form1 f = new Form1();//產生Form1的物件，才可以使用它所提供的Method
            f.ShowDialog(this);//設定elementary為Form1的上層，並開啟elementary視窗。由於在Form1的程式碼內使用this，所以this為Form1的物件本身
            this.Close();
        }

        //下一級按鈕
        private void button3_Click(object sender, EventArgs e)
        {
            Send("R"+User+"已進入中級遊戲"+"|"+label18.Text);
            Send("9" + User); //傳送自己的離線訊息給伺服器
            listBox1.Items.Clear();//清除線上名單
            listBox2.Items.Clear();
            Th.Abort(); //關閉監聽執行緒
            T.Close(); //關閉監聽器
            this.Visible = false;
            F3.ShowDialog(this);
            this.Close();
        }

        private void img2_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img1.BackgroundImage = Properties.Resources.img4;
            if (count == 1)
            {
                click1 = "1".ToCharArray();
            }
            else
            {
                click = "1".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
               
            }


        }

  

        private void img4_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img3.BackgroundImage = Properties.Resources.img2;
            if (count == 1)
            {
                click1 = "3".ToCharArray();
            }
            else
            {
                click = "3".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
              
            }


        }

        private void img5_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img4.BackgroundImage = Properties.Resources.img3;
            if (count == 1)
            {
                click1 = "4".ToCharArray();
            }
            else
            {
                click = "4".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
               
            }


        }

        private void img6_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img5.BackgroundImage = Properties.Resources.img2;
            if (count == 1)
            {
                click1 = "5".ToCharArray();
            }
            else
            {
                click = "5".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
               
            }
        }

        private void img7_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img6.BackgroundImage = Properties.Resources.img1;
            if (count == 1)
            {
                click1 = "6".ToCharArray();
            }
            else
            {
                click = "6".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
            }

        }

        private void img8_Click(object sender, EventArgs e)
        {
            if (play == false) return;
            img7.BackgroundImage = Properties.Resources.img1;
            if (count == 1)
            {
                click1 = "7".ToCharArray();
            }
            else
            {
                click = "7".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
               
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            label18.Enabled = false;
            button3.Enabled = false;
            button1.Enabled = false;
            button6.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false; //忽略跨執行緒操作的錯誤
            User = label7.Text;  //使用者名稱
            string IP = MyIP();//伺服器IP
            int Port = int.Parse(label13.Text);  //伺服器Port
            try
            {
                IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), Port);//建立伺服器端點資訊
                //建立TCP通訊物件
                T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                T.Connect(EP); //連上Server的EP端點(類似撥號連線)
                Th = new Thread(Listen); //建立監聽執行緒
                Th.IsBackground = true; //設定為背景執行緒
                Th.Start(); //開始監聽
                listBox2.Items.Add("初級挑戰");
                //Send("S");
                button4.Enabled = false;
                Send("0" + User); //隨即傳送自己的 UserName 給 Server
                img0.BackgroundImage = Properties.Resources.img4;
                img4.BackgroundImage = Properties.Resources.img3;
                img1.BackgroundImage = Properties.Resources.img4;
                img5.BackgroundImage = Properties.Resources.img2; 
                img2.BackgroundImage = Properties.Resources.img3;
                img6.BackgroundImage = Properties.Resources.img1;
                img3.BackgroundImage = Properties.Resources.img2; 
                img7.BackgroundImage = Properties.Resources.img1;
                if (play == true)
                {
                    MessageBox.Show("由你開始!");
                }
                button3.Enabled = true;
                Send("G"+User+"準備好了"+"|"+label18.Text);
                CountdownTimer.Start();
                ScoreTimer.Start();
                button3.Enabled = false;
            }
            catch
            {
                listBox2.Items.Add("無法連線伺服器");  //連線失敗時顯示訊息
                return;
            }
        }

        private void img2_Click_1(object sender, EventArgs e)
        {
            if (play == false) return;
            img2.BackgroundImage = Properties.Resources.img3;
            if (count == 1)
            {
                click1 = "2".ToCharArray();
            }
            else
            {
                click = "2".ToCharArray();
            }
            count++;
            if (count == 2)
            {
                checkImages();
            }

        }
        //按下結束按鈕
        private void button5_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你確定要結束遊戲?\n若結束遊戲，分數將歸零!", "提示訊息", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                TimeRemaining.Stop();
                button3.Enabled = true;
                play = false;
                youGrade = 0;
                myGrade = 0;
                Send("E" + "結束" + "|" + label18.Text);
                button6.Enabled = true;
                button3.Enabled = true;
                button1.Enabled = true;
                MessageBox.Show("遊戲結束");
            }
        }

    }
}
