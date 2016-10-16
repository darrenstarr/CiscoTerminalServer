using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    class IncomingDataEventArgs : EventArgs
    {
        public string Data { get; set; }
    }

    class CiscoSessionThread
    {
        public event EventHandler<IncomingDataEventArgs> IncomingData;
        public event EventHandler LoggedInEvent;

        AuthenticationMethod _authenticationMethod;
        ConnectionInfo _connectionInfo;
        SshClient _client;
        ShellStream _stream;

        public string Username { get; set; }
        public string Password { get; set; }
        public string EnablePassword { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; } = 22;

        private AutoResetEvent _readyToClose = new AutoResetEvent(false); 

        public CiscoSessionThread()
        {

        }

        public CiscoSessionThread(string hostname, string username, string password, string enablePassword, int port=22)
        {
            HostName = hostname;
            Username = username;
            Password = password;
            EnablePassword = enablePassword;
            Port = port;
        }

        public void Close()
        {
            _readyToClose.Set();
        }

        public void Run()
        {
            System.Diagnostics.Debug.WriteLine("Starting");

            try
            {
                _authenticationMethod = new PasswordAuthenticationMethod(Username, Password);
                _connectionInfo = new ConnectionInfo(HostName, Port, Username, _authenticationMethod);
                _client = new SshClient(_connectionInfo);
                _client.Connect();
                _stream = _client.CreateShellStream("xterm", 80, 25, 800, 600, 16384);
                _stream.DataReceived += _OnDataReceived;
                if (LoggedInEvent != null)
                    LoggedInEvent.Invoke(this, new EventArgs());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            _readyToClose.WaitOne();

            System.Diagnostics.Debug.WriteLine("Ending");
            _client.Disconnect();
        }

        public void Write(string text)
        {
            _stream.Write(text);
            _stream.Flush();
        }

        public void WriteLine(string text)
        {
            _stream.Write(text);
            _stream.Flush();
        }

        private void _OnDataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            string input = Encoding.UTF8.GetString(e.Data);
            if (IncomingData != null)
                IncomingData.Invoke(this, new IncomingDataEventArgs { Data = input });
        }
    }

    class CiscoSshSession
    {
        public event EventHandler LoggedIn;

        Thread _sessionThread = null;
        CiscoSessionThread _sshSession = null;

        string _receivedData = "";

        DeviceJobList jobList = new DeviceJobList();

        public CiscoSshSession(string hostname, string username, string password, string enablePassword, int port = 22)
        {
            _sshSession = new CiscoSessionThread(hostname, username, password, enablePassword, port);
            _sessionThread = new Thread(() => { StartSessionThread(); });
            _sessionThread.Start();
        }

        public void Close()
        {
            _sshSession.Close();
            _sessionThread.Join();
        }

        public void StartSessionThread()
        {
            _sshSession.IncomingData += OnIncomingData;
            _sshSession.LoggedInEvent += OnLoggedIn;
            _sshSession.Run();
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            if (LoggedIn != null)
                LoggedIn.Invoke(this, new EventArgs());
        }

        AutoResetEvent _dataWaitHandle = new AutoResetEvent(false);
        Mutex _receivedDataMutex = new Mutex();
        private void OnIncomingData(object sender, IncomingDataEventArgs e)
        {
            _receivedDataMutex.WaitOne();
            _receivedData += e.Data;
            _receivedDataMutex.ReleaseMutex();

            System.Diagnostics.Debug.WriteLine(_receivedData);
            _dataWaitHandle.Set();
        }

        /// <summary>
        /// An RFC1035 label expression
        /// </summary>
        /// RFC 1035 states a hostname is as follows
        /// <label> ::= <letter> [ [ <ldh-str> ] <let-dig> ]
        /// <ldh-str> ::= <let-dig-hyp> | <let-dig-hyp> <ldh-str>
        /// <let-dig-hyp> ::= <let-dig> | "-"
        /// <let-dig> ::= <letter> | <digit>
        /// <letter> ::= any one of the 52 alphabetic characters A through Z in
        ///              upper case and a through z in lower case
        /// <digit> ::= any one of the ten digits 0 through 9
        const string _rfc1035Label = "([A-Za-z](?:[A-Za-z0-9-]*[A-Za-z0-9])?)";

        // "\r\ngw>"
        Regex _prompt = new Regex("\r\n((" + _rfc1035Label + "(\\(.*\\))?[#>])|([Pp]assword:[ ]*))$");

        public void FlushIncoming()
        {
            _receivedDataMutex.WaitOne();
            _receivedData = "";
            _receivedDataMutex.ReleaseMutex();
        }

        public void Write(string text)
        {
            _sshSession.Write(text);
        }

        public void WriteLine(string text)
        {
            _sshSession.Write(text + "\r");
        }

        public string WaitPrompt()
        {
            while (true)
            {
                _dataWaitHandle.Reset();
                if(!_dataWaitHandle.WaitOne(10000))
                    throw new Exception("Failed to get prompt");

                _receivedDataMutex.WaitOne();
                var received = _receivedData;
                _receivedDataMutex.ReleaseMutex();

                var match = _prompt.Match(received);
                if(match.Success)
                {
                    FlushIncoming();
                    return received;
                }
            }
        }
    }
}
