namespace CodeScan;

public sealed class WhitelistFieldDefine(string name, MType fieldType)
{
	public string Name { get; } = name;
	public MType FieldType { get; } = fieldType;
}
