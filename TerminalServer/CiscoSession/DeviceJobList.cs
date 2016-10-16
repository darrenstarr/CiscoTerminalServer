using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    class DeviceJobList
    {
        List<DeviceJob> m_jobs = new List<DeviceJob>();
        Mutex m_jobMutex = new Mutex();

        public void AddJob(DeviceJob job)
        {
            if (!m_jobMutex.WaitOne(1000))
                throw new Exception("Failed to get access to job list");

            m_jobs.Add(job);

            m_jobMutex.ReleaseMutex();
        }

        public bool IsEmpty {
            get {
                if (!m_jobMutex.WaitOne(1000))
                    throw new Exception("Failed to get access to job list");
                var result = m_jobs.Count == 0;
                m_jobMutex.ReleaseMutex();
                return result;
            }
        }

        public DeviceJob TakeJob(int waitForMs=1000)
        {
            if (!m_jobMutex.WaitOne(waitForMs))
                throw new Exception("Failed to get access to job list");

            DeviceJob result = null;
            if (m_jobs.Count() > 0)
            {
                result = m_jobs[0];
                m_jobs.RemoveAt(0);
            }

            m_jobMutex.ReleaseMutex();
            return result;
        }
    }
}
