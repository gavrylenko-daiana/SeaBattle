import { ErrorMessage, Form, Formik } from "formik";
import { observer } from "mobx-react-lite";
import { Button, Header } from "semantic-ui-react";
import MyTextInput from "../../app/common/form/MyTextInput";
import { useStore } from "../../app/stores/store";
import * as Yup from "yup";
import ValidationError from "../errors/ValidationError.tsx";

export default observer(function LoginForm() {
    const { userStore } = useStore();

    return (
        <Formik
            initialValues={{ email: '', password: '', error: null }}
            onSubmit={(values, { setErrors }) =>
                userStore.login(values).catch(error => {
                    if (error.response && error.response.data && error.response.data.errors) {
                        setErrors({error: error.response.data.errors});
                    } else if (error.response && error.response.data) {
                        setErrors({error: error.response.data.message});
                    }
                })
            }
            validationSchema={Yup.object({
                    email: Yup.string().required(),
                    password: Yup.string().required(),
                })}
        >
            {({ handleSubmit, isSubmitting, errors, isValid, dirty }) => (
                <Form className='ui form error' onSubmit={handleSubmit} autoComplete='off' >
                    <Header as='h2' content='Login to Sea Battle' color="blue" textAlign="center" />
                    <MyTextInput placeholder="Email" name='email' />
                    <MyTextInput placeholder="Password" name='password' type='password' />
                    <ErrorMessage name='error' render={() =>
                        <ValidationError errors={errors.error}/>
                    }/>
                    <Button disabled={!isValid || !dirty || isSubmitting} loading={isSubmitting} color='blue' content='Login' type="submit" fluid />
                </Form>
            )}

        </Formik>
    )
})