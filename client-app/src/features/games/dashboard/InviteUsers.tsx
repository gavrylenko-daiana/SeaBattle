import {Header, Button, List, Segment} from 'semantic-ui-react';
import {store, useStore} from "../../../app/stores/store";
import {observer} from "mobx-react-lite";
import {Formik, Form, FieldArray} from 'formik';
import {useEffect, useState} from "react";
import {User} from "../../../app/models/user.ts";
import {Game} from "../../../app/models/game.ts";

interface Props {
    onCancel: () => void;
    game: Game;
}

const InviteUsers = observer(({onCancel, game}: Props) => {
    const {userStore, gameStore} = useStore();
    const [users, setUsers] = useState<User[]>([]);

    useEffect(() => {
        const loadUsers = async () => {
            try {
                userStore.getUsers().then(u => setUsers(u.filter(u => u.appUserId !== userStore.user!.appUserId)));
            } catch (error) {
                console.error(error);
            }
        };
        loadUsers().then();
    }, []);

    return (
        <Segment>
            <Header icon='users' content='Invite Users'/>
            <Formik
                initialValues={{selectedUsers: []}}
                onSubmit={(values, {setSubmitting}) => {
                    values.selectedUsers.forEach(userId => {
                        gameStore.invite(game.gameId, userId).then();
                    });
                    setSubmitting(false);
                    store.modalStore.closeModal();
                }}
            >
                {({values}) => (
                    <Form>
                        <FieldArray
                            name="selectedUsers"
                            render={({push, remove}) => (
                                <List divided relaxed style={{maxHeight: '250px', overflowY: 'auto'}}>
                                    {users!.map(user => (
                                        <List.Item key={user.appUserId}>
                                            <label>
                                                <input
                                                    name="selectedUsers"
                                                    type="checkbox"
                                                    value={user.appUserId}
                                                    checked={values.selectedUsers.includes(user.appUserId as never)}
                                                    onChange={e => {
                                                        if (e.target.checked) {
                                                            push(user.appUserId);
                                                        }
                                                        else {
                                                            const idx = values.selectedUsers.indexOf(user.appUserId as never);
                                                            remove(idx);
                                                        }
                                                    }}
                                                />
                                                {user.userName}
                                            </label>
                                        </List.Item>
                                    ))}
                                </List>
                            )}
                        />
                        <Button
                            color='red'
                            onClick={onCancel}
                            content='Cancel'/>
                        <Button
                            floated='right'
                            positive
                            type='submit'
                            content='Invite'/>
                    </Form>
                )}
            </Formik>
        </Segment>
    );
});

export default InviteUsers;
