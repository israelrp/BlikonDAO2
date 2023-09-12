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
        public Administrador(DBSettings dBSettings)
        {
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

        public async Task<Response> QueryGetAllAppItems(QueryOperators model)
        {
            Response oRespuesta = new();
            try
            {
                var database = _client.GetDatabase("prueba");
                var collection = database.GetCollection<Item>("Items");

                var builder = Builders<Item>.Filter;
                List<FilterDefinition<Item>> lstfilter = new();
                FilterDefinition<Item> filter = null;
                foreach (var item in model.lstFieldsQuery)
                {
                    if (item.operador == "Eq")
                    {
                        lstfilter.Add(builder.Eq(item.fieldName, item.fieldValue));
                    }
                    else if (item.operador == "And")
                    {
                        lstfilter.Add(builder.And(item.fieldName, item.fieldValue));
                    }
                    else if (item.operador == "Or")
                    {
                        lstfilter.Add(builder.Or(item.fieldName, item.fieldValue));
                    }
                    else if (item.operador == "In")
                    {
                        lstfilter.Add(builder.In(item.fieldName, item.fieldValue));
                    }
                    else if (item.operador == "lt")
                    {
                        lstfilter.Add(builder.Lt(item.fieldName, item.fieldValue));
                    }
                }

                if (model.LogicOperator == "And")
                {
                    filter = builder.And(lstfilter);
                }
                else if (model.LogicOperator == "Or")
                {
                    filter = builder.Or(lstfilter);
                }
                else if (model.LogicOperator == "")
                {
                    filter = lstfilter.FirstOrDefault();
                }


                var result = await collection.FindAsync(filter);
                oRespuesta.Data = result;
                oRespuesta.Exito = 1;
            }
            catch(Exception e)
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