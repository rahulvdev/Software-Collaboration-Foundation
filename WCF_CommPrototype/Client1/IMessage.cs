/////////////////////////////////////////////////////////////////////////
// ICommService.cs - ICommService contract                             //
//                                                                     //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Summer 2008   //
/////////////////////////////////////////////////////////////////////////

using System.Runtime.Serialization;
using System.ServiceModel;

namespace CommunicationPrototype
{
  [ServiceContract]
  public interface IMessage
  {

    [OperationContract(IsOneWay=true)]
    void PostMessage(Message msg);

    // Not a service operation so only server can call

    Message GetMessage();
  }

  [DataContract]
  public class Message
  {
    [DataMember]
    Command cmd = Command.TestRequest;
    [DataMember]
    string body = "default message text";

    public enum Command
    {
      [EnumMember]
      TestRequest,
      [EnumMember]
      TestStatus,
    }

    [DataMember]
    public Command command
    {
      get { return cmd; }
      set { cmd = value; }
    }

    [DataMember]
    public string text
    {
      get { return body; }
      set { body = value; }
    }

  }
}
