﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FtpClient

{
    public class Client
    {
        private string password;
        private string userName;
        private string uri;
        private int bufferSize = 1024;

        public bool passive = true;
        public bool binary = true;
        public bool enableSsl = false;
        public bool hash = false;

        public string URI
        {
            get { return uri; }
        }

        public Client(string uri, string userName, string password)
        {
            this.uri = uri;
            this.userName = userName;
            this.password = password;
        }

        public void ShowMenu()
        {
            Console.WriteLine("1 - open directory");
            Console.WriteLine("2 - download file");
            Console.WriteLine("3 - exit");
        }

        public string DownloadFile(string source, string dest)
        {
            FtpWebRequest request = createRequest(combine(uri, source), WebRequestMethods.Ftp.DownloadFile);

            byte[] buffer = new byte[bufferSize];
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                FileStream fs = new FileStream(dest, FileMode.OpenOrCreate);

                int readCount = stream.Read(buffer, 0, bufferSize);

                while (readCount > 0)
                {
                    if (hash)
                        Console.Write("#");

                    fs.Write(buffer, 0, readCount);
                    readCount = stream.Read(buffer, 0, bufferSize);
                }

                return response.StatusDescription;

            }

            catch (System.Net.WebException)
            {
                Console.WriteLine("It's not file!");
                Console.ReadKey();
                return "";
            }

        }
        
        public string[] ListDirectoryDetails()
        {
            List<string> list = new List<string>();

            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(this.uri);
            request.Credentials = new NetworkCredential(this.userName, this.password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, true);

            while (!reader.EndOfStream)
            {
                list.Add(reader.ReadLine());

            }

            return list.ToArray();
        }

        private FtpWebRequest createRequest(string uri, string method)
        {
            FtpWebRequest r = (FtpWebRequest)WebRequest.Create(uri);

            r.Credentials = new NetworkCredential(userName, password);
            r.Method = method;
            r.UseBinary = binary;
            r.EnableSsl = enableSsl;
            r.UsePassive = passive;

            return r;
        }

        private string combine(string path1, string path2)
        {
            return Path.Combine(path1, path2).Replace("\\", "/");
        }
    }
    
    //information about file to know is it file or directory
    class FileInformation
    {
        string name;
        public string adress;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public FileInformation() { }

        public FileInformation(string name, string adress)
        {
            Name = name;
            this.adress = adress;
        }
    }

    class Function
    {
        public static void PrintFiles(List<FileInformation> list)
        {
            foreach (FileInformation fdi in list)
            {
                Console.WriteLine(fdi.Name);
            }
            Console.WriteLine();
        }

        public static List<FileInformation> GetFilesAndDirectories(Client client)
        {
            try
            {
                List<FileInformation> list = new List<FileInformation>();

                foreach (string s in client.ListDirectoryDetails())

                    list.Add(new FileInformation(s, client.URI));

                list.Insert(0, new FileInformation("...", client.URI));

                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + ": \n" + ex.Message);
            }
            return new List<FileInformation>();
        }
    }
}

