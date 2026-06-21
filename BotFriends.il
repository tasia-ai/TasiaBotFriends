.assembly extern netstandard
{
  .ver 2:1:0:0
  .publickeytoken = (CC 7B 13 FF CD 2D DD 51 ) // .{...-.Q
}
.assembly extern UnityEngine.CoreModule
{
  .ver 0:0:0:0
}
.assembly extern 'Assembly-CSharp'
{
  .ver 0:0:0:0
}
.assembly extern '0Harmony'
{
  .ver 2:9:0:0
}
.assembly extern UnityEngine.AIModule
{
  .ver 0:0:0:0
}
.assembly extern UnityEngine.AnimationModule
{
  .ver 0:0:0:0
}
.assembly extern UnityEngine.PhysicsModule
{
  .ver 0:0:0:0
}
.assembly extern UnityEngine.AudioModule
{
  .ver 0:0:0:0
}
.assembly extern UnityEngine.ParticleSystemModule
{
  .ver 0:0:0:0
}
.assembly extern UnityEngine.TextRenderingModule
{
  .ver 0:0:0:0
}
.assembly extern BepInEx
{
  .ver 5:4:23:5
}
.assembly 'BotFriends'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.ExtensionAttribute::'.ctor'() =  (01 00 00 00 ) // ....

  .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::'.ctor'(int32) =  (01 00 08 00 00 00 00 00 ) // ........

  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::'.ctor'() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .custom instance void class [mscorlib]System.Diagnostics.DebuggableAttribute::'.ctor'(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) =  (01 00 02 00 00 00 00 00 ) // ........

  .custom instance void class [mscorlib]System.Runtime.Versioning.TargetFrameworkAttribute::'.ctor'(string) =  (
		01 00 19 2E 4E 45 54 53 74 61 6E 64 61 72 64 2C   // ....NETStandard,
		56 65 72 73 69 6F 6E 3D 76 32 2E 31 01 00 54 0E   // Version=v2.1..T.
		14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C 61   // .FrameworkDispla
		79 4E 61 6D 65 11 2E 4E 45 54 20 53 74 61 6E 64   // yName..NET Stand
		61 72 64 20 32 2E 31                            ) // ard 2.1

  .custom instance void class [mscorlib]System.Reflection.AssemblyCompanyAttribute::'.ctor'(string) =  (01 00 07 45 6D 70 72 65 73 73 00 00 ) // ...Empress..

  .custom instance void class [mscorlib]System.Reflection.AssemblyConfigurationAttribute::'.ctor'(string) =  (01 00 07 52 65 6C 65 61 73 65 00 00 ) // ...Release..

  .custom instance void class [mscorlib]System.Reflection.AssemblyDescriptionAttribute::'.ctor'(string) =  (
		01 00 12 45 6D 70 72 65 73 73 20 42 6F 74 46 72   // ...Empress BotFr
		69 65 6E 64 73 00 00                            ) // iends..

  .custom instance void class [mscorlib]System.Reflection.AssemblyFileVersionAttribute::'.ctor'(string) =  (01 00 07 31 2E 31 2E 33 2E 30 00 00 ) // ...1.1.3.0..

  .custom instance void class [mscorlib]System.Reflection.AssemblyInformationalVersionAttribute::'.ctor'(string) =  (
		01 00 2E 31 2E 31 2E 33 2B 34 32 34 37 65 61 65   // ...1.1.3+4247eae
		34 62 38 31 63 63 31 34 32 33 39 37 30 33 37 30   // 4b81cc1423970370
		34 36 35 39 38 30 66 39 64 30 38 34 31 64 34 65   // 465980f9d0841d4e
		38 00 00                                        ) // 8..

  .custom instance void class [mscorlib]System.Reflection.AssemblyProductAttribute::'.ctor'(string) =  (
		01 00 12 45 6D 70 72 65 73 73 20 42 6F 74 46 72   // ...Empress BotFr
		69 65 6E 64 73 00 00                            ) // iends..

  .custom instance void class [mscorlib]System.Reflection.AssemblyTitleAttribute::'.ctor'(string) =  (01 00 0A 42 6F 74 46 72 69 65 6E 64 73 00 00 ) // ...BotFriends..

  .permissionset reqmin = {[netstandard]System.Security.Permissions.SecurityPermissionAttribute = {property bool 'SkipVerification' = bool(true)}}
  .hash algorithm 0x00008004
  .ver  1:1:3:0
}
.module BotFriends.dll // GUID = {7F27FCA1-D8AB-4FC5-B5CA-CF65A0411CCA}

.custom instance void class [mscorlib]System.Security.UnverifiableCodeAttribute::'.ctor'() =  (01 00 00 00 ) // ....

.custom instance void class System.Runtime.CompilerServices.RefSafetyRulesAttribute::'.ctor'(int32) =  (01 00 0B 00 00 00 00 00 ) // ........


.namespace Microsoft.CodeAnalysis
{
  .class private auto ansi sealed beforefieldinit EmbeddedAttribute
  	extends [netstandard]System.Attribute
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class Microsoft.CodeAnalysis.EmbeddedAttribute::'.ctor'() =  (01 00 00 00 ) // ....


    // method line 1
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2050
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class [mscorlib]System.Attribute::'.ctor'()
	IL_0006:  ret 
    } // end of method EmbeddedAttribute::.ctor

  } // end of class Microsoft.CodeAnalysis.EmbeddedAttribute
}

