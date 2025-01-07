import {observer} from "mobx-react-lite";
import {useStore} from "../../../app/stores/store.ts";
import {Link, useNavigate, useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import * as Yup from "yup";
import LoadingComponent from "../../../app/layout/LoadingComponent.tsx";
import {Button, Header, Segment} from "semantic-ui-react";
import {Form, Formik} from "formik";
import {ShipFormValues} from "../../../app/models/shipFormValues.ts";
// import MyTextInput from "../../../app/common/form/MyTextInput.tsx";
import MySelectInput from "../../../app/common/form/MySelectInput.tsx";
import {directionOptions} from "../../../app/common/options/directionOptions.ts";

// import {shipTypeOptions} from "../../../app/common/options/shipTypeOptions.ts";

interface Props {
    onCancel: () => void;
    size: number
    gameId: number
    coordinateId: number
}

export default observer(function ShipForm({onCancel, size, coordinateId, gameId}: Props) {
    const {gameStore, modalStore} = useStore();
    const {addShipToField, loadingInitial} = gameStore;
    const {id} = useParams();
    const navigate = useNavigate();

    const [ship] = useState<ShipFormValues>(new ShipFormValues({size, coordinateId, gameId}));

    const validationSchema = Yup.object({
        // direction: Yup.string().required('The direction is required'),
        shipTypeName: Yup.string().required('The type of ship is required'),
        // speed: Yup.string().required('The speed is required'),
    })

    function handleFormSubmit(ship: ShipFormValues) {
        // debugger;
        addShipToField(ship).then(() => navigate(`/game/${id}`))
        gameStore.clearSelectedShipSize();
        modalStore.closeModal();
    }

    useEffect(() => {
        if (size === 1) {
            handleFormSubmit(ship);
        }
    }, [size]);

    if (loadingInitial) {
        return <LoadingComponent content='Loading game...'/>
    }

    return (
        <Segment clearing>
            <Header content='Ship' sub color='purple'/>
            <Formik
                enableReinitialize
                validationSchema={validationSchema}
                initialValues={ship}
                onSubmit={_values => handleFormSubmit(_values)}>
                {({handleSubmit, isSubmitting: isSubmitting}) => (
                    <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                        {/*<MyTextInput name='speed' placeholder='Speed' />*/}
                        {size > 1 &&
                            <MySelectInput options={directionOptions} name='direction' placeholder='Direction'/>}
                        {/*<MySelectInput options={shipTypeOptions} name='shipTypeName' placeholder='Type of ship' />*/}
                        {size > 1 && (
                            <>
                                <Button
                                    disabled={isSubmitting}
                                    loading={isSubmitting}
                                    floated="right"
                                    positive
                                    type="submit"
                                    content="Submit"
                                />
                                <Button onClick={onCancel} as={Link} to={`/game/${id}`} floated='right' type='button'
                                        content='Cancel'/>
                            </>
                        )}
                    </Form>
                )}
            </Formik>
        </Segment>
    )
})