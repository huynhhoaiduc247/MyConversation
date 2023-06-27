using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Repository.Interface
{
    public interface IRepository<T> : IDisposable
    {
        Response<T> Add(T item);
        Response<T> Add(IEnumerable<T> items);
        Response<T> UpdateOne(Expression<Func<T, bool>> expression, T item, string? updateField = null);
        Response<T> UpdateOne(T item, string? updateField = null);
        Response<T> UpdateMany(Expression<Func<T, bool>> expression, T item, string? updateField = null);
        Response<T> Delete(Expression<Func<T, bool>> expression);
        Response<T> Delete(T item);
        Response<T> Delete(List<T> items);
        Response<T> DeleteAll();
        //T Single<T>(Expression<Func<T, bool>> expression) where T : class, new();
        //System.Linq.IQueryable<T> All<T>() where T : class, new();
        //System.Linq.IQueryable<T> All<T>(int page, int pageSize, Expression<Func<T, bool>> expression) where T : class, new();
        Response<T> Single(Expression<Func<T, bool>> expression);
        Response<System.Linq.IQueryable<T>> All();
        Response<IEnumerable<T>> All(int? page, int? pageSize, Expression<Func<T, bool>> expression, Sort<T>? sort=null);

    }
}
