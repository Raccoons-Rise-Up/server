// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused", Scope = "member", Target = "~M:GameServer.Program.Main(System.String[])")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Code used in client and uses older .NET", Scope = "member", Target = "~M:Common.Utils.SharedUtils.GetEnumList``1~System.Collections.Generic.List{``0}")]
