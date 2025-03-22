import { observer } from "mobx-react-lite";
import {IGame} from "../../../app/models/game.ts";
import {useStore} from "../../../app/stores/store.ts";
import {Button} from "semantic-ui-react";
import {Link} from "react-router-dom";

interface Props {
    game: IGame
    userId: number | undefined
}

function calculateShipsLeft(game: IGame, userId: number | undefined) {
    const shipsLeft: { [key: string]: number } = {"4": 1, "3": 2, "2": 3, "1": 4};
    const gameField = game.gameUsers.find(gu => gu.appUserId == userId)!.gameField;
    const filledCoordinates = gameField.coordinates.filter(c => c.coordinateType.type === "Filled" || c.coordinateType.type === "Hit" || c.coordinateType.type === "Destroyed");

    const countedShips: { [key: string]: boolean } = {};

    filledCoordinates.forEach(c => {
        c.shipCoordinates.forEach(shc => {
            const ship = shc.ship;
            if (!countedShips[ship.shipId]) {
                countedShips[ship.shipId] = true;
                if (shipsLeft[ship.size] > 0) {
                    shipsLeft[ship.size] -= 1;
                }
            }
        });
    });

    return shipsLeft;
}

const GameDetailedShips = observer(({game, userId}: Props) => {
    const { gameStore } = useStore();
    const shipsLeft = calculateShipsLeft(game, userId);
    const currentUserGame = game.gameUsers.filter(gu => gu.appUserId === userId)[0];
    const areReady = game.gameUsers.every(gu => gu.isReady);

    const handleShipClick = (size: number) => {
        gameStore.setSelectedShipSize(size);
    };

    const clearSelectShipSize = () => {
        gameStore.clearSelectedShipSize();
    }

    const markUserAsReady = () => {
        gameStore.updateUserStatusGame(game.gameId);
    }

    const renderShips = () => {
        const shipSizes: string[] = Object.keys(shipsLeft).sort((a, b) => Number(b) - Number(a));

        type ShipImages = {
            [key: string]: string;
        };

        const shipImages: ShipImages = {
            "1": '/assets/ships/firstship.png',
            "2": '/assets/ships/secondship.png',
            "3": '/assets/ships/thirdship.png',
            "4": '/assets/ships/fourthship.png',
        };

        return shipSizes.map(size => (
            <div key={size} className="ship-detail" onClick={() => shipsLeft[size] === 0 ? clearSelectShipSize() : handleShipClick(parseInt(size, 10))} style={{opacity: gameStore.selectedShipSize?.toString() === size ? 0.5 : 1}}>
                <span>Size {size}: </span>
                <img src={shipImages[size]} alt={`Ship size ${size}`} className="ship-image"/>
                <span>{shipsLeft[size]} left</span>
            </div>
        ));
    };

    return (
        <div className="ships-container">
            {renderShips()}
            <div className="ship-detail">
                <Button
                    onClick={() => gameStore.autoPlaceShips(game.gameId)}
                    color="purple"
                    floated="right"
                    content="Auto Place Ships"
                />
                {shipsLeft[1] === 0 && shipsLeft[2] === 0 && shipsLeft[3] === 0 && shipsLeft[4] === 0 &&
                    (
                        currentUserGame.isReady ?
                            areReady ?
                                <Button
                                    as={Link}
                                    to={`/game/play/${game.gameId}`}
                                    color='blue'
                                    floated='right'
                                    content='Start'
                                />
                                :
                            <span>Waiting for the second player</span> :
                            <Button
                                onClick={() => {
                                    markUserAsReady();
                                }}
                                color='blue'
                                floated='right'
                                content='Ready'
                            />
                    )
                }
            </div>
        </div>
    );
});

export default GameDetailedShips;
