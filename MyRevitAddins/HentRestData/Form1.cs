using System;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using mySettings = HentRestData.Properties.Settings;
using OfficeDevPnP.Core.IdentityModel.WSTrustBindings;
using Microsoft.IdentityModel.SecurityTokenService;
using System.ServiceModel;

namespace HentRestData
{
    public partial class Form1 : Form
    {
        public string pathToSave;
        public string fullPathAndName;
        private string Token;
        public Form1()
        {
            InitializeComponent();
            pathToSave = mySettings.Default.pathToSave;
            fullPathAndName = mySettings.Default.fullPathAndName;
            textBox1.Text = fullPathAndName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToSave)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToSave;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                fullPathAndName = dialog.FileName + "\\bbrRest.json";
                mySettings.Default.pathToSave = dialog.FileName;
                textBox1.Text = fullPathAndName;
                mySettings.Default.fullPathAndName = fullPathAndName;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mySettings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Get SAML2 token
            GenericXmlSecurityToken saml2Token;
            string ADFS_URL = "https://fs.datafordeler.dk/adfs/services/trust/13/usernamemixed";
            string USER_NAME = "dfdprod01.sys\\MichailGolubje0Q1P7D";
            string PASSWORD = "mgo123IT";

            using (var factory =
                new WSTrustChannelFactory(
                    new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential),
                    new EndpointAddress(ADFS_URL)))

            {
                factory.TrustVersion = TrustVersion.WSTrust13;

                factory.Credentials.UserName.UserName = USER_NAME;
                factory.Credentials.UserName.Password = PASSWORD;

                RequestSecurityToken rst = new RequestSecurityToken
                {
                    RequestType = RequestTypes.Issue,
                    AppliesTo = new EndpointReference(REALM),
                    KeyType = KeyTypes.Bearer,
                    TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
                };

                IWSTrustChannelContract channel = factory.CreateChannel();
                saml2Token = channel.Issue(rst) as GenericXmlSecurityToken;
            }

        //Convert token
        string saml2TokenBase64 =
            Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(saml2Token.TokenXml.OuterXml.ToCharArray()));


    }

    //Convert token
    string saml2TokenBase64 =
        Convert.ToBase64String(
        System.Text.Encoding.UTF8.GetBytes(saml2Token.TokenXml.OuterXml.ToCharArray()));
}

    //string ADFS_URL = "https://fs.datafordeler.dk/adfs/services/trust/13/usernamemixed";
    //string REALM = "urn:dk:kmd:dd:value:sp:switchboard:knownuser";
    //string USER_NAME = "dfdprod01.sys\\MichailGolubje0Q1P7D"
    //string PASSWORD = "yyy";
    //Uri BASE_ADDRESS = new Uri("https://services.datafordeler.dk");
    //string REQUEST_URI
    //= "/GeoDK/topo_skaermkort/1/wms/?service=WMS&version=1.3.0&LAYERS=dtk_skaermkort&FORMAT=IMAGE/PNG&REQUEST=GetMap&STYLES=&CRS=EPSG:25832&WIDTH=800&HEIGHT=545&BBOX=196364,5952066,923636,6447934&hest100=10";


    ////Get SAML2 token
    //GenericXmlSecurityToken saml2Token;

    //using (var factory = new WSTrustChannelFactory(
    //new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential),
    //new EndpointAddress(ADFS_URL))

    //{
    //    factory.TrustVersion = TrustVersion.WSTrust13;

    //    factory.Credentials.UserName.UserName = USER_NAME;
    //    factory.Credentials.UserName.Password = PASSWORD;

    //    RequestSecurityToken rst = new RequestSecurityToken
    //    {
    //        RequestType = RequestTypes.Issue,
    //        AppliesTo = new EndpointReference(REALM),
    //        KeyType = KeyTypes.Bearer,
    //        TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
    //    };

    //    IWSTrustChannelContract channel = factory.CreateChannel();
    //    saml2Token = channel.Issue(rst) as GenericXmlSecurityToken;
    //}

    ////Convert token
    //string saml2TokenBase64 =
    //    Convert.ToBase64String(
    //    System.Text.Encoding.UTF8.GetBytes(saml2Token.TokenXml.OuterXml.ToCharArray()));

    ////request a resource with the token
    //HttpClient client = new HttpClient();
    //client.DefaultRequestHeaders.Authorization
    //    = new AuthenticationHeaderValue("Bearer", saml2TokenBase64);
    //client.BaseAddress = BASE_ADDRESS;
    //HttpResponseMessage httpResponseMessage = await client.GetAsync(REQUEST_URI);

}
}
