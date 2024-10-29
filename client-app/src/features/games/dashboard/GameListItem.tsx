import {Link, NavLink} from "react-router-dom";
import {Item, Button, Segment} from "semantic-ui-react";
import {Game} from "../../../app/models/game.ts";
import {useStore} from "../../../app/stores/store.ts";
import InviteUsers from "./InviteUsers.tsx";
import {observer} from "mobx-react-lite";
import {useEffect, useState} from "react";

interface Props {
    game: Game
}

const GameListItem = observer(({game}: Props) => {
    const {userStore, gameStore, modalStore, invitationStore} = useStore();
    const currentUserId = userStore.user!.appUserId;
    const isCreator = currentUserId === game.creatorId;
    const isParticipant = game.gameUsers.some(gameUser => gameUser.appUserId === currentUserId);
    const isTwoPlayerInGame = game.gameUsers.length === 2;

    const [isInvitedUser, setInvitations] = useState<boolean>(false);

    useEffect(() => {
        const loadUsers = async () => {
            try {
                const invitationsFromStore = invitationStore.getInvitationRegister();
                const isInvited = invitationsFromStore.some(inv => inv.appUserId === currentUserId && inv.gameId === game.gameId);
                setInvitations(isInvited);
            } catch (error) {
                console.error(error);
            }
        };
        loadUsers().then();
    }, [invitationStore]);

    const handleInviteClick = () => {
        const content = (
            <InviteUsers
                key={game.gameId}
                game={game}
                onCancel={() => modalStore.closeModal()}
            />
        );
        modalStore.openModal(content);
    };

    return (
        <Segment.Group style={{height: '170px'}}>
            <Segment>
                <Item.Group>
                    <Item>
                        <Item.Content>
                            <Item.Header>{game.name}</Item.Header>
                        </Item.Content>
                        <Item.Content>
                            {isParticipant && isTwoPlayerInGame && (
                                <Button
                                    as={Link}
                                    to={`/game/${game.gameId}`}
                                    color='blue'
                                    floated='right'
                                    content='Play'
                                />
                            )}
                            {!isParticipant && isInvitedUser && (
                                <Button
                                    onClick={() => gameStore.accept(game.gameId)}
                                    color='blue'
                                    floated='right'
                                    content='Accept'
                                />
                            )}
                            {!isParticipant && !isInvitedUser && !isTwoPlayerInGame && (
                                <Button
                                    onClick={() => gameStore.joinGame(game.gameId)}
                                    color='blue'
                                    floated='right'
                                    content='Join'
                                />
                            )}
                            {isCreator && (
                                <>
                                    <Button
                                        as={NavLink}
                                        to={`/updateGame/${game.gameId}`}
                                        color='orange'
                                        floated='right'
                                        content='Edit'
                                    />
                                    <Button
                                        onClick={() => gameStore.deleteGame(game.gameId)}
                                        color='red'
                                        floated='right'
                                        content='Delete'
                                    />
                                    {game.progress === 0 &&
                                    <Button
                                        onClick={handleInviteClick}
                                        color='blue'
                                        floated='right'
                                        content='Invite'
                                    />
                                    }
                                </>
                            )}
                        </Item.Content>
                    </Item>
                </Item.Group>
                <Segment clearing>
                    <Item.Group>
                        <Item>
                            <Item.Description>
                                Players: <br/>
                                {game.gameUsers.map((g, index) => (
                                    <span
                                        key={index}>{g.appUser.userName}{index < game.gameUsers.length - 1 ? ', ' : ''}</span>
                                ))}
                            </Item.Description>
                        </Item>
                    </Item.Group>
                </Segment>
            </Segment>
        </Segment.Group>
    );
})

export default GameListItem;