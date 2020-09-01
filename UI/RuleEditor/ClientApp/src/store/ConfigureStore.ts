import { applyMiddleware, createStore, Store } from "redux";
import { composeWithDevTools } from "redux-devtools-extension";
import thunk from "redux-thunk";
import { getValue } from "../config";
import { actionTracerMiddleware } from "../metrics/ActionTracerMiddleware";
import { dataCenterRootReducer } from "./RootReducer";

export default function configureStore(): Store {
    let middleware;
    if (getValue("UI:REDUX_DEVTOOLS") === "True") {
        middleware = composeWithDevTools(applyMiddleware(thunk, actionTracerMiddleware));
    } else {
        middleware = applyMiddleware(thunk, actionTracerMiddleware);
    }

    return createStore(
        dataCenterRootReducer,
        middleware
    );
}
