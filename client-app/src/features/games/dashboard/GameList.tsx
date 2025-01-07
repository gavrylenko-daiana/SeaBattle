import { observer } from "mobx-react-lite";
import GameListItem from "./GameListItem.tsx";
import {useStore} from "../../../app/stores/store.ts";
import {useEffect, useState} from "react";
import {Game} from "../../../app/models/game.ts";

const GameList = observer(() => {
    const { gameStore, userStore, invitationStore } = useStore();
    const currentUserId = userStore.user!.appUserId;
    const [sortedGames, setSortedGames] = useState<Game[]>([]);

    useEffect(() => {
        const loadUsers = async () => {
            try {
                const invitations = invitationStore.invitationRegister;
                const games = gameStore.Games;

                const isUserInvited = (game: Game) =>
                    invitations.some(inv =>
                        inv.appUserId === currentUserId &&
                        inv.gameId === game.gameId &&
                        game.progress === 0
                    );

                const sortedGames = games.sort((a, b) => {
                    const isInvitedA = isUserInvited(a) ? 1 : 0;
                    const isInvitedB = isUserInvited(b) ? 1 : 0;
                    return isInvitedB - isInvitedA;
                });

                setSortedGames(sortedGames);
            } catch (error) {
                console.error(error);
            }
        };
        loadUsers().then();
    }, []);

    return (
        <>
            {sortedGames.map((game) => (
                <GameListItem key={game.gameId} game={game} />
            ))}
        </>
    )
})

export default GameList;