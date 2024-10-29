export class GameFormValues {
    gameId?: number = undefined;
    name: string = '';
    creatorId?: number = undefined;

    constructor(game?: GameFormValues) {
        if (game) {
            this.gameId = game.gameId;
            this.name = game.name;
            this.creatorId = game.creatorId;
        }
    }
}