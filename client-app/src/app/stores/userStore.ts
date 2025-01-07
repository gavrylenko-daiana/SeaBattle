import {makeAutoObservable, runInAction} from "mobx";
import agent from "../api/agent";
import {User} from "../models/user";
import {router} from "../router/Routes";
import {store} from "./store";
import {UserFormValues} from "../models/userFormValues.ts";

export default class UserStore {
    user: User | null = null;
    listUsers: User[] = [];

    constructor() {
        makeAutoObservable(this)
    }

    get isLoggedIn() {
        return !!this.user;
    }

    login = async (creds: UserFormValues) => {
        try {
            const user = await agent.Account.login(creds);
            store.commonStore.setToken(user.token!);
            runInAction(() => this.user = user);
            router.navigate('/');
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    register = async (creds: UserFormValues) => {
        try {
            const user = await agent.Account.register(creds);
            store.commonStore.setToken(user.token!);
            runInAction(() => this.user = user);
            router.navigate('/');
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    getUser = async () => {
        try {
            const user = await agent.Account.current();
            store.commonStore.setToken(user.token!);
            runInAction(() => this.user = user);
            return user;
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    getUsers = async () => {
        try {
            const users = await agent.User.list();
            await this.setUsers(users);
            return users;
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    setUsers = async (users: User[]) => {
        this.listUsers = users;
    }

    logout = () => {
        store.commonStore.setToken(null);
        this.user = null;
        router.navigate('/');
    }
}