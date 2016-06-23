using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections;

namespace OxfordAcademic
{
    class OxfordRequest
    {
        static string key = "f7cc29509a8443c5b3a5e56b0e38b5a6";
        //static string[] keys = { "7014e58a8d2b4f078df59cc983926f71", "a1e168f1a1e44d4db7cc64d38e985abc", "cbe444bf32dc4540a5479443d1eda68b", "855cce82d6eb4157a2d03446d0fef836" };
        public static HttpWebRequest init(string expr)
        {
            HttpWebRequest request = WebRequest.Create("http://oxfordhk.azure-api.net/academic/v1.0/evaluate?" + expr + "&subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6") as HttpWebRequest;
            return request;
        }

        public static JSONBack GetInfo(string requestStr)
        {
            HttpWebRequest request = OxfordRequest.init(requestStr);
            JSONBack jsonBack = new JSONBack();
            using (HttpWebResponse response = OxfordRequest.GetResponseNoException(request) as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                //convert JSON to objects list
                jsonBack = OxfordRequest.ConvertToJSONBack(reader.ReadToEnd());
            }
            return jsonBack;
        }

        public static HttpWebResponse GetResponseNoException(HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }

        public static JSONBack ConvertToJSONBack(string jsonStr)
        {
            JSONBack JSONBack = new JSONBack();
            string json = (jsonStr).Replace("\n", "");
            try
            {
                JSONBack = JsonConvert.DeserializeObject<JSONBack>(json);
            }
            catch (Exception e)
            {
                return null;
            }
            return JSONBack;
        }
    }

    class GetInfoBy
    {
        //0 meands from/to AuId, 1 means form/to Id
        public int from = -1;
        public int to = -1;

        //store JSONBack form argFrom
        public JSONBack forward1 = new JSONBack();
        //store JSONBack form forward1
        public JSONBack[] forward2 = new JSONBack[10];
        //store JSONBack form argTo
        public JSONBack back1 = new JSONBack();
        //store JSONBack form back1
        public JSONBack back2 = new JSONBack();
        //store JSONBack form RId=argTo
        public JSONBack rid_back1 = new JSONBack();

        public void GetInfoByArgFrom_ArgTo(Int64 ArgFrom, Int64 ArgTo)
        {
            string requestStr1 = "expr=Id=" + ArgFrom + "&count=1&attributes=AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId";
            string requestStr2 = "expr=Composite(AA.AuId=" + ArgFrom + ")" + "&count=10000&attributes=Id,AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId";
            string requestStr3 = "expr=Id=" + ArgTo + "&count=1&attributes=AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId";
            string requestStr4 = "expr=Composite(AA.AuId=" + ArgTo + ")" + "&count=10000&attributes=Id,AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId";

            JSONBack JSONBack1 = new JSONBack();
            Thread t1 = new Thread(() => { JSONBack1 = OxfordRequest.GetInfo(requestStr1); });
            t1.IsBackground = true;
            t1.Start();

            JSONBack JSONBack2 = new JSONBack();
            Thread t2 = new Thread(() => { JSONBack2 = OxfordRequest.GetInfo(requestStr2); });
            t2.IsBackground = true;
            t2.Start();

            JSONBack JSONBack3 = new JSONBack();
            Thread t3 = new Thread(() => { JSONBack3 = OxfordRequest.GetInfo(requestStr3); });
            t3.IsBackground = true;
            t3.Start();

            JSONBack JSONBack4 = new JSONBack();
            Thread t4 = new Thread(() => { JSONBack4 = OxfordRequest.GetInfo(requestStr4); });
            t4.IsBackground = true;
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();

            from = JSONBack2.entities.Length > 0 ? 0 : 1;
            forward1 = JSONBack2.entities.Length > 0 ? JSONBack2 : JSONBack1;
            to = JSONBack4.entities.Length > 0 ? 0 : 1;
            back1 = JSONBack4.entities.Length > 0 ? JSONBack4 : JSONBack3;

            if (from == 1 && to == 1)
            {
                string requestStr = "expr=RId=" + ArgTo + "&count=1000000&attributes=Id,AA.AuId,AA.AfId,F.FId,C.CId,J.JId";
                rid_back1 = OxfordRequest.GetInfo(requestStr);
            }
            else if (from == 0 && to == 1)
            {
                string requestStr = "expr=RId=" + ArgTo + "&count=1000000&attributes=Id";
                rid_back1 = OxfordRequest.GetInfo(requestStr);
            }
        }

