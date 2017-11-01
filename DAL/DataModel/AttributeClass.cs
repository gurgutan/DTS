using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DTS.DAL.DataModel
{

    [DataContract(Name = "Attribute", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("Attributes")]
    public class AttributeClass 
    {
        [Key]
        public int Handle { get; set; }

        [DataMember(Order = 0)]
        public string Id = "";  //Идентификатор типа атрибута
        [DataMember(Order = 1, EmitDefaultValue = true)]
        public AttributeDataType DataType { get; set; } //Тип данных значения
        private object data;    //Значение
        [DataMember(Order = 2)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                if (value == null)
                {
                    data = null;
                    return;
                }
                DataType = GetTypeByValue(value);
                Type type = value.GetType();
                if (type == typeof(bool))
                    data = (bool)value;
                else if (type == typeof(DateTime))
                    data = (DateTime)value;
                else if (type == typeof(double))
                    data = (double)value;
                else if (type == typeof(long))
                    data = (long)value;
                else if (type == typeof(string))
                    data = (string)value;
                else if (type == typeof(int))
                    data = (long)Convert.ToInt64(value);
                else
                    throw new ArgumentException("Неправильный тип аргумента: " + type.ToString());
            }
        }

        public string AsString
        {
            get
            {
                if (Data != null) return (string)Data;
                else return "";
            }
            set
            {
                data = value;
            }
        }

        public long AsLong
        {
            get
            {
                if (Data != null) return (long)Data;
                else return 0;
            }
            set
            {
                data = value;
            }
        }

        public double AsDouble
        {
            get
            {
                if (Data != null) return (double)Data;
                else return 0;
            }
            set
            {
                data = value;
            }
        }

        public bool AsBool
        {
            get
            {
                if (Data != null) return (bool)Data;
                else return false;
            }
            set
            {
                data = value;
            }
        }

        public DateTime AsDateTime
        {
            get
            {
                if (Data != null) return (DateTime)Data;
                else return DateTime.MinValue;
            }
            set
            {
                data = value;
            }
        }

        public DocumentClass AsDocument
        {
            get
            {
                if (Data != null) return (DocumentClass)Data;
                else return null;
            }
            set
            {
                data = value;
            }
        }

        public AttributeClass()
        {
        }

        public AttributeClass(string _id, object _value)
        {
            if (_id == "") throw new Exception("Id атрибута не может быть пустым");
            Id = _id;
            data = _value;
        }

        public AttributeClass(object _value)
        {
            data = _value;
        }

        public AttributeClass(AttributeClass attr)
        {
            Id = attr.Id;
            DataType = attr.DataType;
            data = attr.data;
        }

        /// <summary>
        /// Функция возвращает истину, если значение val равно значению Data
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IsValueEqual(object val)
        {
            AttributeDataType valType = GetTypeByValue(val);
            if (valType != DataType) return false;
            switch (DataType)
            {
                case AttributeDataType.Bool: return (bool)val == this.AsBool;
                case AttributeDataType.Double: return (double)val == this.AsDouble;
                case AttributeDataType.DateTime: return (DateTime)val == this.AsDateTime;
                case AttributeDataType.String: return (string)val == this.AsString;
                case AttributeDataType.Long: return (long)val == this.AsLong;
                default: throw new ArgumentException("Неправильный тип параметра:" + typeof(AttributeDataType).ToString());
            }
        }

        private AttributeDataType GetTypeByValue(object _value)
        {
            Type type = _value.GetType();
            if (type == typeof(bool))
                return AttributeDataType.Bool;
            else if (type == typeof(DateTime))
                return AttributeDataType.DateTime;
            else if (type == typeof(double))
                return AttributeDataType.Double;
            else if (type == typeof(long))
                return AttributeDataType.Long;
            else if (type == typeof(string))
                return AttributeDataType.String;
            else if (type == typeof(int))
                return AttributeDataType.Long;
            else
                throw new ArgumentException("Неправильный тип аргумента: " + type.ToString());
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }

    }
}
