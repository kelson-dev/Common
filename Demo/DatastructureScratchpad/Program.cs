using Kelson.Common.DataStructures.Sets;
using System.Collections.Immutable;
using static System.Console;

var set = ImmutableSortedSet<int>.Empty;
set = set.Add(1).Add(1).Add(1).Add(1);
WriteLine(set.Min);
WriteLine(set.Count);