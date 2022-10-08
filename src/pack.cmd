del "CachingFramework.Redis\bin\release\*.nupkg"
del "CachingFramework.Redis.MsgPack\bin\release\*.nupkg"
del "CachingFramework.Redis.NewtonsoftJson\bin\release\*.nupkg"

dotnet pack "CachingFramework.Redis/" -c Release
dotnet pack "CachingFramework.Redis.MsgPack/" -c Release
dotnet pack "CachingFramework.Redis.NewtonsoftJson/" -c Release
