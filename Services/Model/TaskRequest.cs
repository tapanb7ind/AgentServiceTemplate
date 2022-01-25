using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Model
{
    public enum TaskType
    {
        Test = 1,
        Extract = 2,
        Other = 99
    }
    public class TaskRequest
    {
        public Guid Identifier { get; private set; } = Guid.NewGuid();
        public TaskType Type { get; set; } = TaskType.Test;
        public string RequestedBy { get; set; } = "Somebody@gmail.com";
        public TimeSpan Received { get; set; } = TimeSpan.Zero;
        public Object ToDo { get; set; } = new { key="", value="" };
    }
}
