import {observer} from "mobx-react-lite";
import {Coordinate} from "../../../app/models/coordinate.ts";
import ShipForm from "../form/ShipForm.tsx";
import {useStore} from "../../../app/stores/store.ts";
import {useEffect, useState} from "react";

interface Props {
    userId: number | undefined;
    selectedShipSize: number | null;
    isVisible: boolean
    isPreparation: boolean
}

const GameDetailedGameField = observer(({userId, selectedShipSize, isVisible, isPreparation}: Props) => {
    const {gameStore, modalStore} = useStore();
    const game = gameStore.selectedGame!;
    const gameField = game.gameUsers.find(gu => gu.appUserId === userId)?.gameField;
    const secondGameField = game.gameUsers.find(gu => gu.appUserId !== userId)?.gameField;
    const [turnMessage, setTurnMessage] = useState('');

    const winMessageStyle: React.CSSProperties = {
        backgroundColor: '#4CAF50',
        color: '#fff',
        padding: '10px',
        borderRadius: '5px',
        textAlign: 'center',
        margin: '10px 0',
        fontWeight: 'bold',
    };

    const loseMessageStyle: React.CSSProperties = {
        backgroundColor: '#f44336',
        color: '#fff',
        padding: '10px',
        borderRadius: '5px',
        textAlign: 'center',
        margin: '10px 0',
        fontWeight: 'bold',
    };

    useEffect(() => {
        const currentPlayerTurn = game.gameUsers.filter(gu => gu.isPlayerTurn).some(gu => gu.appUserId === userId);
        const allShipsDestroyedFirstGameField = gameField?.coordinates.filter(coordinate => coordinate.shipCoordinates.length > 0).every(coordinate => coordinate.coordinateType.type === "Destroyed");
        const allShipsDestroyedSecondGameField = secondGameField?.coordinates.filter(coordinate => coordinate.shipCoordinates.length > 0).every(coordinate => coordinate.coordinateType.type === "Destroyed");

        if (allShipsDestroyedFirstGameField || allShipsDestroyedSecondGameField) {
            const gameOutcomeMessage = currentPlayerTurn ? "You won!" : "You lost!";
            setTurnMessage(gameOutcomeMessage);
        } else {
            setTurnMessage(currentPlayerTurn ? "Your turn" : "Opponent's turn");
        }
    }, [game.gameUsers, userId]);

    const handleCoordinateClick = (coordinate: Coordinate) => {
        const allShipsDestroyedFirstGameField = gameField?.coordinates.filter(coordinate => coordinate.shipCoordinates.length === 10).every(coordinate => coordinate.coordinateType.type === "Destroyed");
        const allShipsDestroyedSecondGameField = secondGameField?.coordinates.filter(coordinate => coordinate.shipCoordinates.length === 10).every(coordinate => coordinate.coordinateType.type === "Destroyed");

        if ((turnMessage === "You won!" || turnMessage === "You lost!") && (allShipsDestroyedFirstGameField || allShipsDestroyedSecondGameField) && !selectedShipSize) {
            return;
        }

        const currentPlayerTurn = game.gameUsers.filter(gu => gu.isPlayerTurn).some(gu => gu.appUserId === userId);

        if (selectedShipSize && coordinate.coordinateType.type === "Empty") {
            const shipFormContent = (
                <ShipForm size={selectedShipSize}
                          coordinateId={coordinate.coordinateId}
                          gameId={game.gameId}
                          onCancel={() => modalStore.closeModal()}/>
            );
            modalStore.openModal(shipFormContent);
        }

        if (!isPreparation && !isVisible && currentPlayerTurn) {
            gameStore.updateCoordinateType(game.gameId, coordinate.coordinateId);
            gameStore.updateTurn(game.gameId, coordinate.coordinateId);
        }
    };

    const getCoordinateAdjacentDisplay = (coordinate: Coordinate) => {
        const areReady = game.gameUsers.every(gu => gu.isReady);

        if (isPreparation && areReady) {
            return '';
        }
        if (isVisible && isPreparation) {
            return coordinate?.coordinateType.type === "Filled" ? '✗' : '';
        } else if (coordinate.coordinateType.type === 'Missed') {
            return <span className="cross-mark">✗</span>;
        }
    };

    const getCoordinateShipDisplay = (coordinate: Coordinate, shipDirection: number, shipSize: number) => {
        if (coordinate?.coordinateType.type === "Filled" && isVisible || (coordinate.coordinateType.type === 'Hit' && isPreparation && isVisible || coordinate.coordinateType.type === "Destroyed")) {
            type ShipImages = {
                [key: string]: string;
            };

            const shipImages: ShipImages = {
                "1": '/assets/ships/firstship.png',
                "2": '/assets/ships/secondship.png',
                "3": '/assets/ships/thirdship.png',
                "4": '/assets/ships/fourthship.png',
            };

            const shipStyle = shipSize === 1 ? {width: '130%', height: '110%'} : {'--ship-size': shipSize};

            if (shipSize === 1) {
                return <img src={shipImages[shipSize]} className="ship-image one-size-ship" alt="ship"/>;
            } else {
                const shipClass = shipDirection === 2 || shipDirection === 3 ? 'ship-horizontal' : 'ship-vertical';
                return <img
                    src={shipImages[shipSize]}
                    className={`ship-image ${shipClass}`}
                    style={shipStyle}
                    alt="ship"
                />;
            }
        } else if (coordinate.coordinateType.type === 'Hit') {
            return <span className="cross-mark">🔥</span>;
        }
    };

    const renderHeaderRow = () => {
        const headerCells = [<div key={`header-0`} className="game-square-header"></div>];
        for (let i = 1; i <= 10; i++) {
            headerCells.push(<div key={`header-${i}`} className="game-square-header">{i}</div>);
        }
        return <div key={`header-row`} className="game-row-header">{headerCells}</div>;
    };

    const renderRows = () => {
        if (!gameField) {
            return [];
        }

        const rows = [renderHeaderRow()];
        const letters = 'ABCDEFGHIJ'.split('');
        for (let y = gameField.boundaryCoordinate, k = 0; y >= -gameField.boundaryCoordinate && k < gameField.fieldSize; y--, k++) {
            if (y === 0) {
                k--;
                continue;
            }
            const rowCells = [<div className="game-square-header">{letters[k]}</div>];
            for (let x = -gameField.boundaryCoordinate; x <= gameField.boundaryCoordinate; x++) {
                if (x === 0) {
                    continue;
                }
                const coordinate = gameField.coordinates.find(c => c.point.x === x && c.point.y === y)!;
                const isShipCoordinate = gameField.coordinates.some(c => c.shipCoordinates.some(sc => sc.coordinateId === coordinate.coordinateId));
                const shipCoordinate = coordinate.shipCoordinates.filter(sc => sc.coordinateId === coordinate.coordinateId)[0];
                const display = coordinate && isShipCoordinate && shipCoordinate
                    ? getCoordinateShipDisplay(coordinate, shipCoordinate.ship.direction, shipCoordinate.ship.size)
                    : getCoordinateAdjacentDisplay(coordinate);
                    rowCells.push(
                        <div className="game-square"
                             onClick={() => handleCoordinateClick(coordinate)}>
                            {display}
                        </div>
                    );
            }
            rows.push(<div key={`rows-${y}`} className="game-row">{rowCells}</div>);
        }

        return rows;
    };

    return (
        <div className="game-field">
            {!isVisible && (
                <div
                    className="turn-message"
                    style={turnMessage === "You won!" ? winMessageStyle : turnMessage === "You lost!" ? loseMessageStyle : {}}>
                    {turnMessage}
                </div>
            )}
            {renderRows()}
        </div>
    );
});

export default GameDetailedGameField;