        public void GetInfoByForward1()
        {
            if (forward1.entities != null && ((from == 1 && to == 1) || (from == 1 && to == 0) || (from == 1 && to == 0)))
            {
                string requestStr = "";
                int count = 0;
                List<string> requestStrs = new List<string>();
                foreach (Entity entity in forward1.entities)
                {
                    if (entity.RId != null)
                    {
                        foreach (Int64 rid in entity.RId)
                        {
                            if (count < 50)
                            {
                                if (string.IsNullOrEmpty(requestStr))
                                    requestStr = "Id=" + rid;
                                else
                                    requestStr = "Or(" + requestStr + ",Id=" + rid + ")";
                                count++;
                            }
                            else
                            {
                                if (from == 1 && to == 1)
                                    requestStrs.Add("expr=" + requestStr + "&count=10000&attributes=Id,AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId");
                                else if (from == 1 && to == 0)
                                    requestStrs.Add("expr=" + requestStr + "&count=10000&attributes=Id,RId");
                                requestStr = "Id=" + rid;
                                count = 0;
                            }
                        }
                    }
                }
                if (count <= 50)
                {
                    if (from == 1 && to == 1)
                        requestStrs.Add("expr=" + requestStr + "&count=10000&attributes=Id,AA.AuId,AA.AfId,F.FId,C.CId,J.JId,RId");
                    else if (from == 1 && to == 0)
                        requestStrs.Add("expr=" + requestStr + "&count=10000&attributes=Id,RId");
                }
                List<Thread> threads = new List<Thread>();
                int index = 0;
                foreach (string str in requestStrs)
                {
                    index = requestStrs.IndexOf(str);
                    int indexCopy = index;
                    Thread t = new Thread(() => { forward2[indexCopy] = OxfordRequest.GetInfo(str); });
                    t.IsBackground = true;
                    t.Start();
                    threads.Add(t);
                }
                foreach (Thread t in threads)
                    t.Join();

                //get AfId by AuId
                requestStr = "";
                List<Int64> AuIds = new List<Int64>();
                if (from == 1 && to == 0)
                {
                    foreach (Entity entity in forward1.entities)
                    {
                        foreach (AA aa in entity.Auth)
                        {
                            AuIds.Add(aa.AuId);
                            if (string.IsNullOrEmpty(requestStr))
                                requestStr = "Composite(AA.AuId=" + aa.AuId + ")";
                            else
                                requestStr = "Or(" + requestStr + ",Composite(AA.AuId=" + aa.AuId + "))";
                        }
                    }
                    requestStr = "expr=" + requestStr + "&count=1000000&attributes=AA.AuId,AA.AfId";
                    forward2[index + 1] = OxfordRequest.GetInfo(requestStr);
                    if (forward2[index + 1].entities != null)
                    {
                        foreach (Entity entity in forward2[index + 1].entities)
                        {
                            ArrayList al = new ArrayList(entity.Auth);
                            for (int i = al.Count - 1; i >= 0; i--)
                            {
                                AA aa = (AA)al[i];
                                if (!AuIds.Contains(aa.AuId))
                                    al.Remove(aa);
                            }
                            entity.Auth = (AA[])al.ToArray(typeof(AA));
                        }
                    }
                }

            }
        }

        public void GetInfoByBack1()
        {
            string requestStr = "";
            List<Int64> AuIds = new List<Int64>();
            if (from == 0 && to == 1)
            {
                foreach (Entity backEntity in back1.entities)
                {
                    foreach (AA aa in backEntity.Auth)
                    {
                        AuIds.Add(aa.AuId);
                        if (string.IsNullOrEmpty(requestStr))
                            requestStr = "Composite(AA.AuId=" + aa.AuId + ")";
                        else
                            requestStr = "Or(" + requestStr + ",Composite(AA.AuId=" + aa.AuId + "))";
                    }
                }
                requestStr = "expr=" + requestStr + "&count=1000000&attributes=AA.AuId,AA.AfId";
                back2 = OxfordRequest.GetInfo(requestStr);
                if (back2.entities != null)
                {
                    foreach (Entity entity in back2.entities)
                    {
                        ArrayList al = new ArrayList(entity.Auth);
                        for (int i = al.Count - 1; i >= 0; i--)
                        {
                            AA aa = (AA)al[i];
                            if (!AuIds.Contains(aa.AuId))
                                al.Remove(aa);
                        }
                        entity.Auth = (AA[])al.ToArray(typeof(AA));
                    }
                }
            }
        }
    }

