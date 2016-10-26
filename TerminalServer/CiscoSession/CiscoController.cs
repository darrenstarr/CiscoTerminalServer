using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CiscoCLIParsers.Model;
using CiscoCLIParsers.Parsers;

namespace TerminalServer.CiscoSession
{
    class CiscoController
    {
        CiscoSshSession m_session = null;
        DeviceJobList m_jobs = new DeviceJobList();
        string EnablePassword { get; set; } = "C1sco12345";
        bool m_loggedIn = false;

        public CiscoController(string hostname, string username, string password, string enablePassword, int port = 22)
        {
            m_session = new CiscoSshSession(hostname, username, password, enablePassword, port);
            m_session.LoggedIn += LoggedInHandler;
        }

        public void Close()
        {
            m_session.Close();
        }

        private void LoggedInHandler(object sender, EventArgs e)
        {
            if (!m_processing)
            {
                m_loggedIn = true;

                SetTerminalLengthZero();

                ProcessWaitingJob();
            }
        }

        bool m_processing = false;
        private void ProcessWaitingJob()
        {
            if (!m_loggedIn)
                return;

            m_processing = true;
            var nextJob = m_jobs.TakeJob();
            if (nextJob == null)
            {
                m_processing = false;
                return;
            }

            var currentTaskName = "Start";
            bool done = false;
            while (!done)
            {
                var task = nextJob.GetTask(currentTaskName);
                if (task == null)
                    throw new Exception("Task " + currentTaskName + " does not exist");

                m_session.WriteLine(task.Command);
                var result = m_session.WaitPrompt();
                if (result != null)
                {
                    foreach (var handler in task.ResultHandlers)
                    {
                        var match = handler.Expression.Match(result);
                        if (match.Success)
                        {
                            System.Diagnostics.Debug.WriteLine("New State = " + handler.NextTask);
                            currentTaskName = handler.NextTask;
                            if (currentTaskName == "Error")
                                throw new Exception(handler.ErrorString);
                            else if (currentTaskName == "Done")
                            {
                                nextJob.Result = result;
                                nextJob.Unlock();
                                done = true;
                            }

                            break;
                        }
                    }
                }
            }

            m_processing = false;
        }

        private void RunJob(DeviceJob job)
        {
            job.Lock();

            bool runNow = m_jobs.IsEmpty;
            m_jobs.AddJob(job);
            if (runNow)
                ProcessWaitingJob();

            if (!job.WaitDone())
                throw new Exception("Job failed to run, state machine unstable");
        }

        const string _rfc1035Label = "([A-Za-z](?:[A-Za-z0-9-]*[A-Za-z0-9])?)";
        Regex _privExecPrompt = new Regex("\r\n(" + _rfc1035Label + ")#$");
        Regex _userModePrompt = new Regex("\r\n" + _rfc1035Label + ">$");
        Regex _configPrompt = new Regex("\r\n" + _rfc1035Label + "\\([A-Z-]+\\)#$");
        Regex _passwordPrompt = new Regex("Password:[ ]*$");

        public void SetTerminalLengthZero()
        {
            ExecuteSingleCommand("terminal length 0");
            ExecuteSingleCommand("terminal width 0");

            return;
        }

        private string ExecuteSingleCommand(string command)
        {
            var newJob = AutomateGetToPrivExec();
            newJob.AddTask(new DeviceJobTask
            {
                Name = "PrivExecReady",
                Command = command,
                ResultHandlers = new List<DeviceJobResultHandler>
                {
                    new DeviceJobResultHandler
                    {
                        Expression = _privExecPrompt,
                        NextTask = "Done"
                    }
                }
            });

            RunJob(newJob);

            var text = newJob.Result;
            System.Diagnostics.Debug.WriteLine("====START====");
            System.Diagnostics.Debug.WriteLine(text);
            System.Diagnostics.Debug.WriteLine("====END====");

            return text;
        }

