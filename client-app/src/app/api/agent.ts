import axios, {AxiosError, AxiosResponse} from "axios";
import {User} from "../models/user.ts";
import {store} from "../stores/store.ts";
import {UserFormValues} from "../models/userFormValues.ts";
import {Game} from "../models/game.ts";
import {GameFormValues} from "../models/gameFormValues.ts";
import {GameInvitation} from "../models/gameInvitation.ts";
import {ShipFormValues} from "../models/shipFormValues.ts";

axios.defaults.baseURL = import.meta.env.VITE_API_URL;

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

axios.interceptors.request.use(config => {
    const token = store.commonStore.token;
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
})

axios.interceptors.response.use(async response => {
    return response;
},(error: AxiosError) => {
    return Promise.reject(error);
})

const requests = {
    get: <T>(url: string) => axios.get<T>(url).then(responseBody),
    post: <T>(url: string, body?: object) => axios.post<T>(url, body).then(responseBody),
    patch: <T>(url: string, body?: object) => axios.patch<T>(url, body).then(responseBody),
    put: <T>(url: string, body?: object) => axios.put<T>(url, body).then(responseBody),
    del: <T>(url: string) => axios.delete<T>(url).then(responseBody)
}

const Account = {
    current: () => requests.get<User>('/account'),
    login: (user: UserFormValues) => requests.post<User>('/account/login', user),
    register: (user: UserFormValues) => requests.post<User>('/account/register', user)
}

const User = {
    list: () => requests.get<User[]>('/user')
}

const Invitation = {
    list: () => requests.get<GameInvitation[]>('/invitation')
}

const Games = {
    list: () => requests.get<Game[]>(`/game`),
    details: (id: number) => requests.get<Game>(`/game/${id}`),
    create: (game: GameFormValues) => requests.post<Game>(`/game`, game),
    update: (game: GameFormValues) => requests.put<void>(`/game/${game.gameId}`, game),
    delete: (id: number) => requests.del<void>(`/game/${id}`),
    join: (id: number) => requests.post<Game>(`/game/join/${id}`),
    invite: (gameInvitation: { gameId: number; userId: number | undefined }) => requests.post<GameInvitation>(`/game/${gameInvitation.gameId}/invite/${gameInvitation.userId}`, gameInvitation),
    accept: (id: number) => requests.post<Game>(`/game/accept/${id}`),
    addShipToField: (ship: ShipFormValues) => requests.put<void>(`/game/add/${ship.gameId}`, ship),
    autoPlaceShips: (id: number) => requests.post<void>(`/game/place-ships/${id}`),
    setPlayerStatusGameAsReady: (id: number) => requests.patch<void>(`/game/ready/${id}`),
    updateCoordinateType: (gameId: number, coordinateId: number) => requests.put<void>(`/game/type/${coordinateId}/${gameId}`),
    updateTurn: (gameId: number, coordinateId: number) => requests.put<void>(`/game/turn/${gameId}/${coordinateId}`),
    findGame: () => requests.get<void>(`/game/find-game`)
}

const agent = {
    Account,
    User,
    Invitation,
    Games
}

export default agent;