import {Coordinate} from "./coordinate.ts";

export interface GameField {
    coordinates: Coordinate[]
    gameFieldId: number
    boundaryCoordinate: number
    fieldSize: number
}