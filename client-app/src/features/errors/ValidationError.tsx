import {Message} from "semantic-ui-react";

interface Props {
    errors?: string | string[];
}

export default function ValidationError({errors}: Props) {
    if (!errors) return null;
    errors = typeof errors === 'string' ? [errors] : errors;
    if (typeof errors === 'object') errors = Object.values(errors);

    return (
        <Message error>
            <Message.List>
                {errors.map((error, index) => (
                    <Message.Item key={index}>{error}</Message.Item>
                ))}
            </Message.List>
        </Message>
    );
}