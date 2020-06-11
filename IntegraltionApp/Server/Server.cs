using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonNet;

namespace Server
{
    class Server
    {
        class ConnectedClient
        {
            public Socket cSocket;
            private NetMessaging net;
            public static List<ConnectedClient> clients = new List<ConnectedClient>();
            public string Name { get; private set; }
            public ConnectedClient(Socket s)
            {
                cSocket = s;
                net = new NetMessaging(cSocket);
                //net.SendData("LOGIN", "?");
                //net.LoginCmdReceived += OnLogin;
                net.MessageCmdReceived += OnMessage;
                net.AppendCmdReceived += Append;
                new Thread(() =>
                {
                    try
                    {
                        net.Communicate();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не удалось получить данные от клиента :(");
                        clients.Remove(this);
                    }
                }).Start();
            }

            private double[,] getMatrix(string str)
            {
                var rows = str.Split('\n');
                if (rows.Length == 0)
                {
                    return null;
                }
                int m = rows.Length;
                int n = rows[0].Split(' ').Length;
                var matrix = new double[m, n];

                for (int i = 0; i < m; i++)
                {
                    var items = rows[i].Split(' ');
                    for (int j = 0; j < n; j++)
                    {
                        matrix[i, j] = Double.Parse(items[i]);
                    }
                }
                return matrix;
            }

            private void Append(string command, string data)
            {
                using (var f = new StreamWriter("matrices.txt", true))
                {
                    f.WriteLine(data);
                }
            }

            private void OnMessage(string command, string data)
            {
                
                Console.WriteLine(data);
                var matrix = getMatrix(data);
                string s;
                using (var f = new StreamReader("matrices.txt", Encoding.GetEncoding(1251)))
                {
                    string matrixData = "";
                    bool isFound = false;
                    while ((s = f.ReadLine()) != null)
                    {
                        if (s != "BackMatrix:" && s != "#")
                        {
                            matrixData += s + '\n';
                        }
                        else
                        {
                            
                            if (isFound && s == "#")
                            {
                                
                                this.net.SendData("MESSAGE", matrixData);
                                return;
                            }
                            if (String.Compare(matrixData.Trim('\n'), data.Trim('\n')) == 0)
                            {
                                Console.WriteLine("Exists");
                                isFound = true;
                                matrixData = "";
                            }
                            else
                            {
                                matrixData = "";
                            }
                        }
                        
                    }this.net.SendData("MESSAGE", "NOTFOUND");
                    
                }
            }

        }
        private String host;
        private Socket sSocket;
        private const int port = 8034;
        public Server()
        {
            Console.WriteLine("Получение локального адреса сервера");
            try
            {
                host = Dns.GetHostName();
                Console.WriteLine("Имя хоста: {0}", host);
                sSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                foreach (var addr in Dns.GetHostEntry(host).AddressList)
                {
                    try
                    {
                        sSocket.Bind(
                            new IPEndPoint(addr, port)
                        );
                        Console.WriteLine("Сокет связан с: {0}:{1}", addr, port);
                        break;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не удалось связать с: {0}:{1}", addr, port);
                    }
                }

                sSocket.Listen(10);
                Console.WriteLine("Прослушивание началось...");
                while (true)
                {
                    Console.WriteLine("Ожидание нового подключения...");
                    var cSocket = sSocket.Accept();
                    Console.WriteLine("Соединение с клиентом установлено!");
                    new ConnectedClient(cSocket);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Что-то пошло не так... :(");
            }
        }
    }
}