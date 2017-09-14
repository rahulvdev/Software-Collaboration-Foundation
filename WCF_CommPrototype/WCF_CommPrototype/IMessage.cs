/////////////////////////////////////////////////////////////////////////
// ICommService.cs - ICommService contract                             //
// Author : Rahul Vijaydev                                             //
// Reference : Jim Fawcett,                                            //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
      TestStatus
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
