using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;



namespace uts_rest_api
{
    /// <summary>
    /// UTS Rest API examples
    /// </summary>
    public partial class Form1 : Form
    {
        #region variables
        UTSRestClass _utsRestClass = null;
        string _umlsKey = string.Empty;
        #endregion variables

        #region constructors
        /// <summary>
        /// initiate form
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            statusStrip1.Items.Add("."); //initialize
        }
        #endregion constuctors

        #region methods

        private void Form1_Load(object sender, EventArgs e)
        {
            _umlsKey = Properties.Settings.Default.umlsKey;
            textBoxKey.Text = _umlsKey;
            _utsRestClass = new UTSRestClass(_umlsKey);
        }

        private void sampleMethod()
        {
            Cursor existingCursor = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        private void sampleMethodPlain()
        {
            try
            {

            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        /// <summary>
        /// get ticket granting ticket which can be used for 
        /// granting single use tickets for requests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTicketGrantingTicket_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            try
            {
                if (!string.IsNullOrWhiteSpace(textBoxKey.Text))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    string result = _utsRestClass.ObtainTicketGrantingTicketUri();
                    textBoxResult.Text = result;
                }
                else
                {
                    MessageBox.Show("Please enter your personal UMLS key in order to " +
                        "make requests to the UTS API.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("Error requesting ticket granting ticket. " +
                    "Do you have an Internet connection and also a valid " +
                    "UMLS API Key in the text box above? \r\n" +
                    "Details of error: " +
                    er.ToString(), "Oops.  ");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //save api key for next time
                //(saves to user.config in the user's AppData-Local file stucture) 
                if (true) // was (!string.IsNullOrWhiteSpace(_umlsKey))
                {
                    Properties.Settings.Default.umlsKey = _umlsKey;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        private void textBoxKey_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _umlsKey = textBoxKey.Text.Trim();
                if (_utsRestClass != null)
                {
                    _utsRestClass.ApiKey = _umlsKey;
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }
        #endregion methods

        private void buttonSingleUseTicket_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            textBoxResult.Text = "...working on it...";
            try
            {
                if (!string.IsNullOrWhiteSpace(textBoxKey.Text))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    string result = _utsRestClass.ObtainSingleUseTicket();
                    textBoxResult.Text = result;
                }
                else
                {
                    MessageBox.Show("Please enter your personal UMLS key in order to " +
                        "make requests to the UTS API.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("Error requesting ticket granting ticket. " +
                    "Do you have an Internet connection and also a valid " +
                    "UMLS API Key in the text box above? \r\n" +
                    "Details of error: " +
                    er.ToString(), "Oops.  ");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        /// <summary>
        /// obtain license from UMLS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetLicense_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Attempt to open https://uts.nlm.nih.gov//uts.html on a local browser?",
                    "Ok to open browser?",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("https://uts.nlm.nih.gov//uts.html");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        /// <summary>
        /// set default string to search for to gout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEgGout_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxString.Text = "gout";
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        /// <summary>
        /// set default value to code to search for
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEg90560007_Click(object sender, EventArgs e)
        {
            try
            {
                //shows SNOMED if shift not pressed when clicked
                if (Control.ModifierKeys != Keys.Shift)
                {
                    textBoxCode.Text = "90560007"; //SNOMED 
                }
                else
                {
                    textBoxCode.Text = "C0018099"; //UMLS CUI
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        private void buttonSearchUMLS_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            textBoxResult.Text = "...working on it...";
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (!string.IsNullOrWhiteSpace(textBoxString.Text))
                {
                    string rawResult = _utsRestClass.SearchUMLS(
                        textBoxString.Text.Trim(),
                        500); //page size
                    textBoxResult.Text = rawResult;
                }
                else
                {
                    MessageBox.Show("Please enter search string to search for.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }
    

        private void buttonSearchSnomed_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            textBoxResult.Text = "...working on it...";
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if(!string.IsNullOrWhiteSpace(textBoxString.Text))
                {
                    List<SnomedConcept> result;
                    string rawResult = _utsRestClass.SearchSnomed(
                        textBoxString.Text.Trim(),
                        100, //max pages
                        100, //page size
                        out result);
                    StringBuilder sb = new StringBuilder();
                    for(int i=0; i<result.Count; i++)
                    {
                        sb.Append(result[i].ToString());
                        sb.Append(Environment.NewLine);
                    }
                    textBoxResult.Text = sb.ToString();
                }
                else
                {
                    MessageBox.Show("Please enter search string to search for.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }
        /// <summary>
        /// open a json viewer to examine the raw json results more clearly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonJsonViewer_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("You can Google 'online json viewer' to view " +
                    "the json-formatted output results in a branching tree format.  " +
                    "\r\nAttempt to open https://jsonformatter.org/json-viewer on a local browser?",
                    "Ok to open browser?",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("https://jsonformatter.org/json-viewer");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            try
            {
                HelpForm dlg = new HelpForm();
                dlg.ShowDialog();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        private void buttonGetSnomedContent_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            textBoxResult.Text = "...working on it...";
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (!string.IsNullOrWhiteSpace(textBoxCode.Text))
                {
                    if(textBoxCode.Text.StartsWith("C"))
                    {
                        MessageBox.Show("The code begins with 'C' as a UMLS CUI (concept unique identifier)." +
                            "This function expectes a SNOMED code which is just numeric.");
                        return;
                    }
                    string rawResult = _utsRestClass.RetrieveSNOMEDContent(textBoxCode.Text.Trim());
                    textBoxResult.Text = rawResult;
                }
                else
                {
                    MessageBox.Show("Please enter SNOMED code to search for.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        private void buttonSnomedParents_Click(object sender, EventArgs e)
        {
            //(but search for children if shift key is pressed)
            bool isForChildren;
            if (Control.ModifierKeys == Keys.Shift)
            {
                isForChildren = true; //look for children
            }
            else
            {
                isForChildren = false; // look for parents (default)
            }
            Cursor existingCursor = Cursor.Current;
            textBoxResult.Text = "...working on it...";
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (!string.IsNullOrWhiteSpace(textBoxCode.Text))
                {
                    if (textBoxCode.Text.StartsWith("C"))
                    {
                        MessageBox.Show("The code begins with 'C' as a UMLS CUI (concept unique identifier)." +
                            "This function expectes a SNOMED code which is just numeric.");
                        return;
                    }
                    List<SnomedConcept> result;
                    string rawResult = _utsRestClass.RetrieveChildrenOrParents(textBoxCode.Text.Trim(),
                        isForChildren,
                        100, //page size
                        100, //max pages
                        out result);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < result.Count; i++)
                    {
                        sb.Append(result[i].ToString());
                        sb.Append(Environment.NewLine);
                    }
                    textBoxResult.Text = sb.ToString();
                }
                else
                {
                    MessageBox.Show("Please enter SNOMED code to search for.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        /// <summary>
        /// sent http GET request (or POST if shift is held down)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetSandbox_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            try
            {
                if (!string.IsNullOrWhiteSpace(textBoxSandbox.Text))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    textBoxResult.Text = "...working on it...";
                    bool makeItAPost = false;//by default send a GET request
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        makeItAPost = true;
                    }
                        string result =
                        _utsRestClass.GetCustomRequest(textBoxSandbox.Text.Trim(),
                        makeItAPost);
                    textBoxResult.Text = result;
                }
                else
                {
                    MessageBox.Show("Enter a uri in the textbox to send http GET request to.");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        /// <summary>
        /// append a single use ticket to the uri in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAppendTicket_Click(object sender, EventArgs e)
        {
            Cursor existingCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string ticket = _utsRestClass.ObtainSingleUseTicket();
                string connectingCharacter = textBoxSandbox.Text.Contains("?") ? "&" : "?";
                StringBuilder sbUri = new StringBuilder(textBoxSandbox.Text.Trim());
                sbUri.Append(connectingCharacter);
                sbUri.Append("ticket=");
                sbUri.Append(ticket);
                textBoxSandbox.Text = sbUri.ToString();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
            finally
            {
                Cursor.Current = existingCursor;
            }
        }

        private void buttonEgC0018099_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxCode.Text = "C0018099"; //UMLS CUI
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString(), "Oops");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
