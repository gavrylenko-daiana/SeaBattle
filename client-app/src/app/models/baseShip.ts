import {ShipCoordinate} from "./shipCoordinate.ts";
import {ShipType} from "./shipType.ts";

export interface BaseShip {
    shipId: number,
    range: number,
    direction: number,
    size: number,
    shipTypeId: number,
    shipType: ShipType,
    speed: number,
    shipCoordinates: ShipCoordinate[]
}