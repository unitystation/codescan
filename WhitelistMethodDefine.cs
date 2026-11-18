namespace CodeScan;

public sealed class WhitelistMethodDefine(
	string name,
	MType returnType,
	List<MType> parameterTypes,
	int genericParameterCount)
{
	public string Name { get; } = name;
	public MType ReturnType { get; } = returnType;
	public List<MType> ParameterTypes { get; } = parameterTypes;
	public int GenericParameterCount { get; } = genericParameterCount;
}
