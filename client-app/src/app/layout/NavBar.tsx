import {Button, Container, Dropdown, Menu} from "semantic-ui-react";
import {NavLink} from "react-router-dom";
import {observer} from "mobx-react-lite";
import {useStore} from "../stores/store.ts";

export default observer(function NavBar() {
    const {userStore: {user, logout}} = useStore();

    return (
        <Menu inverted fixed='top'>
            <Container>
                <Menu.Item as={NavLink} to='/' header>
                    <img src='/assets/ship.png' alt='logo' style={{marginRight: 10}}/>
                    SeaBattle
                </Menu.Item>
                <Menu.Item>
                    {user && (
                        <Button as={NavLink} to='/createGame' inverted content='Create new Game'/>)}
                </Menu.Item>
                <Menu.Item position='right'>
                    {user && (
                        <>
                            <Menu.Item>
                                {user && (<>Rating: {user?.rating}</>)}
                            </Menu.Item>
                            <Dropdown pointing='top left' text={user?.userName}>
                                <Dropdown.Menu>
                                    <Dropdown.Item onClick={logout} text='Logout' icon='power'/>
                                </Dropdown.Menu>
                            </Dropdown>
                        </>
                    )}
                </Menu.Item>
            </Container>
        </Menu>
    )
})