using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ParallelEventHandler
{
    class Connection
    {

        public bool status = false;

        public Connection(){ }


        public void makeConnection(int id)
        {
            Thread.Sleep(2000);
            Console.WriteLine("Task {0} avviato..", id);
            status = true;
        }


    }
}
