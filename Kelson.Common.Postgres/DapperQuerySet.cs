namespace Kelson.Common.Postgres
{
    public readonly struct DapperQuerySet
    {
        public readonly string Table;
        public readonly string WhereId;
        public readonly string FullSelectByIdQuery;
        public readonly string Insert;
        public readonly string Parameters;
        public readonly string FullInsertAndReturnCommand;
        public readonly string Update;
        public readonly string FullUpdateAndReturnCommand;
        public readonly string FullDeleteById;

        public DapperQuerySet(string table, string whereId, string insert, string parameters, string update)
        {
            Table = table;
            WhereId = whereId;
            FullSelectByIdQuery = $"SELECT * FROM {Table} {WhereId};";
            Insert = insert;
            Parameters = parameters;
            FullInsertAndReturnCommand = $"INSERT INTO \"{Table}\" ({Insert}) VALUES ({Parameters}) returning *;";
            Update = update;
            FullUpdateAndReturnCommand = $"UPDATE \"{Table}\" SET {Update} {WhereId}; {FullSelectByIdQuery}";
            FullDeleteById = $"DELETE FROM \"{Table}\" {WhereId};";
        }
    }
}
