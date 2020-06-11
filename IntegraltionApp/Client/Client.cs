using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonNet;

namespace Client
{
    class Client
    {
        private String serverHost;
        private Socket cSocket;
        private int port = 8034;
        private NetMessaging net;

        public delegate void Message(string s);
        public event Message OnMessageEvent;
        public Client(String serverHost)
        {
            try
            {
                this.serverHost = serverHost;
                Console.WriteLine("Подключение к {0}", this.serverHost);
                cSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                cSocket.Connect(this.serverHost, port);
                net = new NetMessaging(cSocket);
                net.MessageCmdReceived += OnMessage;
                new Thread(() =>
                {
                    try
                    {
                        net.Communicate();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не удалось получить данные. Завершение соединения...");
                    }
                }).Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Что-то пошло не так... :(");
            }
        }

        private void OnMessage(string command, string data)
        {
            if(data != "NOTFOUND")
            {
                OnMessageEvent("\n Обратная матрица получена: \n" + data);
            }
            else
            {
                OnMessageEvent("Матрица не найдена на сервере.");
                
            }
        }

        
        public void SendData(string obj)
        {
            net.SendData("MESSAGE", obj);
        }

        

    }
}
