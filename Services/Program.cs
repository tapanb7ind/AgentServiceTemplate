using System;
using Services.Interface;
using Services.Model;


namespace Services
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var _version = "1.0.0";
            Console.WriteLine($"Starting agentService. [{_version}]");

            var agent = new AgentService("Test-Execution", _version, new Tool("Locust", "2.5.0"));
            agent.onServiceStart();
            
            System.Threading.Tasks.Task.Delay(1000).Wait();
            agent.onTaskReceive(dummyTaskHandler);

            System.Threading.Tasks.Task.Delay(5000).Wait();
            agent.onStopServiceRequest();

            Console.ReadLine();
        }
        
        static bool dummyTaskHandler()
        {
            return true;
        }       
    }
}