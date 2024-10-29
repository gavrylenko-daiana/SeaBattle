export class ShipFormValues {
    gameId?: number;
    coordinateId?: number;
    size: number;
    speed?: number;
    direction?: string;
    shipTypeName: string;

    constructor(init?: Partial<ShipFormValues>) {
        this.gameId = init?.gameId;
        this.coordinateId = init?.coordinateId;
        this.size = init?.size ?? 1;
        this.speed = init?.speed;
        this.direction = init?.direction ?? 'up';
        this.shipTypeName = init?.shipTypeName ?? 'hybrid';
    }
}