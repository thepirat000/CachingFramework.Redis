del "CachingFramework.Redis\bin\debug\*.nupkg"
del "CachingFramework.Redis.MsgPack\bin\debug\*.nupkg"
del "CachingFramework.Redis.NewtonsoftJson\bin\debug\*.nupkg"

dotnet pack "CachingFramework.Redis/"
dotnet pack "CachingFramework.Redis.MsgPack/"
dotnet pack "CachingFramework.Redis.NewtonsoftJson/"
