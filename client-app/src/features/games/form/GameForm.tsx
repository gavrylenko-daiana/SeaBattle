import {observer} from "mobx-react-lite";
import {useStore} from "../../../app/stores/store.ts";
import {Link, useNavigate, useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import { v4 as uuid } from 'uuid';
import * as Yup from "yup";
import {GameFormValues} from "../../../app/models/gameFormValues.ts";
import LoadingComponent from "../../../app/layout/LoadingComponent.tsx";
import {Button, Header, Segment} from "semantic-ui-react";
import {Form, Formik} from "formik";
import MyTextInput from "../../../app/common/form/MyTextInput.tsx";

export default observer(function GameForm() {
    const { gameStore } = useStore();
    const { createGame, updateGame, loadGame, loadingInitial } = gameStore;
    const { id } = useParams();
    const navigate = useNavigate();

    const [game, setGame] = useState<GameFormValues>(new GameFormValues());

    const validationSchema = Yup.object({
        name: Yup.string().required('The name is required'),
    })

    useEffect(() => {
        if (id) {
            loadGame(parseInt(id, 10)).then(game => setGame(new GameFormValues(game)))
        }
    }, [id, loadGame])

    function handleFormSubmit(game: GameFormValues) {
        if (!game.gameId) {
            const newGame = {
                ...game,
                id: uuid()
            }
            createGame(newGame).then(() => navigate(`/games`))
        } else {
            updateGame(game).then(() => navigate(`/games`))
        }
    }

    if (loadingInitial) {
        return <LoadingComponent content='Loading game...' />
    }

    return (
        <Segment clearing>
            <Header content='Game' sub color='blue' />
            <Formik
                enableReinitialize
                validationSchema={validationSchema}
                initialValues={game}
                onSubmit={values =>  handleFormSubmit(values)}>
                {({ handleSubmit, isSubmitting: isSubmitting }) => (
                    <Form className='ui form' onSubmit={handleSubmit} autoComplete='off'>
                        <MyTextInput name='name' placeholder='Name' />
                        <Button
                            disabled={isSubmitting}
                            loading={isSubmitting}
                            floated='right'
                            positive
                            type='submit'
                            content='Submit' />
                        <Button as={Link} to='/games' floated='right' type='button' content='Cancel' />
                    </Form>
                )}
            </Formik>
        </Segment>
    )
})