.namespace System.Runtime.CompilerServices
{
  .class private auto ansi sealed beforefieldinit NullableAttribute
  	extends [netstandard]System.Attribute
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class Microsoft.CodeAnalysis.EmbeddedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class [mscorlib]System.AttributeUsageAttribute::'.ctor'(valuetype [mscorlib]System.AttributeTargets) =  (
		01 00 84 6B 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ...k....T..Allow
		4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
		72 69 74 65 64 00                               ) // rited.

    .field  public initonly  unsigned int8[] NullableFlags

    // method line 2
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' (unsigned int8 A_1)  cil managed 
    {
        // Method begins at RVA 0x2058
	// Code size 23 (0x17)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class [mscorlib]System.Attribute::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.1 
	IL_0008:  newarr [netstandard]System.Byte
	IL_000d:  dup 
	IL_000e:  ldc.i4.0 
	IL_000f:  ldarg.1 
	IL_0010:  stelem.i1 
	IL_0011:  stfld unsigned int8[] System.Runtime.CompilerServices.NullableAttribute::NullableFlags
	IL_0016:  ret 
    } // end of method NullableAttribute::.ctor

    // method line 3
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' (unsigned int8[] A_1)  cil managed 
    {
        // Method begins at RVA 0x2070
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class [mscorlib]System.Attribute::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldarg.1 
	IL_0008:  stfld unsigned int8[] System.Runtime.CompilerServices.NullableAttribute::NullableFlags
	IL_000d:  ret 
    } // end of method NullableAttribute::.ctor

  } // end of class System.Runtime.CompilerServices.NullableAttribute
}

.namespace System.Runtime.CompilerServices
{
  .class private auto ansi sealed beforefieldinit NullableContextAttribute
  	extends [netstandard]System.Attribute
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class Microsoft.CodeAnalysis.EmbeddedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class [mscorlib]System.AttributeUsageAttribute::'.ctor'(valuetype [mscorlib]System.AttributeTargets) =  (
		01 00 4C 14 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ..L.....T..Allow
		4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
		72 69 74 65 64 00                               ) // rited.

    .field  public initonly  unsigned int8 Flag

    // method line 4
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' (unsigned int8 A_1)  cil managed 
    {
        // Method begins at RVA 0x207f
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class [mscorlib]System.Attribute::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldarg.1 
	IL_0008:  stfld unsigned int8 System.Runtime.CompilerServices.NullableContextAttribute::Flag
	IL_000d:  ret 
    } // end of method NullableContextAttribute::.ctor

  } // end of class System.Runtime.CompilerServices.NullableContextAttribute
}

.namespace System.Runtime.CompilerServices
{
  .class private auto ansi sealed beforefieldinit RefSafetyRulesAttribute
  	extends [netstandard]System.Attribute
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class Microsoft.CodeAnalysis.EmbeddedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .custom instance void class [mscorlib]System.AttributeUsageAttribute::'.ctor'(valuetype [mscorlib]System.AttributeTargets) =  (
		01 00 02 00 00 00 02 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
		4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65   // Multiple.T..Inhe
		72 69 74 65 64 00                               ) // rited.

    .field  public initonly  int32 Version

    // method line 5
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x208e
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class [mscorlib]System.Attribute::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldarg.1 
	IL_0008:  stfld int32 System.Runtime.CompilerServices.RefSafetyRulesAttribute::Version
	IL_000d:  ret 
    } // end of method RefSafetyRulesAttribute::.ctor

  } // end of class System.Runtime.CompilerServices.RefSafetyRulesAttribute
}

