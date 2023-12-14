using DBMongoDDL.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BlikonDAO.Models;

namespace DBMongoDDL
{
    public class Administrador
    {
        private MongoClient _client;
        private IMongoCollection<Item> _items;
        private DBSettings _dbSettings;
        public DBSettings Settings { get { return _dbSettings; } }
        public Administrador(DBSettings dBSettings)
        {
            _dbSettings = dBSettings;
            _client = new MongoClient(dBSettings.Server);
            var database = _client.GetDatabase(dBSettings.Database);

            var collectionExists = database.ListCollectionNames().ToList().Contains(dBSettings.Collection);
            if (collectionExists == false)
            {
                database.CreateCollection(dBSettings.Collection);
            }
            _items = database.GetCollection<Item>(dBSettings.Collection);

        }
       

        public async Task<Response> CreateItem(Item item)
        {
            Response oRespuesta = new();
            try
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                string json= JsonSerializer.Serialize(item.Fields, options);
                item.Fields = BsonSerializer.Deserialize<object>(item.Fields.ToString());
                await _items.InsertOneAsync(item);
                oRespuesta.Mensaje = "Se realizo el registro correctamente.";
                oRespuesta.Exito = 1;
            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: " + e.Message;
            }
            return oRespuesta;
        }

        public async Task<Response> UpdateItem(Item item)
        {
            Response oRespuesta = new();
            try
            {
                var filter = Builders<Item>
                 .Filter
                 .Eq(s => s.Id, item.Id);
                item.Fields = BsonSerializer.Deserialize<object>(item.Fields.ToString());
                await _items.ReplaceOneAsync(filter, item);
                oRespuesta.Mensaje = "Registro actualizado correctamente.";
                oRespuesta.Exito = 1;
            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: "+e.Message;
            }
            return oRespuesta;
        }

        public async Task<Response> GetItem(string Id)
        {
            Response oRespuesta = new();
            try
            {
                var filter = Builders < Item >
                 .Filter
                 .Eq(s => s.Id, Id);
                Item Item = await _items.FindAsync(filter).Result.FirstOrDefaultAsync();
                oRespuesta.Data = Item;
                oRespuesta.Exito = 1;
            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: " + e.Message;
            }
            return oRespuesta;
        }

        public async Task<Response> GetAllItem()
        {
            Response oRespuesta = new();
            try
            {
                var filter = Builders<Item>.Filter.Empty;
                List<Item> ItemLst = await _items.FindAsync(filter).Result.ToListAsync();
                oRespuesta.Data = ItemLst;
                oRespuesta.Exito = 1;
            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: " + e.Message;
            }
            return oRespuesta;
        }

        //public async Task<Response> QueryGetAllAppItems(QueryOperators model)
        //{
        //    Response oRespuesta = new();
        //    try
        //    {
        //        var database = _client.GetDatabase(_dbSettings.Database);
        //        var collection = database.GetCollection<Item>(_dbSettings.Collection);

        //        var builder = Builders<Item>.Filter;
        //        List<FilterDefinition<Item>> lstfilter = new();
        //        FilterDefinition<Item> filter = null;
        //        foreach (var item in model.lstFieldsQuery)
        //        {
        //            if (item.operador == "Eq")
        //            {
        //                lstfilter.Add(builder.Eq(item.fieldName, item.fieldValue));
        //            }
        //            else if (item.operador == "And")
        //            {
        //                lstfilter.Add(builder.And(item.fieldName, item.fieldValue));
        //            }
        //            else if (item.operador == "Or")
        //            {
        //                lstfilter.Add(builder.Or(item.fieldName, item.fieldValue));
        //            }
        //            else if (item.operador == "In")
        //            {
        //                lstfilter.Add(builder.In(item.fieldName, item.fieldValue));
        //            }
        //            else if (item.operador == "lt")
        //            {
        //                lstfilter.Add(builder.Lt(item.fieldName, item.fieldValue));
        //            }
        //        }

