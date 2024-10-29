import {UserGame} from "./userGame.ts";
import {GameFormValues} from "./gameFormValues.ts";

export interface IGame {
    gameId: number
    name: string
    progress: number
    gameUsers: UserGame[]
    creatorId: number
}

export class Game implements IGame {
    constructor(init: GameFormValues) {
        this.gameId = init.gameId!;
        this.name = init.name;
    }

    gameId: number;
    name: string;
    progress: number = 0;
    gameUsers: UserGame[] = [];
    creatorId: number = 0;
}