import {Button, Container, Header, Segment, Image} from "semantic-ui-react";
import { Link } from "react-router-dom";
import { observer } from "mobx-react-lite";
import {useStore} from "../../app/stores/store.ts";

export default observer(function HomePage() {
    const { userStore } = useStore();

    return (
        <Segment inverted textAlign='center' vertical className='masthead' >
            <Container text>
                <Header as='h1' inverted>
                    <Image size='massive' src='/assets/ship.png' alt='logo' style={{ marginBottom: 12 }} />
                    Sea Battle
                </Header>
                {userStore.isLoggedIn ? (
                    <>
                        <Header as='h2' inverted content={`Welcome back ${userStore.user?.userName}`} />
                        <Button as={Link} to='/games' size='huge' inverted>
                            Go to Game Fields
                        </Button>
                    </>
                ) : (
                    <>
                        <Button as={Link} to='/login' size='huge' inverted>
                            Login
                        </Button>
                        <Button as={Link} to='/register' size='huge' inverted>
                            Register
                        </Button>
                    </>
                )}
            </Container>
        </Segment>
    )
})