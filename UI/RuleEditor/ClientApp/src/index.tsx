import "./index.scss";

import React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { browserHistory, Route, Router } from "react-router";
import { runWithAdal } from "react-adal";
import { authContext } from "./aad/AuthProvider";
import { trackPageView, initAppInsights } from "./metrics";
import configureStore from "./store/ConfigureStore";
import { syncHistoryWithStore } from "react-router-redux";
import { RuleListPage } from "./pages/rules/RuleListPage";

const store = configureStore();
const history = syncHistoryWithStore(browserHistory, store);
initAppInsights();

runWithAdal(authContext, () => {
  ReactDOM.render(
    <Provider store={store}>
      <Router history={history}>
        <Route path="/" component={RuleListPage} onEnter={() => trackPageView("RuleListPage")} />
      </Router>
    </Provider>,
    document.getElementById("root"));
}, false);
