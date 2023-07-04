using DBMongoDDL.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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