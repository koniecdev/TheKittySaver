using System.Linq.Expressions;
using TheKittySaver.AdoptionSystem.API.QueriesSorting;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

internal static class IEnumerableExtensions
{
    extension<TAggregate>(IEnumerable<TAggregate> query)
    {
        public IEnumerable<TAggregate> ApplyInMemoryPagination(int page, int pageSize)
        {
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            
            return query;
        }
    }
}

internal static class IQueryableExtensions
{
    extension<TAggregate>(IQueryable<TAggregate> query)
    {
        public IQueryable<TAggregate> WhereIf(
            bool condition,
            Expression<Func<TAggregate, bool>> predicate)
        {
            query = condition ? query.Where(predicate) : query;
            return query;
        }
        
        public IQueryable<TAggregate> ApplyPagination(int page, int pageSize)
        {
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            
            return query;
        }
        
        public IQueryable<TAggregate> ApplyMultipleSorting(
            string sort,
            Func<string, Expression<Func<TAggregate, object>>> propertySelector)
        {
            if (string.IsNullOrEmpty(sort))
            {
                return query;
            }

            List<SortItem> sortItems = SortParser.Parse(sort).ToList();

            for (int i = 0; i < sortItems.Count; i++)
            {
                SortItem sortItem = sortItems[i];
                Expression<Func<TAggregate, object>> sortExpression = propertySelector(sortItem.PropertyName);
        
                if (i == 0)
                {
                    query = query.ApplySorting(
                        sortExpression,
                        sortItem.Operand is SortOperand.Asc);      
                    continue;
                }
                
                query = query.ApplyThenSorting(
                    sortExpression,
                    sortItem.Operand is SortOperand.Asc);
            }
    
            return query;
        }
        
        private IQueryable<TAggregate> ApplySorting(
            Expression<Func<TAggregate, object>>? orderByExpression,
            bool ascending)
        {
            if (orderByExpression is null)
            {
                return query;
            }

            return ascending
                ? query.OrderBy(orderByExpression)
                : query.OrderByDescending(orderByExpression);
        }
    
        private IQueryable<TAggregate> ApplyThenSorting(
            Expression<Func<TAggregate, object>>? orderByExpression,
            bool ascending)
        {
            if (orderByExpression is null || query is not IOrderedQueryable<TAggregate> orderedQueryable)
            {
                return query;
            }

            return ascending
                ? orderedQueryable.ThenBy(orderByExpression)
                : orderedQueryable.ThenByDescending(orderByExpression);
        }
    }
}
