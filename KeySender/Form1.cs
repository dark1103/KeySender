using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using WindowsInput;

namespace KeySender
{
    public partial class Form1 : Form
    {
        static Form1 form;
        static Thread secondThread;
        List<string> CommandList = new List<string>() {"{ComputerName} - имя компьютера","{AppPath} - расположение приложения","{Delay [время]} - пауза","{Call [путь] [[аргументы]]} - запуск",
        "{Show} - развернуть SendKey","{Hide} - свернуть SendKey","{Close} - закрыть"};
        List<string> ButtonList = new List<string>()
        {
            "{DELETE}","{DOWN}","{UP}","{LEFT}","{RIGHT}","{ENTER}","{ESC}","{TAB}","Допишите это перед строкой,чтобы ввести её с:", "+ - зажатой Shift",
            "^ - зажатой CTRL","% - зажатой ALT"
        };
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            form = this;
            ThreadStart ts = new ThreadStart(NewThread);
            secondThread = new Thread(ts);
            secondThread.Start();
        }

        void NewThread()
        {
            if (File.Exists(Application.StartupPath + @"\SendKeyData.txt"))
            {
                StreamReader stream = File.OpenText(Application.StartupPath + @"\SendKeyData.txt");
                List<string> data = new List<string>();
                while (!stream.EndOfStream)
                {
                    data.Add(stream.ReadLine());
                }
                stream.Close();
                LaunchSendKey(data);
            }
            else
            {
                listBox1.Items.Add("Не найден файл SendKeyData.txt");
            }
        }

        void LaunchSendKey(List<string> commands)
        {
            foreach(var c in commands)
            {
                c.Replace("{ComputerName}", Environment.MachineName);
                c.Replace("{AppPath}", Application.StartupPath);

                string lower = c.ToLower();
                switch (lower)
                {
                    case "{hide}":
                    try {
                        form.Invoke(new Action(() => WindowState = FormWindowState.Minimized));
                    }
                    catch
                    {
                        Thread.CurrentThread.Abort();
                    }
                    Log("Приложение свёрнуто");
                    break;
                    case "{show}":
                    try {
                        form.Invoke(new Action(() => WindowState = FormWindowState.Normal));
                    }
                    catch
                    {
                    Thread.CurrentThread.Abort();
                    }
                    Log("Приложение развёрнуто");
                    break;
                    case "{close}":
                    Log("Приложение закрывается");
                    form.Invoke(new Action(()=> Application.Exit()));
                    break;
                    case "{ctrl-w}":
                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W);
                    break;
                    default:
                    if (lower.StartsWith("{delay") && lower.EndsWith("}"))
                    {
                        string s = lower.Remove(0, 7);
                        s = s.Remove(s.Length - 1, 1);

                        try
                        {
                            float n = float.Parse(s);
                            if (n > 1)
                            {
                                Delay((int)n);
                            }
                            else
                            {
                                Log("Ожидание " + n + " секунд...");
                                Thread.Sleep((int)(1000 * n));
                            }
                        }
                        catch
                        {
                            Log("Ошибка в команде: " + c);
                        }
                    }
                    else if(lower.StartsWith("{call") && lower.EndsWith("}"))
                    {
                        string s = lower.Remove(0, 6);
                        s = s.Remove(s.Length - 1, 1);
                        int argStart = s.IndexOf('['), argEnd = s.IndexOf(']');
                        if(argStart >= 0 && argEnd >= 0)
                        {
                            string arg = s.Substring(argStart+1, argEnd - argStart-1);
                            s = s.Remove(argStart, argEnd - argStart+1);
                            try
                            {
                                Process.Start(s,arg);
                                Log("Запущен процесс: " + s + "  с аргументом: " + arg);
                            }
                            catch
                            {
                                Log("Ошибка запуска процесса: " + s + "  с аргументом: " + arg);
                            }
                        }
                        else
                        {
                            try
                            {
                                Process.Start(s);
                                Log("Запущен процесс: " + s);
                            }
                            catch
                            {
                                Log("Ошибка запуска процесса: " + s);
                            }
                        }
                    }
                    else if(lower.StartsWith("$"))
                    {
                        form.Invoke(new Action(()=>SendKeys.SendWait(c.Remove(0, 1))));
                    }
                    else
                    {
                        form.Invoke(new Action(() => SendKeys.Send(c)));
                    }
                    break;
                }
            }
        }
        void Log(string text)
        {
            try
            {
                listBox1.Invoke(new Action(() => listBox1.Items.Add(text)));
            }
            catch
            {
                Thread.CurrentThread.Abort();
            }
        }

        void Delay(int delay)
        {

            Log("");
            try
            {
                for (int i = delay-1; i >= 0; i--)
                {
                    listBox1.Invoke(new Action(() =>
                    {
                        listBox1.Items[listBox1.Items.Count - 1] = "Ожидание " + delay + " секунд...(Осталось " + i + " )";
                    }));
                    Thread.Sleep(1000);
                }
            }
            catch
            {
                Thread.CurrentThread.Abort();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader stream = File.OpenText(openFileDialog1.FileName);
                List<string> data = new List<string>();
                while (!stream.EndOfStream)
                {
                    data.Add(stream.ReadLine());
                }
                stream.Close();
                LaunchSendKey(data);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Log("-----------");
            foreach(var c in CommandList)
            {
                Log(c);
            }
            Log("-----------");
            foreach (var c in ButtonList)
            {
                Log(c);
            }
            Log("-----------");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            secondThread.Abort();
        }
    }
}
