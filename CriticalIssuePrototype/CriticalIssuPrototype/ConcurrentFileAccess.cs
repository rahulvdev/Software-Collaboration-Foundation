/////////////////////////////////////////////////////////////////////////////
//  ConcurrentFileAccess.cs - Locking files to avoid concurrent file access//
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Platform:     Windows 10,                                              //
//  Application:  RemoteTestHarness Project                                //
//  Author:       Rahul Vijaydev                                           //
//                                                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module demonstrates file locking to avoid concurrent file access.
 * 
 *  
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 7th December 2016
 *     - first release
 * 
 */
using System;
using System.IO;


namespace CriticaLIssuePrototype
{
    public class FileTransferMessage
    {

        public string fname { get; set; }
        public Stream transferStream { get; set; }
    }
    class Repository
    {
        static object objLock = new object();
        string savePath = "..\\..\\..\\FilesToRep";
        string ToSendPath = "..\\..\\..\\FilesToRep";
        int BlockSize = 1024;
        byte[] block;
        public Repository()
        {
            block = new byte[BlockSize];
        }


        //uploads files to repository
        public void uploadFile(FileTransferMessage msg)
        {
            Console.WriteLine("Uploading file {0} to repositpry", msg.fname);
            int totalBytes = 0;
            string filename = msg.fname;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            //putting lock on file
            lock (objLock)
            {
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }

            }


            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes ",
              filename, totalBytes
            );
        }
        //download file from repository
        public Stream downloadFile(string filename)
        {
            Console.WriteLine("");
            Console.WriteLine("Downloading file {0} from repository", filename);
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                //putting lock on file
                lock (objLock)
                {
                    outStream = new FileStream(sfilename, FileMode.Open);
                }

            }
            else
            {
                Console.WriteLine("File {0} not found in Repository", filename);
                return outStream;
            }

            Console.Write("\n  Sent \"{0}\" ", filename);
            return outStream;
        }


        static void Main(string[] args)
        {
            Repository rep = new Repository();
            string fpath = Path.GetFullPath("..\\..\\..\\TestDriver.dll");

            //Upload file to repository
            Console.WriteLine("Upload file to repository");

            try
            {

                using (var inputStream = new FileStream(fpath, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.fname = "TestDriver.dll";
                    msg.transferStream = inputStream;
                    rep.uploadFile(msg);
                }

            }
            catch (Exception e)
            {
                Console.Write("\n  can't find \"{0}\" exception {1}", fpath, e);
            }

            // downloading a file from the repo to the clients loacal machine
            Stream st = rep.downloadFile("TestDriver.dll");
            st.Close();
            Console.WriteLine("Fetch file from repository");
            Console.ReadLine();
        }
    }
}
