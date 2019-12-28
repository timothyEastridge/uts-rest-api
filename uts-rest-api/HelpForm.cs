using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uts_rest_api
{
    /// <summary>
    /// help form
    /// </summary>
    public partial class HelpForm : Form
    {
        string _text = "Demonstration project for using the National Library of Medicine (NLM) \r\n" +
        "Unified Medical Language System(UMLS) \r\n" +
        "UMLS Terminology Services(UTS) \r\n" +
        "Application Program Interface(API) for \r\n" +
        "Representational State Transfer(UTS REST API)\r\n " +
            "https://uts.nlm.nih.gov//uts.html \r\n" +

        "by Wesley Eastridge 2019 \r\n\r\n\r\n\r\n" +

        "NOTICE:  Users MUST use their own UMLS license, which may be optained UMLS Terminology Services " +
          "at the above website, under the option, 'Request a License.' \r\n" +

        "For more information please see the UMLS REST API[technical documentation] (https://documentation.uts.nlm.nih.gov/rest/home.html). \r\n\r\n\r\n" +

        "This program is open source and references the code library Newtonsoft.Json.dll, which is included in the files, " +
        "and the open source code is available at https://www.newtonsoft.com/json \r\n" +

        "You may also see this code used to populate an electronic medical record problem list, " +
        "including relating to ICD-10 codes, at the open source project Mountain Meadow EMR:  https://eastridges.com/m \r\n";

        /// <summary>
        /// help form
        /// </summary>
        public HelpForm()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            textBox1.Text = _text;
            textBox1.Select(0, 0);
        }
    }
}
