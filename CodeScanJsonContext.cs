using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeScan;

[JsonSourceGenerationOptions(
	WriteIndented = true,
	UseStringEnumConverter = true,
	PropertyNameCaseInsensitive = true,
	AllowTrailingCommas = true,
	ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(SandboxConfig))]
internal partial class CodeScanJsonContext : JsonSerializerContext
{
	public static CodeScanJsonContext CreateRelaxedEscapingContext()
	{
		var options = new JsonSerializerOptions(Default.Options)
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			TypeInfoResolver = null
		};
		return new CodeScanJsonContext(options);
	}
}
