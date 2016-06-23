using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

/*
Id		Entity ID				Int64	Equals
Y		JSONBack year				Int32	Equals,IsBetween
D		JSONBack date				Date	Equals,IsBetween
CC		Citation count			Int32	none
AA.AuId	Author ID				Int64	Equals
AA.AfId	Author affiliation ID	Int64	Equals
F.FId	Field of study ID		Int64	Equals
J.JId	Journal ID				Int64	Equals
C.CId	Conference series ID	Int64	Equals
*/

namespace OxfordAcademic
{
    public class JSONBack
    {
        [JsonProperty("expr")]
        public string expr { get; set; }

        [JsonProperty("entities")]
        public Entity[] entities = new Entity[0];
    }

    //Entity means paper
    public class Entity
    {
        [JsonProperty("logprob")]
        public float logprob { get; set; }

        private Int64 _Id = -1;
        [JsonProperty("Id")]
        public Int64 Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [JsonProperty("AA")]
        public AA[] Auth = new AA[0];

        [JsonProperty("F")]
        public F[] Filed = new F[0];

        [JsonProperty("C")]
        public C Conference { get; set; }

        [JsonProperty("J")]
        public J Journal { get; set; }

        [JsonProperty("RId")]
        public Int64[] RId = new Int64[0];
    }

    public class AA
    {
        private Int64 _AuId = -1;

        [JsonProperty("AuId")]
        public Int64 AuId
        {
            get { return _AuId; }
            set { _AuId = value; }
        }
        private Int64 _AfId = -1;

        [JsonProperty("AfId")]
        public Int64 AfId
        {
            get { return _AfId; }
            set { _AfId = value; }
        }
    }


    public class F
    {
        private Int64 _FId = -1;

        [JsonProperty("FId")]
        public Int64 FId
        {
            get { return _FId; }
            set { _FId = value; }
        }
    }

    public class J
    {
        private Int64 _JId = -1;

        [JsonProperty("JId")]
        public Int64 JId
        {
            get { return _JId; }
            set { _JId = value; }
        }
    }

    public class C
    {
        private Int64 _CId = -1;

        [JsonProperty("CId")]
        public Int64 CId
        {
            get { return _CId; }
            set { _CId = value; }
        }
    }
}
