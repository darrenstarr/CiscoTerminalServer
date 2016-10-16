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

            var interfaces = controller.GetInterfaces();
            interfacesView.Items.Clear();
            foreach(var item in interfaces)
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

            var inventory = controller.GetInventory();
            InventoryView.Items.Clear();
            foreach(var item in inventory)
            {
                var newItem = new ListViewItem(new string[] { item.Name, item.Description, item.ProductId, item.VendorId, item.SerialNumber });
                InventoryView.Items.Add(newItem);
            }

            var cdpNeighbors = controller.GetCDPNeighbors();
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
