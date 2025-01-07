import {Point} from "./point.ts";
import {CoordinateType} from "./coordinateType.ts";
import {GameField} from "./gameField.ts";
import {ShipCoordinate} from "./shipCoordinate.ts";

export interface Coordinate {
    coordinateId: number
    point: Point
    pointId: number
    quadrant: number
    coordinateType: CoordinateType
    coordinateTypeId: number
    gameField: GameField
    gameFieldId: number
    shipCoordinates: ShipCoordinate[]
    isFirstCoordinate: boolean
}