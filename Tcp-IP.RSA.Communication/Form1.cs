using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using SuperSimpleTcp;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.IO;
using Org.BouncyCastle.OpenSsl;

namespace Tcp_IP.RSA.Communication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        SimpleTcpServer server;
        private void Form1_Load(object sender, EventArgs e)
        {
            string hostname = Dns.GetHostName();
            txtIP.Text = Dns.GetHostByName(hostname).AddressList[0].ToString();
            btnSend.Enabled = false;
            btnPortAc.Enabled = false;
            btnStop.Enabled = false;
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            try
            {
                string data = "";
                byte[] bytes = e.Data.ToArray();
                data += Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                App key = GetKey();
                data = Decrypt(data, key.PrivateKey);
                txtGelen.Text += $"Client : {data} {Environment.NewLine}";
            }
            catch
            {
                txtGelen.Text = $"Client: {Encoding.UTF8.GetString(e.Data.ToArray())} {Environment.NewLine}";
            }
        }

        private void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            lstStatus.Items.Add($"{e.IpPort} bağlantısı koptu. {Environment.NewLine}");
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            btnSend.Enabled = false;
            lstStatus.Items.Add($"{e.IpPort} bağlandı. {Environment.NewLine}");
            btnSend.Enabled = true;
        }
        static App GetKey()
        {
            string PublicKeyStr = @"-----BEGIN PUBLIC KEY-----
            MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDHnBmmmLuRBzzuxrlS65WFHbLk
            jlbtgJnVgtJ3ngd2avwnD4YseRRbNV7D7y0nY1mlOICsahnndfIcjOe0zkksNEEV
            KfvY6625iRv4Zcc9vVGM39pv+5cmIFvYlhF5I0sOjS1PT8s3tybpcDJVpfhtmJCF
            LPvZunvLF2oOGsyQfQIDAQAB
            -----END PUBLIC KEY-----
            ";
            string PrivateKeyStr = @"-----BEGIN RSA PRIVATE KEY-----
            MIICXQIBAAKBgQDHnBmmmLuRBzzuxrlS65WFHbLkjlbtgJnVgtJ3ngd2avwnD4Ys
            eRRbNV7D7y0nY1mlOICsahnndfIcjOe0zkksNEEVKfvY6625iRv4Zcc9vVGM39pv
            +5cmIFvYlhF5I0sOjS1PT8s3tybpcDJVpfhtmJCFLPvZunvLF2oOGsyQfQIDAQAB
            AoGASeb5eDzD9QYBAUaCBrlCOm6sdQJeHMCoM+yjj7XqfWVOxgdxXbXWW45+73v1
            88dMwWcR/MOro45/fSKOBtr5K8Tufr15Qo4URaCNqTluGoZ8c0WauZMtd90OJ7cq
            3tQc+LzYAjMYPqnwOhLEWMBHy/zhiT4r5Qt9ns5UkWfSq+ECQQDuQT5mDPhvZrQT
            OmFfWjJ743C4iGxrpA65c9esS2uDiKXoY4f8JBbo6vwSAu9L0YLIzjUHpvIPNLEr
            cf8ntoEXAkEA1noEuWHrokWhujbnjpP/TlrY3HlwjRcEC/BPL0vFjZKAATQjjG55
            UsLuxW3Sgqkb4sa4z5tDwuILXMhfV1DviwJBALzT3sV4wClih5O5sFSnIt3Ha54F
            GZDSLI1Uv3khKSvNUFh3Ed6HJ9Uf7/nCc69udC38KFFRQgVFKKmbJrNt1K8CQQC/
            MtezTA/8AYdDsl6LRHR9LY/WPIyRusroubpdt3bN1qQ2bnyiXHnbxduGNXlM8eTi
            LbXjnc6ylUD73cTg2k4xAkA2CHOh5aTepBQstdpQcy1urC63UteGxyiRVhVixPPt
            cuTEO31Wy32mJqwu5IZVkjoKMPhGAIsqj+Dj3bZzTPLZ
            -----END RSA PRIVATE KEY-----";
            RSACryptoServiceProvider publicKey = ImportPublicKey(PublicKeyStr);
            RSACryptoServiceProvider privateKey = ImportPrivateKey(PrivateKeyStr);
            App keys = new App { PublicKey = publicKey.ToXmlString(false), PrivateKey = privateKey.ToXmlString(true) };

            return keys;
        }
        static RSACryptoServiceProvider ImportPublicKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }
        static RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);
            return csp;
        }
        static string Encrypt(string strText, string strPublicKey)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(strPublicKey);

            byte[] byteText = Encoding.UTF8.GetBytes(strText);
            byte[] byteEntry = rsa.Encrypt(byteText, false);

            return Convert.ToBase64String(byteEntry);
        }


        static string Decrypt(string strEntryText, string strPrivateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(strPrivateKey);
            byte[] byteEntry = Convert.FromBase64String(strEntryText);
            byte[] byteText = rsa.Decrypt(byteEntry, false);
            return Encoding.UTF8.GetString(byteText);
        }
        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            if(txtPort.Text.Length == 0)
            {
                btnPortAc.Enabled = false;
            }
            else
            {
                btnPortAc.Enabled = true;
            }
        }
        private void txtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            btnPortAc.Enabled = true;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void btnPortAc_Click(object sender, EventArgs e)
        {
            lstStatus.Items.Add($"Port Açılıyor...{Environment.NewLine}");
            try
            {
                btnStop.Enabled = true;
                server = new SimpleTcpServer(txtIP.Text + ":" + txtPort.Text);
                server.Events.ClientConnected += Events_ClientConnected;
                server.Events.ClientDisconnected += Events_ClientDisconnected;
                server.Events.DataReceived += Events_DataReceived;
                server.Start();
                lstStatus.Items.Add($"Port Açıldı...{Environment.NewLine}");
                btnPortAc.Enabled = false;
            }
            catch
            {
                lstStatus.Items.Add($"Port Açılamadı...{Environment.NewLine}");
            }
            
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (server.IsListening)
            {
                if (!string.IsNullOrEmpty(txtSendMessage.Text))
                {
                    string message;
                    App app = GetKey();
                    message = Encrypt(txtSendMessage.Text, app.PublicKey);
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    server.Send(server.GetClients().First(), data);
                    txtGonderilen.Text += $"Server : {txtSendMessage.Text}{Environment.NewLine}";
                    txtSendMessage.Text = string.Empty;
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            lstStatus.Items.Add("Server Kapatılıyor...");
            btnStop.Enabled = false;
            btnPortAc.Enabled = true;
            try
            {
                server.Stop();
                btnSend.Enabled = false;
                lstStatus.Items.Add("Server Kapatıldı");
            }
            catch
            {
                lstStatus.Items.Add("Kapatılamadı");
            }
            
        }
    }
}