        public List<ShowIpInterfaceBriefItem> ShowIPInterfacesBrief()
        {
            var text = ExecuteSingleCommand("show ip interface brief");
            try
            {
                var parser = new CiscoShowIpInterfaceBrief();
                return parser.Parse(text);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public List<ShowInterfacesItem> GetInterfaces()
        {
            var text = ExecuteSingleCommand("show interfaces");
            try
            {
                var parser = new CiscoShowInterfaces();
                return parser.Parse(text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public List<ShowInterfacesItem> ShowIPARP()
        {
            var text = ExecuteSingleCommand("show ip arp");
            try
            {
                var parser = new CiscoShowInterfaces();
                return parser.Parse(text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public List<ShowIPInterfaceItem> ShowIPInterfaces()
        {
            var text = ExecuteSingleCommand("show ip interface");
            try
            {
                var parser = new CiscoShowIPInterface();
                return parser.Parse(text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public List<ShowVLANBriefItem> GetVLANS()
        {
            var text = ExecuteSingleCommand("show vlan brief");
            try
            {
                var parser = new CiscoShowVLANBrief();
                return parser.Parse(text);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public ShowIPRouteEntry ShowIPRoute()
        {
            var text = ExecuteSingleCommand("show ip route");
            try
            {
                var parser = new CiscoShowIpRoute();
                return parser.Parse(text);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public string GetHostname()
        {
            var text = ExecuteSingleCommand("");

            var m = _privExecPrompt.Matches(text);
            if (m.Count != 1 || m[0].Groups.Count < 2)
                return "<Error getting hostname>";
            return m[0].Groups[1].Value;
        }

        public List<ShowCDPEntryItem>ShowCDPNeighbors()
        {
            var text = ExecuteSingleCommand("show cdp entry *");

            try
            {
                var parser = new CiscoShowCDPEntry();
                return parser.Parse(text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public List<ShowInventoryItem> ShowInventory()
        {
            var text = ExecuteSingleCommand("show inventory");

            try
            {
                var parser = new CiscoShowInventory();
                return parser.Parse(text);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return null;
        }

        private DeviceJob AutomateGetToPrivExec()
        {
            var newJob = new DeviceJob();
            newJob.AddTask(new DeviceJobTask
            {
                Name = "Start",
                Command = "",
                ResultHandlers = new List<DeviceJobResultHandler>
                {
                    new DeviceJobResultHandler
                    {
                        Expression = _userModePrompt,
                        NextTask = "Enable"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _privExecPrompt,
                        NextTask = "PrivExecReady"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _configPrompt,
                        NextTask = "ExitConfigMode"
                    }
                }
            });
            newJob.AddTask(new DeviceJobTask
            {
                Name = "ExitConfigMode",
                Command = "end",
                ResultHandlers = new List<DeviceJobResultHandler>
                {
                    new DeviceJobResultHandler
                    {
                        Expression = _userModePrompt,
                        NextTask = "Enable"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _privExecPrompt,
                        NextTask = "PrivExecReady"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _configPrompt,
                        NextTask = "Error",
                        ErrorString = "Failed to exit config mode"
                    }
                }
            });
            newJob.AddTask(new DeviceJobTask
            {
                Name = "Enable",
                Command = "enable",
                ResultHandlers = new List<DeviceJobResultHandler>
                {
                    new DeviceJobResultHandler
                    {
                        Expression = _userModePrompt,
                        NextTask = "Error",
                        ErrorString = "Device ignored enabled request"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _privExecPrompt,
                        NextTask = "PrivExecReady"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _passwordPrompt,
                        NextTask = "SendEnablePassword"
                    }
                }
            });
            newJob.AddTask(new DeviceJobTask
            {
                Name = "SendEnablePassword",
                Command = EnablePassword,
                ResultHandlers = new List<DeviceJobResultHandler>
                {
                    new DeviceJobResultHandler
                    {
                        Expression = _userModePrompt,
                        NextTask = "Error",
                        ErrorString = "Enable password failed"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _privExecPrompt,
                        NextTask = "PrivExecReady"
                    },
                    new DeviceJobResultHandler
                    {
                        Expression = _passwordPrompt,
                        NextTask = "Error",
                        ErrorString = "Enable password failed"
                    }
                }
            });

            return newJob;
        }
    }
}

