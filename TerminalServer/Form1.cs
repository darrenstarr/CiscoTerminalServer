using System;
using System.Windows.Forms;

namespace TerminalServer
{
    public partial class Form1 : Form
    {
        CiscoSession.CiscoController controller = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controller = new CiscoSession.CiscoController(host.Text, username.Text, password.Text, enablePassword.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            hostname.Text = "Hostname : " + controller.GetHostname();

            var ipInterfacesBrief = controller.ShowIPInterfacesBrief();
            interfacesView.Items.Clear();
            foreach(var item in ipInterfacesBrief)
            {
                var newItem = new ListViewItem(new string[]
                {
                    item.InterfaceId.InterfaceType.ToString(),
                    item.InterfaceId.InterfaceNumber.ToString(),
                    item.Address == null ? "<unassigned>" : item.Address.ToString(),
                    item.Ok.ToString(),
                    item.Method.ToString(),
                    item.Status.ToString(),
                    item.ProtocolStatus.ToString()
                });
                interfacesView.Items.Add(newItem);
            }

            var inventory = controller.ShowInventory();
            InventoryView.Items.Clear();
            foreach(var item in inventory)
            {
                var newItem = new ListViewItem(new string[] { item.Name, item.Description, item.ProductId, item.VendorId, item.SerialNumber });
                InventoryView.Items.Add(newItem);
            }

            var cdpNeighbors = controller.ShowCDPNeighbors();
            var routes = controller.ShowIPRoute();
            RouteView.Items.Clear();
            if (routes != null)
            {
                foreach (var item in routes.Routes)
                {
                    var newItem = new ListViewItem(new string[]
                    {
                        item.Code.Protocol.ToString(),
                        item.Code.Suffix == CiscoCLIParsers.Model.ERoutingProtocol.Unspecified ? "" : item.Code.Suffix.ToString(),
                        item.Code.Candidate ? "*" : "",
                        item.Code.NextHopOverride ? "*" : "",
                        item.Code.Replicated ? "*" : "",
                        item.Prefix.ToString(),
                        "",
                        "",
                        "",
                        "",
                        ""
                    });

                    foreach (var nextHop in item.NextHops)
                    {
                        if (newItem.SubItems[6].Text != "")
                        {
                            for (var i = 6; i < newItem.SubItems.Count; i++)
                                newItem.SubItems[i].Text += "\n";
                        }
                        newItem.SubItems[6].Text += nextHop.RouteMetric.AdministrativeDistance;
                        newItem.SubItems[7].Text += nextHop.RouteMetric.Metric;
                        newItem.SubItems[8].Text += nextHop.Via == System.Net.IPAddress.Any ? "Directly connected" : nextHop.Via.ToString();
                        if (nextHop.Uptime.Ticks > 0)
                            newItem.SubItems[9].Text += nextHop.Uptime.ToString();
                        newItem.SubItems[10].Text += nextHop.OutgoingInterface == null ? "" : nextHop.OutgoingInterface.ToString();
                    }

                    RouteView.Items.Add(newItem);
                }
            }

            var vlans = controller.GetVLANS();
            //var interfaces = controller.GetInterfaces();
            //var ipInterfaces = controller.ShowIPInterfaces();
            var ipArp = controller.ShowIPARP();
            return;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if(controller != null)
                controller.Close();
            base.OnFormClosed(e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
