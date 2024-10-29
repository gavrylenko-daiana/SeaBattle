import agent from "../api/agent.ts";
import {GameInvitation} from "../models/gameInvitation.ts";

export default class InvitationStore {
    invitationRegister: GameInvitation[] = [];

    getInvitations = async () => {
        try {
            const invitations = await agent.Invitation.list();
            this.setInvitations(invitations);
            return invitations;
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    setInvitations = (invitations: GameInvitation[]) => {
        this.invitationRegister = invitations;
    }

    addInvitation = (invitation: GameInvitation) => {
        this.invitationRegister.push(invitation);
    }

    getInvitationRegister = () => {
        return this.invitationRegister;
    }
}