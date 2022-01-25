using Services.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AgentService : Services.Model.Service
    {
        public AgentService(string serviceName, string Version, Tool toolInfo) :
            base(serviceName, Version, toolInfo)
        { }
        static TaskRequest CheckForNextRequest()
        {
            Console.WriteLine($"\t\t[TaskRunner] Checking for next TEST-EXECUTION task...");
            return null;
        }
        public override bool onServiceStart()
        {
            Console.WriteLine($"Starting [{Name}] Service...");
            base.onServiceStart();
            Task.Delay(2000).Wait();

            timer_taskChecker = new TimeTracker("TaskTracker", 1, TimeTracker.Interval.Seconds, CheckForNextRequest);
            timer_taskChecker.startTimer();
            return true;
        }
        public override void onServiceStop()
        {
            Console.WriteLine($"Stopping [{Name}] Service...");
            base.onServiceStop();

        }
        public override void onTaskReceive(Func<bool> functionToHandleRequest)
        {
            Console.WriteLine($"Received new Task.");
            base.onTaskReceive(functionToHandleRequest);
            Execute(functionToHandleRequest);
        }
        public override void onTaskComplete()
        {            
            base.onTaskComplete();
        }
        public override void onTaskFail()
        {
            Console.WriteLine($"[{currentTask.Identifier}] Task Failed.");
        }
        public override void Execute(Func<bool> functionToExecute)
        {
            handleRequest();
        }
        void handleRequest()
        {
            bool processing = true;
            Console.WriteLine($"Handling new Request..");
            Task.Run(() => { while (processing) { Console.WriteLine($"Processing request...."); Task.Delay(1 * 1000).Wait(); } });
            
            Task.Delay(5 * 1000).Wait();                        
            processing = false;
            onTaskComplete();
        }
    }
}

namespace Agent
{
    public class AgentStatus
    {
        public string name { get; set; }
        public string started { get; set; }
        public Status status { get; set; }
    }

    public class Status
    {
        public string current { get; set; }
        public ICollection<History> history { get; set; }
    }

    public class History
    {
        public string status { get; set; }
        public string timestamp { get; set; }
    }

}
