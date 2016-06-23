using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    public string imgs = "";
    public string maps = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        string dirPath = System.Web.HttpContext.Current.Request.MapPath("source/");
        string[] paths = Directory.GetFiles(dirPath);
        imgs += @"<div id=""images"">";
        foreach (string path in paths)
        {
            if (path.EndsWith("jpg") || path.EndsWith("JPG") || path.EndsWith("png"))
            {
                string fileName = System.IO.Path.GetFileName(path);
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
                int width = image.Width / image.Height * 300;
                int height = 300;
                fs.Close();

                imgs += @"
            <div class=""image"" ng-repeat=""img in imgs""><a id=""" + fileName.Remove(fileName.IndexOf('.')) + @""" href = ""source/" + fileName + @""" ><img src = ""source/" + fileName + @""" style=""width:" + width + @";height:" + height + @""" /></a></div> ";
            }
            else if (path.EndsWith("json") || path.EndsWith("JSON"))
            {
                StreamReader fs = new StreamReader(path, Encoding.UTF8);
                maps = fs.ReadToEnd().Replace(System.Environment.NewLine, "").Replace(" ", "");
                fs.Close();
            }
        }
        imgs += @"
        </div>";
    }
}