using Services.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interface
{
    public interface IService
    {
        #region Agent/service related functions
        public bool onServiceStart();
        public void onServiceStop();
        public void onStopServiceRequest();
        public Service.ServiceStatus getStatus();

        #endregion

        #region Task related functions
        public bool onTaskStart();
        public void onTaskFail();
        public void onTaskComplete();        
        public bool onTaskStart(Func<bool> functionToExecute);
        public void onTaskFail(Func<bool> functionToExecute);
        public void onTaskComplete(Func<bool> functionToExecute);
        public void Execute(Func<bool> functionToExecute);        

        #endregion
    }
}


