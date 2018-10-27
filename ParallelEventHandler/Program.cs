using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace ParallelEventHandler
{
    class ConnectionEventArgs : EventArgs
    {
        public Connection conn { get; set; }
    }

    class Notification
    {
        public event EventHandler<ConnectionEventArgs> notification;

        protected virtual void OnComplete(Connection conn)
        {
            notification?.Invoke(this, new ConnectionEventArgs() { conn = conn });
        }

        public void notify(Connection conn)
        {
            OnComplete(conn);
        }
    }

    class Provider
    {
        private ConcurrentQueue<int> queue;
        Notification watcher;

        private const int SIZE_QUEUE = 10000;
        private const int MAX_CONCURRENT_THREAD = 25;

        public Provider()
        {
            queue = new ConcurrentQueue<int>();
            watcher = new Notification();

            watcher.notification += updateQueue;

            initQueue();
        }

        public void printQueue()
        {
            Console.WriteLine("La queue contiene {0} elementi\n", queue.Count.ToString());
        }

        public void updateQueue(object source, ConnectionEventArgs conn)
        {
            if(!queue.IsEmpty)
            {
                int id = popFromQueue();
                callSingleTask(id);
            }
        }

        private void initQueue()
        {
            for (int i = 0; i < SIZE_QUEUE; ++i) queue.Enqueue(i);
        }

        private int popFromQueue()
        {
            queue.TryDequeue(out int id);
            return id;
        }

        private List<int> takeMultipleIDs()
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < MAX_CONCURRENT_THREAD; ++i) ids.Add(i);
            return ids;

        }
        public void startMultipleTasks()
        {
            var ids = takeMultipleIDs();
            Parallel.ForEach(ids, id =>  task(id) );
        }

        private void task(int id)
        {
            var conn = new Connection();
            conn.makeConnection(id);
            var wait = true;

            while (wait)
            {
                if (conn.status)
                {
                    Console.WriteLine("Il task {0} ha concluso.", id.ToString());
                    wait = false;

                    // notifica
                    if (! queue.IsEmpty) watcher.notify(conn);
                }

            }
            
        }
        private void callSingleTask(int id)
        {
            Task singleTask = new Task(() =>
            {
                task(id);
            });

            // Start the task.
            singleTask.Start();

            //Parallel.Invoke(() => task(id));
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Provider provider = new Provider();

            provider.startMultipleTasks();

            while (true)
            {
                provider.printQueue();
                Thread.Sleep(1000);
            }
        }
    }
}
