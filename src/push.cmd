del "CachingFramework.Redis\bin\debug\*.symbols.nupkg"
del "CachingFramework.Redis.MsgPack\bin\debug\*.symbols.nupkg"
del "CachingFramework.Redis.MsgPack.StrongName\bin\debug\*.symbols.nupkg"
del "CachingFramework.Redis.StrongName\bin\debug\*.symbols.nupkg"

nuget push "CachingFramework.Redis\bin\debug\*.nupkg" -source %1
nuget push "CachingFramework.Redis.MsgPack\bin\debug\*.nupkg" -source %1
nuget push "CachingFramework.Redis.MsgPack.StrongName\bin\debug\*.nupkg" -source %1
nuget push "CachingFramework.Redis.StrongName\bin\debug\*.nupkg" -source %1
