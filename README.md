
payssion-donet
============

##Prerequisites

 * net framework 4.0

##Usage

```  csharp

            string api_key = "  ";
            Dictionary<string, string> paras = new Dictionary<string, string>();
            string secret_key = "  ";

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
                 PayssionResponse response_status = (PayssionResponse)json.Deserialize<PayssionResponse>(response);
                if (response_status.todo == "redirect")
                {
                    Response.Redirect(response_status.return_url);
                }
            }
            else
            {
                //faild handle

            }
```            
#PAYSSION donet library