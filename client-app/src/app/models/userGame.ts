import {User} from "./user.ts";
import {GameField} from "./gameField.ts";

export interface UserGame {
    userGamesId: number
    gameId: number
    game: null
    appUserId: number
    appUser: User
    isReady: boolean
    isPlayerTurn: boolean
    gameFieldId: number
    gameField: GameField
}