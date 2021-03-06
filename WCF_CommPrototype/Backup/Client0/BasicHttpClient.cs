﻿/////////////////////////////////////////////////////////////////////////
// BasicHttpClient.cs - Consumer of ICommService contract              //
//                                                                     //
// Jim Fawcett, CSE775 - Distributed Objects, Spring 2009              //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.Runtime.InteropServices;

namespace CommunicationPrototype
{
  class Client
  {
    [DllImport("USER32.DLL", SetLastError = true)]
    public static extern void SetWindowPos(
      IntPtr hwnd, IntPtr order,
      int xpos, int ypos, int width, int height,
      uint flags
    );
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    ICommService channel;

    Client()
    {
      int ht = Console.LargestWindowHeight;
      int wd = Console.LargestWindowWidth;
      SetWindowPos(GetConsoleWindow(), (IntPtr)0, 550, 50, 400, 600, 0);
      Console.Title = "BasicHttpClient";
    }
    void CreateBasicHttpChannel(string url)
    {
      EndpointAddress address = new EndpointAddress(url);
      BasicHttpBinding binding = new BasicHttpBinding();
      channel = ChannelFactory<ICommService>.CreateChannel(binding, address);
    }
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.Write("\n\n  Please enter name of machine hosting service\n\n");
        return;
      }

      Console.Write("\n  BasicHttpClient Starting to Post Messages to Service");
      Console.Write("\n ======================================================\n");

      Client client = new Client();

      // We're parameterizing the channel creation process so 
      // clients can connect to any ICommService server.

      try
      {
        string url = "http://" + args[0] + ":4030/ICommService/BasicHttp";
        Console.Write("\n  connecting to \"{0}\"\n", url);
        client.CreateBasicHttpChannel(url);
        Message msg = new Message();
        msg.command = Message.Command.DoThat;
        for (int i = 0; i < 20; ++i)
        {
          msg.text = "message #" + i.ToString();
          client.channel.PostMessage(msg);
          Console.Write("\n  sending: {0}", msg.text);

          // Sleeping to demonstrate that messages from different
          // clients will interleave on server

          Thread.Sleep(100);
        }

        /////////////////////////////////////////////////////////////
        // This message would shut down the communication service
        // msg.text = "quit";
        // Console.Write("\n  sending message: {0}", msg.text);
        // client.channel.PostMessage(msg);

        ((ICommunicationObject)client.channel).Close();
      }
      catch (Exception ex)
      {
        Console.Write("\n\n  {0}", ex.Message);
      }
      Console.Write("\n\n");
    }
  }
}