    class GetResultBy
    {
        public static void request1(Int64 ArgFrom, JSONBack forward1, Int64 ArgTo, int from, int to, ref List<Int64[]> result)
        {
            foreach (Entity entity in forward1.entities)
            {
                if (entity.Id == ArgTo && from == 0 && to == 1)
                {
                    Int64[] temp = { ArgFrom, ArgTo };
                    AddArrToResult(temp, result);
                }
                if ((from == 1 && to == 0) || (from == 0 && to == 0))
                {
                    foreach (AA aa in entity.Auth)
                    {
                        if (aa.AuId == ArgTo)
                        {
                            if (from == 0 && to == 0)
                            {
                                Int64[] temp = { ArgFrom, entity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 1 && to == 0)
                            {
                                Int64[] temp = { ArgFrom, ArgTo };
                                AddArrToResult(temp, result);
                            }
                        }
                    }
                }
                if ((from == 1 && to == 1) || (from == 0 && to == 1))
                {
                    foreach (Int64 rid in entity.RId)
                    {
                        if (rid == ArgTo)
                        {
                            if (from == 1 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 0 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, entity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                        }
                    }
                }
            }
        }

        public static void request2Byforward1_back1(Int64 ArgFrom, JSONBack forward1, JSONBack back1, JSONBack rid_back1, Int64 ArgTo, int from, int to, ref List<Int64[]> result)
        {
            foreach (Entity forwardEntity in forward1.entities)
            {
                foreach (Entity backEntity in back1.entities)
                {
                    foreach (AA forwardAa in forwardEntity.Auth)
                    {
                        foreach (AA backAa in backEntity.Auth)
                        {
                            if (((from == 1 && to == 1) || (from == 1 && to == 0) || (from == 0 && to == 1)) && forwardAa.AuId != -1 && forwardAa.AuId == backAa.AuId)
                            {
                                if (from == 1 && to == 1)
                                {
                                    Int64[] temp = { ArgFrom, forwardAa.AuId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                                else if (from == 1 && to == 0)
                                {
                                    Int64[] temp = { ArgFrom, forwardAa.AuId, backEntity.Id, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                                else if (from == 0 && to == 1)
                                {
                                    Int64[] temp = { ArgFrom, forwardEntity.Id, forwardAa.AuId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                            if ((from == 0 && to == 0) && forwardAa.AfId != -1 && forwardAa.AfId == backAa.AfId)
                            {
                                if (from == 0 && to == 0 && forwardAa.AuId == ArgFrom && backAa.AuId == ArgTo)
                                {
                                    Int64[] temp = { ArgFrom, forwardAa.AfId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                        }
                    }
                    if ((from == 1 && to == 1) || (from == 1 && to == 0) || (from == 0 && to == 1))
                    {
                        foreach (F forwardF in forwardEntity.Filed)
                        {
                            foreach (F backF in backEntity.Filed)
                            {
                                if (forwardF.FId != -1 && forwardF.FId == backF.FId)
                                {
                                    if (from == 1 && to == 1)
                                    {
                                        Int64[] temp = { ArgFrom, forwardF.FId, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                    else if (from == 1 && to == 0)
                                    {
                                        Int64[] temp = { ArgFrom, forwardF.FId, backEntity.Id, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                    else if (from == 0 && to == 1)
                                    {
                                        Int64[] temp = { ArgFrom, forwardEntity.Id, forwardF.FId, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                }
                            }
                        }
                        if (backEntity.Journal != null && forwardEntity.Journal != null && forwardEntity.Journal.JId != -1 && forwardEntity.Journal.JId == backEntity.Journal.JId)
                        {
                            if (from == 1 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Journal.JId, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 1 && to == 0)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Journal.JId, backEntity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 0 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Id, forwardEntity.Journal.JId, ArgTo };
                                AddArrToResult(temp, result);
                            }
                        }
                        if (backEntity.Conference != null && forwardEntity.Conference != null && forwardEntity.Conference.CId != -1 && forwardEntity.Conference.CId == backEntity.Conference.CId)
                        {
                            if (from == 1 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Conference.CId, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 1 && to == 0)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Conference.CId, backEntity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            else if (from == 0 && to == 1)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Id, forwardEntity.Conference.CId, ArgTo };
                                AddArrToResult(temp, result);
                            }
                        }
                    }
                    if ((from == 1 && to == 0) || (from == 0 && to == 0))
                    {
                        foreach (Int64 forwardRId in forwardEntity.RId)
                        {
                            if (forwardRId == backEntity.Id)
                            {
                                if (from == 1 && to == 0)
                                {
                                    Int64[] temp = { ArgFrom, forwardRId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                                else if (from == 0 && to == 0)
                                {
                                    Int64[] temp = { ArgFrom, forwardEntity.Id, forwardRId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                        }
                    }
                }
                if ((from == 1 && to == 1) || (from == 0 && to == 1))
                {
                    foreach (Entity ridEntity in rid_back1.entities)
                    {
                        if (from == 1 && to == 1)
                        {
                            foreach (AA forwardAa in forwardEntity.Auth)
                            {
                                foreach (AA ridAa in ridEntity.Auth)
                                {
                                    if (forwardAa.AuId == ridAa.AuId)
                                    {
                                        Int64[] temp = { ArgFrom, forwardAa.AuId, ridEntity.Id, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                }
                            }
                            foreach (F forwardF in forwardEntity.Filed)
                            {
                                foreach (F ridF in ridEntity.Filed)
                                {
                                    if (forwardF.FId == ridF.FId)
                                    {
                                        Int64[] temp = { ArgFrom, forwardF.FId, ridEntity.Id, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                }
                            }
                            if (ridEntity.Journal != null && forwardEntity.Journal != null && forwardEntity.Journal.JId == ridEntity.Journal.JId)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Journal.JId, ridEntity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                            if (ridEntity.Conference != null && forwardEntity.Conference != null && forwardEntity.Conference.CId == ridEntity.Conference.CId)
                            {
                                Int64[] temp = { ArgFrom, forwardEntity.Conference.CId, ridEntity.Id, ArgTo };
                                AddArrToResult(temp, result);
                            }
                        }
                        if (from == 0 && to == 1)
                        {
                            foreach (Int64 forwardRId in forwardEntity.RId)
                            {
                                if (forwardRId == ridEntity.Id)
                                {
                                    Int64[] temp = { ArgFrom, forwardEntity.Id, forwardRId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void request2Byforward2(Int64 ArgFrom, JSONBack[] forward2, Int64 ArgTo, int from, int to, ref List<Int64[]> result)
        {
            if (from == 1 && to == 1)
            {
                foreach (JSONBack forwardJsonBack in forward2)
                {
                    if (forwardJsonBack != null)
                    {
                        foreach (Entity forwardEntity in forwardJsonBack.entities)
                        {
                            foreach (Int64 forwardRId in forwardEntity.RId)
                            {
                                if (forwardRId == ArgTo)
                                {
                                    Int64[] temp = { ArgFrom, forwardEntity.Id, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                        }
                    }
                }

            }
        }

        public static void request3Byforward2_back1(Int64 ArgFrom, JSONBack[] forward2, JSONBack back1, JSONBack rid_back1, Int64 ArgTo, int from, int to, ref List<Int64[]> result)
        {
            if ((from == 1 && to == 1) || (from == 1 && to == 0))
            {
                foreach (JSONBack forwardJsonBack in forward2)
                {
                    if (forwardJsonBack != null)
                    {
                        foreach (Entity forwardEntity in forwardJsonBack.entities)
                        {
                            foreach (Entity backEntity in back1.entities)
                            {
                                if ((from == 1 && to == 1) || (from == 1 && to == 0))
                                {
                                    foreach (AA forwardAa in forwardEntity.Auth)
                                    {
                                        foreach (AA backAa in backEntity.Auth)
                                        {
                                            if (from == 1 && to == 1 && forwardAa.AuId == backAa.AuId)
                                            {
                                                Int64[] temp = { ArgFrom, forwardEntity.Id, forwardAa.AuId, ArgTo };
                                                AddArrToResult(temp, result);
                                            }
                                            else if (from == 1 && to == 0 && forwardAa.AfId != -1 && forwardAa.AfId == backAa.AfId && backAa.AuId == ArgTo)
                                            {
                                                Int64[] temp = { ArgFrom, forwardAa.AuId, forwardAa.AfId, ArgTo };
                                                AddArrToResult(temp, result);
                                            }
                                        }
                                    }
                                }
                                if (from == 1 && to == 1)
                                {
                                    foreach (F forwardF in forwardEntity.Filed)
                                    {
                                        foreach (F backF in backEntity.Filed)
                                        {
                                            if (forwardF.FId != -1 && forwardF.FId == backF.FId)
                                            {
                                                Int64[] temp = { ArgFrom, forwardEntity.Id, forwardF.FId, ArgTo };
                                                AddArrToResult(temp, result);
                                            }
                                        }
                                    }
                                    if (backEntity.Journal != null && forwardEntity.Journal != null && forwardEntity.Journal.JId != -1 && forwardEntity.Journal.JId == backEntity.Journal.JId)
                                    {
                                        Int64[] temp = { ArgFrom, forwardEntity.Id, forwardEntity.Journal.JId, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                    if (backEntity.Conference != null && forwardEntity.Conference != null && forwardEntity.Conference.CId != -1 && forwardEntity.Conference.CId == backEntity.Conference.CId)
                                    {
                                        Int64[] temp = { ArgFrom, forwardEntity.Id, forwardEntity.Conference.CId, ArgTo };
                                        AddArrToResult(temp, result);
                                    }
                                }
                                if (from == 1 && to == 0)
                                {
                                    foreach (Int64 forwardRId in forwardEntity.RId)
                                    {
                                        if (forwardRId == backEntity.Id)
                                        {
                                            Int64[] temp = { ArgFrom, forwardEntity.Id, forwardRId, ArgTo };
                                            AddArrToResult(temp, result);
                                        }
                                    }
                                }
                            }
                            if (from == 1 && to == 1)
                            {
                                foreach (Entity backEntity in rid_back1.entities)
                                {
                                    foreach (Int64 forwardRId in forwardEntity.RId)
                                    {
                                        if (forwardRId == backEntity.Id)
                                        {
                                            Int64[] temp = { ArgFrom, forwardEntity.Id, forwardRId, ArgTo };
                                            AddArrToResult(temp, result);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void request3Byforward1_back2(Int64 ArgFrom, JSONBack forward1, JSONBack back2, Int64 ArgTo, int from, int to, ref List<Int64[]> result)
        {
            if (from == 0 && to == 1)
            {
                foreach (Entity forwardEntity in forward1.entities)
                {
                    foreach (Entity backEntity in back2.entities)
                    {
                        foreach (AA forwardAa in forwardEntity.Auth)
                        {
                            foreach (AA backAa in backEntity.Auth)
                            {
                                if (forwardAa.AfId != -1 && forwardAa.AfId == backAa.AfId && forwardAa.AuId == ArgFrom)
                                {
                                    Int64[] temp = { ArgFrom, forwardAa.AfId, backAa.AuId, ArgTo };
                                    AddArrToResult(temp, result);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddArrToResult(Int64[] temp, List<Int64[]> result)
        {
            if (temp.Length == 2 && !result.Any(x => x.Length == 2 && x[0] == temp[0] && x[1] == temp[1]))
                result.Add(temp);
            else if (temp.Length == 3 && !result.Any(x => x.Length == 3 && x[0] == temp[0] && x[1] == temp[1] && x[2] == temp[2]))
                result.Add(temp);
            else if (temp.Length == 4 && !result.Any(x => x.Length == 4 && x[0] == temp[0] && x[1] == temp[1] && x[2] == temp[2] && x[3] == temp[3]))
                result.Add(temp);
        }
    }
}
