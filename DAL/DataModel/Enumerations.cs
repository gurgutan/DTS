using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DTS.DAL.DataModel
{
    [DataContract(Name = "ComplectLocation", Namespace = "http://www.gpp.docker.ru")]
    public enum ComplectLocation
    {
        [EnumMember]
        FileSystem = 1,
        [EnumMember]
        Database = 2
    }

    [DataContract(Name = "AttributeDataType", Namespace = "http://www.gpp.docker.ru")]
    public enum AttributeDataType
    {
        [EnumMember(Value="String")]
        String = 0,
        [EnumMember(Value = "Long")]
        Long = 2,
        [EnumMember(Value = "Double")]
        Double = 3,
        [EnumMember(Value = "Bool")]
        Bool = 4,
        [EnumMember(Value = "DateTime")]
        DateTime = 5
    }

    [DataContract(Name = "CompareMode", Namespace = "http://www.gpp.docker.ru")]
    public enum CompareMode
    {
        [EnumMember]
        ExistsInDocument = 1,
        [EnumMember]
        ExistsInPattern = 2,
        [EnumMember]
        Equality = 3
    }

}
