using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Assign_3;


class MinTCPLytter
{
    public static void Main()
    {
        Api api = new Api();

        TcpListener server = null;
        try
        {
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);

            server.Start();
            Console.WriteLine("Server frisk og klar");
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connection accepted.");

                var childSocketThread = new Thread(() =>
                {
                    using (NetworkStream ns = client.GetStream())
                    {
                        try

                        {
                            byte[] bytes = new byte[1024];
                            int bytesRead = ns.Read(bytes, 0, bytes.Length);
                            string request = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                            string response = api.VerifyInput(request);
                            if (ns.CanWrite)
                            {
                                byte[] byteResponse = System.Text.Encoding.ASCII.GetBytes(response);
                                ns.Write(byteResponse, 0, response.Length);
                                ns.Flush();
                            }

                            client.Close();
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e);
                            client.Close();
                        }
                    }
                });
                childSocketThread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }
}