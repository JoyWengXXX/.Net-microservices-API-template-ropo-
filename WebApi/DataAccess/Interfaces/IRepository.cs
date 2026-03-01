using System.Data;
using System.Linq.Expressions;
using System.Transactions;

namespace DataAccess.Interfaces
{
    /// <summary>
    /// Dapper建立連線
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IRepository<T1> where T1 : IProjectDBConnectionManager
    {
        public IDbConnection GetConnection();

        /// <summary>
        /// 取回資料清單
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="predicate"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<IEnumerable<T2>?> GetListAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork unitOfWork = null) where T2 : class;

        /// <summary>
        /// 取回第一筆資料
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="predicate"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<T2?> GetFirstAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork? unitOfWork = null) where T2 : class;

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <param name="input"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<int> CreateAsync<T2>(T2 input, IUnitOfWork unitOfWork = null) where T2 : class;

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="input">更新資料</param>
        /// <param name="predicate">WHERE條件</param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<int> UpdateAsync<T2>(Expression<Func<T2, bool>> input, Expression<Func<T2, bool>> predicate, IUnitOfWork unitOfWork = null) where T2 : class;

        /// <summary>
        /// 刪除資料
        /// </summary>
        /// <param name="predicate">WHERE條件</param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<int> DeleteAsync<T2>(Expression<Func<T2, bool>> predicate, IUnitOfWork unitOfWork = null) where T2 : class;

        /// <summary>
        /// 複雜查詢
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="sqlStr">SQL查詢語法</param>
        /// <param name="predicate">輸入參數</param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<IEnumerable<T2>> ComplexQueryAsync<T2>(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null);

        /// <summary>
        /// 執行複雜操作
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="predicate">SQL操作語法</param>
        /// <param name="predicate">輸入參數</param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public Task<int> ComplexCommandAsync(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null);

        IUnitOfWork CreateUnitOfWork();
    }
}

