del "CachingFramework.Redis\bin\release\*.symbols.nupkg"
del "CachingFramework.Redis.MsgPack\bin\release\*.symbols.nupkg"
del "CachingFramework.Redis.NewtonsoftJson\bin\release\*.symbols.nupkg"


nuget push "CachingFramework.Redis\bin\release\*.nupkg" -NoSymbols -source %1
nuget push "CachingFramework.Redis.MsgPack\bin\release\*.nupkg" -NoSymbols -source %1
nuget push "CachingFramework.Redis.NewtonsoftJson\bin\release\*.nupkg" -NoSymbols -source %1
