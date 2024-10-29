import {createBrowserRouter, Navigate} from "react-router-dom";
import LoginForm from "../../features/users/LoginForm";
import App from "../layout/App";
import HomePage from "../../features/home/HomePage.tsx";
import RegisterForm from "../../features/users/RegisterForm.tsx";
import RequireAuth from "./RequireAuth.tsx";
import GameDashboard from "../../features/games/dashboard/GameDashboard.tsx";
import GameForm from "../../features/games/form/GameForm.tsx";
import GameDetails from "../../features/games/details/GameDetails.tsx";
import GameDetailedUserGame from "../../features/games/details/GameDetailedUserGame.tsx";

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App/>,
        children: [
            {
                element: <RequireAuth/>, children: [
                    {path: 'games', element: <GameDashboard/>},
                    {path: 'game/:id', element: <GameDetails/>},
                    {path: 'game/play/:id', element: <GameDetailedUserGame />},
                    {path: 'createGame', element: <GameForm key='create'/>},
                    {path: 'updateGame/:id', element: <GameForm key='update'/>},
                ]
            },
            {path: 'login', element: <LoginForm/>},
            {path: 'register', element: <RegisterForm/>},
            {path: '', element: <HomePage/>},
            {path: '*', element: <Navigate replace to=''/>},
        ]
    },
]);
