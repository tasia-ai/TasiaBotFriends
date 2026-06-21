using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices;

[CompilerGenerated]
[Embedded]
[AttributeUsage(/*Could not decode attribute arguments.*/)]
internal sealed class NullableAttribute : Attribute
{
	public readonly byte[] NullableFlags;

	public NullableAttribute(byte P_0)
	{
		NullableFlags = (byte[])(object)new Byte[1] { P_0 };
	}

	public NullableAttribute(byte[] P_0)
	{
		NullableFlags = P_0;
	}
}
