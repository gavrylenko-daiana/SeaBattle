import {Coordinate} from "./coordinate.ts";
import {BaseShip} from "./baseShip.ts";

export interface ShipCoordinate {
    shipCoordinateId: number
    coordinateId: number
    coordinate: Coordinate
    shipId: number
    ship: BaseShip
}