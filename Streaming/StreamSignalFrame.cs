namespace Sapl.Demo.Streaming;

/// <summary>
/// An out-of-band SSE frame describing a streaming boundary crossing or a terminal denial.
/// Serialized as {"type":"...","message":"..."} to match the other SAPL streaming demos.
/// </summary>
public sealed record StreamSignalFrame(string Type, string Message);
