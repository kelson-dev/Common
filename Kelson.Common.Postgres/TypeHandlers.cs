using Dapper;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace Kelson.Common.Postgres
{
    public class BigSerialTypeHandler : SqlMapper.TypeHandler<BigSerial>
    {
        [ModuleInitializer]
        internal static void Register() => SqlMapper.AddTypeHandler(new BigSerialTypeHandler());

        public override BigSerial Parse(object value) => new((long)value);

        public override void SetValue(IDbDataParameter parameter, BigSerial value) => parameter.Value = (long)value;
    }

    public class UpdatedTypeHandler : SqlMapper.TypeHandler<Updated>
    {
        [ModuleInitializer]
        internal static void Register() => SqlMapper.AddTypeHandler(new UpdatedTypeHandler());

        public override Updated Parse(object value) => new((DateTimeOffset)value);

        public override void SetValue(IDbDataParameter parameter, Updated value) => parameter.Value = (DateTimeOffset)value;
    }

    public class CreatedTypeHandler : SqlMapper.TypeHandler<Created>
    {
        [ModuleInitializer]
        internal static void Register() => SqlMapper.AddTypeHandler(new CreatedTypeHandler());

        public override Created Parse(object value) => new((DateTimeOffset)value);

        public override void SetValue(IDbDataParameter parameter, Created value) => parameter.Value = (DateTimeOffset)value;
    }
}
