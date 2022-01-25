using Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace Services.Model
{
    public class Service : IService
    {
        public enum ServiceStatus
        {
            Starting,
            Running,
            Busy,
            Stopping,
            Stopped
        }

        #region Constructor

        public Service(string serviceName, String Version, Tool toolInfo)
        {
            Name = serviceName;
            version = Version;
            ToolInfo = toolInfo;
        }

        #endregion

        public static string Name { get; private set; }
        public static string IpAddress { get; private set; }
        public static string version { get; private set; }
        public ServiceStatus serviceStatus { get; private set; }
        public static Tool ToolInfo { get; private set; }
        public static TaskRequest currentTask { get; private set; } = null;
        protected static TimeTracker timer_taskChecker { get; set; }
        public TimeTracker timer_pingActivity { get; set; } = new TimeTracker("PingActivity", 1, TimeTracker.Interval.Seconds, ping);
        public virtual ServiceStatus getStatus() { return serviceStatus; }
        public virtual void onTick(Func<Object> functionToExecute) => throw new NotImplementedException();
        public virtual bool onServiceStart()
        {
            timer_pingActivity.startTimer();
            Fake.Pinging.onStart(Name, Enum.GetName(typeof(ServiceStatus), serviceStatus));

            serviceStatus = ServiceStatus.Running;

            System.Threading.Tasks.Task.Delay(2000).Wait();
            Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
            return true;
        }
        public virtual void onServiceStop()
        {
            if (timer_pingActivity != null)
                timer_taskChecker.stopTimer();
            timer_pingActivity.stopTimer();            
            
            if(serviceStatus != ServiceStatus.Stopped)
            {
                serviceStatus = ServiceStatus.Stopped;
                Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
            }
        }
        public virtual void onTaskReceive(Func<bool> functionToHandleRequest)
        {
            if (functionToHandleRequest == null)
                throw new ArgumentNullException("Function cannot be NULL");
            else
            {
                currentTask = new TaskRequest();
                timer_taskChecker.Pause();
            }
        }
        void Execute() => throw new NotImplementedException();
        public virtual bool onTaskStart() 
        {   
            Console.WriteLine($"[{Name}] will not be accepting new requests. Reason: Task in progress");
            serviceStatus = ServiceStatus.Busy;
            Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
            return true; 
        }
        public virtual void onTaskFail() 
        {
            Console.WriteLine($"[{currentTask.Identifier}] Task has failed.");
            onTaskComplete(); 
        }
        public virtual void onTaskComplete() 
        {             
            Console.WriteLine($"[{currentTask.Identifier}] Task Completed.");
            Console.WriteLine($"[{Name}] will now accept new requests.");
            currentTask = null;
            serviceStatus = ServiceStatus.Running;
            Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
            timer_taskChecker.Resume();
        }
        public static object ping()
        {            
            Console.WriteLine($"\t\tPinging to stay in status [{Name}, {version}, {IpAddress}][Tool:{ToolInfo.name}]");

            /*
             Fake to get status from API endpoint by reading from file             
            */

            return null;
        }
        public virtual bool onTaskStart(Func<bool> functionToExecute)
        {
            if (functionToExecute == null)
                return onTaskStart();

            return false; 
        }
        public virtual void onTaskFail(Func<bool> functionToExecute)
        {
            if (functionToExecute == null)
                onTaskFail();

        }
        public virtual void onTaskComplete(Func<bool> functionToExecute)
        {
            if (functionToExecute == null)
                onTaskComplete();

            functionToExecute();
        }
        public virtual void Execute(Func<bool> functionToExecute)
        {
            if (functionToExecute == null)
                Execute();         
        }
        public void onStopServiceRequest()
        {
            Console.WriteLine($"[{Name}] Service-Stop request received.");
            Console.WriteLine($"{ (currentTask == null ? "0" : "1") } task in progress. No new tasks will be accepted");
            serviceStatus = ServiceStatus.Stopping;

            Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
            timer_taskChecker.stopTimer();            
            while(currentTask != null)
            {
                Console.WriteLine($"[{Name}] Waiting (5 seconds) for current task to complete..");
                System.Threading.Tasks.Task.Delay(5000).Wait();
            }
            if (currentTask == null)
            {
                Console.WriteLine($"{ (currentTask == null ? "0" : "1") } task in progress.");
                Console.WriteLine($"[{Name}] Service will now stop");
                serviceStatus = ServiceStatus.Stopped;
                Fake.Pinging.UpdateStatus(Enum.GetName(typeof(ServiceStatus), serviceStatus));
                onServiceStop();
            }
        }
    }

    public class TimeTracker : ITimer
    {
        public Timer timer { get; private set; }
        public enum Interval
        {
            milliseconds,
            Seconds,
            Minutes,
            Hours,
        }
        public int CheckerInterval { get; private set; }
        public string name { get; private set; }
        public Interval interval { get; private set; }
        public static Func<object> functionToExecute { get; private set; }
        public object eventResult { get; private set; }
        public TimeTracker(String CheckerName, int timerInterval, Interval intervalType, Func<object> executeThisOnTimeElapse)
        {
            Console.WriteLine($"Creating new Timer instance [{CheckerName}]");
            name = CheckerName;
            CheckerInterval = timerInterval;
            interval = intervalType;
            functionToExecute = executeThisOnTimeElapse;
            switch (intervalType)
            {
                case Interval.Seconds:
                    timerInterval *= 1000;
                    break;
                case Interval.Minutes:
                    timerInterval *= (60 * 1000);
                    break;
                case Interval.Hours:
                    timerInterval *= (60 * 60 * 1000);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(intervalType));
            }

            timer = new Timer((double)timerInterval) { AutoReset = true, Enabled = true };
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);            
        }
        public void startTimer() {
            Console.WriteLine($"Starting {name} timer..");            
            timer.Start(); 
        }
        public void stopTimer() 
        {
            Console.WriteLine($"Stopping {name} timer..");            
            timer.Enabled = false;            
            timer.Stop();
            timer.Close();
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"[Timer:{name}] The Elapsed event was raised at {e.SignalTime}");
            eventResult = onTimeElapsed();
        }
        public object onTimeElapsed()
        {
            if(functionToExecute == null)
                throw new NotImplementedException();
            else
                return functionToExecute();
        }
        public void Pause()
        {
            if (timer != null)
            {
                Console.WriteLine($"[{name}] Timer is Paused");
                timer.Stop();
                timer.Enabled = false;
            }        
        }
        public void Resume()
        {
            if (timer != null)
            {
                Console.WriteLine($"[{name}] Timer is now Running");
                timer.Enabled = true;
                timer.Start();                
            }
        }
    }
}

