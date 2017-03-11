using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace payssion
{
    public partial class Create : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string api_key = "";
            Dictionary<string, string> paras = new Dictionary<string, string>();
            string secret_key = "";

            paras.Add("amount", "1");
            paras.Add("currency", "USD");
            paras.Add("pm_id", "alipay_cn");
            paras.Add("description", "order_description");
            paras.Add("order_id", "HS208474324322");
            paras.Add("return_url", " ");
            PayssionClient payssion = new PayssionClient(api_key, secret_key, true);

            string response = payssion.create(paras);
            if (payssion.is_success)
            {
                //success
                JavaScriptSerializer json = new JavaScriptSerializer();
                payssionResponse response_status = (payssionResponse)json.Deserialize<payssionResponse>(response);
                if (response_status.todo == "redirect")
                {
                    Response.Redirect(response_status.return_url);
                }
            }
            else
            {
                //faild handle

            }

        }
    }
    public class payssionResponse
    {
        public string todo { get; set; }
        public string return_url { get; set; }
    }
}
