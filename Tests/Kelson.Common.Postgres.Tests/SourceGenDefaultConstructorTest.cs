using Kelson.Common.Postgres.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Kelson.Common.Postgres.Tests
{
    public class SourceGenDefaultConstructorTest
    {
        [Fact]
        public void SyntaxMatcherMatchesOnPartialRecordThatExtendsIPostgresEntity()
        {
            Compilation inputCompilation = CreateCompilation(SAMPLE_RECORD_ENTITY_DEF_SOURCE);

            var generator = new PostgresGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            var symbol = (ITypeSymbol)outputCompilation.GetSymbolsWithName(s => s.Contains("LoggedMessage")).Single();
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(SourceGenDefaultConstructorTest).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        const string SAMPLE_RECORD_ENTITY_DEF_SOURCE = @"
using BleepBot.Common;
using Kelson.Common.Postgres;
using System;

namespace Test.Interface
{
    public partial record LoggedMessage(
        long Pk,
        Snowflake ServerId,
        Snowflake ChannelId,
        string ChannelName,
        Snowflake MessageId,
        Snowflake UserId,
        string Username,
        string? Nickname,
        DateTimeOffset MessageDate,
        DateTimeOffset? RevisionDate,
        long RevisionCount,
        string MessageContent) : IPostgresEntity<LoggedMessage, long>;

    public partial record MonitoredUser(
        Snowflake ServerId,
        Snowflake UserId,
        string Username,
        DateTimeOffset StartDate,
        DateTimeOffset? EndDate,
        string Reason) : IPostgresEntity<MonitoredUser, (Snowflake serverId, Snowflake userId)>;

    public partial record RoomMessage(
        Snowflake MessageId,
        Snowflake ServerId,
        long PrivateRoomPk,
        Snowflake AuthorId,
        Snowflake? ReplyMessageId,
        byte[] ContentUtf8)
        : IPostgresEntity<RoomMessage, Snowflake>;

    public partial class MonitoredUserRepo : PostgresqlCrudRepository<MonitoredUser, (Snowflake serverId, Snowflake userId)>
    {
    }
}
";
    }
}
