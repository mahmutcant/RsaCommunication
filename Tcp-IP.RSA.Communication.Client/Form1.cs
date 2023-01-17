using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Tcp_IP.RSA.Communication.Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SimpleTcpClient client;
        private void Form1_Load(object sender, EventArgs e)
        {
            
            btnSend.Enabled = false;
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
                txtChatBox.Text += $"Server : {data} {Environment.NewLine}";
            }catch
            {
                txtChatBox.Text += $"Server: {Encoding.UTF8.GetString(e.Data.ToArray())} {Environment.NewLine}";
            }
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
        private void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            lstStatus.Items.Add($"Server bağlantısı kesildi. {Environment.NewLine}");
        }

        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            lstStatus.Items.Add($"Server Bağlantısı Kuruldu {Environment.NewLine}");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            client = new SimpleTcpClient(txtIP.Text + ":" + txtPort.Text);
            client.Events.Connected += Events_Connected;
            client.Events.Disconnected += Events_Disconnected;
            client.Events.DataReceived += Events_DataReceived;
            try
            {
                client.Connect();
                btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                lstStatus.Items.Add(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                lstStatus.Items.Add("Bağlantı kesildi");
                btnSend.Enabled = false;
            }
            catch
            {
                lstStatus.Items.Add("Bağlantı kesilmesi tamamlanamadı");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client.IsConnected)
            {
                string message;
                App app = GetKey();
                message = Encrypt(txtSendMessage.Text, app.PublicKey);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                client.Send(data);
                txtChatBox.Text += $"Client : {txtSendMessage.Text}{Environment.NewLine}";
                txtSendMessage.Text = string.Empty;
            }
        }
    }
}
