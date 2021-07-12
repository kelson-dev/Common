using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelson.Common.Postgres.Generators.Demo
{
    public partial class ItemCountRepo : PostgresqlCrudRepository<ItemCount, BigSerial>
    {
    }
}