.namespace Empress.StupidBots
{
  .class private auto ansi sealed beforefieldinit EmpressBotAvatarVisual
  	extends [UnityEngine.CoreModule]UnityEngine.MonoBehaviour
  {
    .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 01 00 00 ) // .....

    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

    .field private static literal  float32 DefaultSpeed = float32(4.1999998092651367)
    .field private static literal  int32 MaxRandomCosmetics = int32(0x00000004)
    .field  private static initonly  string[] HiddenVisualNameParts
    .field  private static initonly  string[] PreferredVisibleLayers
    .field  private static initonly  Invalid type due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. RandomCosmeticTypes
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001a due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> PlayerAvatarCosmeticsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001c due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> VisualsPlayerCosmeticsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001c due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> PlayerCosmeticsVisualsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> CosmeticParentsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000017 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> CosmeticParentTypeRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001f due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> CosmeticParentSpringImpulseRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> CosmeticParentParentRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> CosmeticParentResetTransformRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> BaseMeshParentsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> BaseMeshesRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> FirstSetupRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> FirstSetupCoroutineRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000021 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> CosmeticAssetsRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000017 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> CosmeticAssetTypeRef
    .field  private static initonly  <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000023 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> CosmeticAssetPrefabRef
    .field  private static initonly  class [mscorlib]System.Reflection.MethodInfo InstantiateCosmeticMethod
    .field  private  <BROKEN CLASS token_ 1000025 due to Could not load file or assembly 'UnityEngine.AIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> _agent
    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

    .field  private  Invalid type due to Could not load file or assembly 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. _animators
    .field  private  <BROKEN CLASS token_ 1000027 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> _lastPosition
    .field  private  float32 _fallbackSpeed

    // method line 6
    .method assembly hidebysig 
           (null) cil managed 
    {
        .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x20a0
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 7
    .method private hidebysig 
           instance default void LateUpdate ()  cil managed 
    {
        // Method begins at RVA 0x2128
    } // end of method EmpressBotAvatarVisual::LateUpdate

    // method line 8
    .method assembly static hidebysig 
           (null) cil managed 
    {
        .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

	.param [1]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 01 00 00 ) // .....

        // Method begins at RVA 0x23dc
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 9
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [0]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x249c
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 10
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [0]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x25b0
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 11
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [3]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x265c
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 12
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x2908
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 13
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [3]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x2b2c
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 14
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x2e34
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 15
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [1]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8[]) =  (01 00 02 00 00 00 02 01 00 00 ) // ..........

        // Method begins at RVA 0x2ee0
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 16
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [0]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

	.param [1]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 02 00 00 ) // .....

        // Method begins at RVA 0x2f28
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 17
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x2fbc
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 18
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x3048
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 19
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x309c
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 20
    .method private static hidebysig 
           (null) cil managed 
    {
	.param [1]
	.custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8[]) =  (01 00 02 00 00 00 02 01 00 00 ) // ..........

        // Method begins at RVA 0x3174
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 21
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x31b4
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 22
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x31ec
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 23
    .method private static hidebysig 
           default bool ShouldHide (string name)  cil managed 
    {
        // Method begins at RVA 0x323c
	// Code size 54 (0x36)
	.maxstack 3
	.locals init (
		string	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call bool string::IsNullOrWhiteSpace(string)
	IL_0006:  brfalse.s IL_000a

	IL_0008:  ldc.i4.0 
	IL_0009:  ret 
	IL_000a:  ldarg.0 
	IL_000b:  callvirt instance string string::ToLowerInvariant()
	IL_0010:  stloc.0 
	IL_0011:  ldc.i4.0 
	IL_0012:  stloc.1 
	IL_0013:  br.s IL_002a

	IL_0015:  ldloc.0 
	IL_0016:  ldsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::HiddenVisualNameParts
	IL_001b:  ldloc.1 
	IL_001c:  ldelem.ref 
	IL_001d:  callvirt instance bool string::Contains(string)
	IL_0022:  brfalse.s IL_0026

	IL_0024:  ldc.i4.1 
	IL_0025:  ret 
	IL_0026:  ldloc.1 
	IL_0027:  ldc.i4.1 
	IL_0028:  add 
	IL_0029:  stloc.1 
	IL_002a:  ldloc.1 
	IL_002b:  ldsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::HiddenVisualNameParts
	IL_0030:  ldlen 
	IL_0031:  conv.i4 
	IL_0032:  blt.s IL_0015

	IL_0034:  ldc.i4.0 
	IL_0035:  ret 
    } // end of method EmpressBotAvatarVisual::ShouldHide

    // method line 24
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x3280
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 25
    .method private static hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x33a8
    } // end of method EmpressBotAvatarVisual::<NULL METHOD SIGNATURE>

    // method line 26
    .method private static hidebysig 
           default int32 ResolveVisibleLayer ()  cil managed 
    {
        // Method begins at RVA 0x33f4
	// Code size 39 (0x27)
	.maxstack 2
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  br.s IL_001b

	IL_0004:  ldsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::PreferredVisibleLayers
	IL_0009:  ldloc.0 
	IL_000a:  ldelem.ref 
	IL_000b:  call int32 [UnityEngine.CoreModule]UnityEngine.LayerMask::NameToLayer(string)
	IL_0010:  stloc.1 
	IL_0011:  ldloc.1 
	IL_0012:  ldc.i4.0 
	IL_0013:  blt.s IL_0017

	IL_0015:  ldloc.1 
	IL_0016:  ret 
	IL_0017:  ldloc.0 
	IL_0018:  ldc.i4.1 
	IL_0019:  add 
	IL_001a:  stloc.0 
	IL_001b:  ldloc.0 
	IL_001c:  ldsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::PreferredVisibleLayers
	IL_0021:  ldlen 
	IL_0022:  conv.i4 
	IL_0023:  blt.s IL_0004

	IL_0025:  ldc.i4.0 
	IL_0026:  ret 
    } // end of method EmpressBotAvatarVisual::ResolveVisibleLayer

    // method line 27
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x3427
	// Code size 29 (0x1d)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call !!0[]<<BROKEN CLASS token_ 1000026 due to Could not load file or assembly 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> ()
	IL_0006:  stfld Invalid type due to Could not load file or assembly 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. Empress.StupidBots.EmpressBotAvatarVisual::_animators
	IL_000b:  ldarg.0 
	IL_000c:  ldc.r4 4.1999998092651367
	IL_0011:  stfld float32 Empress.StupidBots.EmpressBotAvatarVisual::_fallbackSpeed
	IL_0016:  ldarg.0 
	IL_0017:  call instance void [UnityEngine.CoreModule]UnityEngine.MonoBehaviour::.ctor()
	IL_001c:  ret 
    } // end of method EmpressBotAvatarVisual::.ctor

    // method line 28
    .method private static hidebysig specialname rtspecialname 
           default void '.cctor' ()  cil managed 
    {
        // Method begins at RVA 0x3448
	// Code size 388 (0x184)
	.maxstack 6
	IL_0000:  ldc.i4.7 
	IL_0001:  newarr [netstandard]System.String
	IL_0006:  dup 
	IL_0007:  ldc.i4.0 
	IL_0008:  ldstr "grab"
	IL_000d:  stelem.ref 
	IL_000e:  dup 
	IL_000f:  ldc.i4.1 
	IL_0010:  ldstr "grabb"
	IL_0015:  stelem.ref 
	IL_0016:  dup 
	IL_0017:  ldc.i4.2 
	IL_0018:  ldstr "holder"
	IL_001d:  stelem.ref 
	IL_001e:  dup 
	IL_001f:  ldc.i4.3 
	IL_0020:  ldstr "suction"
	IL_0025:  stelem.ref 
	IL_0026:  dup 
	IL_0027:  ldc.i4.4 
	IL_0028:  ldstr "vacuum"
	IL_002d:  stelem.ref 
	IL_002e:  dup 
	IL_002f:  ldc.i4.5 
	IL_0030:  ldstr "tool"
	IL_0035:  stelem.ref 
	IL_0036:  dup 
	IL_0037:  ldc.i4.6 
	IL_0038:  ldstr "cart"
	IL_003d:  stelem.ref 
	IL_003e:  stsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::HiddenVisualNameParts
	IL_0043:  ldc.i4.2 
	IL_0044:  newarr [netstandard]System.String
	IL_0049:  dup 
	IL_004a:  ldc.i4.0 
	IL_004b:  ldstr "PlayerVisuals"
	IL_0050:  stelem.ref 
	IL_0051:  dup 
	IL_0052:  ldc.i4.1 
	IL_0053:  ldstr "Default"
	IL_0058:  stelem.ref 
	IL_0059:  stsfld string[] Empress.StupidBots.EmpressBotAvatarVisual::PreferredVisibleLayers
	IL_005e:  ldc.i4.s 0x0b
	IL_0060:  newarr ['Assembly-CSharp']SemiFunc/CosmeticType
	IL_0065:  dup 
	IL_0066:  ldtoken field valuetype '<PrivateImplementationDetails>'/'__StaticArrayInitTypeSize=44' '<PrivateImplementationDetails>'::'476B1FA51D0E44404F59FD181238240B82402ED542A959C1D96DDE336D51D944'
	IL_006b:  call void class [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array, valuetype [mscorlib]System.RuntimeFieldHandle)
	IL_0070:  stsfld Invalid type due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. Empress.StupidBots.EmpressBotAvatarVisual::RandomCosmeticTypes
	IL_0075:  ldstr "playerCosmetics"
	IL_007a:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_007f:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001a due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::PlayerAvatarCosmeticsRef
	IL_0084:  ldstr "playerCosmetics"
	IL_0089:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_008e:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001c due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::VisualsPlayerCosmeticsRef
	IL_0093:  ldstr "playerAvatarVisuals"
	IL_0098:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_009d:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001c due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::PlayerCosmeticsVisualsRef
	IL_00a2:  ldstr "cosmeticParents"
	IL_00a7:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00ac:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticParentsRef
	IL_00b1:  ldstr "cosmeticType"
	IL_00b6:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00bb:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000017 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticParentTypeRef
	IL_00c0:  ldstr "springImpulse"
	IL_00c5:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00ca:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 100001f due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticParentSpringImpulseRef
	IL_00cf:  ldstr "parent"
	IL_00d4:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00d9:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticParentParentRef
	IL_00de:  ldstr "resetTransform"
	IL_00e3:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00e8:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticParentResetTransformRef
	IL_00ed:  ldstr "baseMeshParents"
	IL_00f2:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_00f7:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> Empress.StupidBots.EmpressBotAvatarVisual::BaseMeshParentsRef
	IL_00fc:  ldstr "baseMeshes"
	IL_0101:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0106:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001e due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000020 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> Empress.StupidBots.EmpressBotAvatarVisual::BaseMeshesRef
	IL_010b:  ldstr "firstSetup"
	IL_0110:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0115:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> Empress.StupidBots.EmpressBotAvatarVisual::FirstSetupRef
	IL_011a:  ldstr "firstSetupCoroutine"
	IL_011f:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0124:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> Empress.StupidBots.EmpressBotAvatarVisual::FirstSetupCoroutineRef
	IL_0129:  ldstr "cosmeticAssets"
	IL_012e:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0133:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000021 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticAssetsRef
	IL_0138:  ldstr "type"
	IL_013d:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0142:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000017 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticAssetTypeRef
	IL_0147:  ldstr "prefab"
	IL_014c:  call Could not decode method token 0x157 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0151:  stsfld <BROKEN CLASS token_ 1000019 due to Could not load file or assembly '0Harmony, Version=2.9.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><<BROKEN CLASS token_ 1000022 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,<BROKEN CLASS token_ 1000023 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.EmpressBotAvatarVisual::CosmeticAssetPrefabRef
	IL_0156:  ldtoken ['Assembly-CSharp']PlayerCosmetics
	IL_015b:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_0160:  ldstr "InstantiateCosmetic"
	IL_0165:  ldc.i4.1 
	IL_0166:  newarr [netstandard]System.Type
	IL_016b:  dup 
	IL_016c:  ldc.i4.0 
	IL_016d:  ldtoken ['Assembly-CSharp']CosmeticAsset
	IL_0172:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_0177:  stelem.ref 
	IL_0178:  ldnull 
	IL_0179:  call class [mscorlib]System.Reflection.MethodInfo ['0Harmony']HarmonyLib.AccessTools::Method(class [mscorlib]System.Type, string, class [mscorlib]System.Type[], class [mscorlib]System.Type[])
	IL_017e:  stsfld class [mscorlib]System.Reflection.MethodInfo Empress.StupidBots.EmpressBotAvatarVisual::InstantiateCosmeticMethod
	IL_0183:  ret 
    } // end of method EmpressBotAvatarVisual::.cctor

  .class nested private auto ansi sealed serializable beforefieldinit '<>c'
  	extends [netstandard]System.Object
  {
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  public static initonly  <BROKEN CLASS token_ 2000014 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> '<>9'
    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

    .field  public static  class [mscorlib]System.Func`2<<BROKEN CLASS token_ 100001b due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,bool> '<>9__31_0'
    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

    .field  public static  class [mscorlib]System.Func`2<<BROKEN CLASS token_ 1000017 due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>,float32> '<>9__31_1'
    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....


    // method line 182
    .method private static hidebysig specialname rtspecialname 
           default void '.cctor' ()  cil managed 
    {
        // Method begins at RVA 0x88ac
	// Code size 11 (0xb)
	.maxstack 8
	IL_0000:  newobj instance void class '<>c'::'.ctor'()
	IL_0005:  stsfld class '<>c' Empress.StupidBots.EmpressBotAvatarVisual/'<>c'::'<>9'
	IL_000a:  ret 
    } // end of method <>c::.cctor

    // method line 183
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x88b8
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <>c::.ctor

    // method line 184
    .method assembly hidebysig 
           (null) cil managed 
    {
        .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

        // Method begins at RVA 0x88c0
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.1 
	IL_0001:  call bool [UnityEngine.CoreModule]UnityEngine.Object::op_Implicit(<BROKEN CLASS token_ 1000028 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>)
	IL_0006:  ret 
    } // end of method <>c::<NULL METHOD SIGNATURE>

    // method line 185
    .method assembly hidebysig 
           (null) cil managed 
    {
        // Method begins at RVA 0x88c8
	// Code size 6 (0x6)
	.maxstack 8
	IL_0000:  call float32 [UnityEngine.CoreModule]UnityEngine.Random::get_value()
	IL_0005:  ret 
    } // end of method <>c::<NULL METHOD SIGNATURE>

  } // end of class <>c

  } // end of class Empress.StupidBots.EmpressBotAvatarVisual
}

