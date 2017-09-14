/////////////////////////////////////////////////////////////////////////////
//  BlockingQueue.cs - demonstrate threads communicating via Queue         //
//  Author      :  Rahul Vijaydev                                          //
//  Reference   :  Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module demonstrates communication between two threads using a 
 *   blocking queue.  If the queue is empty when the reader attempts to deQ
 *   an item then the reader will block until the writing thread enQs an item.
 *   Thus waiting is efficient.
 */

//
using System;
using System.Collections;
using System.Threading;

namespace SWTools
{
  public class BlockingQueue<T>
  {
    private Queue blockingQ;
    ManualResetEvent ev;

    //----< constructor >--------------------------------------------

    public BlockingQueue()
    {
      Queue Q = new Queue();
      blockingQ = Q;
      ev = new ManualResetEvent(false);
    }
    //----< enqueue a string >---------------------------------------

    public void enQ(T msg)
    {
      lock(blockingQ)
      {
        blockingQ.Enqueue(msg);
        ev.Set();
      }
    }
    //
    //----< dequeue a T >---------------------------------------
    //
    //    If a single item is in the queue and a thread
    //    moves toward the deQ but finishes its time allocation
    //    before deQ'ing another thread may get throught the locks
    //    and deQ.  Then the first thread wakes up and since its
    //    waitFlag is false, attempts to deQ the empty queue.
    //  This is the reason for the second count check.

    public T deQ()
    {
      T msg = default(T);
      while(true)
      {
        if (this.size() == 0)
        {
          ev.Reset();
          ev.WaitOne();
        }
        lock (blockingQ)
        {
          if (blockingQ.Count != 0)
          {
            msg = (T)blockingQ.Dequeue();
            break;
          }
        }
      }
      return msg;
    }
    //----< return number of elements in queue >---------------------

    public int size()
    {
      int count;
      lock (blockingQ) { count = blockingQ.Count; }
      return count;
    }
    //----< purge elements from queue >------------------------------

    public void clear() 
    {
      lock(blockingQ) { blockingQ.Clear(); }
    }
  }
}
