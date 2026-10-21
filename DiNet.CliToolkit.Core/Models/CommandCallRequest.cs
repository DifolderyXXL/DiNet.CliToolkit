using DiNet.CliToolkit.Core.Models;
namespace DiNet.CliToolkit.Core.Models;

public record class CommandCallRequest(string? FirstUnflaggedArgument, ConsoleParameter[] Parameters);