.namespace Empress.StupidBots
{
  .class private auto ansi beforefieldinit BotChatterAgent
  	extends [UnityEngine.CoreModule]UnityEngine.MonoBehaviour
  {
    .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 01 00 00 ) // .....

    .custom instance void class System.Runtime.CompilerServices.NullableAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

    .field  private  <BROKEN CLASS token_ 1000025 due to Could not load file or assembly 'UnityEngine.AIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> _agent
    .field  private  <BROKEN CLASS token_ 1000033 due to Could not load file or assembly 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> _rb
    .field  private  <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> _src
    .field  private initonly  class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 100003f due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> _buf
    .field  private  float32 _lastLocalSpeak
    .field  private  bool _lastCarrying
    .field  private  string _lastCarriedName
    .field  private  <BROKEN CLASS token_ 2000015 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> '<State>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  bool '<IsCarrying>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  string '<CarriedLabel>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....


    // method line 29
    .method public hidebysig specialname 
           instance default valuetype BotState get_State ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x35d8
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld valuetype BotState Empress.StupidBots.BotChatterAgent::'<State>k__BackingField'
	IL_0006:  ret 
    } // end of method BotChatterAgent::get_State

    // method line 30
    .method private hidebysig specialname 
           instance default void set_State (valuetype BotState 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x35e0
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld valuetype BotState Empress.StupidBots.BotChatterAgent::'<State>k__BackingField'
	IL_0007:  ret 
    } // end of method BotChatterAgent::set_State

    // method line 31
    .method public hidebysig specialname 
           instance default bool get_IsCarrying ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x35e9
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld bool Empress.StupidBots.BotChatterAgent::'<IsCarrying>k__BackingField'
	IL_0006:  ret 
    } // end of method BotChatterAgent::get_IsCarrying

    // method line 32
    .method private hidebysig specialname 
           instance default void set_IsCarrying (bool 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x35f1
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld bool Empress.StupidBots.BotChatterAgent::'<IsCarrying>k__BackingField'
	IL_0007:  ret 
    } // end of method BotChatterAgent::set_IsCarrying

    // method line 33
    .method public hidebysig specialname 
           instance default string get_CarriedLabel ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x35fa
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld string Empress.StupidBots.BotChatterAgent::'<CarriedLabel>k__BackingField'
	IL_0006:  ret 
    } // end of method BotChatterAgent::get_CarriedLabel

    // method line 34
    .method private hidebysig specialname 
           instance default void set_CarriedLabel (string 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3602
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld string Empress.StupidBots.BotChatterAgent::'<CarriedLabel>k__BackingField'
	IL_0007:  ret 
    } // end of method BotChatterAgent::set_CarriedLabel

    // method line 35
    .method private hidebysig 
           instance default void Awake ()  cil managed 
    {
        // Method begins at RVA 0x360c
	// Code size 127 (0x7f)
	.maxstack 2
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.0 
	IL_0002:  call Could not decode method token 0x15d due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0007:  stfld <BROKEN CLASS token_ 1000025 due to Could not load file or assembly 'UnityEngine.AIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_agent
	IL_000c:  ldarg.0 
	IL_000d:  ldarg.0 
	IL_000e:  call Could not decode method token 0x15d due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_0013:  stfld <BROKEN CLASS token_ 1000033 due to Could not load file or assembly 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_rb
	IL_0018:  ldarg.0 
	IL_0019:  ldarg.0 
	IL_001a:  call Could not decode method token 0x15d due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_001f:  stfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0024:  ldarg.0 
	IL_0025:  ldfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_002a:  call bool [UnityEngine.CoreModule]UnityEngine.Object::op_Implicit(<BROKEN CLASS token_ 1000028 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>)
	IL_002f:  brtrue.s IL_0042

	IL_0031:  ldarg.0 
	IL_0032:  ldarg.0 
	IL_0033:  call instance <BROKEN CLASS token_ 100002b due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> [UnityEngine.CoreModule]UnityEngine.Component::get_gameObject()
	IL_0038:  callvirt Could not decode method token 0x85 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.
	IL_003d:  stfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0042:  ldarg.0 
	IL_0043:  ldfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0048:  ldc.r4 1.
	IL_004d:  callvirt instance void [UnityEngine.AudioModule]UnityEngine.AudioSource::set_spatialBlend(float32)
	IL_0052:  ldarg.0 
	IL_0053:  ldfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0058:  ldc.i4.0 
	IL_0059:  callvirt instance void [UnityEngine.AudioModule]UnityEngine.AudioSource::set_rolloffMode(<BROKEN CLASS token_ 1000081 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>)
	IL_005e:  ldarg.0 
	IL_005f:  ldfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0064:  ldc.r4 2.
	IL_0069:  callvirt instance void [UnityEngine.AudioModule]UnityEngine.AudioSource::set_minDistance(float32)
	IL_006e:  ldarg.0 
	IL_006f:  ldfld <BROKEN CLASS token_ 1000034 due to Could not load file or assembly 'UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotChatterAgent::_src
	IL_0074:  ldc.r4 18.
	IL_0079:  callvirt instance void [UnityEngine.AudioModule]UnityEngine.AudioSource::set_maxDistance(float32)
	IL_007e:  ret 
    } // end of method BotChatterAgent::Awake

    // method line 36
    .method private hidebysig 
           instance default void OnEnable ()  cil managed 
    {
        // Method begins at RVA 0x3697
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call void(class Empress.StupidBots.BotChatterAgent)
	IL_0006:  ret 
    } // end of method BotChatterAgent::OnEnable

    // method line 37
    .method private hidebysig 
           instance default void OnDisable ()  cil managed 
    {
        // Method begins at RVA 0x369f
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call void class Empress.StupidBots.BotChatterManager::Unregister(class Empress.StupidBots.BotChatterAgent)
	IL_0006:  ret 
    } // end of method BotChatterAgent::OnDisable

    // method line 38
    .method private hidebysig 
           instance default void Update ()  cil managed 
    {
        // Method begins at RVA 0x36a7
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Empress.StupidBots.BotChatterAgent::UpdateState()
	IL_0006:  ret 
    } // end of method BotChatterAgent::Update

    // method line 39
    .method private hidebysig 
           instance default void UpdateState ()  cil managed 
    {
        // Method begins at RVA 0x36b0
    } // end of method BotChatterAgent::UpdateState

    // method line 40
    .method private hidebysig 
           instance default void DetectCarry ([out] bool& carrying, [out] string& label)  cil managed 
    {
        // Method begins at RVA 0x3764
    } // end of method BotChatterAgent::DetectCarry

    // method line 41
    .method assembly hidebysig 
           instance default void PlayClip (valuetype Empress.StupidBots.DecTalkSynth/Clip clip, float32 volume)  cil managed 
    {
        // Method begins at RVA 0x3824
    } // end of method BotChatterAgent::PlayClip

    // method line 42
    .method assembly hidebysig 
           instance default bool TickLocal (float32 now, bool canSpeak)  cil managed 
    {
        // Method begins at RVA 0x38b4
	// Code size 115 (0x73)
	.maxstack 3
	.locals init (
		bool	V_0,
		string	V_1)
	IL_0000:  ldarg.2 
	IL_0001:  brtrue.s IL_0005

	IL_0003:  ldc.i4.0 
	IL_0004:  ret 
	IL_0005:  ldarg.1 
	IL_0006:  ldarg.0 
	IL_0007:  ldfld float32 Empress.StupidBots.BotChatterAgent::_lastLocalSpeak
	IL_000c:  bge.un.s IL_0010

	IL_000e:  ldc.i4.0 
	IL_000f:  ret 
	IL_0010:  ldc.i4.0 
	IL_0011:  stloc.0 
	IL_0012:  ldarg.0 
	IL_0013:  call instance bool class Empress.StupidBots.BotChatterAgent::get_IsCarrying()
	IL_0018:  brfalse.s IL_0024

	IL_001a:  ldarg.0 
	IL_001b:  ldfld bool Empress.StupidBots.BotChatterAgent::_lastCarrying
	IL_0020:  brtrue.s IL_0024

	IL_0022:  ldc.i4.1 
	IL_0023:  stloc.0 
	IL_0024:  ldarg.0 
	IL_0025:  ldarg.0 
	IL_0026:  call instance bool class Empress.StupidBots.BotChatterAgent::get_IsCarrying()
	IL_002b:  stfld bool Empress.StupidBots.BotChatterAgent::_lastCarrying
	IL_0030:  ldloc.0 
	IL_0031:  brtrue.s IL_0035

	IL_0033:  ldc.i4.0 
	IL_0034:  ret 
	IL_0035:  ldarg.0 
	IL_0036:  call instance bool class Empress.StupidBots.BotChatterAgent::get_IsCarrying()
	IL_003b:  brtrue.s IL_0044

	IL_003d:  ldstr "Moving."
	IL_0042:  br.s IL_0049

	IL_0044:  ldstr "Moving with package."
	IL_0049:  stloc.1 
	IL_004a:  ldsfld <BROKEN CLASS token_ 1000066 due to Could not load file or assembly 'BepInEx, Version=5.4.23.5, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><bool> Empress.StupidBots.EmpressBotFriendsChatterPlugin::UseDECtalk
	IL_004f:  callvirt instance !0 <BROKEN CLASS token_ 1000066 due to Could not load file or assembly 'BepInEx, Version=5.4.23.5, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><bool>::get_Value()
	IL_0054:  brfalse.s IL_0071

	IL_0056:  ldarg.0 
	IL_0057:  ldloc.1 
	IL_0058:  call void class Empress.StupidBots.DecTalkSynth::Enqueue(class Empress.StupidBots.BotChatterAgent, string)
	IL_005d:  ldarg.0 
	IL_005e:  ldarg.1 
	IL_005f:  ldsfld <BROKEN CLASS token_ 1000066 due to Could not load file or assembly 'BepInEx, Version=5.4.23.5, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><float32> Empress.StupidBots.EmpressBotFriendsChatterPlugin::SelfCooldown
	IL_0064:  callvirt instance !0 <BROKEN CLASS token_ 1000066 due to Could not load file or assembly 'BepInEx, Version=5.4.23.5, Culture=neutral, PublicKeyToken=null' or one of its dependencies.><float32>::get_Value()
	IL_0069:  add 
	IL_006a:  stfld float32 Empress.StupidBots.BotChatterAgent::_lastLocalSpeak
	IL_006f:  ldc.i4.1 
	IL_0070:  ret 
	IL_0071:  ldc.i4.0 
	IL_0072:  ret 
    } // end of method BotChatterAgent::TickLocal

    // method line 43
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x3933
	// Code size 18 (0x12)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  newobj instance void class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 100003f due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>>::.ctor()
	IL_0006:  stfld class [mscorlib]System.Collections.Generic.List`1<<BROKEN CLASS token_ 100003f due to Could not load file or assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.>> Empress.StupidBots.BotChatterAgent::_buf
	IL_000b:  ldarg.0 
	IL_000c:  call instance void [UnityEngine.CoreModule]UnityEngine.MonoBehaviour::.ctor()
	IL_0011:  ret 
    } // end of method BotChatterAgent::.ctor

	.property instance valuetype BotState State ()
	{
		.get instance default valuetype BotState Empress.StupidBots.BotChatterAgent::get_State () 
		.set instance default void Empress.StupidBots.BotChatterAgent::set_State (valuetype BotState 'value') 
	}
	.property instance bool IsCarrying ()
	{
		.get instance default bool Empress.StupidBots.BotChatterAgent::get_IsCarrying () 
		.set instance default void Empress.StupidBots.BotChatterAgent::set_IsCarrying (bool 'value') 
	}
	.property instance string CarriedLabel ()
	{
		.get instance default string Empress.StupidBots.BotChatterAgent::get_CarriedLabel () 
		.set instance default void Empress.StupidBots.BotChatterAgent::set_CarriedLabel (string 'value') 
	}
  .class nested public auto ansi sealed BotState
  	extends [netstandard]System.Enum
  {
    .custom instance void class System.Runtime.CompilerServices.NullableContextAttribute::'.ctor'(unsigned int8) =  (01 00 00 00 00 ) // .....

    .field  public specialname rtspecialname  int32 value__
    .field public static literal  valuetype BotState Idle = int32(0x00000000)
    .field public static literal  valuetype BotState Moving = int32(0x00000001)
    .field public static literal  valuetype BotState Carrying = int32(0x00000002)
    .field public static literal  valuetype BotState Ragdoll = int32(0x00000003)

  } // end of class BotState

  } // end of class Empress.StupidBots.BotChatterAgent
}

.namespace Empress.StupidBots
{
  .class private auto ansi sealed BotPersonalityType
  	extends [netstandard]System.Enum
  {
    .field  public specialname rtspecialname  int32 value__
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Clumsy = int32(0x00000000)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Fast = int32(0x00000001)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Slow = int32(0x00000002)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Aggressive = int32(0x00000003)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Coward = int32(0x00000004)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Curious = int32(0x00000005)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Guardian = int32(0x00000006)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Jittery = int32(0x00000007)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Pacifist = int32(0x00000008)
    .field public static literal  valuetype Empress.StupidBots.BotPersonalityType Trickster = int32(0x00000009)

  } // end of class Empress.StupidBots.BotPersonalityType
}

.namespace Empress.StupidBots
{
  .class private auto ansi beforefieldinit BotPersonality
  	extends [UnityEngine.CoreModule]UnityEngine.MonoBehaviour
  {
    .field  private  valuetype Empress.StupidBots.BotPersonalityType '<Type>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  float32 '<SpeedMultiplier>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  bool '<AllowValuables>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  bool '<AllowWeapons>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  float32 '<AimConeDegrees>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  <BROKEN CLASS token_ 1000042 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> '<FireInterval>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  float32 '<ToyingChance>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

    .field  private  float32 '<DropChanceOnShoot>k__BackingField'
    .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....


    // method line 44
    .method public hidebysig specialname 
           instance default valuetype Empress.StupidBots.BotPersonalityType get_Type ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3946
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld valuetype Empress.StupidBots.BotPersonalityType Empress.StupidBots.BotPersonality::'<Type>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_Type

    // method line 45
    .method private hidebysig specialname 
           instance default void set_Type (valuetype Empress.StupidBots.BotPersonalityType 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x394e
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld valuetype Empress.StupidBots.BotPersonalityType Empress.StupidBots.BotPersonality::'<Type>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_Type

    // method line 46
    .method public hidebysig specialname 
           instance default float32 get_SpeedMultiplier ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3957
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float32 Empress.StupidBots.BotPersonality::'<SpeedMultiplier>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_SpeedMultiplier

    // method line 47
    .method private hidebysig specialname 
           instance default void set_SpeedMultiplier (float32 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x395f
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld float32 Empress.StupidBots.BotPersonality::'<SpeedMultiplier>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_SpeedMultiplier

    // method line 48
    .method public hidebysig specialname 
           instance default bool get_AllowValuables ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3968
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld bool Empress.StupidBots.BotPersonality::'<AllowValuables>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_AllowValuables

    // method line 49
    .method private hidebysig specialname 
           instance default void set_AllowValuables (bool 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3970
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld bool Empress.StupidBots.BotPersonality::'<AllowValuables>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_AllowValuables

    // method line 50
    .method public hidebysig specialname 
           instance default bool get_AllowWeapons ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3979
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld bool Empress.StupidBots.BotPersonality::'<AllowWeapons>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_AllowWeapons

    // method line 51
    .method private hidebysig specialname 
           instance default void set_AllowWeapons (bool 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3981
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld bool Empress.StupidBots.BotPersonality::'<AllowWeapons>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_AllowWeapons

    // method line 52
    .method public hidebysig specialname 
           instance default float32 get_AimConeDegrees ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x398a
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float32 Empress.StupidBots.BotPersonality::'<AimConeDegrees>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_AimConeDegrees

    // method line 53
    .method private hidebysig specialname 
           instance default void set_AimConeDegrees (float32 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x3992
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld float32 Empress.StupidBots.BotPersonality::'<AimConeDegrees>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_AimConeDegrees

    // method line 54
    .method public hidebysig specialname 
           (null) cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x399b
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld <BROKEN CLASS token_ 1000042 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotPersonality::'<FireInterval>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::<NULL METHOD SIGNATURE>

    // method line 55
    .method private hidebysig specialname 
           (null) cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x39a3
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld <BROKEN CLASS token_ 1000042 due to Could not load file or assembly 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.> Empress.StupidBots.BotPersonality::'<FireInterval>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::<NULL METHOD SIGNATURE>

    // method line 56
    .method public hidebysig specialname 
           instance default float32 get_ToyingChance ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x39ac
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float32 Empress.StupidBots.BotPersonality::'<ToyingChance>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_ToyingChance

    // method line 57
    .method private hidebysig specialname 
           instance default void set_ToyingChance (float32 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x39b4
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld float32 Empress.StupidBots.BotPersonality::'<ToyingChance>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_ToyingChance

    // method line 58
    .method public hidebysig specialname 
           instance default float32 get_DropChanceOnShoot ()  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x39bd
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float32 Empress.StupidBots.BotPersonality::'<DropChanceOnShoot>k__BackingField'
	IL_0006:  ret 
    } // end of method BotPersonality::get_DropChanceOnShoot

    // method line 59
    .method private hidebysig specialname 
           instance default void set_DropChanceOnShoot (float32 'value')  cil managed 
    {
        .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....

        // Method begins at RVA 0x39c5
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.1 
	IL_0002:  stfld float32 Empress.StupidBots.BotPersonality::'<DropChanceOnShoot>k__BackingField'
	IL_0007:  ret 
    } // end of method BotPersonality::set_DropChanceOnShoot

    // method line 60
    .method public hidebysig 
           instance default void InitRandom (int32 seed)  cil managed 
    {
        // Method begins at RVA 0x39d0
	// Code size 58 (0x3a)
	.maxstack 4
	.locals init (
		class [mscorlib]System.Random	V_0,
		valuetype Empress.StupidBots.BotPersonalityType[]	V_1)
	IL_0000:  ldarg.1 
	IL_0001:  newobj instance void class [mscorlib]System.Random::'.ctor'(int32)
	IL_0006:  stloc.0 
	IL_0007:  ldtoken Empress.StupidBots.BotPersonalityType
	IL_000c:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_0011:  call class [mscorlib]System.Array class [mscorlib]System.Enum::GetValues(class [mscorlib]System.Type)
	IL_0016:  castclass valuetype Empress.StupidBots.BotPersonalityType[]
	IL_001b:  stloc.1 
	IL_001c:  ldarg.0 
	IL_001d:  ldloc.1 
	IL_001e:  ldloc.0 
	IL_001f:  ldloc.1 
	IL_0020:  ldlen 
	IL_0021:  conv.i4 
	IL_0022:  callvirt instance int32 class [mscorlib]System.Random::Next(int32)
	IL_0027:  ldelem.i4 
	IL_0028:  call instance void(valuetype Empress.StupidBots.BotPersonalityType)
	IL_002d:  ldarg.0 
	IL_002e:  ldarg.0 
	IL_002f:  call instance valuetype Empress.StupidBots.BotPersonalityType class Empress.StupidBots.BotPersonality::get_Type()
	IL_0034:  call instance void class Empress.StupidBots.BotPersonality::ApplyDefau