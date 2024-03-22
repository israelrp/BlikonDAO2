using BlikonDAO.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Tools
{
    public class AggregateGroupCount
    {
        public BsonDocument outPutQuery(List<ItemFunction> query)
        {
            string campos = string.Empty;
            foreach (var item in query[0].Expressions)
            {
                campos += "\"" + item.FieldName + "\" : \"$fields." + item.FieldName + "\",";
            }
            BsonDocument querypipeline = BsonSerializer.Deserialize<BsonDocument>("{ \"$group\" : { \"_id\" : { " + campos.TrimEnd(',') + " }, " + query[0].NewFieldName + " : { \"$count\" : { } } } }");
            return querypipeline;
        }
    }
}
