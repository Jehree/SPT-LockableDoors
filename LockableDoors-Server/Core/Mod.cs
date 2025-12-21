using LockableDoorsServer.Models;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using System.Reflection;

namespace LockableDoorsServer.Core;

[Injectable]
public class CustomStaticRouter : StaticRouter
{
    private static HttpResponseUtil? _httpResponseUtil;
    private static ModHelper? _modHelper;
    private static JsonUtil? _jsonUtil;
    private static ISptLogger<CustomStaticRouter>? _logger;
    const string DataToServerRouteURL = "/jehree/lockabledoors/data_to_server";
    const string DataToClientRouteURL = "/jehree/lockabledoors/data_to_client";

    public CustomStaticRouter(
    JsonUtil jsonUtil,
    HttpResponseUtil httpResponseUtil,
    ModHelper modHelper,
    ISptLogger<CustomStaticRouter> logger) : base(
    jsonUtil,
    GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _modHelper = modHelper;
        _jsonUtil = jsonUtil;
        _logger = logger;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                DataToServerRouteURL,
                async (url, info, sessionId, output) => await OnDataToServer(url, info as ServerDataPack, sessionId)
            ),
            new RouteAction(
                DataToClientRouteURL,
                async (url, info, sessionId, output) => await OnDataToClient(url, info as ServerDataPack, sessionId)
            )
        ];
    }

    private static ValueTask<string> OnDataToServer(string url, ServerDataPack? info, MongoId sessionId)
    {
        if (info == null || _jsonUtil == null)
        {
            _logger.Success((info == null).ToString());
            _logger.Success((_jsonUtil == null).ToString());
            return new ValueTask<string>(_httpResponseUtil!.NullResponse());
        }

        string filePath = GetFilePath(info.ProfileId, info.MapId);
        _logger.Success(filePath);
        File.WriteAllText(filePath, _jsonUtil.Serialize(info));

        return new ValueTask<string>(_httpResponseUtil!.NullResponse());
    }

    private static ValueTask<string> OnDataToClient(string url, ServerDataPack? info, MongoId sessionId)
    {
        if (info == null || _jsonUtil == null)
        {
            return new ValueTask<string>(_httpResponseUtil!.NullResponse());
        }

        string filePath = GetFilePath(info.ProfileId, info.MapId);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return new ValueTask<string>(jsonData);
        }
        else
        {
            string jsonData = _jsonUtil.Serialize(ServerDataPack.GetEmpty(info.ProfileId, info.MapId))!;
            return new ValueTask<string>(jsonData);
        }
    }

    public static string GetFilePath(string profileId, string mapId)
    {
        string pathToMod = _modHelper!.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        ModConfig config = _modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json");

        string mapIdAdjusted = mapId;

        if (mapId == "factory4_day" || mapId == "factory4_night")
        {
            mapIdAdjusted = "factory";
        }
        if (mapId == "sandbox_high")
        {
            mapIdAdjusted = "sandbox";
        }

        string profileIdAdjusted = profileId;
        if (config.GlobalDoorDataProfile)
        {
            profileIdAdjusted = "global";
        }

        string folderPath = Path.Combine(pathToMod, "LockedDoorData", profileIdAdjusted);
        string filePath = Path.Combine(folderPath, $"{mapIdAdjusted}.json");

        Directory.CreateDirectory(folderPath);
        return filePath;
    }
}
