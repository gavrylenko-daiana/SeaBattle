import {observer} from "mobx-react-lite";
import {useStore} from "../../../app/stores/store.ts";
import LoadingComponent from "../../../app/layout/LoadingComponent.tsx";
import {Grid} from "semantic-ui-react";
import GameDetailedGameField from "./GameDetailedGameField.tsx";
import {useParams} from "react-router-dom";
import {useEffect} from "react";

const GameDetailedUserGame = observer(() => {
    const {gameStore, userStore} = useStore();
    const {selectedGame: game, loadingInitial, clearSelectedGame} = gameStore;
    const { id } = useParams();

    useEffect(() => {
        if (id) {
            gameStore.loadGame(parseInt(id));
        }
        return () => {
            clearSelectedGame();
        }
    }, [id]);

    useEffect(() => {
        if (id) {
            gameStore.createHubConnection(parseInt(id));
        }
        return () => {
            gameStore.stopHubConnection();
        }
    }, []);

    if (loadingInitial || !game) {
        return <LoadingComponent/>
    }

    const secondPlayer = game!.gameUsers.filter(gu => gu.appUserId !== userStore.user!.appUserId)[0];

    return (
        <Grid>
            <Grid.Column width='8'>
                <GameDetailedGameField
                    key={secondPlayer.appUserId}
                    userId={secondPlayer.appUserId}
                    selectedShipSize={gameStore.selectedShipSize}
                    isVisible={false}
                    isPreparation={false}/>
            </Grid.Column>
            <Grid.Column width='8' className='second-field'>
                <GameDetailedGameField
                    key={userStore.user!.appUserId}
                    userId={userStore.user!.appUserId}
                    selectedShipSize={gameStore.selectedShipSize}
                    isVisible={true}
                    isPreparation={false}/>
            </Grid.Column>
        </Grid>
    )
});

export default GameDetailedUserGame;