using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    class DeviceJob
    {
        public List<DeviceJobTask> Tasks = new List<DeviceJobTask>();
        AutoResetEvent m_jobLock = new AutoResetEvent(false);
        public string Result = "";

        public void Lock()
        {
            m_jobLock.Reset();
        }

        public void Unlock()
        {
            m_jobLock.Set();
        }

        public bool WaitDone()
        {
            return m_jobLock.WaitOne();
        }

        public void AddTask(DeviceJobTask newTask)
        {
            Tasks.Add(newTask);
        }

        public DeviceJobTask GetTask(string name)
        {
            return Tasks.Where(x => x.Name == name).FirstOrDefault();
        }
    }
}
