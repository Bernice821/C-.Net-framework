using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;//匯入網路通訊協定相關函數
using System.Net.Sockets;//匯入網路插座功能函數
using System.Threading;//匯入多執行緒功能函數

namespace CardGame_Client
{
    public partial class Form1 : Form
    {
        public elementary F2 = new elementary();
        public intermediate F3 = new intermediate();
        public Form1()
        {
            InitializeComponent();
        }
    
    //全域變數
        Socket T;//通訊物件
        Thread Th;//網路監聽執行緒
        string User;//使用者
        int youScore;
        int youGrade;
        int FlipResult;
        //找出本機IP
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

    //監聽 Server 訊息 (Listening to the Server)
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
                  
                    case "V":
                        string[] C = Str.Split('|');
                        if (MessageBox.Show(C[0] + "邀請你玩遊戲，是否接受?", "邀請訊息", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            int i1 = listBox1.Items.IndexOf(C[0]);
                            listBox1.SetSelected(i1, true);
                            Send("A" + User + "|" + listBox1.SelectedItem);
                            listBox1.Enabled = false;
                            label1.Text = (listBox1.SelectedItem).ToString();
                            button3.Enabled = true;
                            button1.Enabled = false;
                        }
                        else
                        {
                            Send("U" + User + "|" + C[0]);
                        }
                        break;
                    //同意邀請
                    case "A":
                        listBox1.Enabled = false;
                        MessageBox.Show(listBox1.SelectedItem + "同意你的邀請，可以開始遊戲!");
                        button3.Enabled = true;
                        button1.Enabled = false;
                        label1.Text = (listBox1.SelectedItem).ToString();
                        F2.turn = true;
                        F3.turn = true;
                        break;
						
                    //不同意邀請
                    case "U":
                        MessageBox.Show(listBox1.SelectedItem + "不同意你的邀請!");
                        break;
                    //接收線上名單
                    case "L":
                        listBox1.Items.Clear(); //清除名單
                        string[] M = Str.Split(','); //拆解名單成陣列
                        
                        for (int i = 0; i < M.Length; i++)
                        {
                            listBox1.Items.Add(M[i]); //加入名單

                        }
                        if (M.Length > 1)
                        {
                            //F2.test5 = M[1].ToString();
                        }
                        break;
                    //使用者名稱重複
                    case "3":
                        listBox2.Items.Add("使用者名稱" + Str + "重複");
                        T.Close();//關閉通訊器
                        listBox1.Items.Clear();//清除線上名單
                        listBox2.Items.Add("伺服器斷線了！");//顯示斷線
                        button1.Enabled = true;//連線按鍵恢復可用
                        Th.Abort();//刪除執行緒
                        button2.Enabled = true;
                        textBox3.Enabled = true;
                        continue;
                   
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; //忽略跨執行緒操作的錯誤
            User = textBox3.Text;  //使用者名稱
            string IP =MyIP();//伺服器IP
            int Port = int.Parse(textBox2.Text);  //伺服器Port
            try
            {
                IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), Port);//建立伺服器端點資訊
                //建立TCP通訊物件
                T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                T.Connect(EP); //連上Server的EP端點(類似撥號連線)
                Th = new Thread(Listen); //建立監聽執行緒
                Th.IsBackground = true; //設定為背景執行緒
                Th.Start(); //開始監聽
                listBox2.Items.Add("已連線伺服器");
                Send("0" + User); //隨即傳送自己的 UserName 給 Server
                button1.Enabled = false; //讓連線按鍵失效，避免重複連線
                F2.test4 = this.textBox3.Text;//Form1 to Form2
            
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
            }
            catch
            {
                listBox2.Items.Add("無法連線伺服器");  //連線失敗時顯示訊息
                textBox3.Enabled = true;
                button2.Enabled = true;
                return;
            }
        }
        private void Send(string Str)
        {
            byte[] B = Encoding.Default.GetBytes(Str); //翻譯文字成Byte陣列
            T.Send(B, 0, B.Length, SocketFlags.None); //傳送訊息給伺服器
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                if (listBox1.SelectedItem.ToString() != User)
                {
                    Send("V" + User + "|" + listBox1.SelectedItem);
                }
                else
                {
                    MessageBox.Show("不可邀請自己!");
                }
            }
            else
            {
                MessageBox.Show("你沒有選取任何玩家");
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += "" + MyIP();
            textBox1.Text = MyIP();
            button1.Enabled = false;
            button3.Enabled = false;
            textBox2.Enabled = false;
        }

        private void button3_Click_1(object sender, EventArgs e) //開始遊戲按鈕
        {
            F2.test5 = (listBox1.SelectedItem).ToString();
            MessageBox.Show("歡迎!\n遊戲說明:\n遊戲共分成三個等級，希望您挑戰成功!\n提醒您:\n若您配對錯誤會-1分，配對正確則會+10分!\n加油!");
            this.Visible = false;
            F2.ShowDialog(this); //進到初級遊戲頁面
            this.Close();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Send("9" + User);
                listBox1.Items.Clear();
                Th.Abort();
                T.Close();
            }
            catch
            {

            }
        }
    }
}
