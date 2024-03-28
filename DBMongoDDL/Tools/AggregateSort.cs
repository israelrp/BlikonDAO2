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
    public class AggregateSort
    {
        public BsonDocument outPutQuery(ItemFunction query)
        {
            string sort = string.Empty;
            foreach (var s in query.Expressions)
            {
                sort += String.Format("'{0}':{1},", s.FieldName, s.FieldValue);
            }
            sort = "{$sort: {" + sort.TrimEnd(',') + "}}";
            BsonDocument querypipeline = BsonSerializer.Deserialize<BsonDocument>(sort);
            return querypipeline;
        }
    }
}
