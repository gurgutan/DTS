using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DTS.DAL.DataModel
{
    [DataContract(Name = "AttributePattern", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("AttributesPatterns")]

    public class AttributePatternClass
    {
        [Key]
        public int Handle { get; set; }

        [DataMember]
        public string Id = "";
        //[DataMember]
        public AttributeDataType DataType { get; set; }
        //[DataMember]
        public long MinLongValue = long.MinValue;
        //[DataMember]
        public long MaxLongValue = long.MaxValue;
        //[DataMember]
        public double MinDoubleValue = double.MinValue;
        //[DataMember]
        public double MaxDoubleValue = double.MaxValue;
        //[DataMember]
        public string StringPattern = "";   //Пустая строка трактуется как "любая"
        //[DataMember]
        public bool MinBoolValue = false;
        //[DataMember]
        public bool MaxBoolValue = true;
        //[DataMember]
        public DateTime MinDateValue = DateTime.MinValue;
        //[DataMember]
        public DateTime MaxDateValue = DateTime.MaxValue;

        [IgnoreDataMember]
        public string MatchResult = "";

        public AttributePatternClass()
        {
        }

        public AttributePatternClass(string _id = "", AttributeDataType _type = AttributeDataType.String)
        {
            Id = _id;
            DataType = _type;
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }

        public bool IsMatch(AttributeClass attr)
        {
            MatchResult = "";
            if (attr == null)
            {
                throw new Exception("Matching: проверка шаблона " + this.Id + " для null");
            }
            if (attr.Id != "" && this.Id != "" && attr.Id != this.Id)
            {
                return false;
            }
            if (attr.DataType != this.DataType)
            {
                MatchResult = "неверный тип значения " + Id + ": " + attr.DataType.ToString() + "\n";
                return false;
            }
            bool success = false;
            switch (DataType)
            {
                case AttributeDataType.Bool: success = IsValueMatch(attr.AsBool); break;
                case AttributeDataType.Double: success = IsValueMatch(attr.AsDouble); break;
                case AttributeDataType.DateTime: success = IsValueMatch(attr.AsDateTime); break;
                case AttributeDataType.Long: success = IsValueMatch(attr.AsLong); break;
                case AttributeDataType.String: success = IsValueMatch(attr.AsString); break;
                default: throw new ArgumentException("Некорректный тип атрибута для сравнения " + DataType.ToString());
            }
            if (!success) MatchResult = "неверное значение";
            return success;
        }

        public bool IsValueMatch(long val)
        {
            return val <= MaxLongValue && val >= MinLongValue;
        }

        public bool IsValueMatch(double val)
        {
            return val <= MaxDoubleValue && val >= MinDoubleValue;
        }

        public bool IsValueMatch(bool val)
        {
            return val == MinBoolValue || val == MaxBoolValue;
        }

        public bool IsValueMatch(DateTime val)
        {
            return val <= MaxDateValue && val >= MinDateValue;
        }

        public bool IsValueMatch(string val)
        {
            if (StringPattern == "") return true;
            return Regex.IsMatch(val, StringPattern);
        }
    }
}
