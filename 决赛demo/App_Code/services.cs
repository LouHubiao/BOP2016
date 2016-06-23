using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

using Chronic;
using System.Xml;

/// <summary>
/// Summary description for services
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class services : System.Web.Services.WebService
{

    public services()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public void store(string json)
    {

        string dirPath = System.Web.HttpContext.Current.Request.MapPath("source/map.json");
        FileStream fs = new FileStream(dirPath, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(json);
        fs.Write(data, 0, data.Length);
        fs.Close();
    }

    [WebMethod]
    public string dateByDuration(string date)
    {
        TimeSpan ts = XmlConvert.ToTimeSpan(date);
        DateTime dt = DateTime.Now.Subtract(ts);
        return dt.ToString();
    }

    [WebMethod]
    public string dateByWeek(string date)
    {
        string weeks = date.Substring(date.LastIndexOf('W'));
        DateTime dt = new DateTime(2016, 1, 1).AddDays(7 * int.Parse(weeks));
        return dt.ToString();
    }
}