        //        if (model.LogicOperator == "And")
        //        {
        //            filter = builder.And(lstfilter);
        //        }
        //        else if (model.LogicOperator == "Or")
        //        {
        //            filter = builder.Or(lstfilter);
        //        }
        //        else if (model.LogicOperator == "")
        //        {
        //            filter = lstfilter.FirstOrDefault();
        //        }


        //        var result = await collection.FindAsync(filter);
        //        oRespuesta.Data = result;
        //        oRespuesta.Exito = 1;
        //    }
        //    catch(Exception e)
        //    {
        //        oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: " + e.Message;
        //    }
            
        //    return oRespuesta;
        //}

        public Response GetQueryItems(QueryOperators model)
        {
            Response oRespuesta = new();
            try
            {
                string jsonQuery = string.Empty;
                string expression = string.Empty;
                Char trimChar = ',';
                foreach (var items in model.lstFieldsQuery)
                {
                    if (items.LogicOperator != "$not" && (!string.IsNullOrEmpty(items.LogicOperator) && items.Fields.Count() < 2))
                    {
                        throw new ArgumentException("si incluye un operador lógico debe contener más de dos expresiones. ");
                    }
                    expression = string.Empty;
                    string valor = string.Empty;
                    foreach (var item in items.Fields)
                    {

                        if (item.operador == "$in" || item.operador == "$nin")
                        {
                            string ArrayFields = string.Empty;
                            ArrayFields = String.Join("','", item.ArrayFieldsValue);
                            ArrayFields = ArrayFields.TrimEnd(trimChar);
                            valor = String.Format("{{ {0} : ['{1}'] }}", item.operador, ArrayFields);
                        }
                        else if (item.operador == "$regex")
                        {
                            valor = String.Format("/^{0}.*/i", item.fieldValue);
                        }
                        else
                        {
                            valor = String.Format("{{ {0} : '{1}' }}", item.operador, item.fieldValue);
                        }
                        if (items.LogicOperator == "$not")
                        {
                            valor = String.Format("{{ $not: {0} }}", valor);
                        }
                        expression += String.Format("{{ '{0}' : {1} }},", item.fieldName, valor);
                    }
                    expression = expression.TrimEnd(trimChar);
                    if (string.IsNullOrEmpty(items.LogicOperator) || items.LogicOperator == "$not")
                    {
                        jsonQuery += expression + ",";
                    }
                    else
                    {
                        jsonQuery += String.Format("{{ {0}: [ {1} ] }},", items.LogicOperator, expression);
                    }

                }

                jsonQuery = jsonQuery.TrimEnd(trimChar);
                if (!string.IsNullOrEmpty(model.LogicOperator))
                {
                    jsonQuery = String.Format("{{ {0} : [ {1} ] }}", model.LogicOperator, jsonQuery);
                }

                BsonDocument queryDoc = BsonSerializer.Deserialize<BsonDocument>(jsonQuery);

                string sort = string.Empty;
                if (model.Sort != null)
                {
                    foreach (var s in model.Sort)
                    {
                        sort += String.Format("'{0}':{1},", s.Field, s.Value);
                    }
                    sort = "{" + sort.TrimEnd(trimChar) + "}";
                }
                else
                {
                    sort = "{'_id': -1}";
                }

                oRespuesta.Data= _items.Find(queryDoc).Sort(sort).ToList();
                oRespuesta.Exito = 1;

            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: " + e.Message;
            }
            return oRespuesta;
        }

        public Response DeleteItem(string Id)
        {
            Response oRespuesta = new();
            try
            {
                _items.DeleteOne(x => x.Id == Id);
                oRespuesta.Mensaje = "Registro eliminado correctamente.";
                oRespuesta.Exito = 1;
            }
            catch (Exception e)
            {
                oRespuesta.Mensaje = "Ocurrió un error al procesar su solicitud: "+ e.Message;
            }
            return oRespuesta;
        }
    }
}