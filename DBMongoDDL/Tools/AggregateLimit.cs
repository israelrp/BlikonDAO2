using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlikonDAO.Models;

namespace BlikonDAO.Tools
{
    public class AggregateLimit
    {
        public BsonDocument outPutQuery(ItemFunction query)
        {
            BsonDocument querypipeline = BsonSerializer.Deserialize<BsonDocument>("{ $limit : " + query.Expressions[0].FieldValue + " }");
            return querypipeline;
        }
    }
}
