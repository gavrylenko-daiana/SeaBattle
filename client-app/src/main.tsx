import ReactDOM from 'react-dom/client'
import {store, StoreContext} from "./app/stores/store.ts";
import './app/layout/styles.css'
import {RouterProvider} from "react-router-dom";
import {router} from "./app/router/Routes.tsx";

ReactDOM.createRoot(document.getElementById('root')!).render(
    <StoreContext.Provider value={store}>
        <RouterProvider router={router}/>
    </StoreContext.Provider>
)
