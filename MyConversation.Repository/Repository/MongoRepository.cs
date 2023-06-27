using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MyConversation.Model.SystemModel;
using MyConversation.Repository.Helper;
using MyConversation.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Repository.Repository
{
    public class MongoRepository<T> : IRepository<T>
    {
        private string dbName;
        public MongoRepository(string _dbName)
        {
            dbName = _dbName;
        }
        #region create
        public Response<T> Add(T item)
        {
            Response<T> response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).InsertOne(item);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Response<T> Add(IEnumerable<T> items)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).InsertMany(items);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        #endregion

        #region update
        public Response<T> UpdateOne(T item, string? updateField = null)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                var updateFieldList = updateField == null ? new List<string>() : updateField.Split(";").ToList();
                BsonDocument bsonItem = BsonSerializer.Deserialize<BsonDocument>(item.ToJson());
                string _id = bsonItem.GetValue("_id", string.Empty).ToString();
                bsonItem.Remove("_id");
                UpdateDefinition<T> updateStatement = null;
                foreach (var field in bsonItem.Where(x => updateFieldList.Count == 0 || updateFieldList.Contains(x.Name)).ToList())
                {
                    if (updateStatement == null)
                    {
                        var val = field.Value.IsBsonNull ? null : field.Value;
                        updateStatement = Builders<T>.Update.Set(field.Name, val);
                    }
                    else
                    {
                        var val = field.Value.IsBsonNull ? null : field.Value;
                        updateStatement = updateStatement.Set(field.Name, val);
                    }
                }
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).UpdateOne(Builders<T>.Filter.Eq("_id", _id), updateStatement);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public Response<T> UpdateOne(System.Linq.Expressions.Expression<Func<T, bool>> expression, T item, string? updateField=null)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                var updateFieldList = updateField == null ? new List<string>() : updateField.Split(";").ToList();
                BsonDocument bsonItem = BsonSerializer.Deserialize<BsonDocument>(item.ToJson());
                bsonItem.Remove("_id");
                UpdateDefinition<T> updateStatement = null;
                foreach (var field in bsonItem.Where(x => updateFieldList.Count == 0 || updateFieldList.Contains(x.Name)).ToList())
                {
                    if (updateStatement == null)
                    {
                        updateStatement = Builders<T>.Update.Set(field.Name, field.Value);
                    }
                    else
                    {
                        updateStatement = updateStatement.Set(field.Name, field.Value);
                    }
                }
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).UpdateOne(expression, updateStatement);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public Response<T> UpdateMany(System.Linq.Expressions.Expression<Func<T, bool>> expression, T item, string? updateField = null)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                var updateFieldList = updateField == null ? new List<string>() : updateField.Split(";").ToList();
                BsonDocument bsonItem = BsonSerializer.Deserialize<BsonDocument>(item.ToJson());
                bsonItem.Remove("_id");
                UpdateDefinition<T> updateStatement = null;
                foreach (var field in bsonItem.Where(x => updateFieldList.Count == 0 || updateFieldList.Contains(x.Name)).ToList())
                {
                    if (updateStatement == null)
                    {
                        updateStatement = Builders<T>.Update.Set(field.Name, field.Value);
                    }
                    else
                    {
                        updateStatement = updateStatement.Set(field.Name, field.Value);
                    }
                }
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).UpdateMany(expression, updateStatement);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        #endregion

        #region delete
        public Response<T> Delete(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                // Remove the object.
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).DeleteMany(expression);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public Response<T> Delete(T item)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                // Remove the object.
                BsonDocument bsonItem = BsonSerializer.Deserialize<BsonDocument>(item.ToJson());
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).DeleteOne(Builders<T>.Filter.Eq("_id", bsonItem.GetValue("_id", string.Empty).ToString()));
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public Response<T> Delete(List<T> items)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                // Remove the object.
                List<BsonDocument> bsonItems = BsonSerializer.Deserialize<List<BsonDocument>>(items.ToJson());
                DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).DeleteMany(Builders<T>.Filter.Eq("_id", bsonItems.Select(x => x["_id"].ToString())));
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Response<T> DeleteAll()
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                DbContext.Instance.GetDB(dbName).DropCollection(typeof(T).Name);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        #endregion

        #region get
        public Response<T> Single(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            var response = new Response<T>()
            {
                IsSuccess = true,
            };
            try
            {
                response.Data = All().Data.Where(expression).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Response<IQueryable<T>> All()
        {
            var response = new Response<IQueryable<T>>()
            {
                IsSuccess = true,
            };
            try
            {
                response.Data = DbContext.Instance.GetDB(dbName).GetCollection<T>(typeof(T).Name).AsQueryable();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Response<IEnumerable<T>> All(int? page, int? pageSize, System.Linq.Expressions.Expression<Func<T, bool>> expression, Sort<T>? sort=null)
        {
            var response = new Response<IEnumerable<T>>()
            {
                IsSuccess = true,
            };
            try
            {
                response.Data = PagingExtensions.Page(All().Data.Where(expression), page, pageSize, sort).ToList();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        #endregion

        #region dispose
        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
