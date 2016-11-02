del "CachingFramework.Redis\bin\debug\*.nupkg"
del "CachingFramework.Redis.MsgPack\bin\debug\*.nupkg"
del "CachingFramework.Redis.MsgPack.StrongName\bin\debug\*.nupkg"
del "CachingFramework.Redis.StrongName\bin\debug\*.nupkg"

dotnet pack "CachingFramework.Redis/"
dotnet pack "CachingFramework.Redis.MsgPack/"
dotnet pack "CachingFramework.Redis.MsgPack.StrongName/"
dotnet pack "CachingFramework.Redis.StrongName/"
