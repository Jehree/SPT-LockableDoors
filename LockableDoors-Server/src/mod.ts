import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { FileUtils, InitStage, ModHelper } from "./mod_helper";
import * as fs from "fs";
import Config from "../config.json";

class Mod implements IPreSptLoadMod {
    public Helper = new ModHelper();

    public DataToServer = "/jehree/lockabledoors/data_to_server";
    public DataToClient = "/jehree/lockabledoors/data_to_client";

    public preSptLoad(container: DependencyContainer): void {
        this.Helper.init(container, InitStage.PRE_SPT_LOAD);

        this.Helper.registerStaticRoute(this.DataToServer, "LockableDoors-DataToServer", Routes.onDataToServer, Routes);
        this.Helper.registerStaticRoute(this.DataToClient, "LockableDoors-DataToClient", Routes.onDataToClient, Routes, true);
    }
}

export class Routes {
    public static onDataToServer(url: string, info: any, sessionId: string, output: string, helper: ModHelper): void {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;
        const path: string = this.getPath(profileId, mapId);
        fs.writeFileSync(path, JSON.stringify(info));
    }

    public static onDataToClient(url: string, info: any, sessionId: string, output: string, helper: ModHelper): string {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;
        const path: string = this.getPath(profileId, mapId);
        if (!fs.existsSync(path)) {
            return `{"ProfileId": "${profileId}", "MapId": "${mapId}", "LockedDoorIds": []}`;
        } else {
            return fs.readFileSync(path, "utf8");
        }
    }

    public static getPath(profileId: string, mapId: string): string {
        let mapName: string = mapId;
        if (mapId === "factory4_day" || mapId === "factory4_night") {
            mapName = "factory";
        }
        if (mapId === "sandbox_high") {
            mapName = "sandbox";
        }
        let profileName: string = profileId;
        if (Config.global_door_data_profile) {
            profileName = "global";
        }

        const folderPath: string = FileUtils.pathCombine(ModHelper.modPath, "locked_door_data", profileName);
        const filePath: string = FileUtils.pathCombine(folderPath, `${mapName}.json`);
        fs.mkdirSync(folderPath, { recursive: true });

        return filePath;
    }
}

export const mod = new Mod();
