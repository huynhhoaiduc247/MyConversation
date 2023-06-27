using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Repository.Helper
{
    public static class PagingExtensions
    {
        /// <summary>
        /// Pages a LINQ query to return just the subset of rows from the database. Use as follows:
        /// 
        /// var query = from s in _context.Table
        ///             orderby s.Id ascending
        ///             select s;
        ///
        /// return _myRepository.Find(query, page, pageSize).ToList();
        /// </summary>
        /// <typeparam name="TSource">Entity</typeparam>
        /// <param name="source">LINQ query</param>
        /// <param name="page">Page Index</param>
        /// <param name="pageSize">Number of Rows</param>
        /// <returns>IQueryable</returns>
        public static IQueryable<TSource> Page<TSource>(IQueryable<TSource> source, int? page, int? pageSize, Sort<TSource>? sort=null)
        {
            IQueryable<TSource> query = source;
            if (sort != null)
            {
                if (sort.IsAscent)
                {
                    query = query.OrderBy(sort.expression);
                }
                else
                {
                    query = query.OrderByDescending(sort.expression);
                }
            }

            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }
    }
}
