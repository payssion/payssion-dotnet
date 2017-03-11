using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace payssion
{
    public partial class Detail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string api_key = "";
            Dictionary<string, string> paras = new Dictionary<string, string>();
            string secret_key = "";
            paras.Add("transaction_id", "");
            paras.Add("order_id", "");
            PayssionClient payssion = new PayssionClient(api_key, secret_key, true);

            string response=payssion.getDetails(paras);
            if (payssion.is_success)
            {
                //success
                
            }
            else
            { 
                //faild handle

            }
 
        }
    }
}
