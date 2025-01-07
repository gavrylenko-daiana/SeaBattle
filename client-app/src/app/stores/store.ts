import {createContext, useContext} from "react";
import CommonStore from "./commonStore";
import UserStore from "./userStore";
import GameStore from "./gameStore.ts";
import ModalStore from "./modalStore.ts";
import InvitationStore from "./invitationStore.ts";

interface Store {
    commonStore: CommonStore;
    userStore: UserStore;
    gameStore: GameStore;
    modalStore: ModalStore;
    invitationStore: InvitationStore;
}

export const store: Store = {
    commonStore: new CommonStore(),
    userStore: new UserStore(),
    gameStore: new GameStore(),
    modalStore: new ModalStore(),
    invitationStore: new InvitationStore()
}

export const StoreContext = createContext(store);

export function useStore() {
    return useContext(StoreContext);
}