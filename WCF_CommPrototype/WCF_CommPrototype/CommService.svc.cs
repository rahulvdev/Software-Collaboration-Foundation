/////////////////////////////////////////////////////////////////////////
// CommService.svc.cs - Implementation of IMessage contract            //
// Author       :  Rahul Vijaydev                                      //
// Reference    :  Jim Fawcett                                         //
/////////////////////////////////////////////////////////////////////////

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using SWTools;
using System.Runtime.InteropServices;

namespace CommunicationPrototype
{
  // PerSession activation creates an instance of the service for each
  // client.  That instance lives for a pre-determined lease time.  
  // - If the creating client calls back within the lease time, then
  //   the lease is renewed and the object stays alive.  Otherwise it
  //   is invalidated for garbage collection.
  // - This behavior is a reasonable compromise between the resources
  //   spent to create new objects and the memory allocated to persistant
  //   objects.
  // 

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]

  public class CommService : IMessage
  {
    [DllImport("USER32.DLL", SetLastError = true) ]
    public static extern void SetWindowPos(
      IntPtr hwnd, IntPtr order, 
      int xpos, int ypos, int width, int height, 
      uint flags
    );
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    // We want the queue to be shared by all clients and the server,
    // so make it static.

    static BlockingQueue<Message> BlockingQ = null;
    ServiceHost host = null;

    CommService()
    {
      // Only one service, the first, should create the queue

      if(BlockingQ == null)
        BlockingQ = new BlockingQueue<Message>();

      SetWindowPos(GetConsoleWindow(), (IntPtr)0, 100, 100, 400, 600, 0);
      Console.Title = "Test Harness Server";
    }

    public void PostMessage(Message msg)
    {
      BlockingQ.enQ(msg);
    }

    // Since this is not a service operation only server can call

    public Message GetMessage()
    {
      return BlockingQ.deQ();
    }

    // Method for server's child thread to run to process messages.
    // It's virtual so you can derive from this service and define
    // some other server functionality.

    protected virtual void ThreadProc()
    {
       while (true)
       {
         Message msg = this.GetMessage();
         string cmdStr = "";
         switch (msg.command)
         {
           case Message.Command.TestRequest:
             cmdStr = "XML";
             break;
           case Message.Command.TestStatus:
             cmdStr = "StatusOfLogs";
             break;
           default:
             cmdStr = "unknown command";
             break;
         }
         Console.Write("\n  received: {0}\t{1}",cmdStr,msg.text);
         if (msg.text == "quit")
           break;
       } 
    }

    public static void Main()
    {
      Console.Write("\n  Test Harness Server Starting up");
      Console.Write("\n ==================================\n");

      try
      {
        CommService service = new CommService();

        // - We're using WSHttpBinding and NetTcpBinding so digital certificates
        //   are required.
        // - Both these bindings support ordered delivery of messages by default.

        
        NetTcpBinding binding2 = new NetTcpBinding();
        Uri address2 = new Uri("net.tcp://localhost:4050/ICommService/NetTcp");

        using (service.host = new ServiceHost(typeof(CommService), address2))
        {
          service.host.AddServiceEndpoint(typeof(IMessage), binding2, address2);
          service.host.Open();

          Console.Write("\n  Test Harness Server is ready.");
          Console.WriteLine();

          Thread child = new Thread(new ThreadStart(service.ThreadProc));
          child.Start();
          child.Join();

          Console.Write("\n\n  Press <ENTER> to terminate service.\n\n");
          Console.ReadLine();
        }
      }
      catch (Exception ex)
      {
        Console.Write("\n  {0}\n\n", ex.Message);
      }
    }
  }
}
