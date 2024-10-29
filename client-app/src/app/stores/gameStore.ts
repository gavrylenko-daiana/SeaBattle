import {makeAutoObservable, runInAction} from "mobx";
import {Game} from "../models/game.ts";
import agent from "../api/agent.ts";
import {GameFormValues} from "../models/gameFormValues.ts";
import {store} from "./store.ts";
import {ShipFormValues} from "../models/shipFormValues.ts";
import {HubConnection, HubConnectionBuilder, LogLevel} from "@microsoft/signalr";

export default class GameStore {
    gameRegistry: Game[] = [];
    selectedGame?: Game = undefined;
    loadingInitial = false;
    selectedShipSize: number | null = null;
    hubConnection: HubConnection | null = null;

    constructor() {
        makeAutoObservable(this)
    }

    createHubConnection = (gameId?: number) => {
        const token = store.userStore.user?.token;

        if (token) {
            this.hubConnection = new HubConnectionBuilder()
                .withUrl(import.meta.env.VITE_PLAY_URL + '?gameId=' + gameId, {
                    accessTokenFactory: () => token
                })
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            this.hubConnection.start().catch(error => console.log('Error establishing connection: ', error));

            this.hubConnection.on('ReceiveCoordinateUpdate', (gameId: number) => {
                this.loadGame(gameId);
            });

            this.hubConnection.on('ReceiveGameListUpdate', () => {
                this.setLoadingInitial(true);
                this.loadGames();
            });
        } else {
            console.error("Token is not available. Cannot establish hub connection.");
        }
    }

    stopHubConnection = () => {
        this.hubConnection?.stop().catch(error => console.log('Error stopping connection: ', error));
    }

    loadGames = async () => {
        try {
            const games = await this.getGames();
            const user = store.userStore.user!;
            this.gameRegistry = games.filter(g => g.gameUsers.some(ug => ug.appUserId === user.appUserId) || g.progress === 0);
            this.setLoadingInitial(false);
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    }

    loadGame = async (id: number) => {
        let game = await this.getGame(id);
        if (game) {
            this.setGame(game);
            return game;
        } else {
            this.setLoadingInitial(true);
            try {
                game = await agent.Games.details(id);
                this.setGame(game);
                runInAction(() => this.selectedGame = game);
                this.setLoadingInitial(false);
                return game;
            } catch (error) {
                console.log(error);
                this.setLoadingInitial(false);
            }
        }
    }

    setGame = (game: Game) => {
        const index = this.gameRegistry.findIndex(g => g.gameId === game.gameId);
        if (index !== -1) {
            this.gameRegistry[index] = game;
            this.selectedGame = game;
        } else {
            this.gameRegistry.push(game);
            this.selectedGame = game;
        }
    }

    createGame = async (game: GameFormValues) => {
        try {
            await agent.Games.create(game);
            await this.loadGames();
        } catch (error) {
            console.log(error);
        }
    }

    deleteGame = async (id: number) => {
        this.setLoadingInitial(true);
        try {
            await agent.Games.delete(id);
            runInAction(() => {
                const index = this.gameRegistry.findIndex(game => game.gameId === id);
                if (index !== -1) {
                    this.gameRegistry.splice(index, 1);
                }
                if (this.selectedGame?.gameId === id) {
                    this.selectedGame = undefined;
                }
                this.setLoadingInitial(false);
            });
        } catch (error) {
            console.log(error);
            runInAction(() => {
                this.setLoadingInitial(false);
            });
        }
    }

    updateGame = async (game: GameFormValues) => {
        try {
            await agent.Games.update(game);
            runInAction(() => {
                if (game.gameId) {
                    this.gameRegistry = this.gameRegistry.map(g =>
                        g.gameId === game.gameId ? {...g, ...game} : g
                    );
                    this.selectedGame = this.gameRegistry.find(g => g.gameId === game.gameId);
                }
            })
        } catch (error) {
            console.log(error);
        }
    }

    joinGame = async (gameId: number) => {
        this.setLoadingInitial(true);
        try {
            await agent.Games.join(gameId);
            runInAction(() => {
                this.loadGames();
                this.setLoadingInitial(false);
            })
        } catch (error) {
            console.error('Error joining the game:', error);
            this.setLoadingInitial(false);
        }
    };

    invite = async (gameId: number, userId: number | undefined) => {
        this.setLoadingInitial(true);
        try {
            const gameInvitation = {gameId, userId};
            const getInvitation = await agent.Games.invite(gameInvitation);
            this.setLoadingInitial(false);
            store.invitationStore.addInvitation(getInvitation);
        } catch (error) {
            console.error('Error sending invitation to game:', error);
            this.setLoadingInitial(false);
        }
    }

    accept = async (gameId: number) => {
        this.setLoadingInitial(true);
        try {
            await agent.Games.accept(gameId);
            await this.loadGames();
            this.setLoadingInitial(false);
        } catch (error) {
            console.error('Error accepting the invitation to game:', error);
            runInAction(() => this.setLoadingInitial(false));
        }
    }

    addShipToField = async (ship: ShipFormValues) => {
        this.setLoadingInitial(true);
        try {
            await agent.Games.addShipToField(ship);
            await this.loadGame(ship.gameId!);
            this.setLoadingInitial(false);
        } catch (error) {
            console.error('Cannot add ship to the game field to game:', error);
            this.setLoadingInitial(false);
        }
    }

    updateUserStatusGame = async (gameId: number) => {
        try {
            await agent.Games.setPlayerStatusGameAsReady(gameId);
            runInAction(() => {
                this.loadGame(gameId);
            });
        } catch (error) {
            console.error('Cannot update user game status to ready:', error);
        }
    }

    updateCoordinateType = async (gameId: number, coordinateId: number) => {
        try {
            await agent.Games.updateCoordinateType(gameId, coordinateId);
        } catch (error) {
            console.error('Cannot update coordinate type:', error);
        }
    }

    updateTurn = async (gameId: number, coordinateId: number) => {
        try {
            await agent.Games.updateTurn(gameId, coordinateId);
        } catch (error) {
            console.error('Cannot update player turn:', error);
        }
    }

    getGames = () => {
        return agent.Games.list();
    }

    get Games() {
        return this.gameRegistry;
    }

    getGame = (id: number) => {
        return agent.Games.details(id);
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    clearSelectedGame = () => {
        this.selectedGame = undefined;
    }

    setSelectedShipSize(size: number | null) {
        this.selectedShipSize = size;
    }

    clearSelectedShipSize = () => {
        this.selectedShipSize = null;
    }
}
