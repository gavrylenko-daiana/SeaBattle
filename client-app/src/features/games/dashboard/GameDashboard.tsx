import {useEffect} from "react";
import {observer} from "mobx-react-lite";
import {Grid} from "semantic-ui-react";
import GameList from "./GameList.tsx";
import LoadingComponent from "../../../app/layout/LoadingComponent.tsx";
import {useStore} from "../../../app/stores/store.ts";

const GameDashboard = observer(() => {
    const {gameStore, invitationStore} = useStore();
    const {loadGames} = gameStore;

    useEffect(() => {
        invitationStore.getInvitations();
        gameStore.setLoadingInitial(true);
        loadGames();
    }, []);

    useEffect(() => {
        gameStore.createHubConnection();
        return () => {
            gameStore.stopHubConnection();
        }
    }, []);

    if (gameStore.loadingInitial) {
        return <LoadingComponent content='Loading games...'/>
    }

    return (
        <Grid>
            <Grid.Column width='16'>
                <GameList/>
            </Grid.Column>
        </Grid>
    )
})

export default GameDashboard;