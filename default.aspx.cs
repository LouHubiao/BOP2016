using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OxfordAcademic;
using System.Text;

namespace BOP2016_2
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string id1Str = Request.QueryString["id1"];
            string id2Str = Request.QueryString["id2"];
            if (id1Str == null || id2Str == null)
                return;
            Int64 id1 = -1;
            Int64.TryParse(id1Str, out id1);
            Int64 id2 = -1;
            Int64.TryParse(id2Str, out id2);
            if (id1 == -1 || id2 == -1)
                return;
            Response.Write(GetResult(id1, id2));
        }
        private String GetResult(Int64 arg1, Int64 arg2)
        {
            List<Int64[]> result = new List<Int64[]>();

            GetInfoBy getInfo = new GetInfoBy();

            //get info from ArgFrom and ArgTo
            getInfo.GetInfoByArgFrom_ArgTo(arg1, arg2);
            getInfo.GetInfoByForward1();
            getInfo.GetInfoByBack1();

            //get hop 1 result
            GetResultBy.request1(arg1, getInfo.forward1, arg2, getInfo.from, getInfo.to, ref result);
            GetResultBy.request2Byforward1_back1(arg1, getInfo.forward1, getInfo.back1, getInfo.rid_back1, arg2, getInfo.from, getInfo.to, ref result);
            GetResultBy.request2Byforward2(arg1, getInfo.forward2, arg2, getInfo.from, getInfo.to, ref result);
            GetResultBy.request3Byforward2_back1(arg1, getInfo.forward2, getInfo.back1, getInfo.rid_back1, arg2, getInfo.from, getInfo.to, ref result);
            GetResultBy.request3Byforward1_back2(arg1, getInfo.forward1, getInfo.back2, arg2, getInfo.from, getInfo.to, ref result);

            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(@"[");
            foreach (Int64[] item in result)
            {
                strBuilder.Append(@"[");
                strBuilder.Append(string.Join(",", item));
                strBuilder.Append(@"],");
            }
            strBuilder.Length--;
            strBuilder.Append(@"]");

            return strBuilder.ToString();
        }
    }
}