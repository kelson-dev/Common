using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelson.Common.Postgres.Generators.Demo
{
    public partial record ItemCount(BigSerial Id, string Name, int Count)
        : IPostgresEntity<ItemCount, BigSerial>;
}
