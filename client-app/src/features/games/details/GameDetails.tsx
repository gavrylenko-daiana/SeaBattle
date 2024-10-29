import {observer} from "mobx-react-lite";
import {useStore} from "../../../app/stores/store.ts";
import {useParams} from "react-router-dom";
import {useEffect} from "react";
import LoadingComponent from "../../../app/layout/LoadingComponent.tsx";
import { Grid } from "semantic-ui-react";
import GameDetailedGameField from "./GameDetailedGameField.tsx";
import GameDetailedShips from "./GameDetailedShips.tsx";

const GameDetails = observer(() => {
    const { gameStore, userStore } = useStore();
    const { selectedGame: game, loadGame, loadingInitial, clearSelectedGame } = gameStore;
    const { id } = useParams();

    useEffect(() => {
        if (id) {
            loadGame(parseInt(id));
        }
        return () => clearSelectedGame();
    }, [id]);

    useEffect(() => {
        gameStore.createHubConnection();
        return () => {
            gameStore.stopHubConnection();
        }
    }, []);

    if (loadingInitial || !game) {
        return <LoadingComponent />
    }

    return (
        <Grid>
            <Grid.Column width='10'>
                <GameDetailedGameField
                    userId={userStore.user!.appUserId}
                    selectedShipSize={gameStore.selectedShipSize}
                    isVisible={true}
                    isPreparation={true} />
            </Grid.Column>
            <Grid.Column width='6'>
                <GameDetailedShips
                    game={game}
                    userId={userStore.user!.appUserId} />
            </Grid.Column>
        </Grid>
    )
})

export default GameDetails;