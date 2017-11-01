using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;


namespace DTS.DAL.DataModel
{
    [DataContract(Name = "MatchResult", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("MatchResults")]    
    public class MatchResult
    {
        [Key]
        public int Handle { get; set; }
        public DocumentPatternClass Pattern;
        public DocumentClass Document;

        [DataMember(Order = 1)]
        public bool Success;

        [DataMember(Order = 2)]
        public string PatternName;

        [DataMember(Order = 3)]
        public string DocumentName;

        private string text;
        [DataMember(Order = 4)]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        [DataMember(Order = 5)]
        public List<MatchResult> Childs;

        public MatchResult()
        {
            Success = false;
            Text = "";
        }

        public MatchResult(DocumentPatternClass _pattern, DocumentClass _document, string _text = "")
        {
            Pattern = _pattern;
            Document = _document;
            PatternName = Pattern.Name;
            DocumentName = Document.Name;
            Text = _text;
            Success = false;
            Childs = new List<MatchResult>();
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }
    }
}