namespace Fake
{
    public class Pinging
    {
        public static string GetTimeStamp()
        {
            TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1);            
            return $"{t.TotalSeconds}";
        }
        public static void UpdateStatus(string newStatus)
        {
            Console.WriteLine($"Updating Service status");
            var serviceStatusObj = GetStatus();
            if (serviceStatusObj != null)
            {
                if (serviceStatusObj.status.current.ToLower() != newStatus.ToLower())
                {
                    serviceStatusObj.status.history.Add(new Agent.History { status = serviceStatusObj.status.current, timestamp = GetTimeStamp() });
                    serviceStatusObj.status.current = newStatus;
                }
                else
                    Console.WriteLine($"Skipping service status update [New Status:{newStatus}, current Status:{serviceStatusObj.status}]");
            }
        }
        public static void onStart(string serviceName, string servicestatus)
        {
            var obj = new 
            { 
                name = serviceName, 
                started = DateTime.UtcNow, 
                status = new 
                { 
                    current =  servicestatus, 
                    history = new List<Agent.History>() { 
                        { new Agent.History 
                            { 
                                status = servicestatus, 
                                timestamp = GetTimeStamp()
                            } 
                        }
                    }
                }
            };
            var fileLocation = Path.Combine(Environment.CurrentDirectory, "output", "serviceinfo.json");
            if(File.Exists(fileLocation))
                File.WriteAllText(fileLocation, Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented));
            else
                throw new FileNotFoundException(fileLocation);
        }
        public static Agent.AgentStatus GetStatus()
        {
            var fileLocation = Path.Combine(Environment.CurrentDirectory, "output", "serviceinfo.json");
            Console.WriteLine($"File Path: {fileLocation}");


            if (File.Exists(fileLocation))
            {
                var f = File.ReadAllText(fileLocation);
                if (f.Length > 0)                
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<Agent.AgentStatus>(f);                
            }

            return null;
        }
    }
}