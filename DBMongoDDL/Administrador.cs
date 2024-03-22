using DBMongoDDL.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BlikonDAO.Models;
using BlikonDAO.Tools;
using System.Dynamic;
using System.Reflection;

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

        public Response GetQueryItems(QueryOperators model)
        {
            Response oRespuesta = new();
            try
            {
                string jsonQuery = string.Empty;
                string expression = string.Empty;
                Char trimChar = ',';
                SetTypes oSetType = new();
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
                        item.fieldValue = oSetType.SetType(item.fieldType, item.fieldValue);

                        if (item.operador == "$in" || item.operador == "$nin")
                        {
                            valor = oSetType.ArraySetType(item.fieldType, item.ArrayFieldsValue, item.operador);
                        }
                        else if (item.operador == "$regex")
                        {
                            valor = String.Format("/^{0}.*/i", item.fieldValue);
                        }
                        else
                        {
                            valor = String.Format("{{ {0} : {1}  }}", item.operador, item.fieldValue);
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

                string pipeProject = string.Empty;
                string pipe = string.Empty;
                string fields = string.Empty;
                List<string> nombres = new();

                var queryDoc1 = BsonSerializer.Deserialize<BsonDocument>("{ \"$match\" : " + jsonQuery + " }}");

                BsonDocument[] pipeline = new BsonDocument[] { queryDoc1 };

                if (model.Funcion != null)
                {
                    var project = model.Funcion.Where(x => x.FunctionName.Equals("project")).FirstOrDefault();

                    foreach (var func in model.Funcion)
                    {
                        fields = string.Empty;
                        switch (func.FunctionName)
                        {
                            case "sum":
                                nombres.Add(func.NewFieldName);
                                foreach (var exp in func.Expressions)
                                {
                                    fields += "\"$fields." + exp.FieldName + "\",";
                                }
                                fields = fields.TrimEnd(trimChar);
                                pipe += "\"" + func.NewFieldName + "\": { \"$sum\": [ " + fields + " ] },";
                                break;
                            case "concat":
                                nombres.Add(func.NewFieldName);
                                foreach (var exp in func.Expressions)
                                {
                                    fields += "\"" + exp.Concat + "\",\"$fields." + exp.FieldName + "\",";
                                }
                                fields = fields.TrimEnd(trimChar);
                                pipe += func.NewFieldName + ": { \"$concat\": [ " + fields + " ] },";
                                break;
                            case "dateDiff":
                                nombres.Add(func.NewFieldName);
                                pipe += "\"" + func.NewFieldName + "\": { $dateDiff: { startDate: \"$fields." + func.Expressions[0].FieldName + "\", endDate: \"$fields." + func.Expressions[1].FieldName + "\", unit: \"" + func.Unit + "\" }},";
                                break;
                            case "GroupCount":
                                break;
                        }
                    }

                    if (project != null)
                    {
                        pipe = pipe.TrimEnd(trimChar);
                        fields = string.Empty;
                        foreach (var exp in project.Expressions)
                        {
                            fields += " \"fields." + exp.FieldName + "\":" + exp.FieldValue + ",";
                        }
                        fields = fields.TrimEnd(trimChar);
                        pipeProject = "{ \"$project\": {" + fields + " ," + pipe + " } }";
                        pipeProject = pipeProject.TrimEnd(trimChar);
                        BsonDocument querypipeline = BsonSerializer.Deserialize<BsonDocument>(pipeProject);
                        pipeline = pipeline.Append(querypipeline).ToArray();
                    }
                    else
                    {
                        pipeProject = "{ \"$project\": { } }";
                        nombres.Add(model.Funcion[0].NewFieldName);
                        string nombreClase = "WebApplication1.Tools.Aggregate" + model.Funcion[0].FunctionName;
                        Type type = Type.GetType(nombreClase);
                        object instance = Activator.CreateInstance(type);
                        MethodInfo method = type.GetMethod("outPutQuery");
                        object[] parametersArray = new object[] { model.Funcion };
                        var res = method.Invoke(instance, parametersArray);
                        pipeline = pipeline.Append(res.ToBsonDocument()).ToArray();
                    }

                }
                BsonDocument pipelineSort = BsonSerializer.Deserialize<BsonDocument>(sort);
                pipeline = pipeline.Append(pipelineSort).ToArray();
                List<BsonDocument> data = _items.Aggregate<BsonDocument>(pipeline).ToList();
                List<Item> Listitem = new();
                foreach (var doc in data)
                {
                    Item item = new();
                    item.Id = doc.GetValue("_id", null).ToString();
                    item.Fields = new();
                    var p = doc.GetValue("fields", null).ToBsonDocument();
                    if (p == null)
                    {
                        p = new BsonDocument();
                    }
                    foreach (var field in nombres)
                    {
                        p.Add(doc.GetElement(field));
                    }
                    var pru1 = p.ToJson();
                    item.Fields = BsonSerializer.Deserialize<ExpandoObject>(p);
                    Listitem.Add(item);
                }
                oRespuesta.Data = Listitem;
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